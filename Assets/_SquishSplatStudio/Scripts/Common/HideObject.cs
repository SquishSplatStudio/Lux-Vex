using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SquishSplatStudio
{
    public class HideObject : MonoBehaviour
    {
        float _currentTimer = 0f;
        public bool currentlyVisible = false;
        List<GameObject> _childTransforms = new List<GameObject>();
        public LightLink lastLightLinkInRange;
        bool beenSeen = false;


        // Build List of my Transforms
        private void Start()
        {
            foreach (Transform tran in GetComponentsInChildren<Transform>())
            {
                _childTransforms.Add(tran.gameObject);
            }

            if (currentlyVisible) MakeVisible();
            if (!currentlyVisible) MakeHidden();
        }

        // Update is called once per frame
        void Update()
        {
            // Only keep checking if Object hasn't been seen
            if (!beenSeen)
            {
                // Deduct from the Timer
                if (_currentTimer > 0)
                    _currentTimer -= Time.deltaTime;

                // Check if we need to Process once Timer is done
                if (_currentTimer <= 0)
                    CheckVisibility();
            }
        }

        void CheckVisibility()
        {
            // Do check on previous found Item
            if (lastLightLinkInRange != null)
            {
                if (Vector3.Distance(transform.position, lastLightLinkInRange.transform.position) > lastLightLinkInRange.linkRange)
                {
                    lastLightLinkInRange = null;
                }
            }

            // Update the Light Link we have stored
            if (lastLightLinkInRange == null)
            {
                LightLink[] currentLLArray = PlacementHandler.Instance.allLightLinks;

                for (int i = 0; i < currentLLArray.Length; i++)
                {
                    if (Vector3.Distance(currentLLArray[i].transform.position, transform.position) <= currentLLArray[i].linkRange)
                    {
                        lastLightLinkInRange = currentLLArray[i];
                        break;
                    }
                }

                if (lastLightLinkInRange != null && !currentlyVisible)
                    MakeVisible();

                if (lastLightLinkInRange == null && currentlyVisible)
                    MakeHidden();
            }

            // Reset Timer
            _currentTimer = GameHandler.Instance.TickRate;
        }

        void MakeVisible()
        {
            currentlyVisible = true;

            foreach (GameObject go in _childTransforms)
            {
                if (go.gameObject != this.gameObject)
                {
                    go.gameObject.SetActive(true);
                }

                if (go.GetComponent<MeshRenderer>())
                    go.GetComponent<MeshRenderer>().enabled = true;
            }

            if (GetComponent<BuildRequirements>())
            {
                if (GetComponent<BuildRequirements>().ObjectType != PlacementType.Shade)
                    beenSeen = true;
            }
            
        }

        void MakeHidden()
        {
            currentlyVisible = false;

            foreach (GameObject go in _childTransforms)
            {
                if (go.gameObject != this.gameObject)
                {
                    go.gameObject.SetActive(false);
                }

                if (go.GetComponent<MeshRenderer>())
                    go.GetComponent<MeshRenderer>().enabled = false;
            }
        }
    }
}