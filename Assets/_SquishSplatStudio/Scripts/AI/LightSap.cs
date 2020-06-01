using System.Collections;
using UnityEngine;

namespace SquishSplatStudio
{
    public class LightSap : MonoBehaviour
    {
        [SerializeField] float _myLife = 2f;
        [SerializeField] Transform projectile;
        [SerializeField] float projectileSpeed = 2f;
        [SerializeField] float projectileFireDelay = 1f;
        [SerializeField] public int damageToDeal = 0;
        bool oneTime;
        float _fireProjectile;

        // Start is called before the first frame update
        void Start()
        {
            if (projectile != null)
                _fireProjectile = projectileFireDelay;
        }

        private void LateUpdate()
        {
            if (projectile != null)
            {
                if (_fireProjectile > 0)
                    _fireProjectile -= Time.deltaTime;

                if (_fireProjectile <= 0)
                    FireProjectile();
            }

            if (oneTime) return;
            oneTime = true;
            Invoke("CleanUp", 10f);
        }

        void FireProjectile() => projectile.position += transform.forward * (Time.deltaTime * projectileSpeed);

        public void HitObject(GameObject go)
        {
            // Do Stuff
            if (go.GetComponent<ObjectHealth>())
                go.GetComponent<ObjectHealth>().AdjustHealth(-damageToDeal);

            // Do Cleanup
            CleanUp();
        }

        void CleanUp()  => GameObject.Destroy(gameObject);
    }
}
