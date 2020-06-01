/* SquishSplatStudio - https://discord.gg/HVjBM9T
 * You are welcome to use this file or contents for learning and/or non-commercial projects
 * Written by Dubh @Dubh#1508 <Discord> - 2020
 */

using UnityEngine;

namespace SquishSplatStudio
{
    public class Attack : State
    {
        float rotationSpeed = 2.0f;
        public Attack(AiParameters aiParameters) : base(aiParameters) => name = AgentCommandType.Attack;

        float _actionInSeconds = 2f;
        float _lastAction;
        bool soundPlayed = false;

        ObjectHealth _targetHealth;

        public override void Enter()
        {
            if (_target)
                _targetHealth = _target.GetComponent<ObjectHealth>();

            _agent.Freeze();
            base.Enter();
        }

        public override void Update()
        {
            
            if (!_target)
            {
                _nextState = new Idle(_aiParameters);
                _stage = EventType.Exit;
            }
            else if (!CanAttackTarget())
            {
                _nextState = new Idle(_aiParameters);
                _stage = EventType.Exit;
            }
            else
            {
                Vector3 direction = _target.position - _npc.transform.position;
                float angle = Vector3.Angle(direction, _npc.transform.forward);
                direction.y = 0;

                _npc.transform.rotation = Quaternion.Slerp(_npc.transform.rotation,
                                                    Quaternion.LookRotation(direction),
                                                    Time.deltaTime * rotationSpeed);

                _lastAction += Time.deltaTime;
                if (_targetHealth._deathDone || !_target.gameObject.activeInHierarchy)
                {
                    _target = null;
                    _aiParameters.Target = null;
                    _nextState = new Guard(_aiParameters);
                    _stage = EventType.Exit;
                }
                else if (_lastAction > _actionInSeconds)
                {
                    _lastAction = 0f;
                    var db = PlacementType.LightBolt.InstantiatedPrefab();
                    db.transform.position = _npc.GetComponent<LightWorker>().handPosition.position;
                    db.transform.rotation = _npc.GetComponent<LightWorker>().handPosition.rotation;
                    db.GetComponent<LightSap>().damageToDeal = GameHandler.Instance.LightWorkerDamage;
                    db.transform.LookAt(_target.GetComponent<Collider>().bounds.center);
                    db.SetActive(true);

                    if (!soundPlayed)
                    {
                        soundPlayed = true;
                        AudioController.Instance.PlayWorkerAttacking();
                    }
                    //_targetHealth.AdjustHealth(-5);
                }
            }
        }

        public override void Exit()
        {
            base.Exit();
        }
    }


}