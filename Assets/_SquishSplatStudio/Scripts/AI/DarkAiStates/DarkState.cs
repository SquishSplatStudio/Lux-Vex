/* SquishSplatStudio - https://discord.gg/HVjBM9T
 * You are welcome to use this file or contents for learning and/or non-commercial projects
 * Written by Dubh @Dubh#1508 <Discord> - 2020
 */

using UnityEngine;

namespace SquishSplatStudio
{
    public class DarkState
    {
        public AgentCommandType name;
        protected EventType _stage;
        protected DarkState _nextState;
        protected Transform _target;

        protected DarkAiParameters _aiParameters;

        protected AgentCommandType _issuedCommand;

        [SerializeField] float _visibilityDistance = 60.0f;
        [SerializeField] float _visibilityAngle = 60.0f;
        [SerializeField] float _shootingDistance = 15f;

        public DarkState(DarkAiParameters aiParameters)
        {
            _aiParameters = aiParameters;
            _stage = EventType.Enter;
            _target = _aiParameters.Target;
            if (_aiParameters.Point == Vector3.zero)
                _aiParameters.Point = _aiParameters.Npc.transform.position;
        }

        public virtual void Enter() => _stage = EventType.Update;
        public virtual void Update() 
        {
            if (NewCommand())
                GetAssignedWork();
        } 
        public virtual void Exit() => _stage = EventType.Exit;

        public DarkState Process()
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
            if (_target == null) return false;
            var direction = _target.position - _aiParameters.Npc.transform.position;
            var angle = Vector3.Angle(direction, _aiParameters.Npc.transform.forward);

            if (direction.magnitude < _visibilityDistance && angle < _visibilityAngle) 
                return true;

            return false;
        }

        protected bool CanAttackTarget()
        {
            if (_target == null) return false;
            var direction = _target.position - _aiParameters.Npc.transform.position;
            if (direction.magnitude < _shootingDistance)
                return true;
            
            return false;
        }

        protected bool NewCommand()
        {
            if (!_aiParameters.AssignedWork || _aiParameters.DoingWork) return false;
            _aiParameters.DoingWork = true;
            return true;
        }

        protected void GetAssignedWork()
        {
            switch (_aiParameters.AgentCommandType)
            {
                case AgentCommandType.Attack:
                    _nextState = new DarkAttack(_aiParameters);
                    break;
                case AgentCommandType.Pursue:
                    _nextState = new DarkPursue(_aiParameters);
                    break;
                case AgentCommandType.Guard:
                default:
                    _nextState = new DarkGuard(_aiParameters);
                    break;
            }
            _stage = EventType.Exit;
        }

        protected bool CanSeeATarget()
        {
            //var enemies = GameObject.FindObjectsOfType<PlayerStructure>();
            var enemies = PlacementHandler.Instance.allLightLinks; // GameObject.FindObjectsOfType<LightLink>();
            Transform chosenTarget = null;
            float dist = Mathf.Infinity;
            bool targetFound = false;

            for (int i = 0; i < enemies.Length; i++)
            {
                var target = enemies[i];
                if (target.name.Contains("LightAccelerator")) continue;
                if (target.name.Contains("(Placing)")) continue;
                var targetDir = target.transform.position - _aiParameters.Npc.transform.position;
                var targetPos = target.transform.position + targetDir.normalized * 10;

                if (Vector3.Distance(_aiParameters.Npc.transform.position, targetPos) < dist)
                {
                    RaycastHit raycastInfo;
                    var rayToTarget = target.transform.position - _aiParameters.Npc.transform.position;
                    if (Physics.Raycast(_aiParameters.Npc.transform.position, rayToTarget, out raycastInfo))
                    {
                        _target = target.transform;
                        if (CanSeeTarget())
                        {
                            chosenTarget = target.transform;
                            dist = Vector3.Distance(_aiParameters.Npc.transform.position, targetPos);
                            targetFound = true;
                        }
                    }
                }
            }
            _target = chosenTarget;
            _aiParameters.Target = _target;
            return targetFound;
        }
    }
}