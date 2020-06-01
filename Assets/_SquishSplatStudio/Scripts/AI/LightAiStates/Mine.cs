/* SquishSplatStudio - https://discord.gg/HVjBM9T
 * You are welcome to use this file or contents for learning and/or non-commercial projects
 * Written by Dubh @Dubh#1508 <Discord> - 2020
 */

using UnityEngine;

namespace SquishSplatStudio
{
    public class Mine : State
    {
        public Mine(AiParameters aiParameters) : base(aiParameters) => name = AgentCommandType.Mine;

        float _actionInSeconds = 5f;
        float _lastAction = 5f;
        float distance = Mathf.Infinity;
        GameObject[] mines;
        bool _isMining;
        MineCapacity mineCapacity;

        public override void Enter()
        {
            if (mines == null)
                mines = GameObject.FindGameObjectsWithTag("Mine");

            for (int i = 0; i < mines.Length; i++)
            {
                var tempDistance = Vector3.Distance(_aiParameters.LightWorker.assignedWork.WaypointPosition, mines[i].transform.position);
                if (tempDistance < distance)
                {
                    distance = tempDistance;
                    _aiParameters.Target = mines[i].transform;
                    _target = mines[i].transform;
                    if (distance < 15f)
                        _aiParameters.Point = (distance <= 15f) ? _target.position : _aiParameters.LightWorker.assignedWork.WaypointPosition;
                }
            }

            if (_target != null)
                _aiParameters.Target = _target;

            if (mineCapacity == null)
            {
                mineCapacity = _target.GetComponent<MineCapacity>();
                mineCapacity.RegisterWorkerCommands(_aiParameters.LightWorker.assignedWork);
            }

            _animator.SetTrigger("startAction");
            _animator.SetBool("doingAction", true);

            _agent.Freeze();

            base.Enter();
        }

        public override void Update()
        {
            if (Vector3.Distance(_aiParameters.Npc.transform.position, _aiParameters.Target.position) <= 5f)
            {
                _agent.Freeze();

                if (mineCapacity.CurrentCrystals <= 0)
                {
                    _nextState = new Guard(_aiParameters);
                    _stage = EventType.Exit;
                    base.Update();
                    return;
                }

                if (_aiParameters.Target == null)
                {
                    base.Update();
                    return;
                }

                _lastAction += Time.deltaTime;
                if (_lastAction <= _actionInSeconds)
                {
                    base.Update();
                    return;
                }
                
                _lastAction = 0f;
                _isMining = !_isMining;
                if (!_isMining)
                {
                    mineCapacity.ConsumeCrystal(5); 
                    _nextState = new Deposit(_aiParameters);
                    _stage = EventType.Exit;
                }
                else
                {
                    var db = PlacementType.MineBeam.InstantiatedPrefab();
                    db.transform.position = _aiParameters.Target.position;
                    db.transform.LookAt(_aiParameters.Npc.transform.position);
                }
                
            }
            else
            {
                _nextState = new Pursue(_aiParameters);
                _stage = EventType.Exit;
            }

            base.Update();
        }

        public override void Exit()
        {
            _animator.ResetTrigger("startAction");
            _animator.SetBool("doingAction", false);
            base.Exit();
        }
    }
}