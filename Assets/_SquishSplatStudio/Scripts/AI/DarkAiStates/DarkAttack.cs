/* SquishSplatStudio - https://discord.gg/HVjBM9T
 * You are welcome to use this file or contents for learning and/or non-commercial projects
 * Written by Dubh @Dubh#1508 <Discord> - 2020
 */

using UnityEngine;

namespace SquishSplatStudio
{
    public class DarkAttack : DarkState
    {
        float rotationSpeed = 2.0f;

        public DarkAttack(DarkAiParameters aiParameters) : base(aiParameters) => name = AgentCommandType.Attack;

        float _actionInSeconds = 1f;
        float _lastAction;

        ObjectHealth _targetHealth;

        public override void Enter()
        {
            _aiParameters.Agent.Freeze();
            if (_target != null)
                _targetHealth = _target.GetComponent<ObjectHealth>();
            base.Enter();
        }

        public override void Update()
        {
            if (!CanAttackTarget())
            {
                _nextState = new DarkPursue(_aiParameters);
                _stage = EventType.Exit;
            }
            else
            {
                _lastAction += Time.deltaTime;
                if (_targetHealth._deathDone || !_target.gameObject.activeInHierarchy)
                {
                    _aiParameters.DoingWork = false;
                    _aiParameters.AssignedWork = false;
                    _nextState = new DarkGuard(_aiParameters);
                    _stage = EventType.Exit;
                }
                else if (_lastAction > _actionInSeconds)
                {
                    _lastAction = 0f;


                    Vector3 direction = _target.position - _aiParameters.Agent.transform.position;
                    float angle = Vector3.Angle(direction, _aiParameters.Agent.transform.forward);
                    direction.y = 0;

                    _aiParameters.Agent.transform.rotation = Quaternion.Slerp(_aiParameters.Agent.transform.rotation,
                                                        Quaternion.LookRotation(direction),
                                                        Time.deltaTime * rotationSpeed);

                    var db = PlacementType.DarkBeam.InstantiatedPrefab();
                    db.transform.position = _target.position + (Vector3.up * _aiParameters.Agent.GetComponent<Collider>().bounds.center.y);
                    db.transform.LookAt(_aiParameters.Agent.transform.position);
                    _targetHealth.AdjustHealth(-GameHandler.Instance.ShadeDamage);
                    var ot = _aiParameters.Target.GetComponent<BuildRequirements>().ObjectType;
                    if (ot.HasFlag(PlacementType.Structure))
                    {
                        AudioController.Instance.PlayEnemyAttackingStructure();
                    }
                    else
                    {
                        AudioController.Instance.PlayEnemyAttacking();
                    }
                    
                }
            }

            base.Update();
        }

        public override void Exit() => base.Exit();
    }
}