using SquishSplatStudio;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SquishSplatStudio
{
    public class Planet : MonoBehaviour
    {
        public PlanetShapeSettings shapeSettings;
        public PlanetColourSettings colourSettings;
        [Range(2, 256)]
        public int resolution = 128;

        [HideInInspector]
        public bool shapeSettingsFoldout;
        [HideInInspector]
        public bool colourSettingsFoldout;
        public bool autoUpdate = true;

        PlanetShapeGenerator planetShapeGenerator = new PlanetShapeGenerator();
        PlanetColourGenerator planetColourGenerator = new PlanetColourGenerator();

        [HideInInspector, SerializeField]
        MeshFilter[] meshFilters;
        PlanetFace[] planetFaces;

        private void Initialize()
        {
            planetShapeGenerator.UpdateSettings(shapeSettings);
            planetColourGenerator.UpdateSettings(colourSettings);

            if (meshFilters == null || meshFilters.Length == 0)
                meshFilters = new MeshFilter[6];
            planetFaces = new PlanetFace[6];

            Vector3[] directions =
            {
                Vector3.up,
                Vector3.down,
                Vector3.left,
                Vector3.right,
                Vector3.forward,
                Vector3.back
            };

            for (int i = 0; i < 6; i++)
            {
                if (meshFilters[i] == null)
                {
                    GameObject meshObj = new GameObject("Mesh");
                    meshObj.transform.SetParent(this.transform);

                    meshObj.AddComponent<MeshRenderer>();
                    meshFilters[i] = meshObj.AddComponent<MeshFilter>();
                    meshFilters[i].sharedMesh = new Mesh();
                }
                meshFilters[i].GetComponent<MeshRenderer>().sharedMaterial = colourSettings.planetMaterial;

                planetFaces[i] = new PlanetFace(planetShapeGenerator, meshFilters[i].sharedMesh, resolution, directions[i]);
            }
        }

        public void GeneratePlanet()
        {
            Initialize();
            GenerateMesh();
            GenerateColours();

            //CombineMesh.GetCombined(transform.GetComponentsInChildren<MeshFilter>());
        }

        public void OnShapeSettingsUpdated()
        {
            if (autoUpdate)
            {
                Initialize();
                GenerateMesh();
            }
        }

        public void OnColourSettingsUpdated()
        {
            if (autoUpdate)
            {
                Initialize();
                GenerateColours();
            }
        }

        void GenerateMesh()
        {
            foreach (PlanetFace pf in planetFaces)
            {
                pf.ConstructMesh();
            }

            planetColourGenerator.UpdateElevation(planetShapeGenerator.elevationMinMax);
        }

        void GenerateColours()
        {
            planetColourGenerator.UpdateColours();

            for (int i = 0; i < meshFilters.Length; i++)
            {
                if (meshFilters[i].gameObject.activeSelf)
                {
                    planetFaces[i].UpdateUVs(planetColourGenerator);
                }
            }
        }


        // END OF PLANET
    }
}