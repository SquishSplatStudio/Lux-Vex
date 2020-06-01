/* SquishSplatStudio - https://discord.gg/HVjBM9T
 * You are welcome to use this file or contents for learning and/or non-commercial projects
 * Written by Dubh @Dubh#1508 <Discord> - 2020
 */

using UnityEngine;

namespace SquishSplatStudio
{
    public class Guard : State
    {
        public Guard(AiParameters aiParameters) : base(aiParameters) => name = AgentCommandType.Guard;

        float _actionInSeconds = 3f;
        float _lastAction;
        float wanderJitter = 1f;

        public override void Enter()
        {
            if (_aiParameters.Point == Vector3.zero)
                _aiParameters.Point = _aiParameters.Npc.transform.position;

            base.Enter();
        }

        public override void Update()
        {
            if (!_aiParameters.LightWorker.HasWork() && _aiParameters.LightWorker.assignedWork.WorkType == AgentCommandType.Guard)
            {


            }
            else if(!_aiParameters.LightWorker.HasWork())
            {
                _nextState = new Idle(_aiParameters);
                _stage = EventType.Exit;
            }
            else if (_aiParameters.LightWorker.assignedWork.WorkType != AgentCommandType.Guard)
            {
                _nextState = new Idle(_aiParameters);
                _stage = EventType.Exit;
            }
            else if(Vector3.Distance(_aiParameters.Npc.transform.position, _aiParameters.Point) >= 5f)
            {
                _nextState = new Pursue(_aiParameters);
                _stage = EventType.Exit;
            }
            else
            {
                _lastAction += Time.deltaTime;
                if (_lastAction > _actionInSeconds)
                {
                    _lastAction = 0f;
                    var wanderTarget = _point;
                    wanderTarget += new Vector3(Random.Range(-1.0f, 1.0f) * wanderJitter, 0, Random.Range(-1.0f, 1.0f) * wanderJitter);
                    _agent.Go(wanderTarget);
                }
            }

            if (CanSeeATarget())
            {
                _nextState = new Pursue(_aiParameters);
                _stage = EventType.Exit;
            }
            
            base.Update();
        }

        public override void Exit()
        {
            base.Exit();
        }
    }
}