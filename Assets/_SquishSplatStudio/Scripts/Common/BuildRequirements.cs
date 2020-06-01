using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using ObjectType2 = SquishSplatStudio.ObjectType;

namespace SquishSplatStudio
{
    [Serializable]
    public class BuildRequirements : MonoBehaviour
    {
        [SerializeField] public bool snapPlacement;
        [SerializeField] public GameObject snapObject;
        [SerializeField] public PlacementType ObjectType;
        public List<ResourceRequirement> RequiredResources;
        [SerializeField] public List<Material> originalMaterials;
        [SerializeField] float buildTime;
        [SerializeField] bool _objectBuilt;
        public bool objectBuilt
        {
            get { return _objectBuilt; }
            set { _objectBuilt = value; }
        }
        public bool canContribute
        {
            get { return !_isWorking; }
        }
        public bool animHandleConnection;

        public List<MeshRenderer> listOfMeshRenderers = new List<MeshRenderer>();
        public float _newCompletePercent = 0f;
        public int _totalResourceUnitNumber = 0;
        public int _resourceUnitsAdded = 0;
        [SerializeField] public WorkCommand assignedWorkCommand;
        bool _isWorking = false;


        // Primary Update Loop
        private void Update()
        {
            if (!_objectBuilt && listOfMeshRenderers.Count > 0)
            {
                foreach (MeshRenderer mr in listOfMeshRenderers)
                {
                    if (mr.material.HasProperty("_progress"))
                    {
                        float _setComplete = Mathf.Lerp(mr.material.GetFloat("_progress"), _newCompletePercent, Time.deltaTime * 2f);
                        mr.material.SetFloat("_progress", _setComplete);
                    }
                }

                CheckBuildProgress();
            }
        }

        /// <summary>
        /// Marks the Object to Be Built
        /// </summary>
        public void SetToBuild()
        {
            // Set Primary Built Bool
            _objectBuilt = false;

            // Set All MeshRenderers to use Dissolve Shader
            foreach (MeshRenderer mr in GetComponentsInChildren<MeshRenderer>())
            {
                if (mr.GetComponent<AdjustObjectVisuals>())
                    listOfMeshRenderers.Add(mr);
            }

            // Get Total Number of Resource Units Required - This will determine completion percent
            foreach (ResourceRequirement rr in RequiredResources)
            {
                if (rr.resource != ResourceType.Capacity)
                    _totalResourceUnitNumber += rr.amount;
            }

            // Enable Collider for Selection
            if (GetComponent<Collider>())
                GetComponent<Collider>().enabled = true;

            // Disable Initial Placement Collider Object
            transform.GetChild(0).gameObject.SetActive(false);

            // Disable Light Beam
            //if (GetComponent<LightLink>())
            //    GetComponent<LightLink>().ToggleLightBeam(false, false);

            // Create Build Work Command
            //assignedWorkCommand = WaypointHandler.Instance.CreateWorkCommand(AgentCommandType.Build, transform.position);
        }

        /// <summary>
        /// If the building hasn't finished Cancel the Build and Refund?
        /// </summary>
        public void CancelBuild()
        {
            // Refund

            // Play Deteriorate Anim ?? Reverse the Build Percent to Zero then Destroy

            // Destroy Associated World Object
            DestroyImmediate(gameObject);
        }

        /// <summary>
        /// Used for the AI to contribute to build
        /// </summary>
        public void AddToBuild()
        {
            _isWorking = true;

            // Prevent additional work where not needed
            if (_newCompletePercent < 1)
            {
                // We offset by 1 as we don't want to deduct from or increase Capacity -- This will remain static
                int applyTo = UnityEngine.Random.Range(1, RequiredResources.Count);
                RequiredResources[applyTo].amount--;
                _resourceUnitsAdded++;
                _newCompletePercent = (float)_resourceUnitsAdded / (float)_totalResourceUnitNumber;

                if (RequiredResources[applyTo].amount <= 0)
                {
                    RequiredResources.RemoveAt(applyTo);
                    AudioController.Instance.PlayStructureCompleted();
                }
            }

            _isWorking = false;
        }

        /// <summary>
        /// Checks for Build Completion
        /// </summary>
        void CheckBuildProgress()
        {
            if (_newCompletePercent == 1 && !_objectBuilt)
            {
                // Set Built Flags
                _objectBuilt = true;
                _isWorking = true;

                // Add Structure to Control Crystal
                if (GetComponent<PlayerStructure>())
                {
                    ControlCrystal.Instance.RegisterStructure(gameObject);
                    GetComponent<NavMeshObstacle>().enabled = true;
                    GetComponent<LightLink>().enabled = true;
                    GetComponent<ObjectHealth>().enabled = true;
                    GetComponent<ObjectResource>().enabled = true;
                }

                // Update the Light Link Array
                PlacementHandler.Instance.RebuildLightLinkArray();

                // Update Work Command
                WaypointHandler.Instance.RemoveMarker(assignedWorkCommand);

                // ReAssign Original Materials
                for (int mr = 0; mr < listOfMeshRenderers.Count; mr++)
                {
                    listOfMeshRenderers[mr].material = originalMaterials[mr];
                    if (GetComponent<AdjustObjectVisuals>())
                        listOfMeshRenderers[mr].GetComponent<AdjustObjectVisuals>().Initialize();
                }

                // Start the Animation
                if (GetComponent<Animator>() && animHandleConnection)
                {
                    GetComponent<Animator>().SetTrigger("doStart");
                }
                else
                {
                    if (GetComponent<LightLink>())
                        GetComponent<LightLink>().ConnectToOthers();
                }

                // If the first Resource Requirement is Capacity, we want to add to our Maximum
                if (RequiredResources[0].resource == ResourceType.Capacity)
                    ConsumeCapacity(RequiredResources[0].amount);

                // Register Purifier
                if (ObjectType == PlacementType.Purifier)
                    SpawnHandler.Instance.Initialize(GetComponent<PlayerStructure>().spawnPoint.gameObject);

                // Play Completed Audio
                PlayCompleteAudio();
            }
        }

        void PlayCompleteAudio()
        {
            switch (ObjectType)
            {
                case ObjectType2.LightTower:
                    AudioController.Instance.PlayLightTowerComplete();
                    break;
                case ObjectType2.LightWell:
                    AudioController.Instance.PlayLightWellComplete();
                    break;
                case ObjectType2.Purifier:
                    AudioController.Instance.PlayPurifierComplete();
                    break;
            }
        }

        /// <summary>
        /// Consumes a given amount of Capacity
        /// </summary>
        /// <param name="amount"></param>
        void ConsumeCapacity(int amount)
        {
            if (ObjectType.HasFlag(PlacementType.LightWell))
                ResourceType.Capacity.IncreaseMaxBy(-amount);

            if (ObjectType.HasFlag(PlacementType.Structure))
                ResourceType.Capacity.DecreaseBy(amount);
        }

        /// <summary>
        /// Returns the Consumed Capacity
        /// </summary>
        public void ReturnCapacity()
        {
            if (RequiredResources[0].resource == ResourceType.Capacity)
            {
                if (ObjectType.HasFlag(PlacementType.LightWell))
                {
                    ResourceType.Capacity.DecreaseMaxBy(-RequiredResources[0].amount);
                    ResourceType.Capacity.DecreaseBy(-RequiredResources[0].amount);
                }
                else
                {
                    ResourceType.Capacity.IncreaseBy(RequiredResources[0].amount);
                }
            }
        }


            // END OF BUILDREQUIREMENTS
        }

        [Serializable]
    public class ResourceRequirement
    {
        [SerializeField] public ResourceType resource;
        [SerializeField] public int amount;
    }
}
