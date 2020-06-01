using UnityEngine;

namespace SquishSplatStudio
{
    public class WeaponProjectile : MonoBehaviour
    {
        [SerializeField] GameObject hitRegistrationEffect;

        private void OnCollisionEnter(Collision collision)
        {
            if (GetComponentInParent<LightSap>())
            {
                // Report hit to Parent
                GetComponentInParent<LightSap>().HitObject(collision.gameObject);

                // Instantiate Hit Registration at Collision Point
                GameObject hre = Instantiate(hitRegistrationEffect, collision.contacts[0].point, Quaternion.Euler(collision.contacts[0].normal));
                hre.transform.rotation = Quaternion.Euler(new Vector3(Random.Range(0, 360), Random.Range(0, 360), Random.Range(0, 360)));

                // Disable GameObject and wait to be destroyed
                gameObject.SetActive(false);
            }
        }
    }
}