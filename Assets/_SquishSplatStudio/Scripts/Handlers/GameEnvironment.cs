/* SquishSplatStudio - https://discord.gg/HVjBM9T
 * You are welcome to use this file or contents for learning and/or non-commercial projects
 * Written by Dubh @Dubh#1508 <Discord> - 2020
 */

using UnityEngine;
using UnityEngine.AI;

namespace SquishSplatStudio
{
    public sealed class GameEnvironment : MonoBehaviour
    {
        #region ---[ singleton code base ]---

        static GameEnvironment() { }

        private GameEnvironment() { }

        public static GameEnvironment Instance { get; private set; }

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

        [SerializeField] private Terrain _terrain;
        TerrainData _terrainData;
        [SerializeField] int _shadeCount;
        [SerializeField] GameObject _controlCrystal;
        [SerializeField] float _controlCrystalRadius;
        //[SerializeField] float _startTime;
        //[SerializeField] float _minutesToNova;
        [SerializeField] LightLink SolarAccelerator;
        ObjectHealth _controlCrystalHealth;
        
        [SerializeField] LevelLoader _levelLoader; // temp
        [SerializeField] Canvas _winCanvas; // temp
        [SerializeField] Canvas _loseCanvas; // temp

        float _x, _z;

        void Start()
        {
            //_startTime = Time.time;
            //_minutesToNova *= 60;
            AudioController.Instance.PlayAmbience();

            _terrainData = _terrain.terrainData;
            _x = _terrainData.size.x * .5f;
            _z = _terrainData.size.z * .5f;

            SpawnShades();
            SolarAccelerator.ConnectToOthers();
            InvokeRepeating("CheckEndGameRequirements", 5f, 1f);
            //InvokeRepeating("GetDimmer", 0f, 1f);
        }

        //void GetDimmer() => GameHandler.Instance.startingSunBrightness -= (float)GameHandler.Instance.startingSunBrightness /GameHandler.Instance.sunTimer;

        void SpawnShades()
        {
            for (var i = 0; i < _shadeCount ; i++)
            {
                Vector3 pos;
                while (InCrystalRadius(pos = GenerateRandomLocation())) { }
                
                var shade = PlacementType.Shade.InstantiatedPrefab();
                shade.transform.position = pos;
                shade.transform.rotation = Quaternion.identity;
                shade.GetComponent<NavMeshAgent>().enabled = true;
                shade.GetComponent<DarkAgent>().enabled = true;
                shade.GetComponent<DarkAI>().enabled = true;
                
            }
        }

        public Vector3 GenerateRandomLocation()
        {
            Vector3 pos = new Vector3(Random.Range(-_x, _x), 0, Random.Range(-_z, _z));
            pos.y = _terrain.SampleHeight(pos) + 2;
            return pos;
        }

        public bool InCrystalRadius(Vector3 point)
        {
            // not near crystal 
            var dist = Vector3.Distance(point, _controlCrystal.transform.position);
            if (dist <= _controlCrystalRadius) return true;

            // On NavMesh
            var result = NavMesh.SamplePosition(point, out NavMeshHit hit, 2.0f, NavMesh.AllAreas);
            return !result;
        }

        void CheckEndGameRequirements()
        {
            if (WinningConditions())
            {
                _levelLoader.CreditCanvas = _winCanvas;
                _levelLoader.LoadCreditScreen(true);
            }
            else if (LosingConditions())
            {
                _levelLoader.CreditCanvas = _loseCanvas;
                _levelLoader.LoadCreditScreen(true);
            }
        }

        bool WinningConditions()
        {
            if (SolarAccelerator.connectedToLightBase)
            {
                ResetConnectedWellFlags();
                FlagWellsAsConnected(SolarAccelerator);
            }

            bool exists = false;
            var LeylineNode = GameObject.FindGameObjectsWithTag("LeylineNode");
            for (int i = 0; i < LeylineNode.Length; i++)
            {
                if (LeylineNode[i].activeInHierarchy)
                {
                    exists = true;
                    break;
                }
            }

            return (!exists && CheckIfWellsHaveFlag());
        }

        void ResetConnectedWellFlags()
        {
            var llList = GameObject.FindObjectsOfType<LightLink>();
            for (int i = 0; i < llList.Length; i++)
                llList[i].SolarConnected = false;
        }

        void FlagWellsAsConnected(LightLink llObject)
        {
            llObject.SolarConnected = true;
            var llChildren = llObject.connectedLightLinks;
            for (int i = 0; i < llChildren.Count; i++)
                if(!llChildren[i].SolarConnected)
                    FlagWellsAsConnected(llChildren[i]);
        }

        bool CheckIfWellsHaveFlag()
        {
            var llList = GameObject.FindObjectsOfType<LightLink>();
            for (int i = 0; i < llList.Length; i++)
                if (!llList[i].SolarConnected && llList[i].gameObject.GetComponent<BuildRequirements>().ObjectType == ObjectType.LightWell) return false;
            return true;
        }

        bool LosingConditions()
        {
            // time has passed ...
            if (GameHandler.Instance.sunTimer <= 0)
                return true;

            // out of resources to place the last light tower ...
            var mines = GameObject.FindObjectsOfType<MineCapacity>();
            var mineTally = 0;
            for (int i = 0; i < mines.Length; i++)
                mineTally += mines[i].CurrentCrystals;

            var costOfLightTower = ObjectType.LightTower.ResourceCost(ResourceType.Crystal);

            if (!ObjectType.LightTower.HasResources())
                if (mineTally <= costOfLightTower)
                    return true;

            // can't build purifiers structures
            var activePurifier = ObjectType.Purifier.AnyActiveOnScreen();

            var costOfPurifier = ObjectType.Purifier.ResourceCost(ResourceType.Crystal);
            var canBuildPurifier = ObjectType.Purifier.HasResources() || mineTally >= costOfPurifier;

            var lw = GameObject.FindObjectsOfType<LightWorker>();
            var lwCount = lw.Length;

            if ((lwCount <= 0 && !activePurifier) || (lwCount <= 0 && !canBuildPurifier && !activePurifier))
                return true;

            // control crytal health ...
            AssignControlCrystal();
            if (_controlCrystalHealth != null)
                if (_controlCrystalHealth.currentHealth <= 0)
                    return true;

            return false;
        }

        void AssignControlCrystal()
        {
            if (_controlCrystalHealth == null)
                _controlCrystalHealth = GameObject.FindGameObjectWithTag("ControlCrystal")?.GetComponent<ObjectHealth>();
        }

    }
}
