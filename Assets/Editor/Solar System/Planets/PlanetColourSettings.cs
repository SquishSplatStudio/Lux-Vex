using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SquishSplatStudio
{
    [CreateAssetMenu(fileName = "NewColourSetting", menuName = "Squish Splat Studio/Solar System/New Planet Colour Setting", order = 0)]
    public class PlanetColourSettings : ScriptableObject
    {
        public Material planetMaterial;
        public BiomeColourSettings biomeColourSettings;

        [Serializable]
        public class BiomeColourSettings
        {
            public Biome[] biomes;

            [Serializable]
            public class Biome
            {
                public Gradient gradient;
                public Color tint;
                [Range(0f, 1f)]
                public float tintPercent;
                [Range(0f, 1f)]
                public float startHeight;
            }
        }
    }
}