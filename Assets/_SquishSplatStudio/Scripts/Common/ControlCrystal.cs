using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SquishSplatStudio
{
    public class ControlCrystal : MonoBehaviour
    {
        [SerializeField] public List<StructureDistance> playerStructures = new List<StructureDistance>();
        [SerializeField] List<PlayerStructure> activePurifiers = new List<PlayerStructure>();

        float _nextTick = 0f;
        int _lastValue = 0;

        #region ---[ singleton code base ]---

        static ControlCrystal() { }

        private ControlCrystal() { }

        public static ControlCrystal Instance { get; private set; }

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        #endregion

        // Update is called once per frame
        void Update()
        {
            if (_nextTick > 0)
                _nextTick -= Time.deltaTime;

            if (_nextTick <= 0)
                CheckLightCapacity();
        }

        /// <summary>
        /// Checks the Current Capacity
        /// </summary>
        void CheckLightCapacity()
        {
            _nextTick = GameHandler.Instance.TickRate;
            int _currentValue = ResourceType.Capacity.GetValue();

            if (_lastValue != _currentValue)
            {
                bool _lastValueNegative = _lastValue < 0;
                bool _currentValueNegative = _currentValue < 0;

                if (_lastValueNegative && !_currentValueNegative || !_lastValueNegative && _currentValueNegative)
                {
                    if (ResourceType.Capacity.GetValue() < 0)
                        PowerCascade(true);

                    if (ResourceType.Capacity.GetValue() > 0)
                        PowerCascade(false);
                }

                // Update last value checked
                _lastValue = _currentValue;
            }
        }

        /// <summary>
        /// Cascades the Power out to the furthest reaches of the players structures
        /// </summary>
        void PowerCascade(bool disable)
        {
            // Determine how many structures we need to effect
            int currentCapacity = -ResourceType.Capacity.GetValue();
            int capacityCost = ObjectType.LightTower.ResourceCost(ResourceType.Capacity);
            int numberOfStructures = currentCapacity / capacityCost;

            if (numberOfStructures > 0)
            {
                for (int i = 0; i < numberOfStructures; i++)
                {
                    playerStructures[i].PlayerStructure.GetComponent<LightLink>().ControlOverride(disable);
                }
            }
        }

        /// <summary>
        /// Registers a Player Structure for Tracking
        /// </summary>
        /// <param name="ps"></param>
        public void RegisterStructure(GameObject ps)
        {
            // Add to the List of Structures
            playerStructures.Add(new StructureDistance(ps, Vector3.Distance(ps.transform.position, transform.position)));

            // Re-sort the List by Distance
            playerStructures = playerStructures.OrderByDescending(o => o.DistanceToStructure).ThenBy(o => o.PlayerStructure.GetComponent<LightLink>().jumpsToBase).ToList();

            // If the Structure is a Purifier, add it to the list of activePurifiers
            if (ps.GetComponent<BuildRequirements>().ObjectType.HasFlag(PlacementType.Purifier))
                activePurifiers.Add(ps.GetComponent<PlayerStructure>());
        }

        /// <summary>
        /// Removes a Player Structure from Tracking
        /// </summary>
        /// <param name="ps"></param>
        public void RemoveStructure(GameObject ps)
        {
            List<StructureDistance> missingReferences = new List<StructureDistance>();
            int _foundIndex = -1;

            foreach (StructureDistance sd in playerStructures)
            {
                if (sd.PlayerStructure == ps)
                    _foundIndex = playerStructures.IndexOf(sd);

                if (sd.PlayerStructure == null)
                    missingReferences.Add(sd);
            }

            if (_foundIndex > -1)
                playerStructures.RemoveAt(_foundIndex);

            if (missingReferences.Count > 0)
            {
                foreach (StructureDistance msd in missingReferences)
                {
                    playerStructures.Remove(msd);
                }
            }

            // If the Structure is a Purifier, remove it from the list of activePurifiers
            if (ps.GetComponent<BuildRequirements>().ObjectType.HasFlag(PlacementType.Purifier))
                activePurifiers.Remove(ps.GetComponent<PlayerStructure>());

            PlacementHandler.Instance.RebuildLightLinkArray();
        }

        /// <summary>
        /// Sends a free purifier the command to spawn light worker
        /// </summary>
        public void PurifySoul()
        {
            bool addedToQueue = false;
            bool structureSpawning = false;

            // Do Two Loops;
            // First for Spawning
            foreach (PlayerStructure ps in activePurifiers)
            {
                if (ps.CanSpawn)
                {
                    ps.SpawnLightWorker();
                    structureSpawning = true;
                }

                if (structureSpawning)
                    break;
            }

            // Second for Queueing
            if (!structureSpawning)
            {
                foreach (PlayerStructure ps in activePurifiers)
                {
                    if (ps.GetSpawnQueue < ps.SpawnQueue)
                    {
                        ps.AddToSpawnQueue();
                        addedToQueue = true;
                    }

                    if (addedToQueue)
                        break;
                }
            }
        }

        /// <summary>
        /// Returns a True/False of whether the given object position is clear of all other structures by the given distance.
        /// </summary>
        /// <param name="position"></param>
        /// <param name="distance"></param>
        /// <returns></returns>
        public bool ClearOfOtherStructures(Vector3 position, float distance)
        {
            bool clearOfStructures = true;

            foreach (StructureDistance sd in playerStructures)
            {
                if (sd.PlayerStructure == null) continue;
                if (Vector3.Distance(sd.PlayerStructure.transform.position, position) <= distance)
                    clearOfStructures = false;
            }

            return clearOfStructures;
        }

        /// <summary>
        /// Returns whether or not there is an active and free purifier
        /// </summary>
        /// <returns></returns>
        public bool ActiveAndFreePurifier()
        {
            // Quick out
            if (activePurifiers.Count == 0)
                return false;

            // Temp Variables
            bool returnBool = false;

            // Determine Bool
            foreach (PlayerStructure ps in activePurifiers)
                if (ps.CanSpawn || ps.GetSpawnQueue < ps.SpawnQueue) returnBool = true;

            // Finally Return the Bool
            return returnBool;
        }
    }

    [Serializable]
    public struct StructureDistance
    {
        [SerializeField] public GameObject PlayerStructure;
        [SerializeField] public float DistanceToStructure;

        public StructureDistance(GameObject ps, float distance)
        {
            PlayerStructure = ps;
            DistanceToStructure = distance;
        }
    }
}