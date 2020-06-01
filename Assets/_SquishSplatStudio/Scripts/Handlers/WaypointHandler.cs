using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SquishSplatStudio
{
    [Serializable]
    public class WaypointHandler : MonoBehaviour
    {
        [Header("Waypoint Settings")]
        [SerializeField] public Transform markerParent;
        [SerializeField] public GameObject generalPrefab;
        [SerializeField] public List<WaypointMarker> waypointMarkers;
        [SerializeField] public List<WorkCommand> availableWork;
        [SerializeField] public List<AssignedWork> assignedWork;
        [SerializeField] public List<LightWorker> availableWorkers;
        [SerializeField] public List<LightWorker> totalWorkers;

        [SerializeField] Event _workAssigned; // event to trigger work assigned.

        List<LightWorker> _removeAvailableWorkers = new List<LightWorker>();

        float _workCheckTick = 1f;
        float _workCheckTimer = 1f;

        // Singleton Reference
        private static WaypointHandler _instance;
        public static WaypointHandler Instance { get { return _instance; } }

        // Setup Variables
        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(this.gameObject);
            }
            else
            {
                _instance = this;
            }
        }

        // Update is called once per frame
        void Update()
        {
            // Work Check Timer
            if (_workCheckTimer > 0f)
                _workCheckTimer -= Time.deltaTime;

            if (_workCheckTimer <= 0f)
            {
                // Reset Timer
                _workCheckTimer = _workCheckTick;

                // Process Available Work information
                AssignWorkCommands();
            }
        }

        /// <summary>
        /// Loops through the current list of Available Work and assigns workers to roles that aren't fulfilled
        /// </summary>
        void AssignWorkCommands()
        {
            // Do Work Cleanup
            List<AssignedWork> awCleanup = new List<AssignedWork>();
            for (int awcu = 0; awcu < assignedWork.Count; awcu++)
            {
                if (assignedWork[awcu].WorkCommand.WaypointObject == null && assignedWork[awcu].WorkCommand.WorkObject == null)
                    awCleanup.Add(assignedWork[awcu]);
            }
            if (awCleanup.Count > 0)
            {
                for (int x = 0; x < awCleanup.Count; x++)
                {
                    // Remove any Light Works attached
                    foreach(LightWorker lw in awCleanup[x].AssignedWorkers)
                    {
                        RemoveWorkerFromWork(assignedWork.IndexOf(awCleanup[x]), lw);
                    }

                    // Remove the Assigned Work
                    assignedWork.Remove(awCleanup[x]);
                }
            }

            // If there is work to be Assigned by no available workers, recall some workers
            if (availableWork.Count > 0 && availableWorkers.Count == 0)
            {
                int numberOfWorkersNeeded = 0;

                foreach (WorkCommand wc in availableWork)
                {
                    numberOfWorkersNeeded += GetPriority(wc.Priority);
                }

                TryRecallWorkers(numberOfWorkersNeeded);
            }

            // Go through all Available Workers and get them Assigned to a Job
            for (int i = 0; i < availableWorkers.Count; i++)
            {
                AssignWork(availableWorkers[i]);
            }

            // Once they've been assigned a Job, they'll be added to the removal list. We can now safely clear them out of Available Workers
            for (int i = 0; i < _removeAvailableWorkers.Count; i++)
            {
                // We're doing an If check here because the LoadBalancing Method can move the Workers around without assigning to Available Workers.
                if (availableWorkers.Contains(_removeAvailableWorkers[i]))
                    availableWorkers.Remove(_removeAvailableWorkers[i]);
            }

            // Once they've been removed from Available workers, clear this temp list to make ready for next wave.
            _removeAvailableWorkers.Clear();

            // Load Balance Available Work
            if (assignedWork.Count > 1)
                LoadBalanceAssignedWork();
        }

        /// <summary>
        /// We will balance all workers between Jobs
        /// </summary>
        void LoadBalanceAssignedWork()
        {
            // Divide the Total Worker Count between Number of Assigned Work
            int workerSplit = Mathf.FloorToInt(totalWorkers.Count / assignedWork.Count);

            // Early Out
            if (workerSplit == 1)
                return;

            // Temp Arrays of Number of Recalls per Work
            List<int> awIndex = new List<int>();
            //List<int> workersToRecall = new List<int>();
            List<int> addToWork = new List<int>();
            List<int> workersToAdd = new List<int>();
            List<LightWorker> recalledWorkers = new List<LightWorker>();

            // Loop through the Assigned Work
            for (int a = 0; a < assignedWork.Count; a++)
            {
                // Remove if they have more than the Split amount
                if (assignedWork[a].AssignedWorkers.Count > workerSplit)
                {
                    int numberToReAssign = assignedWork[a].AssignedWorkers.Count - workerSplit;

                    for (int b = 0; b < numberToReAssign; b++)
                    {
                        RemoveWorkerFromWork(a, assignedWork[a].AssignedWorkers[GetFurthestWorkerIndex(assignedWork[a])]);
                    }
                }
            }
        }

        /// <summary>
        /// Registers a Work Waypoint to be delegated out to Workers
        /// </summary>
        /// <param name="priority"></param>
        /// <param name="work"></param>
        public void RegisterWork(WorkCommand work)
        {
            // Temp Variable
            bool addWorkCommand = true;

            // Loop through available work to see if we need to update the work that's had it's priority changed.
            if (availableWork.Count > 0)
            {
                // See if the Work Command already exists
                foreach (WorkCommand wc in availableWork)
                {
                    // Check to see if this work already exits
                    if (wc.WaypointPosition == work.WaypointPosition && wc.WorkType == work.WorkType)
                    {
                        addWorkCommand = false;

                        if (wc.Priority != work.Priority)
                        {
                            wc.Priority = work.Priority;

                            //UpdateWorkPriorities();
                        }
                    }
                }
            }

            // Add the work if we need to
            if (addWorkCommand)
                availableWork.Add(work);

            // Assign new Work
            AssignWorkCommands();
        }

        /// <summary>
        /// Updates a given Worker Command to a new Priority level and adjusts worker counts accordingly
        /// </summary>
        /// <param name="wc"></param>
        /// <param name="priority"></param>
        public void UpdatePriority(int workList, WorkCommand wc, int priority)
        {
            // New Priority
            priority = Mathf.Clamp(priority, 0, 3);
            int newPriority = priority;
            int currentWorkerCount = 0;
            int workerChange = 0;
            int newRequiredWorkerCount = 0;

            // Loop through the Available Work List if that's the list we're to look at
            if (workList == 0)
            {
                for (int i = 0; i < availableWork.Count; i++)
                {
                    if (availableWork[i] == wc)
                    {
                        availableWork[i].Priority = priority;

                        if (priority == 3)
                            TryRecallWorkers(totalWorkers.Count, null);
                    }
                }
            }
            else
            {
                // Loop through Assigned Work to find the right job
                foreach (AssignedWork aw in assignedWork)
                {
                    // Did we find the right Work Command?
                    if (aw.WorkCommand == wc)
                    {
                        Debug.LogFormat("{0} > UpdatePriority() > {1} has had its priority changed from {2} to {3}", this.name, aw.WorkCommand.Name, aw.WorkCommand.Priority, newPriority);

                        aw.WorkCommand.Priority = newPriority;
                        newRequiredWorkerCount = GetPriority(newPriority);
                        currentWorkerCount = aw.AssignedWorkers.Count;

                        // Determine the Change of Worker
                        if (newRequiredWorkerCount != currentWorkerCount)
                        {
                            // Do we have More?
                            workerChange = newRequiredWorkerCount - currentWorkerCount;
                            Debug.LogFormat("{0} > UpdatePriority() > {1} needs {2} Worker(s)", this.name, aw.WorkCommand.Name, workerChange);
                        }

                        // Do we have More or Less than what's required
                        if (workerChange > 0)
                        {
                            // We need X more workers
                            if (availableWorkers.Count < workerChange)
                                TryRecallWorkers(workerChange - availableWorkers.Count, aw.WorkCommand);
                        }
                        else
                        {
                            // We need Y less workers
                            for (int i = 0; i < workerChange * -1; i++)
                            {
                                aw.AssignedWorkers[aw.AssignedWorkers.Count - 1].AssignWork(new WorkCommand());
                                availableWorkers.Add(aw.AssignedWorkers[aw.AssignedWorkers.Count - 1]);
                                aw.AssignedWorkers.Remove(aw.AssignedWorkers[aw.AssignedWorkers.Count - 1]);
                            }
                        }

                        // Break loop as we've found the right Work Command
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Trys to recall a given number of Workers
        /// </summary>
        void TryRecallWorkers(int numberToRecall, WorkCommand dontRecallFrom = null)
        {
            // Early Out for Max Priority Work
            if (numberToRecall == totalWorkers.Count)
            {
                // Recall From All Work
                if (dontRecallFrom == null)
                {
                    for (int i = 0; i < assignedWork.Count; i++)
                    {
                        // for (int x = 0; x < assignedWork[i].assignedWorkers.Count; x++)
                        for (int x = assignedWork[i].AssignedWorkers.Count; x > 0; x--)
                        {
                            RemoveWorkerFromWork(i, assignedWork[i].AssignedWorkers[x]);
                        }
                    }

                    // Return out as we don't need to proceed further
                    return;
                }

                // Don't Recall Workers from this Work
                //if (dontRecallFrom != null || dontRecallFrom != new WorkCommand())
                //{
                //
                //}
            }

            // Temp Variables
            List<AssignedWork> _sortedLowPriority = assignedWork.OrderBy(o => o.WorkCommand.Priority).ToList();
            int recalledWorkers = 0;
            LightWorker[] recallWorkers = new LightWorker[numberToRecall];
            int[] assignedWorkIndex = new int[numberToRecall];

            // Loop through the Assigned Work
            foreach (AssignedWork aw in _sortedLowPriority)
            {
                if (recalledWorkers < numberToRecall && aw.WorkCommand != dontRecallFrom)
                {
                    // Placeholder for when we need to prioritize work?
                    if (dontRecallFrom != null)
                    {
                        if (aw.WorkCommand.Priority < dontRecallFrom.Priority)
                        {

                        }
                    }

                    // Set the Number of Assigned Workers to the this Work
                    int assignedWorkersCount = aw.AssignedWorkers.Count;

                    // Set the Number of Required Workers for this work
                    int requiredWorkerCount = GetPriority(aw.WorkCommand.Priority);

                    // Determined Spare worker Count
                    int spareAssignedWorkers = assignedWorkersCount - requiredWorkerCount;

                    // Determine Recall Number
                    int recallNumber = numberToRecall - recalledWorkers;

                    // Recall appropriate number of Workers from this work.
                    if (spareAssignedWorkers >= recallNumber)
                    {
                        for (int i = 0; i < recallNumber; i++)
                        {
                            recallWorkers[i] = aw.AssignedWorkers[GetFurthestWorkerIndex(aw)];
                            assignedWorkIndex[i] = assignedWork.IndexOf(aw);

                            // Adjust the number of Workers to be recalled by 1
                            recalledWorkers++;
                        }
                    }
                }
            }

            // Recall All Found Workers
            if (recalledWorkers > 0)
            {
                for (int r = 0; r < numberToRecall; r++)
                {
                    RemoveWorkerFromWork(assignedWorkIndex[r], recallWorkers[r]);
                }
            }
        }

        /// <summary>
        /// Removes a given Light Worker from a given Work Command
        /// </summary>
        /// <param name="work"></param>
        /// <param name="lw"></param>
        void RemoveWorkerFromWork(int workID, LightWorker lw)
        {
            // Remove the AI from the Assigned Workers List
            assignedWork[workID].AssignedWorkers.Remove(lw);

            // Assign Null Command to Light Worker
            lw.AssignWork(new WorkCommand());

            // Add Light Worker to Available Workers
            if (!availableWorkers.Contains(lw))
                availableWorkers.Add(lw);
        }

        /// <summary>
        /// Register a new Light Worker and Assign them a job.
        /// </summary>
        /// <param name="lightWorker"></param>
        public void RegisterWorker(LightWorker lightWorker)
        {
            // Add them to the List if they aren't already on it - Which they shouldn't be
            if (!availableWorkers.Contains(lightWorker))
            {
                availableWorkers.Add(lightWorker);
                totalWorkers.Add(lightWorker);
            }

            // Assign Work to the new Worker
            AssignWork(lightWorker);
        }

        /// <summary>
        /// Assigns some work to the Worker
        /// </summary>
        /// <param name="lightWorker"></param>
        void AssignWork(LightWorker lightWorker)
        {
            // Highest Priority Work in 'Assigned Work'
            AssignedWork hpAssWC = GetAssignedWork(true, true, true, lightWorker);

            // Highest Priority Work in 'Available Work'
            WorkCommand hpAvlWC = GetClosestHighPriority(availableWork, lightWorker.transform.position);

            // If we have a Max Priority Task, assign the Worker to it
            if (hpAssWC != null && hpAssWC.WorkCommand.Priority == 3)
            {
                AssignLightWorkerToWork(hpAssWC, lightWorker);
                return;
            }

            // If we don't have a Max Priority Assigned Task, Create a new Assigned Work
            if (hpAvlWC != null)
            {
                CreateAssignedWork(hpAvlWC, lightWorker);
                return;
            }

            // If we have an Assigned Task and no Work to Assign, assign to closest highest priority.
            if (hpAssWC != null)
            {
                AssignLightWorkerToWork(hpAssWC, lightWorker);
                return;
            }
        }

        /// <summary>
        /// Adds a given Light Worker to a given Work Command
        /// </summary>
        /// <param name="work"></param>
        /// <param name="lw"></param>
        void AssignLightWorkerToWork(AssignedWork work, LightWorker lw)
        {
            // Add to the Work
            work.AssignedWorkers.Add(lw);

            // Assign Work to the Light Worker
            lw.AssignWork(work.WorkCommand);

            // Trigger Work Assiged to AI
            _workAssigned.Trigger(lw);

            // Add to Removal List
            _removeAvailableWorkers.Add(lw);
        }

        /// <summary>
        /// Creates and Assigned Work item given the Work Command and a Light Worker
        /// </summary>
        /// <param name="workCommand"></param>
        /// <param name="lightWorker"></param>
        void CreateAssignedWork(WorkCommand workCommand, LightWorker lightWorker)
        {
            // Create new Assigned Work item and assign this Worker
            AssignedWork newAW = new AssignedWork(workCommand);

            // Add to Assigned Work List
            assignedWork.Add(newAW);

            // Add the Light Worker
            AssignLightWorkerToWork(newAW, lightWorker);

            // Remove the WorkCommand from the Available Work
            availableWork.Remove(workCommand);
        }

        /// <summary>
        /// Removes a given Work Command and reassigns the Listed Workers
        /// </summary>
        /// <param name="workCommand"></param>
        public void RemoveMarker(WorkCommand workCommand)
        {
            // Store a temp list of Assigned Works so we don't accidently reassign them to the same task
            List<LightWorker> assignedWorkers = new List<LightWorker>();
            int _awIndex = -1;

            // Loop through the assigned Work
            for (int i = 0; i < assignedWork.Count; i++)
            {
                if (assignedWork[i].WorkCommand == workCommand)
                {
                    for (int y = 0; y < assignedWork[i].AssignedWorkers.Count; y++)
                    {
                        assignedWorkers.Add(assignedWork[i].AssignedWorkers[y]);
                    }

                    _awIndex = i;

                    break;
                }
            }

            // ReAssign the Workers
            for (int i = 0; i < assignedWorkers.Count; i++)
            {
                RemoveWorkerFromWork(_awIndex, assignedWorkers[i]);
            }

            // Do we need to Cancel the Build and Potentially refund whats been placed?
            if (workCommand.WorkObject != null && !workCommand.WorkObject.GetComponent<BuildRequirements>().objectBuilt)
                workCommand.WorkObject.GetComponent<BuildRequirements>().CancelBuild();

            // Remove the Work Command from the Available Work List
            if (availableWork.Contains(workCommand))
            {
                availableWork.Remove(workCommand);
            }

            // Remove the Work Command from the Assigned Work List
            if (_awIndex != -1)
                assignedWork.RemoveAt(_awIndex);

            // Destroy the Marker Object
            if (workCommand.WaypointObject != null)
                DestroyImmediate(workCommand.WaypointObject);
        }

        /// <summary>
        /// Removes the Light Worker from the Available work list or Assigned Work
        /// </summary>
        /// <param name="lightWorker"></param>
        public void RemoveLightWorker(LightWorker lightWorker)
        {
            // Remove from the Assigned Workers list if present
            foreach (AssignedWork aw in assignedWork)
            {
                if (aw.AssignedWorkers.Contains(lightWorker))
                    aw.AssignedWorkers.Remove(lightWorker);
            }

            // Remove from the Available Workers list if present
            if (availableWorkers.Contains(lightWorker))
                availableWorkers.Remove(lightWorker);

            // Remove from Total Worker List
            if (totalWorkers.Contains(lightWorker))
                totalWorkers.Remove(lightWorker);
        }

        /// <summary>
        /// Returns a required worker number based on weighted Percentage
        /// </summary>
        /// <param name="priority"></param>
        int GetPriority(int priority)
        {
            // Temp Variable
            int returnInt = 0;
            float priorityPercent = 0f;

            // calc
            switch (priority)
            {
                // Low Priority
                case 0:
                    priorityPercent = 0.1f;
                    break;

                // Medium Priority
                case 1:
                    priorityPercent = 0.33f;
                    break;

                // High Priority
                case 2:
                    priorityPercent = 0.75f;
                    break;

                // Extreme Priority
                case 3:
                    priorityPercent = 1.0f;
                    break;
            }

            // Calculate required Number of Workers
            returnInt = Mathf.CeilToInt(totalWorkers.Count * priorityPercent);

            // Return Int
            return returnInt;
        }

        /// <summary>
        /// Returns the Assigned Works Worker Index for the furthest Light Work to the Assigned Work.
        /// </summary>
        /// <param name="assignedWork"></param>
        /// <returns></returns>
        int GetFurthestWorkerIndex(AssignedWork assignedWork)
        {
            // Get the furthest Assigned worker and Recall them
            float furthestLW = 0f;
            int lwIndex = 0;

            for (int lwi = 0; lwi < assignedWork.AssignedWorkers.Count; lwi++)
            {
                float distToWork = Vector3.Distance(assignedWork.AssignedWorkers[lwi].transform.position, assignedWork.WorkCommand.WaypointPosition);

                if (distToWork > furthestLW)
                {
                    furthestLW = distToWork;
                    lwIndex = lwi;
                }
            }

            return lwIndex;
        }

        /// <summary>
        /// Returns an Assigned Work by given sorting parameters
        /// </summary>
        /// <param name="sortByPriority"></param>
        /// <param name="sortByClosest"></param>
        /// <param name="sortByLowestWorkerCount"></param>
        /// <param name="lightWorker"></param>
        /// <returns></returns>
        AssignedWork GetAssignedWork(bool sortByPriority, bool sortByLowestWorkerCount, bool sortByClosest, LightWorker lightWorker)
        {
            // Temp Variables
            List<SortedWork> workSort = new List<SortedWork>();

            // Loop through the Assigned work
            foreach (AssignedWork aw in assignedWork)
            {
                // Add to the workSort List
                workSort.Add(
                    new SortedWork
                    (
                        assignedWork.IndexOf(aw),
                        aw.WorkCommand.Priority,
                        aw.AssignedWorkers.Count,
                        Vector3.Distance(aw.WorkCommand.WaypointPosition, lightWorker.transform.position)
                    )
                );
            }

            // Sort By Priority
            if (sortByPriority && !sortByLowestWorkerCount && !sortByClosest)
            {
                workSort = workSort.OrderByDescending(o => o.Priority).ToList();
            }

            // Sort By Prioriry and Lowest Worker Count
            if (sortByPriority && sortByLowestWorkerCount && !sortByClosest)
            {
                workSort = workSort.OrderByDescending(o => o.Priority).ThenBy(p => p.LowestWorkers).ToList();
            }

            // Sort By Prioriry and Lowest Worker Count and Closest
            if (sortByPriority && sortByLowestWorkerCount && sortByClosest)
            {
                workSort = workSort.OrderByDescending(o => o.Priority).ThenBy(p => p.LowestWorkers).ThenBy(d => d.WorkDistance).ToList();
            }

            // Sort By Lowest Worker Count
            if (!sortByPriority && sortByLowestWorkerCount && !sortByClosest)
            {
                workSort = workSort.OrderBy(o => o.LowestWorkers).ToList();
            }

            // Sort By Lowest Worker Count and Closest
            if (!sortByPriority && sortByLowestWorkerCount && sortByClosest)
            {
                workSort = workSort.OrderBy(o => o.LowestWorkers).ThenBy(d => d.WorkDistance).ToList();
            }

            // Sort By Closest
            if (!sortByPriority && !sortByLowestWorkerCount && sortByClosest)
            {
                workSort = workSort.OrderByDescending(o => o.WorkDistance).ToList();
            }

            // Sort By Prioriry and Lowest Worker Count
            if (sortByPriority && !sortByLowestWorkerCount && sortByClosest)
            {
                workSort = workSort.OrderByDescending(o => o.Priority).ThenBy(p => p.WorkDistance).ToList();
            }

            // Return the found Value
            if (workSort.Count > 0)
                return assignedWork[workSort[0].WorkIndex];
            else
                return null;
        }

        /// <summary>
        /// Returns the Closest WorkerCommand from a given List relative to a given Position
        /// </summary>
        /// <param name="workList"></param>
        /// <param name="position"></param>
        /// <returns></returns>
        WorkCommand GetClosestHighPriority(List<WorkCommand> workList, Vector3 position)
        {
            int _highestPriority = -1;
            float _closestDist = -1f;
            WorkCommand _closestWork = null;

            for (int i = 0; i < workList.Count; i++)
            {
                WorkCommand currentWork = workList[i];

                if (_highestPriority == -1)
                {
                    _highestPriority = currentWork.Priority;
                    _closestDist = Vector3.Distance(position, currentWork.WaypointPosition);
                    _closestWork = currentWork;
                }

                if (currentWork.Priority >= _highestPriority)
                {
                    float _distCheck = Vector3.Distance(position, currentWork.WaypointPosition);
                    if (_distCheck < _closestDist)
                    {
                        _closestDist = _distCheck;
                        _closestWork = currentWork;
                    }
                }


            }

            //Debug.LogFormat("{0} > GetClosestHighPriority() > Returning '{1}')", this.name, _closestWork.worldObject.name);

            return _closestWork;
        }

        /// <summary>
        /// Creates a Work Command of a Given Type at a Given Position and Registers it
        /// </summary>
        /// <param name="workType"></param>
        /// <param name="inputPosition"></param>
        public WorkCommand CreateWorkCommand(AgentCommandType workType, Vector3 position)
        {
            // Create Work Order
            WorkCommand newCommand = new WorkCommand();
            newCommand.Name = workType.ToString();
            newCommand.Priority = 0;
            newCommand.WaypointPosition = position;
            newCommand.WorkType = workType;

            //RegisterWork(newCommand);

            return newCommand;
        }

        /// <summary>
        /// Returns a Material specific to the AgentCommandType requested
        /// </summary>
        /// <param name="commandType"></param>
        /// <returns></returns>
        public Material GetMarkerMaterial(AgentCommandType commandType)
        {
            foreach (WaypointMarker wpm in waypointMarkers)
            {
                if (wpm.Command == commandType)
                {
                    return wpm.MarkerMaterial;
                }
            }

            // Shouldn't ever get here
            Debug.LogError(this.name + " > GetMarkerMaterial() > Couldn't find Material associated with " + commandType.ToString());
            return null;
        }

        /// <summary>
        /// Returns the Closest Light Work to a given Work Position
        /// </summary>
        /// <param name="listOfWorkers"></param>
        /// <param name="workPosition"></param>
        /// <returns></returns>
        LightWorker GetClosestLightWorker(List<LightWorker> listOfWorkers, Vector3 workPosition)
        {
            float closestDistance = 999f;
            int workerIndex = 0;

            foreach (LightWorker lw in listOfWorkers)
            {
                float currentDistance = Vector3.Distance(lw.transform.position, workPosition);
                if (currentDistance < closestDistance)
                {
                    closestDistance = currentDistance;
                    workerIndex = listOfWorkers.IndexOf(lw);
                }
            }

            return listOfWorkers[workerIndex];
        }

        /// <summary>
        /// Finds and returns the current Worker ID
        /// </summary>
        /// <returns></returns>
        public int GetLightWorkerNumber()
        {
            int returnInt = 1;

            if (totalWorkers.Count > 0)
                returnInt = totalWorkers[totalWorkers.Count - 1].workerID + 1;

            return returnInt;
        }

        /// <summary>
        /// Is there already the same Work Type in a given Position and given minimum distance
        /// </summary>
        /// <param name="type"></param>
        /// <param name="pos"></param>
        /// <param name="distance"></param>
        /// <returns></returns>
        public bool SameTaskWithin(AgentCommandType type, Vector3 pos, float distance)
        {
            // Temp Variables
            bool returnBool = false;

            // Determine Variables
            foreach (WorkCommand wc in availableWork)
            {
                if (wc.WorkType == type && Vector3.Distance(wc.WaypointPosition, pos) < distance)
                    returnBool = true;
            }

            foreach (AssignedWork aw in assignedWork)
            {
                if (aw.WorkCommand.WorkType == type && Vector3.Distance(aw.WorkCommand.WaypointPosition, pos) < distance)
                    returnBool = true;
            }

            // Return Variables
            return returnBool;
        }

        // END OF WAYPOINTHANDLER
    }

    [Serializable]
    public class WaypointMarker
    {
        [SerializeField] public AgentCommandType Command;
        [SerializeField] public Material MarkerMaterial;
    }

    [Serializable]
    public class WorkCommand
    {
        [SerializeField] public string Name;
        [SerializeField] public GameObject WorkObject;
        [SerializeField] public GameObject WaypointObject;
        [SerializeField] public Vector3 WaypointPosition;
        [SerializeField] public AgentCommandType WorkType;
        [Range(0, 3)]
        [SerializeField] public int Priority;

        /// <summary>
        /// Basic Constructor for Null Check
        /// </summary>
        public WorkCommand()
        {
            Name = string.Empty;
            WorkObject = null;
            WaypointObject = null;
            WaypointPosition = Vector3.zero;
            WorkType = AgentCommandType.Guard;
            Priority = 0;
        }
    }

    [Serializable]
    public class AssignedWork
    {
        [SerializeField] public WorkCommand WorkCommand;
        [SerializeField] public List<LightWorker> AssignedWorkers = new List<LightWorker>();

        public AssignedWork(WorkCommand wc)
        {
            WorkCommand = wc;
        }

        public AssignedWork(WorkCommand wc, LightWorker lw)
        {
            WorkCommand = wc;
            AssignedWorkers.Add(lw);
        }
    }

    [Serializable]
    public class SortedWork
    {
        [SerializeField] public int WorkIndex = 0;
        [SerializeField] public int Priority = 0;
        [SerializeField] public int LowestWorkers = 999;
        [SerializeField] public float WorkDistance = 999;

        public SortedWork(int wi, int p, int lw, float dist)
        {
            WorkIndex = wi;
            Priority = p;
            LowestWorkers = lw;
            WorkDistance = dist;
        }
    }
}