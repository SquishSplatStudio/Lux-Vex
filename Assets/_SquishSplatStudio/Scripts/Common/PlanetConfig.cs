using System;
using System.Collections.Generic;
using UnityEngine;


namespace SquishSplatStudio
{
    [Serializable]
    public class PlanetConfig : MonoBehaviour
    {
        [Header("Planet Info")]
        public string planetName;
        public float planetRadius;
        public float planetGravity;
        public int numberOfLeylinePoints;
        public int leylinePointsPerFace;
        public int numberOfCrystalSources;
        public int crystalSourcesPerFace;
        public List<GameObject> planetFaces;
        public List<GameObject> landscapePrefabs;
        public GameObject leylinePrefab;
        public GameObject crystalPrefab;

        [Header("Planetary Biome")]
        public Transform landscapeObjectsParent;
        public List<GameObject> leylinePoints;
        public List<GameObject> crystalSources;

        static string _faceParentName = "Faces";
        static string _leylineParentName = "Leylines";
        static string _crystalParentName = "CrystalSources";
        GameObject facesParent;
        GameObject leylineParent;
        GameObject crystalParent;

        Vector3 _faceWidth;
        Vector3 _faceOffset;


        // Start is called before the first frame update
        void Start()
        {
            // Generate Planet Faces to Planet Radius
            _faceWidth = planetFaces[0].GetComponent<Terrain>().terrainData.size;
            _faceOffset = planetFaces[0].transform.position;

            // Create Parent Objects
            facesParent = new GameObject();
            facesParent.transform.SetParent(transform);
            facesParent.name = _faceParentName;
            leylineParent = new GameObject();
            leylineParent.transform.SetParent(transform);
            leylineParent.name = _leylineParentName;
            crystalParent = new GameObject();
            crystalParent.transform.SetParent(transform);
            crystalParent.name = _crystalParentName;

            // Generate Leyline Positions
            // Spawn in the Required Number of Leyline Points
            if (leylinePointsPerFace > 0 && leylinePrefab != null)
            {
                for (int ll = 0; ll < leylinePointsPerFace; ll++)
                {
                    GameObject newLLPoint = Instantiate(leylinePrefab, GetRandomPosition(), leylinePrefab.transform.rotation, leylineParent.transform);
                    newLLPoint.name = "LeyLine " + ll.ToString("D2");
                }
            }

            // Generate Crystal Sources Positions
            // Spawn in the Required Number of Crystal Sources
            if (crystalSourcesPerFace > 0 && crystalPrefab != null)
            {
                for (int cs = 0; cs < crystalSourcesPerFace; cs++)
                {
                    GameObject newCSPoint = Instantiate(crystalPrefab, GetRandomPosition(), crystalPrefab.transform.rotation, crystalParent.transform);
                    newCSPoint.name = "Crystal " + cs.ToString("D2");
                }
            }
        }

        /// <summary>
        /// Returns a Random Position on the Face's surface
        /// </summary>
        /// <returns></returns>
        Vector3 GetRandomPosition()
        {
            Vector3 returnPosition = new Vector3();

            returnPosition.x = UnityEngine.Random.Range((int)_faceOffset.x, (int)(_faceOffset.x + _faceWidth.x));
            returnPosition.z = UnityEngine.Random.Range((int)_faceOffset.z, (int)(_faceOffset.z + _faceWidth.z));
            //float _yPos = 0f; // planetFaces[0].GetComponent<Terrain>().terrainData.GetHeight(_xPos, _zPos);

            // Raycast Y Position
            Ray yPosRay = new Ray(new Vector3(returnPosition.x, 100f, returnPosition.z), Vector3.down);
            RaycastHit yRayHit;
            if (Physics.Raycast(yPosRay, out yRayHit, 200f))
            {
                if (yRayHit.collider.tag == "Terrain")
                {
                    returnPosition.y = yRayHit.point.y;
                }
            }

            return returnPosition;
        }


        // END OF PLANETCONFIG
    }
}