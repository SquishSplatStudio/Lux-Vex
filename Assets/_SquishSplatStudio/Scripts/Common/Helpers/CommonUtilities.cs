using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SquishSplatStudio
{
    [Serializable]
    public class CommonUtilities
    {
        public static LightLink FindClosestLightLink(GameObject worldObject, bool ignoreChild)
        {
            LightLink returnObj = null;
            float _closestLL = -1;

            LightLink[] _llArray = GameObject.FindObjectsOfType<LightLink>();

            for (int i = 0; i < _llArray.Length; i++)
            {
                if (_llArray[i].GetComponent<BuildRequirements>().objectBuilt && _llArray[i].GetComponent<BuildRequirements>().ObjectType.HasFlag(PlacementType.Structure) && _llArray[i].gameObject != worldObject)
                {
                    float _dist = Vector3.Distance(worldObject.transform.position, _llArray[i].gameObject.transform.position);

                    if (_closestLL == -1 || _dist < _closestLL)
                    {
                        if (!ignoreChild)
                        {
                            _closestLL = _dist;
                            returnObj = _llArray[i];
                        }
                        /*
                        if (ignoreChild)
                        {
                            if (!worldObject.GetComponent<LightLink>().childList.Contains(_llArray[i]))
                            {
                                _closestLL = _dist;
                                returnObj = _llArray[i];
                            }
                        }
                        */
                    }
                }
            }

            return returnObj;
        }

        /// <summary>
        /// Return the Distance between the two Objects
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <returns></returns>
        public static float DistanceFromTo(GameObject from, GameObject to)
        {
            return Vector3.Distance(from.transform.position, to.transform.position);
        }
        public static float DistanceFromTo(Vector3 from, Vector3 to)
        {
            return Vector3.Distance(from, to);
        }

        public static BuildRequirements ClosestBuild(Vector3 relativePosition)
        {
            BuildRequirements[] allBuildObjects = GameObject.FindObjectsOfType<BuildRequirements>();
            BuildRequirements closestBuild = null;

            foreach (BuildRequirements br in allBuildObjects)
            {
                if (!br.objectBuilt && br.ObjectType.HasFlag(PlacementType.Structure))
                {
                    if (Vector3.Distance(br.gameObject.transform.position, relativePosition) < 10f)
                    {
                        closestBuild = br;
                        break;
                    }
                }
            }

            return closestBuild;
        }

        // END OF COMMONUTILITIES
    }

}