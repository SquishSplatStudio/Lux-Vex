using UnityEngine;

namespace SquishSplatStudio
{
    public class InColliderCheck : MonoBehaviour
    {
        public bool IsInsideAnother;

        private void OnTriggerEnter(Collider other) => IsInsideAnother = true;

        private void OnTriggerExit(Collider other) => IsInsideAnother = false;

        private void OnTriggerStay(Collider other)
        {
            if (!IsInsideAnother)
                IsInsideAnother = true;
        }
    }
}