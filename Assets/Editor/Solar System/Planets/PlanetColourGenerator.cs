using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SquishSplatStudio
{
    public class PlanetColourGenerator
    {
        PlanetColourSettings settings;
        Texture2D texture;
        const int textureResolution = 64;

        public void UpdateSettings(PlanetColourSettings settings)
        {
            this.settings = settings;
            if (texture == null || texture.height != settings.biomeColourSettings.biomes.Length)
                texture = new Texture2D(textureResolution, settings.biomeColourSettings.biomes.Length);
        }

        public void UpdateElevation(PlanetSurfaceMinMax elevationMinMax)
        {
            settings.planetMaterial.SetVector("_elevationMinMax", new Vector4(elevationMinMax.Min, elevationMinMax.Max));
        }

        public float BiomePercentFromPoint(Vector3 pointOnUnitSphere)
        {
            float heightPecent = (pointOnUnitSphere.y + 1) / 2f;
            float biomeIndex = 0;
            int numberOfBiomes = settings.biomeColourSettings.biomes.Length;

            for (int i = 0; i < numberOfBiomes; i++)
            {
                if (settings.biomeColourSettings.biomes[i].startHeight < heightPecent)
                {
                    biomeIndex = i;
                }
                else
                {
                    break;
                }
            }

            return biomeIndex / Mathf.Max(1, numberOfBiomes - 1);
        }

        public void UpdateColours()
        {
            Color[] colours = new Color[texture.width * texture.height];
            int colourIndex = 0;

            foreach (var biome in settings.biomeColourSettings.biomes)
            {
                for (int i = 0; i < textureResolution; i++)
                {
                    Color gradientColour = biome.gradient.Evaluate(i / (textureResolution - 1f));
                    Color tintColor = biome.tint;
                    colours[colourIndex] = gradientColour * (1 - biome.tintPercent) + tintColor * biome.tintPercent;
                    colourIndex++;
                }
            }

            texture.SetPixels(colours);
            texture.Apply();
            settings.planetMaterial.SetTexture("_texture", texture);
        }
    }
}