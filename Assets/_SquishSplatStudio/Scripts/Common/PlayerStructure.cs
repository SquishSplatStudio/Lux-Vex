using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace SquishSplatStudio
{
    [Serializable]
    [RequireComponent(typeof(BuildRequirements), typeof(LightLink))]
    [RequireComponent(typeof(ObjectHealth), typeof(ObjectResource))]
    public class PlayerStructure : PlaceableObject
    {
        [SerializeField] LightLink _lightLink;
        [SerializeField] ObjectResource _objectResource;
        [SerializeField] GameObject _brokenVersion;
        [SerializeField] List<GameObject> _brokenPieces;
        [SerializeField] float spawnTime;
        [SerializeField] public Transform spawnPoint;
        [SerializeField] Transform moveToPoint;
        [SerializeField] bool canSpawn = true;

        GameObject _moveLightWorker = null;

        void Update()
        {
            if (_moveLightWorker != null)
            {
                if (Vector3.Distance(_moveLightWorker.transform.position, moveToPoint.position) > 0.1f)
                {
                    MoveToPoint();
                }
                else
                    SetupWorker();
            }
        }

        public void SpawnDeathObject()
        {
            // Spawn the Broken Version
            if (_brokenVersion != null)
            {
                GameObject deathObject = Instantiate(_brokenVersion, transform.position, transform.rotation, transform.parent);
                deathObject.transform.localScale = transform.localScale;
                deathObject.name = "Destroyed " + transform.name;
            }

            // Enable the Broken Pieces
            if (_brokenPieces.Count > 0)
            {
                foreach (GameObject go in _brokenPieces)
                {
                    go.transform.parent.gameObject.SetActive(true);
                    go.transform.parent.SetParent(null);
                    go.SetActive(true);
                }
            }
        }

        public bool SpawnLightWorker()
        {
            // Early out
            if (!canSpawn) return false;

            // Prevent Loop
            canSpawn = false;

            // Trigger Animations
            GetComponent<Animator>().SetTrigger("doPurification");

            // Start Spawn CoRoutine
            StartCoroutine(SpawnWorker());

            return true;
        }

        void MoveToPoint()
        {
            //_moveLightWorker.transform.position += _moveLightWorker.transform.forward * (Time.deltaTime * 1.5f);
            _moveLightWorker.transform.position = Vector3.MoveTowards(_moveLightWorker.transform.position, moveToPoint.position, 2.5f * Time.deltaTime);
        }

        void SetupWorker()
        {
            _moveLightWorker.GetComponent<NavMeshAgent>().enabled = true;
            _moveLightWorker.GetComponent<AI>().enabled = true;
            _moveLightWorker.GetComponent<LightWorker>().enabled = true;

            _moveLightWorker = null;
            canSpawn = true;

            AudioController.Instance.PlayWorkerCompleted();
        }

        /// <summary>
        /// Registers this object with the SpawnHandler
        /// </summary>
        public void RegisterSpawner()
        {
            SpawnHandler.Instance.Initialize(gameObject);
        }

        IEnumerator SpawnWorker()
        {
            yield return new WaitForSeconds(spawnTime);

            int wID = WaypointHandler.Instance.GetLightWorkerNumber();
            _moveLightWorker = Instantiate(GameHandler.Instance.lightWorkerPrefab, spawnPoint.position, spawnPoint.rotation, PlacementHandler.Instance.GetParent(PlacementType.Agent));
            _moveLightWorker.name = GameHandler.Instance.lightWorkerPrefab.name + " " + wID.ToString("D3");
            _moveLightWorker.GetComponent<LightWorker>().workerID = wID;

            ResourceType.Worker.IncreaseBy(1);
        }

        // END OF PLAYERSTRUCTURE
    }
}