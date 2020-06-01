using System.Collections;
using UnityEngine;

namespace SquishSplatStudio
{
    public class ObjectCleanup : MonoBehaviour
    {
        public bool doPhysicsCleanup = false;
        public float cleanupDelay = 5f;

        // Start is called before the first frame update
        void Start()
        {
            if (doPhysicsCleanup)
                StartCoroutine(PhysicsCleanup());

            if (!doPhysicsCleanup)
                StartCoroutine(DeathCleanup());
        }

        // Death Cleanup
        IEnumerator DeathCleanup()
        {
            yield return new WaitForSeconds(cleanupDelay);

            DestroyImmediate(gameObject);
        }

        // Physics Cleanup
        IEnumerator PhysicsCleanup()
        {
            yield return new WaitForSeconds(10f);

            foreach (Collider c in transform.GetComponentsInChildren<Collider>())
            {
                c.enabled = false;
            }

            StartCoroutine(DeathCleanup());
        }


        // END OF OBJECTCLEANUP
    }
}