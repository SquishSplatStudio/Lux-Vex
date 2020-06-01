using UnityEngine;

namespace SquishSplatStudio
{
    public class InputHandler : MonoBehaviour
    {
        [Header("Public I/O")]
        public GameObject mouseCursor;
        public Vector3 currentWorldMousePosition;
        public GameObject selectedObject;
        public GameObject hoverObject;
        public string hoverObjectTag;

        [Header("Camera Settings")]
        public Transform cameraRig;
        public Transform cameraTransform;
        public bool controlCamera;
        public bool followControlObject;
        public GameObject objectToControl;
        public Vector3 cameraOffset;
        public Vector2 minMaxZoom;
        public float moveSpeed;
        public float turnSpeed;
        public float zoomSpeed;
        public float minTerrainHeight;
        public float maxTerrainHeight;
        bool overrideMove = false;
        Vector3 overrideMoveTo = Vector3.zero;

        [Header("Input Flags")]
        public LayerMask mouseRayMask;
        public LayerMask objectSelectMask;
        public ControlMode currentMode;

        [Header("MiniMap Camera Settings")]
        public Camera MiniMapCamera;

        Ray mouseRay, objectRay;
        RaycastHit _mouseRayHit, _objectRayHit;
        GameObject _raycastObject;
        
        // Smooth Camera Control
        Vector3 newZoomLevel;

        // Singleton Reference
        private static InputHandler _instance;
        public static InputHandler Instance { get { return _instance; } }

        bool _disabled = true;

        [SerializeField] LevelLoader _levelLoader;

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
            // Track Mouse Position in the World
            mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);
            objectRay = Camera.main.ScreenPointToRay(Input.mousePosition);

            // Raycast for Mouse Position in the World
            if (Physics.Raycast(mouseRay, out _mouseRayHit, 500f, mouseRayMask))
            {
                currentWorldMousePosition = _mouseRayHit.point;

                // set mouse position
                //mouseCursor.transform.position = _mouseRayHit.point;
                //mouseCursor.transform.GetChild(0).transform.rotation = Quaternion.FromToRotation(mouseCursor.transform.GetChild(0).transform.up, _mouseRayHit.normal) * mouseCursor.transform.GetChild(0).transform.rotation;
            }

            // Raycast for Object Selection
            if (Physics.Raycast(objectRay, out _objectRayHit, 500f, objectSelectMask))
            {
                if (_objectRayHit.collider.tag == "Selectable")
                    _raycastObject = _objectRayHit.collider.gameObject;

                // Set Object Hover Tag
                if (_objectRayHit.collider.tag == "Selectable" || _objectRayHit.collider.tag == "LeylineNode")
                {
                    hoverObject = _objectRayHit.collider.gameObject;
                    hoverObjectTag = _objectRayHit.collider.tag;
                }
            }
            else
            {
                hoverObject = null;
                hoverObjectTag = string.Empty;
            }

            // BASIC CAMERA MOVEMENT / ROTATION \\
            if (controlCamera)
            {
                CameraControl();
            }

            if (objectToControl != null)
            {
                ControlObject();
            }

            if (_disabled) return;

            // Do Specific Calls based on Input
            switch (currentMode)
            {
                case ControlMode.Default:

                    WorldInteractions();
                    MiniMapInteractions();

                    break;

                case ControlMode.ObjectPlacement:

                    ObjectPlacement();

                    break;

                case ControlMode.UserInterface:

                    break;
            }

            /// GLOBAL INPUTS \\\
            if (overrideMove && Input.anyKeyDown)
                overrideMove = false;

            if (Input.GetKeyDown(KeyCode.H))
            {
                overrideMove = true;
                overrideMoveTo = ControlCrystal.Instance.transform.position;
            }
        }

        internal void EnableInput()
        {
            _disabled = false;
        }

        internal void AdjustInput(bool value)
        {
            _disabled = value;
        }

        /// <summary>
        /// Sets the Control Mode for Input
        /// </summary>
        /// <param name="mode"></param>
        public void SetControlMode(ControlMode mode) => currentMode = mode;

        /// <summary>
        /// Handles the Camera Control
        /// </summary>
        void CameraControl()
        {
            if (followControlObject)
            {
                Vector3 newPos = objectToControl.transform.position + objectToControl.transform.forward * cameraOffset.z + objectToControl.transform.right * cameraOffset.x + objectToControl.transform.up * cameraOffset.y; //objectToControl.transform.position + cameraOffset;
                cameraRig.position = Vector3.Lerp(cameraRig.position, newPos, Time.deltaTime * moveSpeed);

                Vector3 _objDir = objectToControl.transform.position - cameraRig.position;

                // Set the required rotation vector
                Quaternion _camRot = Quaternion.LookRotation(_objDir, Vector3.up);

                // Assigned the determined rotation vector
                cameraRig.rotation = Quaternion.Lerp(cameraRig.rotation, _camRot, Time.deltaTime * (turnSpeed * 10));

            }

            if (cameraTransform.localPosition.z >= minMaxZoom.x && cameraTransform.localPosition.z <= minMaxZoom.y)
            {
                if (Input.mouseScrollDelta.y != 0 && !Input.GetKey(KeyCode.LeftControl))
                {
                    newZoomLevel = cameraTransform.localPosition;
                    newZoomLevel.z += Input.mouseScrollDelta.y * zoomSpeed;
                    newZoomLevel.z = Mathf.Clamp(newZoomLevel.z, minMaxZoom.x, minMaxZoom.y);
                }

                cameraTransform.localPosition = Vector3.Lerp(cameraTransform.localPosition, newZoomLevel, Time.deltaTime * (zoomSpeed * 0.5f));
            }
        }

        /// <summary>
        /// Moves around a specifc object
        /// </summary>
        void ControlObject()
        {
            // Do We have Override Running?
            if (overrideMove)
            {
                objectToControl.transform.position = Vector3.MoveTowards(objectToControl.transform.position, overrideMoveTo, (moveSpeed * 10f) * Time.deltaTime);

                if (Vector3.Distance(objectToControl.transform.position, overrideMoveTo) < 0.1f)
                    overrideMove = false;
            }

            if (Input.GetAxis("Vertical") != 0f)
            {
                float _verticalMoveSpeedMod = Input.GetAxis("Vertical") * moveSpeed;
                if (Input.GetKey(KeyCode.LeftShift))
                {
                    _verticalMoveSpeedMod *= 2f;
                }

                objectToControl.transform.position += objectToControl.transform.forward * (Time.deltaTime * _verticalMoveSpeedMod);
            }

            if (Input.GetAxis("Horizontal") != 0f)
            {
                float _horizontalMoveSpeedMod = Input.GetAxis("Horizontal") * moveSpeed;
                if (Input.GetKey(KeyCode.LeftShift))
                {
                    _horizontalMoveSpeedMod *= 2f;
                }

                objectToControl.transform.position += objectToControl.transform.right * (Time.deltaTime * _horizontalMoveSpeedMod);
            }

            if (Input.GetMouseButton(2))
            {
                if (Input.GetAxis("Mouse X") != 0f)
                {
                    objectToControl.transform.Rotate(Vector3.up, Input.GetAxis("Mouse X") * (turnSpeed * 2.5f));
                }

                /*
                if (Input.GetAxis("Mouse Y") != 0f)
                {
                    objectToControl.transform.Rotate(Vector3.right, Input.GetAxis("Mouse Y") * (turnSpeed * 2.5f));
                }
                */
            }
        }

        void MiniMapInteractions()
        {
            if (Input.GetMouseButtonDown(0))
            {
                RaycastHit hit;
                var mmLayer = 1 << 14;
                //var mmLayer = LayerMask.GetMask("MiniMap");
                Ray ray = MiniMapCamera.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out hit, Mathf.Infinity, mmLayer))
                {
                    //MiniMapCamera.transform.position = hit.point;
                    //Debug.Log("Step3");
                    // hit.point contains the point where the ray hits the
                    // object named "MinimapBackground"
                    //Debug.Log(hit.point);
                }
                //itsMainCamera.transform.position = Vector3.Lerp(itsMainCamera.transform.position, hit.point, 0.1f);
            }
        }

        /// <summary>
        /// Basic Control for Interacting with the World
        /// </summary>
        void WorldInteractions()
        {
            // Mouse Input \\
            // Left Mouse
            if (Input.GetMouseButtonDown(0))
            {
                // Object Selection \\
                if (_raycastObject != null)
                {
                    selectedObject = _raycastObject;
                }
            }

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                _levelLoader.LoadCreditScreen(true);
            }

            // Keyboard Inputs
            if (Input.GetKeyDown(KeyCode.J))
            {
                if (selectedObject != null && selectedObject.GetComponent<ObjectHealth>() && selectedObject.GetComponent<BuildRequirements>().objectBuilt)
                    selectedObject.GetComponent<ObjectHealth>().AdjustHealth(-100);
            }

            if (Input.GetKeyDown(KeyCode.Delete))
            {
                if (selectedObject != null && selectedObject.layer == 12)
                {
                    WaypointHandler.Instance.RemoveMarker(selectedObject.GetComponent<WorkCommandHelper>().AssignedWork);
                    selectedObject = null;
                }
            }

            // ACTUAL

            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                SpawnController.Instance.PlaceBuildMarker();
            }

            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                SpawnController.Instance.PlaceScoutMarker();
            }

            if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                SpawnController.Instance.PlaceGuardMarker();
            }

            if (Input.GetKeyDown(KeyCode.Alpha4))
            {
                SpawnController.Instance.PlaceMineMarker();
            }

            if (Input.GetKeyDown(KeyCode.Alpha5))
            {
                SpawnController.Instance.PlaceAttackMarker();
            }

            if (Input.GetKeyDown(KeyCode.Z))
            {
                SpawnController.Instance.BuildLightTower();
            }

            if (Input.GetKeyDown(KeyCode.X))
            {
                SpawnController.Instance.BuildLightWell();
            }

            if (Input.GetKeyDown(KeyCode.C))
            {
                SpawnController.Instance.BuildPurifier();
            }

            if (Input.GetKeyDown(KeyCode.V))
            {
                SpawnController.Instance.PurifySoul();
            }
        }

        /// <summary>
        /// Controls for when Placing Structures
        /// </summary>
        void ObjectPlacement()
        {
            // Left Mouse (Place)
            if (Input.GetMouseButtonDown(0))
            {
                if (!Input.GetKey(KeyCode.LeftShift))
                {
                    // DO CHECK TO SEE IF IT IS A VALID PLACEMENT;
                    // NOTE ALSO; SHOW RADIUS FOR DEFAULT LIGHT RADIUS PLACEMENT RANGES
                    if (PlacementHandler.Instance.ConfirmPlacement(false))
                    {
                        if (SpawnController.Instance.MarkerPlacing)
                            AudioController.Instance.PlayWayPointPlacement();
                        else
                            AudioController.Instance.PlayStructurePlacement();
                        SpawnController.Instance.MarkerPlacing = false;
                        currentMode = ControlMode.Default;
                    }
                    else
                    {
                        PlacementHandler.Instance.CancelPlacement(true);
                        SpawnController.Instance.MarkerPlacing = false;
                    }
                }
                //else // commented out for future
                //{
                //    // DO CHECK HERE TO SEE IF WE CAN AFFORD ANOTHER PLACEMENT
                //    if (PlacementHandler.Instance.ConfirmPlacement(true))
                //    {
                //        AudioController.Instance.PlayStructurePlacement();
                //    }
                //    else
                //    {
                //        PlacementHandler.Instance.CancelPlacement(true);
                //        SpawnController.Instance.MarkerPlacing = false;
                //    }
                //}
            }

            // Right Mouse (Cancel)
            if (Input.GetMouseButtonDown(1))
            {
                PlacementHandler.Instance.CancelPlacement(true);
                currentMode = ControlMode.Default;
                SpawnController.Instance.MarkerPlacing = false;
                selectedObject = null;
            }

            /*
            // Left Mouse (Place)
            if (Input.GetMouseButtonDown(0))
            {
                if (!Input.GetKey(KeyCode.LeftShift))
                {
                    // DO CHECK TO SEE IF IT IS A VALID PLACEMENT;
                    // NOTE ALSO; SHOW RADIUS FOR DEFAULT LIGHT RADIUS PLACEMENT RANGES
                    if (PlacementHandler.Instance.ConfirmPlacement(false))
                    {
                        if (SpawnController.Instance.MarkerPlacing)
                            AudioController.Instance.PlayWayPointPlacement();
                        else
                            AudioController.Instance.PlayStructurePlacement();
                        SpawnController.Instance.MarkerPlacing = false;
                        currentMode = ControlMode.Default;
                    } else
                    {
                        PlacementHandler.Instance.CancelPlacement(true);
                        SpawnController.Instance.MarkerPlacing = false;
                    }
                }
                else
                {
                    // DO CHECK HERE TO SEE IF WE CAN AFFORD ANOTHER PLACEMENT
                    if (PlacementHandler.Instance.ConfirmPlacement(false))
                    {
                        SpawnController.Instance.PlaceCurrentBuild();
                        AudioController.Instance.PlayStructurePlacement();
                    }
                    else
                    {
                        PlacementHandler.Instance.CancelPlacement(true);
                        SpawnController.Instance.MarkerPlacing = false;
                    }
                }
            }

            // Right Mouse (Cancel)
            if (Input.GetMouseButtonDown(1))
            {
                PlacementHandler.Instance.CancelPlacement(true);
                SpawnController.Instance.RefundCurrentBuild();
                currentMode = ControlMode.Default;
                SpawnController.Instance.MarkerPlacing = false;
            }
            */

            // Rotate Object
            if (Input.mouseScrollDelta.y != 0 && Input.GetKey(KeyCode.LeftControl))
            {
                PlacementHandler.Instance.RotateObject(Input.mouseScrollDelta.y * zoomSpeed);
            }
        }
    }


    // END OF CLASS
}