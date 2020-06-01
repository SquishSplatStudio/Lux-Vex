using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SquishSplatStudio
{
    public class PlanetNoiseFilter
    {
        PlanetNoiseSettings settings;
        Noise noise = new Noise();

        public PlanetNoiseFilter(PlanetNoiseSettings settings)
        {
            this.settings = settings;
        }

        public float Evaluate(Vector3 point)
        {
            float noiseValue = 0f;
            float frequency = settings.baseRoughness;
            float amplitude = 1f;

            for (int i = 0; i < settings.numberOfLayers; i++)
            {
                float v = noise.Evaluate(point * frequency + settings.centre);
                noiseValue += (v + 1) * 0.5f * amplitude;
                frequency *= settings.roughness;
                amplitude *= settings.persistence;
            }

            noiseValue = Mathf.Max(0, noiseValue - settings.minValue);
            return noiseValue * settings.strength;
        }
    }
}