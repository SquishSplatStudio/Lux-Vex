using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SquishSplatStudio
{
    [Serializable]
    public class PlanetNoiseSettings
    {
        [Range(1,8)]
        public int numberOfLayers = 1;
        public float strength = 1f;
        public float baseRoughness = 1f;
        public float roughness = 2f;
        public float persistence = 0.5f;
        public Vector3 centre;
        public float minValue = 1f;
    }
}