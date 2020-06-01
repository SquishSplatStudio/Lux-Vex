using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

namespace SquishSplatStudio
{
    public class ShadeSpawner : MonoBehaviour
    {
        public bool Destroyed;
        public GameObject shadePrefab;
        public float timeToSpawn = 5f;
        public Transform spawnPoint;
        public List<Transform> waypoints;
        public float spawnMovementSpeed = 5f;
        public List<GameObject> spawnCrystals;
        ObjectHealth myHealth;

        float _nextSpawn;
        public List<ShadeControl> _controlObjects = new List<ShadeControl>();
        List<int> _controlReleaseIdexes = new List<int>();


        // Start is called before the first frame update
        void Start() => myHealth = GetComponent<ObjectHealth>();

        void Update()
        {
            if (_nextSpawn > 0)
                _nextSpawn -= Time.deltaTime;

            if (_nextSpawn <= 0)
                SpawnShade();

            // Control Shade to Move To WP
            ControlShade();

            // Release Shades if there are any
            if (_controlReleaseIdexes.Count > 0)
            {
                _controlReleaseIdexes = _controlReleaseIdexes.OrderByDescending(i => i).ToList();
                for (int i = 0; i < _controlReleaseIdexes.Count; i++)
                {
                    ReleaseControl(_controlObjects[_controlReleaseIdexes[i]]);
                    _controlObjects.RemoveAt(_controlReleaseIdexes[i]);
                }

                // Clear the List
                _controlReleaseIdexes.Clear();
            }
        }

        // Spawns a Shade
        void SpawnShade()
        {
            GameObject newShade = Instantiate(shadePrefab, spawnPoint.position, spawnPoint.rotation, PlacementHandler.Instance.GetParent(PlacementType.Shade));
            newShade.name = shadePrefab.name;

            _controlObjects.Add(new ShadeControl(newShade, 0, waypoints[0].position));

            // Set the next Spawn Cycle
            _nextSpawn = timeToSpawn;
        }

        void ControlShade()
        {
            // Loop through Controlled Objects and move them to Release Position
            foreach (ShadeControl sc in _controlObjects)
            {
                float distToWP = Vector3.Distance(sc.Shade.transform.position, sc.MoveTo);

                if (distToWP > 0.1f)
                {
                    sc.Shade.transform.position = Vector3.MoveTowards(sc.Shade.transform.position, sc.MoveTo, spawnMovementSpeed * Time.deltaTime);
                }
                else
                {
                    if (sc.WaypointID == waypoints.Count - 1)
                    {
                        _controlReleaseIdexes.Add(_controlObjects.IndexOf(sc));
                    }
                    else
                    {
                        sc.WaypointID++;
                        sc.MoveTo = waypoints[sc.WaypointID].position;
                    }
                }
            }
        }

        // Releases the Control of the Shade
        void ReleaseControl(ShadeControl controlledShade)
        {
            controlledShade.Shade.GetComponent<NavMeshAgent>().enabled = true;
            controlledShade.Shade.GetComponent<DarkAgent>().enabled = true;
            controlledShade.Shade.GetComponent<DarkAI>().enabled = true;
            controlledShade.Shade.GetComponent<DarkAI>().IssueCommand(AgentCommandType.Idle, null);
        }
    }

    [Serializable]
    public class ShadeControl
    {
        [SerializeField] public GameObject Shade;
        [SerializeField] public int WaypointID;
        [SerializeField] public Vector3 MoveTo;

        public ShadeControl(GameObject shade, int wpID, Vector3 moveTo)
        {
            Shade = shade;
            WaypointID = wpID;
            MoveTo = moveTo;
        }
    }
}