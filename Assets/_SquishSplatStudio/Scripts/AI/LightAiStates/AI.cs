/* SquishSplatStudio - https://discord.gg/HVjBM9T
 * You are welcome to use this file or contents for learning and/or non-commercial projects
 * Written by Dubh @Dubh#1508 <Discord> - 2020
 */

using UnityEngine;
using UnityEngine.AI;

namespace SquishSplatStudio
{
    public class AI : MonoBehaviour
    {
        AiParameters _aiParameters;
        State _currentState;

        float _nextStateCheck;

        void Start()
        {
            _aiParameters = new AiParameters()
            {
                Agent = GetComponent<NavMeshAgent>(),
                Npc = gameObject,
                Animator = GetComponent<Animator>(),
                Target = null,
                Point = gameObject.transform.position,
                LightWorker = GetComponent<LightWorker>(),
                AssignedWork = false
            };

            _currentState = new Guard(_aiParameters);
        }

        private void Update()
        {
            if (Time.frameCount % 2 == 1) return;
            _currentState = _currentState.Process();
        }

        public void WorkAssigned(LightWorker lightWorker)
        {
            if (!lightWorker || _aiParameters == null) return;
            if (lightWorker == _aiParameters?.LightWorker)
                _aiParameters.AssignedWork = true;
        }
    }

    public class AiParameters
    {
        public GameObject Npc { get; set; }
        public NavMeshAgent Agent { get; set; }
        public Animator Animator { get; set; }
        public Transform Target { get; set; }
        public Vector3 Point { get; set; }
        public LightWorker LightWorker { get; set; }
        public bool AssignedWork { get; set; }
    }
}
