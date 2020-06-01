using System;
using System.Collections.Generic;
using UnityEngine;

namespace SquishSplatStudio
{
    public class PlacementHandler : MonoBehaviour
    {
        [SerializeField] public LightLink[] allLightLinks = new LightLink[0];
        [SerializeField] public LightLink[] lightLinkArray = new LightLink[0];
        [SerializeField] Material buildMaterial = null;
        [SerializeField] Material validPlacementMaterial = null;
        [SerializeField] Material invalidPlacementMaterial = null;
        [SerializeField] List<PlacementLink> placementLinks = new List<PlacementLink>();
        GameObject primaryPlacingObject = null;
        List<GameObject> objectsToBePlaced = new List<GameObject>();
        List<ValidationCheck> _checkObjectValidation = new List<ValidationCheck>();
        bool _checkValidations;

        [Header("Debug - DeleteME")]
        public GameObject Validating;
        public bool InsideAnother;
        public bool CollidingWithOther;
        public bool OutOfRange;
        public bool ClearLOS;
        public float DistanceToLL;

        GameObject _placingPrefab;
        PlacementType _placingType;
        bool _positionSet = false;
        AgentCommandType _agentCommandType;
        WorkCommand _toBeAssigned;
        Vector3 _placementPosition;

        // Singleton Reference
        private static PlacementHandler _instance;
        public static PlacementHandler Instance { get { return _instance; } }

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

        // First Start Operations
        private void Start()
        {
            RebuildLightLinkArray();
        }

        // Primary Update Loop
        private void LateUpdate()
        {
            if (objectsToBePlaced.Count > 0 && _checkValidations)
            {
                foreach (GameObject go in objectsToBePlaced)
                {
                    Vector3 placementOffset = Vector3.zero;
                    if (go.GetComponent<PlayerStructure>())
                        placementOffset = go.GetComponent<PlayerStructure>().placementOffset;

                    go.transform.position = InputHandler.Instance.currentWorldMousePosition + placementOffset;
                    ValidatePlacementLocations();
                }
            }
        }

        /// <summary>
        /// Sets the Final Position of the Tower
        /// </summary>
        public void SetPosition(GameObject go, bool snapPosition)
        {
            // Set the Position to Prevent late Material updates
            //_positionSet = true;

            // Set the Links if we've found one
            if (go.GetComponent<LightLink>())
            {
                LightLink _tmpLL = go.GetComponent<LightLink>();
                _tmpLL.finalPositionSet = true;
            }

            if (snapPosition)
            {
                go.transform.position = InputHandler.Instance.hoverObject.transform.position;

                if (_placingType.HasFlag(PlacementType.LightWell) && InputHandler.Instance.hoverObject.tag == "LeylineNode")
                {
                    go.GetComponent<BuildRequirements>().snapObject = InputHandler.Instance.hoverObject;
                    InputHandler.Instance.hoverObject.SetActive(false);
                }
            }
        }

        /// <summary>
        /// Handles the placement of an Object given a Prefab. This could be a structure or a waypoint
        /// </summary>
        /// <param name="prefab"></param>
        public void SpawnPlacingObject(GameObject prefab, PlacementType type, AgentCommandType commandType = AgentCommandType.None)
        {
            // Set Placement Variables
            _placingPrefab = prefab;
            _placingType = type;

            // button press for command type
            _agentCommandType = commandType;

            // Reset Set Position
            _positionSet = false;

            // Rebuild Light Link Array
            RebuildLightLinkArray();

            // Instantiate Object
            if (_placingType != ObjectType.Waypoint)
            {
                GameObject newObject = Instantiate(prefab);
                newObject.transform.position = InputHandler.Instance.currentWorldMousePosition;

                if (!newObject.name.Contains(" (Placing)"))
                    newObject.name = prefab.name + " (Placing)";

                _checkObjectValidation.Add(new ValidationCheck(newObject, true));
                objectsToBePlaced.Add(newObject);
                primaryPlacingObject = newObject;

                // Create Marker for this Task
                CreateMarker(AgentCommandType.Build, newObject);
            }
            else
            {
                primaryPlacingObject = CreateMarker(prefab, _agentCommandType);
            }

            // Store the Primary Items Materials for later reapplication
            foreach (MeshRenderer mr in primaryPlacingObject.GetComponentsInChildren<MeshRenderer>())
            {
                primaryPlacingObject.GetComponent<BuildRequirements>().originalMaterials.Add(mr.material);
            }

            // Mark ready for Validation Checks
            _checkValidations = true;
        }

        /// <summary>
        /// Confirms placement of object and checks whether or not another can be placed
        /// </summary>
        /// <param name="placeAnother"></param>
        public bool ConfirmPlacement(bool placeAnother)
        {
            // Return if Invalid Placement
            if (ValidatePlacementLocations())
            {
                // Loop through the Validations
                for (int i = 0; i < _checkObjectValidation.Count; i++)
                {
                    ValidationCheck vc = _checkObjectValidation[i];

                    // Do Waypoint Checks
                    if (vc.WorldObject.GetComponent<WorkCommandHelper>())
                    {
                        vc.WorldObject.GetComponent<WorkCommandHelper>().AssignedWork.WaypointPosition = vc.WorldObject.transform.position;

                        // Play Audio
                        vc.WorldObject.GetComponent<WorkCommandHelper>().AssignedWork.WorkType.PlayAudio();

                        // Register the Work Command
                        WaypointHandler.Instance.RegisterWork(vc.WorldObject.GetComponent<WorkCommandHelper>().AssignedWork);
                    }

                    // Do Structure Placement
                    if (vc.WorldObject.GetComponent<PlayerStructure>())
                    {
                        // Temp Variable
                        BuildRequirements br = vc.WorldObject.GetComponent<BuildRequirements>();

                        // Set the Object Name
                        vc.WorldObject.name = _placingPrefab.name + string.Format(" ({0}, {1}, {2})", vc.WorldObject.transform.position.x, vc.WorldObject.transform.position.y, vc.WorldObject.transform.position.z);

                        // Set the Parenting
                        vc.WorldObject.transform.SetParent(GetParent(br.ObjectType));

                        // Set the Mesh Renderer Materials to that of the Build Material
                        MeshRenderer[] mrArray = vc.WorldObject.GetComponentsInChildren<MeshRenderer>();
                        foreach (MeshRenderer mr in mrArray)
                        {
                            if (mr.GetComponent<AdjustObjectVisuals>())
                                mr.material = buildMaterial;
                        }

                        // Set the Final Position
                        SetPosition(vc.WorldObject, br.snapPlacement);

                        // Set the Object to Build
                        br.SetToBuild();
                    }
                }

                // Do we wish to place another?
                if (placeAnother && _placingType.HasResources())
                {
                    // Clear Validation List
                    objectsToBePlaced.Clear();
                    _checkObjectValidation.Clear();

                    // Place down a new object
                    SpawnPlacingObject(_placingPrefab, _placingType, _agentCommandType);
                }
                else
                {
                    CancelPlacement(false);
                }

                return true;
            }
            else
            {
                //CancelPlacement(true);
                return false;
            }
        }

        /// <summary>
        /// Cancels the placement of the currently set object
        /// </summary>
        public void CancelPlacement(bool deleteObj)
        {
            if (deleteObj)
            {
                foreach (GameObject go in objectsToBePlaced)
                {
                    // Refund the current Build progress
                    go.GetComponent<BuildRequirements>()?.CancelBuild();

                    // Delete the Object once we're done
                    if (deleteObj)
                        DestroyImmediate(go);
                }
            }

            InputHandler.Instance.currentMode = ControlMode.Default;

            primaryPlacingObject = null;
            _placingPrefab = null;
            _placingType = PlacementType.None;
            _agentCommandType = AgentCommandType.None;

            // Clear the Validation List and Object To Be Placed List
            _checkValidations = false;
            objectsToBePlaced.Clear();
            _checkObjectValidation.Clear();
        }

        /// <summary>
        /// Rotate the placing object by a given amount
        /// </summary>
        /// <param name="rawInput"></param>
        public void RotateObject(float rawInput)
        {
            primaryPlacingObject.transform.Rotate(Vector3.up, rawInput);
        }

        /// <summary>
        /// Returns the Parent transform for a given Placement Type
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public Transform GetParent(PlacementType type)
        {
            for (int i = 0; i < placementLinks.Count; i++)
            {
                if (placementLinks[i].Type.HasFlag(type))
                    return placementLinks[i].Parent;
            }

            return null;
        }

        /// <summary>
        /// Creates a Marker for the given Work Command and adds it to the Available Work List
        /// </summary>
        /// <param name="work"></param>
        public GameObject CreateMarker(GameObject prefab, AgentCommandType type)
        {
            GameObject newMarker = Instantiate(prefab, GetParent(PlacementType.Waypoint));
            newMarker.transform.GetChild(0).GetComponent<ParticleSystemRenderer>().material = WaypointHandler.Instance.GetMarkerMaterial(type);
            newMarker.name = prefab.name + " " + type.ToString();//string.Format("{0} Marker {1}", work.workCommand.ToString(), (GetParent(PlacementType.Waypoint).childCount).ToString("D3"));//  + " Marker " + ;
            newMarker.GetComponent<WorkCommandHelper>().AssignedWork.WorkType = type;

            WorkCommand wc = WaypointHandler.Instance.CreateWorkCommand(type, Vector3.zero);
            wc.WaypointObject = newMarker;
            newMarker.GetComponent<WorkCommandHelper>().AssignedWork = wc;

            _checkObjectValidation.Add(new ValidationCheck(newMarker, false));
            objectsToBePlaced.Add(newMarker);

            return newMarker;
        }
        public GameObject CreateMarker(WorkCommand work)
        {
            GameObject newMarker = Instantiate(WaypointHandler.Instance.generalPrefab, work.WaypointPosition, new Quaternion(), GetParent(PlacementType.Waypoint));
            newMarker.name = string.Format("{0} Marker {1}", work.WorkType.ToString(), (GetParent(PlacementType.Waypoint).childCount).ToString("D3"));//  + " Marker " + ;
            work.WaypointObject = newMarker;
            newMarker.transform.GetChild(0).GetComponent<ParticleSystemRenderer>().material = WaypointHandler.Instance.GetMarkerMaterial(work.WorkType);
            newMarker.GetComponent<WorkCommandHelper>().AssignedWork = work;

            _checkObjectValidation.Add(new ValidationCheck(newMarker, false));
            objectsToBePlaced.Add(newMarker);

            return newMarker;
        }
        public GameObject CreateMarker(AgentCommandType type, GameObject worldObject)
        {
            WorkCommand wc = WaypointHandler.Instance.CreateWorkCommand(type, Vector3.zero);
            wc.WorkType = type;
            wc.WorkObject = worldObject;

            GameObject newMarker = Instantiate(WaypointHandler.Instance.generalPrefab, GetParent(PlacementType.Waypoint));
            newMarker.transform.GetChild(0).GetComponent<ParticleSystemRenderer>().material = WaypointHandler.Instance.GetMarkerMaterial(type);
            newMarker.name = type.ToString() + " (Placing)";//string.Format("{0} Marker {1}", work.workCommand.ToString(), (GetParent(PlacementType.Waypoint).childCount).ToString("D3"));//  + " Marker " + ;
            wc.WaypointObject = newMarker;

            // Assign WC to Waypoint and Associated World Object
            worldObject.GetComponent<BuildRequirements>().assignedWorkCommand = wc;
            newMarker.GetComponent<WorkCommandHelper>().AssignedWork = wc;

            // Add to Validation Checks
            _checkObjectValidation.Add(new ValidationCheck(newMarker, false));
            objectsToBePlaced.Add(newMarker);

            return newMarker;
        }

        /// <summary>
        /// Returns True or False for valid Object Placement
        /// </summary>
        bool ValidatePlacementLocations()
        {
            // Temp Variables
            bool allValid = false;
            bool structureWithinLL = false;
            bool intersectingAnother = false;
            int numberOfValidObjects = 0;
            LightLink _closestLightLink = null;
            float _distanceToClosestLightLink = -1;
            float _minDistToOtherStructure = 12f;
            bool _clearOfOtherStructures = false;

            // Loop through the Items
            foreach (ValidationCheck vc in _checkObjectValidation)
            {
                // Reset Valid Location on Each Loop.
                vc.ValidLocation = false;
                Validating = vc.WorldObject;

                // Loop Temp Variables
                LightLink myLightLink = vc.WorldObject.GetComponent<LightLink>();

                // Setup Light Link Info
                if (myLightLink != null)
                {
                    _closestLightLink = GetClosestLightBase(vc.WorldObject.transform.position); //CommonUtilities.FindClosestLightLink(primaryPlacingObject, false);
                    if (_closestLightLink != null)
                    {
                        _distanceToClosestLightLink = CommonUtilities.DistanceFromTo(vc.WorldObject, _closestLightLink.gameObject);
                        DistanceToLL = _distanceToClosestLightLink;
                    }
                    else
                    {
                        break;
                    }
                }

                // Check Waypoint Validation
                if (vc.WorldObject.GetComponent<WorkCommandHelper>())
                {
                    if (!WaypointHandler.Instance.SameTaskWithin(vc.WorldObject.GetComponent<WorkCommandHelper>().AssignedWork.WorkType, vc.WorldObject.transform.position, 10f))
                    {
                        vc.ValidLocation = true;
                        numberOfValidObjects++;
                    }
                }

                // Blanket Structure Validation
                if (vc.WorldObject.GetComponent<PlayerStructure>())
                {
                    // Determine distance from other structures
                    _clearOfOtherStructures = ControlCrystal.Instance.ClearOfOtherStructures(vc.WorldObject.transform.position, _minDistToOtherStructure);
                    CollidingWithOther = !_clearOfOtherStructures;

                    // Grab the Collider for Bounds Checks
                    Bounds objBounds = vc.WorldObject.GetComponent<Collider>().bounds;

                    // Are we within the LL range
                    if (_distanceToClosestLightLink <= myLightLink.linkRange)
                    {
                        OutOfRange = false;

                        if (vc.CheckLOS)
                        {
                            if (CheckLOS(vc.WorldObject, _closestLightLink, _distanceToClosestLightLink))
                            {
                                structureWithinLL = true;
                                ClearLOS = structureWithinLL;
                            }
                            else
                            {
                                ClearLOS = false;
                            }
                        }
                        else
                        {
                            // Return true
                            structureWithinLL = true;
                            ClearLOS = true;
                        }
                    }
                    else
                    {
                        OutOfRange = true;
                    }

                    // Is the Object Obstructed? IE Placing in part of another object
                    if (vc.WorldObject.transform.GetChild(0).GetComponent<InColliderCheck>())
                    {
                        intersectingAnother = vc.WorldObject.transform.GetChild(0).GetComponent<InColliderCheck>().IsInsideAnother;
                        InsideAnother = intersectingAnother;
                    }

                    // Individual Rule Check
                    switch (vc.WorldObject.GetComponent<BuildRequirements>().ObjectType)
                    {
                        // Rule check for Purifier
                        case ObjectType.Purifier:

                            // Leyline Check - Can't place on Leylines
                            bool onLeyLine = InputHandler.Instance.hoverObjectTag == "LeylineNode";

                            // Distance check... Can't be placed too far from the Control Crystal
                            vc.ValidLocation = !onLeyLine && CommonUtilities.DistanceFromTo(vc.WorldObject, ControlCrystal.Instance.gameObject) < 50f;

                            break;

                        // Rule check for Light Tower
                        case ObjectType.LightTower:

                            vc.ValidLocation = InputHandler.Instance.hoverObjectTag != "LeylineNode";

                            break;

                        // Rule check for Light Well
                        case ObjectType.LightWell:

                            vc.ValidLocation = InputHandler.Instance.hoverObjectTag == "LeylineNode";

                            break;
                    }

                    // Is the Structure placement Valid?
                    if (!intersectingAnother && _clearOfOtherStructures && structureWithinLL && vc.ValidLocation)
                    {
                        numberOfValidObjects++;
                    }
                }
            }

            // Are all Objects Valid
            allValid = numberOfValidObjects == _checkObjectValidation.Count;

            // Update Materials for Both
            foreach (ValidationCheck vc in _checkObjectValidation)
            {
                // Object is Structure
                if (vc.WorldObject.GetComponent<PlayerStructure>())
                {
                    // Update Mesh Materials
                    MeshRenderer[] mrArray = vc.WorldObject.GetComponentsInChildren<MeshRenderer>();
                    foreach (MeshRenderer mr in mrArray)
                    {
                        if (mr.GetComponent<AdjustObjectVisuals>())
                            mr.material = allValid ? validPlacementMaterial : invalidPlacementMaterial;
                    }
                }

                // Object is Waypoint
                if (vc.WorldObject.GetComponent<WorkCommandHelper>())
                {
                    // Update Particle Effects
                    ParticleSystem[] psArray = vc.WorldObject.GetComponentsInChildren<ParticleSystem>();
                    foreach (ParticleSystem ps in psArray)
                    {
                        ParticleSystem.MainModule mainModule = ps.main;

                        mainModule.startColor = allValid ? Color.white : Color.red;
                    }
                }
            }

            // Return allValid
            return allValid;
        }

        /// <summary>
        /// Checks if there is Line Of Sight Issue
        /// </summary>
        /// <param name="checkAgainst"></param>
        /// <returns></returns>
        public bool CheckLOS(GameObject primary, LightLink checkAgainst, float suppliedDistance = 100f)
        {
            Transform _placingObjCrystal = primary.GetComponent<LightLink>().crystalPosition;
            Vector3 _placementHeading = checkAgainst.crystalPosition.position - _placingObjCrystal.position;
            var _distance = _placementHeading.magnitude;
            Vector3 _placementDirection = _placementHeading / _distance;

            // Check Line Of Sight
            RaycastHit losHit;

            if (Physics.Raycast(_placingObjCrystal.position, _placementDirection, out losHit, suppliedDistance + 5f))
            {
                if (losHit.collider.gameObject != checkAgainst.gameObject)
                {
                    Debug.DrawRay(_placingObjCrystal.position, _placementDirection * Vector3.Distance(_placingObjCrystal.position,losHit.point) , Color.red);
                    return false;
                }
                else
                {
                    Debug.DrawRay(_placingObjCrystal.position, _placementDirection * _distance, Color.green);
                }
            }

            return true;
        }

        /// <summary>
        /// Rebuilds the Light Link Array
        /// </summary>
        public void RebuildLightLinkArray()
        {
            // Rebuild Master List
            LightLink[] llArray = FindObjectsOfType<LightLink>();
            List<LightLink> validLL = new List<LightLink>();

            for (int i = 0; i < llArray.Length; i++)
            {
                if (llArray[i].enabled)
                    validLL.Add(llArray[i]);
            }

            // Rebuild Array
            allLightLinks = validLL.ToArray();

            // Terminate if we have no Structures
            if (ControlCrystal.Instance == null) return;
            if (ControlCrystal.Instance.playerStructures.Count == 0) return;

            // Rebuild the Light Link that only includes Structures and Active GameObjects
            lightLinkArray = new LightLink[ControlCrystal.Instance.playerStructures.Count];

            for (int i = 0; i < ControlCrystal.Instance.playerStructures.Count; i++)
            {
                var ps = ControlCrystal.Instance.playerStructures[i].PlayerStructure;
                if (ps == null) continue;
                lightLinkArray[i] = ps.GetComponent<LightLink>();
            }
        }

        /// <summary>
        /// Returns the Closes available Light Link
        /// </summary>
        /// <param name="myself"></param>
        /// <returns></returns>
        public LightLink FindClosestLightLink(LightLink myself)
        {
            Vector3 myselfPos = myself.transform.position;
            LightLink closestLink = null;
            float closestDistance = -1f;

            foreach (LightLink ll in lightLinkArray)
            {
                float distToLL = Vector3.Distance(ll.transform.position, myselfPos);

                if (ll != myself && distToLL <= myself.linkRange)
                {
                    if (ll.connectedToLightBase || ll.lightBase)
                    {
                        if (closestDistance == -1f || distToLL < closestDistance)
                        {
                            closestDistance = distToLL;
                            closestLink = ll;
                        }
                    }
                }
            }

            return closestLink;
        }

        /// <summary>
        /// Finds the closest Light Base / Link Distance
        /// </summary>
        /// <param name="relativePosition"></param>
        /// <returns></returns>
        public LightLink GetClosestLightBase(Vector3 relativePosition)
        {
            // Temp return value
            float closestDistance = -1f;
            LightLink closestLightLink = null;
            // Find the Closest Light Base
            foreach (LightLink ll in lightLinkArray)
            {
                if (ll == null) continue;
                if (ll.gameObject.activeInHierarchy)
                {
                    if (ll.connectedToLightBase || ll.lightBase)
                    {
                        float llDist = Vector3.Distance(ll.transform.position, relativePosition);

                        if (llDist < closestDistance || closestDistance == -1f)
                        {
                            closestDistance = llDist;
                            closestLightLink = ll;
                        }
                    }
                }
            }

            // Return Temp Value
            return closestLightLink;
        }


        // END OF PLACEMENTHANDLER
    }


    [Serializable]
    public class ValidationCheck
    {
        [SerializeField] public GameObject WorldObject;
        [SerializeField] public bool ValidLocation;
        [SerializeField] public bool CheckLOS;

        public ValidationCheck(GameObject go, bool los)
        {
            WorldObject = go;
            ValidLocation = false;
            CheckLOS = los;
        }
    }

    [Serializable]
    public class BoundsRay
    {
        [SerializeField] public Ray Ray;
        [SerializeField] public Vector3 Direction;
        [SerializeField] public float Distance;
        [SerializeField] public Color Colour;

        public BoundsRay(Ray ray, Vector3 dir, float dist, Color col)
        {
            Ray = ray;
            Direction = dir;
            Distance = dist;
            Colour = col;
        }
    }
}