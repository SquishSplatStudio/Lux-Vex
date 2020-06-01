using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SquishSplatStudio
{
    public class PlanetSurfaceMinMax
    {
        public float Min { get; private set; }
        public float Max { get; private set; }

        public PlanetSurfaceMinMax()
        {
            Min = float.MaxValue;
            Max = float.MinValue;
        }

        public void AddValue(float value)
        {
            if (value > Max)
                Max = value;

            if (value < Min)
                Min = value;
        }
    }
}