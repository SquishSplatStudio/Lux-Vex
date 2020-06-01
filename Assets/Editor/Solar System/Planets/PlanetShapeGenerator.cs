using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SquishSplatStudio
{
    public class PlanetShapeGenerator
    {
        PlanetShapeSettings shapeSettings;
        PlanetNoiseFilter noiseFilter;
        public PlanetSurfaceMinMax elevationMinMax;

        public void UpdateSettings(PlanetShapeSettings shapeSettings)
        {
            this.shapeSettings = shapeSettings;
            noiseFilter = new PlanetNoiseFilter(shapeSettings.planetNoiseSettings);
            elevationMinMax = new PlanetSurfaceMinMax();
        }

        public Vector3 CalculatePointOnPlanet(Vector3 pointOnUnitSphere)
        {
            float elevation = noiseFilter.Evaluate(pointOnUnitSphere);
            elevation = shapeSettings.planetRadius * (1 + elevation);
            elevationMinMax.AddValue(elevation);
            return pointOnUnitSphere * elevation;
        }
    }
}