using System.Collections.Generic;
using UnityEngine;

namespace SquishSplatStudio
{
    public class MiniMap : MonoBehaviour
    {
        float mapScale = 2.0f;

        [SerializeField] Transform MouseLocation;

        // enable to follow mouse and have a different mini map zoom
        //private void LateUpdate()
        //{
        //    var newPos = MouseLocation.position;
        //    newPos.y = transform.position.y;
        //    transform.position = newPos;
        //}

    }
}