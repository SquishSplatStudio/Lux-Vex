/* SquishSplatStudio - https://discord.gg/HVjBM9T
 * You are welcome to use this file or contents for learning and/or non-commercial projects
 * Written by Dubh @Dubh#1508 <Discord> - 2020
 */

using UnityEngine;
using UnityEngine.AI;

namespace SquishSplatStudio
{
    public class DarkAI : MonoBehaviour
    {
        public DarkAiParameters _aiParameters;
        public DarkState _currentState;

        //float _nextStateCheck;
        bool _isInit;

        void Start()
        {
            Init();
        }

        void Init()
        {
            if (_isInit) return;

            _aiParameters = new DarkAiParameters()
            {
                Agent = GetComponent<NavMeshAgent>(),
                Npc = gameObject,
                Animator = GetComponent<Animator>(),
                Target = null,
                Point = Vector3.zero,
                AssignedWork = false
            };

            _currentState = new DarkGuard(_aiParameters);

            _isInit = true;
        }

        void Update()
        {
            if (Time.frameCount % 2 == 0) return;
            _currentState = _currentState.Process();
        }

        public void IssueCommand(AgentCommandType command, Transform target)
        {
            Init();
            _aiParameters.Target = target;
            _aiParameters.AgentCommandType = command;

            switch (command)
            {
                case AgentCommandType.Idle:
                    _aiParameters.Point = transform.position;
                    _currentState = new DarkIdle(_aiParameters);
                    break;
                case AgentCommandType.Guard:
                    _aiParameters.Point = transform.position;
                    break;
                case AgentCommandType.Attack:
                    _aiParameters.Point = target.position;
                    break;
            }
            
            _aiParameters.AssignedWork = true;
        }
    }

    public class DarkAiParameters
    {
        public GameObject Npc { get; set; }
        public NavMeshAgent Agent { get; set; }
        public Animator Animator { get; set; }
        public Transform Target { get; set; }
        public Vector3 Point { get; set; }
        public bool AssignedWork { get; set; }
        public bool DoingWork { get; set; }
        public AgentCommandType AgentCommandType { get; set; }
    }
}
