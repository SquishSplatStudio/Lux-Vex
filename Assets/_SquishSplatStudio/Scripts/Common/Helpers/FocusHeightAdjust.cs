using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SquishSplatStudio
{
    [Serializable]
    public class FocusHeightAdjust : MonoBehaviour
    {
        public Vector3 checkOffset;
        Ray groundRay;
        RaycastHit groundHit;

        // Update is called once per frame
        void Update()
        {
            groundRay = new Ray(transform.position + checkOffset, -Vector3.up);

            if (Physics.Raycast(groundRay, out groundHit))
            {
                if (groundHit.collider.tag == "Terrain")
                {
                    transform.position = groundHit.point;
                }
            }
        }


        // END OF FOCUSHEIGHTADJUST
    }
}