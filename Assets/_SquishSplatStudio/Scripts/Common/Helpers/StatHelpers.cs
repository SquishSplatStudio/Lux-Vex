/* SquishSplatStudio - https://discord.gg/HVjBM9T
 * You are welcome to use this file or contents for learning and/or non-commercial projects
 * Written by Dubh @Dubh#1508 <Discord> - 2020
 */

using UnityEngine;
using UnityEngine.AI;

namespace SquishSplatStudio
{
    public static class StatHelpers
    {
        public static void IncreaseBy(this ResourceType resourceType, int amount)
        {
            switch (resourceType)
            {
                case ResourceType.Soul:
                    ResourceHandler.Instance.AdjustSouls(amount);
                    break;
                case ResourceType.Crystal:
                    ResourceHandler.Instance.AdjustCrystal(amount);
                    break;
                case ResourceType.Light:
                    ResourceHandler.Instance.AdjustLight(amount);
                    break;
                case ResourceType.Worker:
                    ResourceHandler.Instance.AdjustWorkers(amount);
                    break;
                case ResourceType.Capacity:
                    ResourceHandler.Instance.AdjustCapacity(amount);
                    break;
                case ResourceType.Any:
                default:
                    break;
            }
        }

        public static void DecreaseBy(this ResourceType resourceType, int amount)
        {
            switch (resourceType)
            {
                case ResourceType.Soul:
                    ResourceHandler.Instance.AdjustSouls(amount * -1);
                    break;
                case ResourceType.Crystal:
                    ResourceHandler.Instance.AdjustCrystal(amount * -1);
                    break;
                case ResourceType.Light:
                    ResourceHandler.Instance.AdjustLight(amount * -1);
                    break;
                case ResourceType.Worker:
                    ResourceHandler.Instance.AdjustWorkers(amount * -1);
                    break;
                case ResourceType.Capacity:
                    ResourceHandler.Instance.AdjustCapacity(amount * -1);
                    break;
                case ResourceType.Any:
                default:
                    break;
            }
        }

        public static void IncreaseMaxBy(this ResourceType resourceType, int amount)
        {
            switch (resourceType)
            {
                case ResourceType.Soul:
                    ResourceHandler.Instance.AdjustMaximumSouls(amount);
                    break;
                case ResourceType.Crystal:
                    ResourceHandler.Instance.AdjustMaximumCrystal(amount);
                    break;
                case ResourceType.Light:
                    ResourceHandler.Instance.AdjustMaximumLight(amount);
                    break;
                case ResourceType.Worker:
                    ResourceHandler.Instance.AdjustMaximumWorkers(amount);
                    break;
                case ResourceType.Capacity:
                    ResourceHandler.Instance.AdjustMaximumCapacity(amount);
                    break;
                case ResourceType.Any:
                default:
                    break;
            }
        }

        public static void DecreaseMaxBy(this ResourceType resourceType, int amount)
        {
            switch (resourceType)
            {
                case ResourceType.Soul:
                    ResourceHandler.Instance.AdjustMaximumSouls(amount * -1);
                    break;
                case ResourceType.Crystal:
                    ResourceHandler.Instance.AdjustMaximumCrystal(amount * -1);
                    break;
                case ResourceType.Light:
                    ResourceHandler.Instance.AdjustMaximumLight(amount * -1);
                    break;
                case ResourceType.Worker:
                    ResourceHandler.Instance.AdjustMaximumWorkers(amount * -1);
                    break;
                case ResourceType.Capacity:
                    ResourceHandler.Instance.AdjustMaximumCapacity(amount * -1);
                    break;
                case ResourceType.Any:
                default:
                    break;
            }
        }

        public static bool Changed(this ResourceType resourceType) => ResourceHandler.Instance.AnyStatChanged;

        public static int GetValue(this ResourceType resourceType)
        {
            switch (resourceType)
            {
                case ResourceType.Soul:
                    return ResourceHandler.Instance.Souls;
                case ResourceType.Crystal:
                    return ResourceHandler.Instance.Crystal;
                case ResourceType.Light:
                    return ResourceHandler.Instance.Light;
                case ResourceType.Worker:
                    return ResourceHandler.Instance.Workers;
                case ResourceType.Capacity:
                    return ResourceHandler.Instance.Capacity;
                case ResourceType.Any:
                default:
                    return 0;
            }
        }

        public static int GetMaxValue(this ResourceType resourceType)
        {
            switch (resourceType)
            {
                case ResourceType.Soul:
                    return ResourceHandler.Instance.MaximumSouls;
                case ResourceType.Crystal:
                    return ResourceHandler.Instance.MaximumCrystal;
                case ResourceType.Light:
                    return ResourceHandler.Instance.MaximumLight;
                case ResourceType.Worker:
                    return ResourceHandler.Instance.MaximumWorkers;
                case ResourceType.Capacity:
                    return ResourceHandler.Instance.MaximumCapacity;
                case ResourceType.Any:
                default:
                    return 0;
            }
        }

        public static bool HasResources(this PlacementType placementType)
        {
            return SpawnController.Instance.HasResources(placementType);
        }

        public static GameObject Prefab(this PlacementType placementType)
        {
            return ObjectPool.Instance.GetItemFromPool(placementType);
        }

        public static GameObject InstantiatedPrefab(this PlacementType placementType)
        {
            return ObjectPool.Instance.GetInstantiatedPrefab(placementType);
        }

        public static GameObject PrimaryPrefab(this PlacementType placementType)
        {
            return ObjectPool.Instance.GetItemPrefab(placementType);
        }

        public static GameObject ClaimPrefab(this PlacementType placementType)
        {
            var prefab = ObjectPool.Instance.GetItemFromPool(placementType);
            prefab.SetActive(true);
            return prefab;
        }

        public static int ResourceCost(this PlacementType placementType, ResourceType resourceType)
        {
            var requirements = placementType.Prefab()?.GetComponent<BuildRequirements>()?.RequiredResources;
            for (var i = 0; i < requirements.Count; i++)
                if (resourceType == requirements[i].resource)
                    return requirements[i].amount;
            return -1;
        }

        public static bool AnyActiveOnScreen(this PlacementType placementType)
        {
            var playerObjects = GameObject.FindObjectsOfType<PlayerStructure>();
            for(var i = 0; i < playerObjects.Length; i++)
            {
                var br = playerObjects[i].GetComponent<BuildRequirements>();
                if (br.ObjectType == ObjectType.Purifier && playerObjects[i].gameObject.activeInHierarchy) 
                    return true;
            }
            return false;
        }

        public static void Go(this NavMeshAgent agent, Vector3 target)
        {
            if (!agent.enabled) return;
            agent.isStopped = false;
            agent.SetDestination(target);
        }

        public static void Freeze(this NavMeshAgent agent)
        {
            if (!agent.enabled) return;
            agent.isStopped = true;
        }

        public static void PlayAudio(this AgentCommandType wc)
        {
            switch (wc)
            {
                case AgentCommandType.Explore:
                    AudioController.Instance.PlayExploring();
                    break;
                case AgentCommandType.Guard:
                    AudioController.Instance.PlayGuarding();
                    break;
                case AgentCommandType.Build:
                    AudioController.Instance.PlayStructureBuilding();
                    break;
                case AgentCommandType.Mine:
                    AudioController.Instance.PlayMining();
                    break;
            }
        }

    }

    public class ObjectType
    {
        public const PlacementType Purifier = PlacementType.Purifier | PlacementType.Structure;
        public const PlacementType LightTower = PlacementType.LightTower | PlacementType.Structure;
        public const PlacementType LightWell = PlacementType.LightWell | PlacementType.Structure;
        public const PlacementType Waypoint = PlacementType.Waypoint | PlacementType.Structure;
        //public const PlacementType Structure = PlacementType.LightTower | PlacementType.LightWell | PlacementType.Purifier;
    }
}