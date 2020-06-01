/* SquishSplatStudio - https://discord.gg/HVjBM9T
 * You are welcome to use this file or contents for learning and/or non-commercial projects
 * Written by Dubh @Dubh#1508 <Discord> - 2020
 */

using UnityEngine;
using UnityEngine.AI;

namespace SquishSplatStudio
{
    public class State
    {
        //public AiStateType name;
        public AgentCommandType name;
        protected EventType _stage;
        protected GameObject _npc;
        protected Animator _animator;
        protected Transform _target;
        protected Vector3 _point;
        protected State _nextState;
        protected NavMeshAgent _agent;
        protected LightWorker _lightWorker;
        protected AiParameters _aiParameters;

        float _visibilityDistance = 30.0f;
        float _visibilityAngle = 60.0f;
        float _shootingDistance = 15.0f;

        public State(AiParameters aiParameters)
        {
            _aiParameters = aiParameters;
            _npc = _aiParameters.Npc;
            _agent = _aiParameters.Agent;
            _animator = _aiParameters.Animator;
            _stage = EventType.Enter;
            _target = _aiParameters.Target;
            _point = _aiParameters.Point;
            _lightWorker = _aiParameters.LightWorker;
        }

        public virtual void Enter() => _stage = EventType.Update;
        public virtual void Update() 
        {
            //_stage = EventType.Update;
            if (NewCommand())
                GetAssignedWork();
        } 
        public virtual void Exit() => _stage = EventType.Exit;

        public State Process()
        {
            if (_stage == EventType.Enter) Enter();
            if (_stage == EventType.Update) Update();
            if (_stage == EventType.Exit)
            {
                Exit();
                return _nextState;
            }
            return this;
        }

        protected bool CanSeeTarget()
        {
            var direction = _target.position - _npc.transform.position;
            var angle = Vector3.Angle(direction, _npc.transform.forward);

            if (direction.magnitude < _visibilityDistance && angle < _visibilityAngle) 
                return true;

            return false;
        }

        protected bool CanAttackTarget()
        {
            if (_target == null) return false;
            var direction = _target.position - _npc.transform.position;
            if (direction.magnitude < _shootingDistance && _target.tag != "Mine" && _target.gameObject.activeInHierarchy)
                return true;
            
            return false;
        }

        protected bool NewCommand()
        {
            if (_aiParameters.AssignedWork == false) return false;
            _aiParameters.AssignedWork = false;
            return true;
        }

        protected void GetAssignedWork() // do I need this in the future?
        {
            switch (_lightWorker.assignedWork.WorkType)
            {
                case AgentCommandType.Build:
                    _nextState = new Build(_aiParameters);
                    break;
                case AgentCommandType.Mine:
                    _nextState = new Mine(_aiParameters);
                    break;
                case AgentCommandType.Explore:
                    _nextState = new Explore(_aiParameters);
                    break;
                case AgentCommandType.Escort:
                case AgentCommandType.Patrol:
                case AgentCommandType.Guard:
                    _nextState = new Guard(_aiParameters);
                    break;
                case AgentCommandType.Attack:
                    _nextState = new Attack(_aiParameters);
                    break;
                case AgentCommandType.Seek:
                case AgentCommandType.Attacked:
                case AgentCommandType.Pursue:
                    _nextState = new Pursue(_aiParameters);
                    break;
                case AgentCommandType.Idle:
                case AgentCommandType.Any:
                default:
                    _nextState = new Idle(_aiParameters);
                    break;
            }

            _aiParameters.Point = _lightWorker.assignedWork.WaypointPosition;
            _stage = EventType.Exit;
        }

        protected bool CanSeeATarget()
        {
            var enemies = GameObject.FindGameObjectsWithTag("Respawn"); // (this is not efficient) (this whole object isn't, haha!)

            Transform chosenTarget = null;
            float dist = Mathf.Infinity;
            bool targetFound = false;

            for (int i = 0; i < enemies.Length; i++)
            {
                var target = enemies[i];
                var targetDir = target.transform.position - _npc.transform.position;
                var targetPos = target.transform.position + targetDir.normalized * 10;

                if (Vector3.Distance(_npc.transform.position, targetPos) < dist)
                {
                    RaycastHit raycastInfo;
                    var rayToTarget = target.transform.position - _npc.transform.position;
                    if (Physics.Raycast(_npc.transform.position, rayToTarget, out raycastInfo))
                    {
                        _target = target.transform;
                        if (CanSeeTarget())
                        {
                            chosenTarget = target.transform;
                            dist = Vector3.Distance(_npc.transform.position, targetPos);
                            targetFound = true;
                        }
                    }
                }
            }
            _aiParameters.Target = chosenTarget;
            _aiParameters.Point = (_aiParameters.Target == null) ? _aiParameters.Npc.transform.position : _aiParameters.Target.position; // last known point
            return targetFound;
        }
    }
}