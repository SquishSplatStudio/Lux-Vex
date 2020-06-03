using System.Collections.Generic;
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

        public List<ParticleSystem> myParticleSystems = new List<ParticleSystem>();
        Vector3 ui2D3DPosition;
        bool ui2DCursorSet = false;
        float lastTime;
        float deltaTime;

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
        void Start()
        {
            foreach (ParticleSystem ps in world3DCursor.GetComponentsInChildren<ParticleSystem>())
                myParticleSystems.Add(ps);

            Cursor.visible = !use3DCursor;
            lastTime = Time.realtimeSinceStartup;
        }

        // Update is called once per frame
        void Update()
        {
            // Sim DeltaTime
            deltaTime = Time.realtimeSinceStartup - lastTime;

            // Move mouse around scene
            GetScreenSpacePosition();
            MoveMouseCursor();
            UpdateParticleSystems();

            // Adjust DeltaTime
            lastTime = Time.realtimeSinceStartup;
        }

        /// <summary>
        /// Updates the Particle Systems with this Simulated Delta Time
        /// </summary>
        void UpdateParticleSystems()
        {
            foreach(ParticleSystem ps in myParticleSystems)
            {
                ps.Simulate(deltaTime, true, false);
            }
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
            ui2D3DPosition = Vector3.Lerp(ui2D3DPosition, mouseScreenToWorld, 1.0f - Mathf.Exp(-speed * deltaTime));
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
            if (groundLight != null)
                groundLight.position = InputHandler.Instance.currentWorldMousePosition;
        }
    }
}