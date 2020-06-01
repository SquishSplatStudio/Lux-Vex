using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SquishSplatStudio
{
    [Serializable]
    public class PlaceableObject : MonoBehaviour
    {
        [Tooltip("What is this Objects Name?")]
        [SerializeField] public string Name;
        [SerializeField] public Vector3 placementOffset;
        //[Tooltip("This will be the Prefab for the Ingame Object")]
        //[SerializeField] GameObject baseObject;

    }
}