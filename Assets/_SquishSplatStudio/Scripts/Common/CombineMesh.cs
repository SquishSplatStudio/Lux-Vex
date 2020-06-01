using UnityEngine;

namespace SquishSplatStudio
{
    public class CombineMesh
    {
        public static GameObject GetCombined(MeshFilter[] meshList)
        {
            CombineInstance[] meshCombination = new CombineInstance[meshList.Length];

            GameObject newObject = new GameObject();
            newObject.AddComponent<MeshFilter>();
            newObject.AddComponent<MeshRenderer>();

            Mesh objMesh = new Mesh();

            for (int i = 0; i < meshCombination.Length; i++)
            {
                meshCombination[i].subMeshIndex = 0;
                meshCombination[i].mesh = meshList[i].sharedMesh;
                meshCombination[i].transform = meshList[i].transform.localToWorldMatrix;
            }

            objMesh.CombineMeshes(meshCombination);

            newObject.GetComponent<MeshFilter>().sharedMesh = objMesh;

            return newObject;
        }
    }
}