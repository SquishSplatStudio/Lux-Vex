using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace SquishSplatStudio
{
    public class LevelInitialSpawn : MonoBehaviour
    {
        
        public bool doSpawn;
        public Animator myAnimator;
        public GameObject controlCrystal;
        public GameObject lightWorker;
        public int startingWorkers = 4;

        public List<GameObject> spawnPoints;

        // Update is called once per frame
        void Update()
        {
            if (!doSpawn) return;
            
            doSpawn = false;
            myAnimator.SetTrigger("spawnIn");
        }

        void PlaySound() => AudioController.Instance.PlayControlCrystalSpawn();

        void SpawnStartingCompliment()
        {
            GameObject cc = Instantiate(controlCrystal, spawnPoints[0].transform.position, Quaternion.identity, PlacementHandler.Instance.GetParent(PlacementType.Structure));
            cc.name = controlCrystal.name;

            // We need to do initial Build of the Light Link Array
            PlacementHandler.Instance.RebuildLightLinkArray();

            // Tell the Control Crystal to start Connections
            cc.GetComponent<LightLink>().ConnectToOthers();

            int workerNumber = 1;

            for (int i = 0; i < startingWorkers; i++)
            {
                GameObject lw = Instantiate(lightWorker, spawnPoints[i + 1].transform.position, spawnPoints[i + 1].transform.rotation, PlacementHandler.Instance.GetParent(PlacementType.Agent));
                lw.name = lightWorker.name + " " + workerNumber.ToString("D3");

                lw.GetComponent<LightWorker>().workerID = workerNumber;
                lw.GetComponent<NavMeshAgent>().enabled = true;
                lw.GetComponent<AI>().enabled = true;
                lw.GetComponent<LightWorker>().enabled = true;
                workerNumber++;
            }

            ResourceType.Worker.IncreaseBy(startingWorkers);

            // Update the Light Link Array
            PlacementHandler.Instance.RebuildLightLinkArray();
        }
    }
}