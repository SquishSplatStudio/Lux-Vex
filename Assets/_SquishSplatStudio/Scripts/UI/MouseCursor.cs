using UnityEngine;

namespace SquishSplatStudio
{
    public class MouseCursor : MonoBehaviour
    {
        public Camera RenderCamera;
        public bool use3DCursor;
        public bool useCursorFX;
        public GameObject cursor3D;
        public GameObject cursor2D;
        public Transform groundLight;
        public Transform world3DCursor;
        public float speed = 8.0f;
        public float distanceFromCamera = 0.1f;

        Vector3 ui2D3DPosition;
        bool ui2DCursorSet = false;


        // Singleton Reference
        private static MouseCursor _instance;
        public static MouseCursor Instance { get { return _instance; } }

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

        // Start is called before the first frame update
        void Start() => Cursor.visible = !use3DCursor;

        // Update is called once per frame
        void Update()
        {
            GetScreenSpacePosition();
            MoveMouseCursor();
        }

        /// <summary>
        /// Displays the Mouse Cursor by a given Bool toggle (True/False)
        /// </summary>
        /// <param name="toggle"></param>
        public void DisplayMouseCursors(bool toggle)
        {
            world3DCursor.gameObject.SetActive(toggle);
        }

        /// <summary>
        /// Gets the screen position for the cursor
        /// </summary>
        void GetScreenSpacePosition()
        {
            Vector3 mousePosition = Input.mousePosition;
            mousePosition.z = distanceFromCamera;

            Vector3 mouseScreenToWorld = RenderCamera.ScreenToWorldPoint(mousePosition);
            ui2D3DPosition = Vector3.Lerp(ui2D3DPosition, mouseScreenToWorld, 1.0f - Mathf.Exp(-speed * Time.deltaTime));
        }

        /// <summary>
        /// Moves the Mouse cursor relative to the camera
        /// </summary>
        void MoveMouseCursor()
        {
            // Using 3D Cursor
            if (use3DCursor)
                world3DCursor.position = ui2D3DPosition;

            // Using 2D Cursor
            if (!use3DCursor && !ui2DCursorSet)
            {
                // Set the 2D Cursor

                // Mark as Set
                ui2DCursorSet = true;
            }

            // Move the Ground Light
            groundLight.position = InputHandler.Instance.currentWorldMousePosition;
        }
    }
}