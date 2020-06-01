/* SquishSplatStudio - https://discord.gg/HVjBM9T
 * You are welcome to use this file or contents for learning and/or non-commercial projects
 * Written by Dubh @Dubh#1508 <Discord> - 2020
 */

using System.Collections.Generic;
using UnityEngine;

namespace SquishSplatStudio
{
    public class SpawnController : MonoBehaviour
    {

        #region ---[ singleton code base ]---

        static SpawnController() { }

        private SpawnController() { }

        public static SpawnController Instance { get; private set; }

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

        List<ResourceRequirement> _resourceRequirements;
        public PlacementType CurrentBuildType { get; set; }
        public List<ResourceRequirement> CurrentResourceRequirements { get; set; }

        public bool CompareResources(List<ResourceRequirement> resourceRequirements, int capacityCheck)
        {
            for (var i = 0; i < resourceRequirements.Count; i++)
            {
                if (ResourceType.Capacity == resourceRequirements[i].resource)
                {
                    if (resourceRequirements[i].resource.GetValue() < resourceRequirements[i].amount + capacityCheck) return false;
                }
                else
                {
                    if (resourceRequirements[i].resource.GetValue() < resourceRequirements[i].amount) return false;
                }
            }
            return true;
        }

        private int TallyCapacity()
        {
            int capacityTally = 0;
            var bo = GameObject.FindObjectsOfType<BuildRequirements>();
            for (int i = 0; i < bo.Length; i++)
            {
                var cap = bo[i].RequiredResources;
                if (cap.Count == 0) continue;
                if (bo[i].gameObject.activeInHierarchy && cap[0].resource == ResourceType.Capacity && !bo[i].objectBuilt)
                {
                    capacityTally += bo[i].RequiredResources[0].amount;
                }
            }
            return capacityTally;
        }

        internal bool HasResources(PlacementType placementType)
        {
            _resourceRequirements = placementType.Prefab()?.GetComponent<BuildRequirements>()?.RequiredResources;
            var result = CompareResources(_resourceRequirements, TallyCapacity());
            return result; // CompareResources(_resourceRequirements, TallyCapacity());
        }

        bool repeat;
        public void PlaceCurrentBuild()
        {
            repeat = true;
            switch (CurrentBuildType)
            {
                case ObjectType.LightTower:
                    BuildLightTower();
                    break;
                case ObjectType.Purifier:
                    BuildPurifier();
                    break;
                case ObjectType.LightWell:
                    BuildLightWell();
                    break;
            }
        }

        public void RefundCurrentBuild()
        {
            if (CurrentResourceRequirements == null) return;
            ResourceHandler.Instance.RefundStats(CurrentResourceRequirements, ResourceType.Capacity);
        }

        public void BuildLightTower()
        {
            var lightTower = ObjectType.LightTower;
            if (!lightTower.HasResources()) return;
            SetCurrentBuild(lightTower, _resourceRequirements);
            InputHandler.Instance.SetControlMode(ControlMode.ObjectPlacement);
            PlacementHandler.Instance.SpawnPlacingObject(lightTower.PrimaryPrefab(), lightTower);
            ResourceHandler.Instance.AdjustStats(_resourceRequirements, ResourceType.Capacity);
            if (repeat) return;
            repeat = false;
            AudioController.Instance.PlayButtonClick();
            //AudioController.Instance.PlayStructureBuilding();
        }

        void SetCurrentBuild(PlacementType _placementType, List<ResourceRequirement> _requirements)
        {
            CurrentBuildType = _placementType;
            CurrentResourceRequirements = _requirements;
        }

        public void BuildPurifier()
        {
            var purifier = ObjectType.Purifier;
            if (!purifier.HasResources()) return;
            SetCurrentBuild(purifier, _resourceRequirements);
            InputHandler.Instance.SetControlMode(ControlMode.ObjectPlacement);
            PlacementHandler.Instance.SpawnPlacingObject(purifier.PrimaryPrefab(), purifier);
            ResourceHandler.Instance.AdjustStats(_resourceRequirements, ResourceType.Capacity);
            if (repeat) return;
            repeat = false;
            AudioController.Instance.PlayButtonClick();
            //AudioController.Instance.PlayStructureBuilding();
        }

        public void BuildLightWell()
        {
            var lightwell = ObjectType.LightWell;
            if (!lightwell.HasResources()) return;
            SetCurrentBuild(lightwell, _resourceRequirements);
            InputHandler.Instance.SetControlMode(ControlMode.ObjectPlacement);
            PlacementHandler.Instance.SpawnPlacingObject(lightwell.PrimaryPrefab(), lightwell);
            ResourceHandler.Instance.AdjustStats(_resourceRequirements, ResourceType.Capacity);
            if (repeat) return;
            repeat = false;
            AudioController.Instance.PlayButtonClick();
            //AudioController.Instance.PlayStructureBuilding();
        }

        public void PurifySoul()
        {
            if (!PlacementType.Agent.HasResources()) return;
            //if (!SpawnHandler.Instance.BuildLightWorker()) return;
            if (!SpawnHandler.Instance.SpawnLightWorker()) return;
            ResourceHandler.Instance.AdjustStats(_resourceRequirements);
            AudioController.Instance.PlayButtonClick();
            AudioController.Instance.PlayBuildingWorker();
        }

        public void PlaceScoutMarker()
        {
            InputHandler.Instance.SetControlMode(ControlMode.ObjectPlacement);
            PlacementHandler.Instance.SpawnPlacingObject(PlacementType.Waypoint.PrimaryPrefab(), ObjectType.Waypoint, AgentCommandType.Explore);
            AudioController.Instance.PlayButtonClick();
            //AudioController.Instance.PlayExploring();
            MarkerPlacing = true;
        }

        public void PlaceAttackMarker()
        {
            InputHandler.Instance.SetControlMode(ControlMode.ObjectPlacement);
            PlacementHandler.Instance.SpawnPlacingObject(PlacementType.Waypoint.PrimaryPrefab(), ObjectType.Waypoint, AgentCommandType.Attack);
            AudioController.Instance.PlayButtonClick();
            //AudioController.Instance.PlayWorkerAttacking();
            MarkerPlacing = true;
        }

        public void PlaceGuardMarker()
        {
            InputHandler.Instance.SetControlMode(ControlMode.ObjectPlacement);
            PlacementHandler.Instance.SpawnPlacingObject(PlacementType.Waypoint.PrimaryPrefab(), ObjectType.Waypoint, AgentCommandType.Guard);
            AudioController.Instance.PlayButtonClick();
            //AudioController.Instance.PlayGuarding();
            MarkerPlacing = true;
        }

        public void PlaceMineMarker()
        {
            InputHandler.Instance.SetControlMode(ControlMode.ObjectPlacement);
            PlacementHandler.Instance.SpawnPlacingObject(PlacementType.Waypoint.PrimaryPrefab(), ObjectType.Waypoint, AgentCommandType.Mine);
            AudioController.Instance.PlayButtonClick();
            //AudioController.Instance.PlayMining();
            MarkerPlacing = true;
        }

        public bool MarkerPlacing;
        public void PlaceBuildMarker()
        {
            InputHandler.Instance.SetControlMode(ControlMode.ObjectPlacement);
            PlacementHandler.Instance.SpawnPlacingObject(PlacementType.Waypoint.PrimaryPrefab(), ObjectType.Waypoint, AgentCommandType.Build);
            AudioController.Instance.PlayButtonClick();
            //AudioController.Instance.PlayStructureBuilding();
            MarkerPlacing = true;
        }
    }
}