/* SquishSplatStudio - https://discord.gg/HVjBM9T
 * You are welcome to use this file or contents for learning and/or non-commercial projects
 * Written by Dubh @Dubh#1508 <Discord> - 2020
 */

using System;
using UnityEngine;
using UnityEngine.AI;

namespace SquishSplatStudio
{
    [Serializable]
    [RequireComponent(typeof(NavMeshAgent), typeof(BuildRequirements))]
    [RequireComponent(typeof(ObjectHealth), typeof(ObjectResource), typeof(LightLink))]
    public class LightWorker : MonoBehaviour
    {
        [SerializeField] public int workerID;
        [SerializeField] ObjectResource resources;
        [SerializeField] WorkCommand _assignedWork;
        [SerializeField] public Transform handPosition;
        public WorkCommand assignedWork
        {
            get { return _assignedWork; }
        }

        private void Start()
        {
            WaypointHandler.Instance.RegisterWorker(this);
        }

        // Primary Loop
        private void Update()
        {

        }

        /// <summary>
        /// Assigns a given Work Command to the Light Worker
        /// </summary>
        /// <param name="work"></param>
        public void AssignWork(WorkCommand work)
        {
            _assignedWork = work;
        }

        /// <summary>
        /// Returns True/False if the Worker has a Work Command assigned
        /// </summary>
        /// <returns></returns>
        public bool HasWork()
        {
            // Checks against Default workCommand and whether the Vector3 position is Zero : IE default
            if (assignedWork.WorkType == AgentCommandType.Guard && assignedWork.WaypointPosition == Vector3.zero)
            {
                return false;
            }

            // Checks against Default workCommand and ensures the guard position isn't default
            if (assignedWork.WorkType == AgentCommandType.Guard && assignedWork.WaypointPosition != Vector3.zero)
            {
                return true;
            }

            // If anything other workCommand is assigned, they have work
            if (assignedWork.WorkType != AgentCommandType.Guard)
            {
                return true;
            }

            // Should never reach here - if we do, it'll override the work assigned
            return false;
        }

        // END OF LIGHTWORKER
    }
}