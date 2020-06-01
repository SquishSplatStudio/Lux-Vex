using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SquishSplatStudio
{
    [CreateAssetMenu(fileName = "NewShapeSetting", menuName = "Squish Splat Studio/Solar System/New Planet Shape Setting", order = 0)]
    public class PlanetShapeSettings : ScriptableObject
    {
        public float planetRadius = 1f;
        public PlanetNoiseSettings planetNoiseSettings;
    }
}