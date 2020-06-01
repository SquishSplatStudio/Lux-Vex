using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SquishSplatStudio
{
    public class PlanetFace
    {
        PlanetShapeGenerator planetShapeGenerator;
        Mesh mesh;
        int resolution;
        Vector3 localUp;
        Vector3 axisA;
        Vector3 axisB;


        public PlanetFace(PlanetShapeGenerator planetShapeGenerator, Mesh mesh, int res, Vector3 up)
        {
            this.planetShapeGenerator = planetShapeGenerator;
            this.mesh = mesh;
            this.resolution = res;
            this.localUp = up;

            axisA = new Vector3(up.y, up.z, up.x);
            axisB = Vector3.Cross(up, axisA);
        }

        public void ConstructMesh()
        {
            Vector3[] vertices = new Vector3[resolution * resolution];
            int[] triangles = new int[(resolution - 1) * (resolution - 1) * 6];
            int triangleIndex = 0;
            Vector2[] uv = mesh.uv;

            for (int y = 0; y < resolution; y++)
            {
                for (int x = 0; x < resolution; x++)
                {
                    int i = x + y * resolution;
                    Vector2 percent = new Vector2(x, y) / (resolution - 1);
                    Vector3 pointOnUnitCube = localUp + (percent.x - 0.5f) * 2 * axisA + (percent.y - 0.5f) * 2 * axisB;
                    Vector3 pointOnUnitySphere = pointOnUnitCube.normalized;
                    vertices[i] = planetShapeGenerator.CalculatePointOnPlanet(pointOnUnitySphere);

                    if (x != resolution - 1 && y != resolution - 1)
                    {
                        triangles[triangleIndex + 0] = i;
                        triangles[triangleIndex + 1] = i + resolution + 1;
                        triangles[triangleIndex + 2] = i + resolution;

                        triangles[triangleIndex + 3] = i;
                        triangles[triangleIndex + 4] = i + 1;
                        triangles[triangleIndex + 5] = i + resolution + 1;

                        triangleIndex += 6;
                    }
                }
            }

            // Clear the current Mesh Data
            mesh.Clear();

            // Assign to the Mesh
            mesh.vertices = vertices;
            mesh.triangles = triangles;
            //mesh.normals = vertices;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            mesh.RecalculateTangents();
            mesh.uv = uv;
        }

        public void UpdateUVs(PlanetColourGenerator colourGenerator)
        {
            Vector2[] uv = new Vector2[resolution * resolution];

            for (int y = 0; y < resolution; y++)
            {
                for (int x = 0; x < resolution; x++)
                {
                    int i = x + y * resolution;
                    Vector2 percent = new Vector2(x, y) / (resolution - 1);
                    Vector3 pointOnUnitCube = localUp + (percent.x - 0.5f) * 2 * axisA + (percent.y - 0.5f) * 2 * axisB;
                    Vector3 pointOnUnitySphere = pointOnUnitCube.normalized;

                    uv[i] = new Vector2(colourGenerator.BiomePercentFromPoint(pointOnUnitySphere), 0f);
                }
            }

            mesh.uv = uv;
        }


        // END OF PLANETFACE
    }
}