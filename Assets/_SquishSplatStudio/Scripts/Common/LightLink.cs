using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SquishSplatStudio
{
    [Serializable]
    public class LightLink : MonoBehaviour
    {
        [SerializeField] public ObjectResource objectResources = null;
        [SerializeField] public bool controlOverride = false;
        [SerializeField] public bool lightBase = false;
        [SerializeField] public bool connectedToLightBase = false;
        [SerializeField] public int jumpsToBase = 0;
        [SerializeField] public List<LightLink> connectedLightLinks = new List<LightLink>();
        [SerializeField] public float linkRange = 0f;
        [SerializeField] public Transform crystalPosition = null;
        [SerializeField] public GameObject lightBeamPrefab = null;
        [SerializeField] List<LightBeam> lightBeams = new List<LightBeam>();
        public bool SolarConnected;

        public bool finalPositionSet;
        bool _powerChecksStarted;


        //debug
        public bool DestroyMe;

        // DEBUG UPDATE LOOP \\
        private void Update()
        {
            if (DestroyMe)
                Destroyed();
        }

        /// <summary>
        /// Check and Update the Light Beams
        /// </summary>
        void CheckLightBeams()
        {
            List<LightBeam> missingBeams = new List<LightBeam>();

            foreach (LightBeam lb in lightBeams)
            {
                if (lb.targetObject == null)
                {
                    missingBeams.Add(lb);
                }

                if (lb.targetObject != null)
                {
                    if (lb.targetBeam.GetPosition(1) != lb.targetObject.crystalPosition.position)
                    {
                        lb.targetBeam.SetPosition(1, lb.targetObject.crystalPosition.position);
                    }

                    if (lb.targetObject.connectedToLightBase || lb.targetObject.lightBase)
                    {
                        if (connectedToLightBase)
                            lb.targetBeam.enabled = true;
                    }
                }
            }

            // Cleanup Missing Beams
            for (int x = 0; x < missingBeams.Count; x++)
            {
                DestroyImmediate(missingBeams[x].targetBeam.gameObject);
                lightBeams.Remove(missingBeams[x]);
            }
        }

        void CreateLightBeam(LightLink target)
        {
            LightBeam newBeam = new LightBeam();
            GameObject newLightBeam = Instantiate(lightBeamPrefab, transform);
            newLightBeam.GetComponent<LineRenderer>().SetPosition(0, crystalPosition.position);
            newLightBeam.GetComponent<LineRenderer>().SetPosition(1, target.crystalPosition.position);

            newBeam.targetBeam = newLightBeam.GetComponent<LineRenderer>();
            newBeam.targetObject = target;
            newBeam.targetBeam.enabled = connectedToLightBase;

            lightBeams.Add(newBeam);
        }

        /// <summary>
        /// Adds a new child to the list
        /// </summary>
        /// <param name="newConnection"></param>
        public void AddConnection(LightLink newConnection)
        {
            if (!connectedLightLinks.Contains(newConnection))
            {
                connectedLightLinks.Add(newConnection);
                CreateLightBeam(newConnection);
            }
        }

        /// <summary>
        /// Removes a Specific Child and does updates accordingly
        /// </summary>
        /// <param name="connection"></param>
        public void RemoveConnection(LightLink connection)
        {
            connectedLightLinks.Remove(connection);

            // Remove the Light Beam Associated with
            LightBeam removeBeam = new LightBeam();
            foreach (LightBeam lb in lightBeams)
            {
                if (lb.targetObject == connection.gameObject)
                {
                    removeBeam = lb;
                    DestroyImmediate(lb.targetBeam.gameObject);
                }
            }

            lightBeams.Remove(removeBeam);
        }

        /// <summary>
        /// Adds a Light Link to the connected Array
        /// </summary>
        public void ConnectToOthers()
        {
            LightLink[] allNearbyLinks = PlacementHandler.Instance.lightLinkArray;

            foreach (LightLink ll in allNearbyLinks)
            {
                if (ll == null) continue;
                if (ll != this && ll.GetComponent<BuildRequirements>().ObjectType != PlacementType.Agent && ll.gameObject.activeInHierarchy)
                {
                    // Only connect to Built Structures
                    if (ll.GetComponent<BuildRequirements>().objectBuilt)
                    {
                        float distanceToLL = Vector3.Distance(transform.position, ll.transform.position);

                        if (distanceToLL <= linkRange)
                        {
                            if (PlacementHandler.Instance.CheckLOS(gameObject, ll, distanceToLL))
                                AddConnection(ll);
                        }
                    }
                }
            }

            // Start Power Checks
            if (!_powerChecksStarted)
            {
                _powerChecksStarted = true;
                StartCoroutine(PowerCheck());
            }
        }

        /// <summary>
        /// We need to remove this from other lists
        /// </summary>
        public void Destroyed()
        {
            DestroyMe = false;

            foreach (LightLink ll in connectedLightLinks)
            {
                if (ll != this)
                {
                    ll.OnDestruction(this);
                }
            }

            // NOTE :: CHECK FOR LIGHT BEAMS ??? NO IDEA WHAT THIS REFERENCE IS FOR :( LETS BE CLEARER WHEN THIS HAPPENS AGAIN
        }

        /// <summary>
        /// Update Child Lists
        /// </summary>
        public void OnDestruction(LightLink ll)
        {
            // Remove Child from List
            if (connectedLightLinks.Contains(ll))
            {
                connectedLightLinks.Remove(ll);
            }

            LightBeam tempBeam = null;
            // Update the Light Beam List
            foreach (LightBeam lb in lightBeams)
            {
                if (lb.targetObject == ll)
                {
                    tempBeam = lb;
                    DestroyImmediate(lb.targetBeam.gameObject);
                }
            }
            lightBeams.Remove(tempBeam);

            CheckLightBeams();
        }

        /// <summary>
        /// Simply Turns the Light Links Beam off
        /// </summary>
        public void ToggleLightBeam(LineRenderer lightBeam, bool overide, bool value)
        {
            if (!overide)
                lightBeam.enabled = !lightBeam.enabled;

            if (overide)
                lightBeam.enabled = value;
        }

        public bool CanConsume(ResourceRequirement resource)
        {
            if (resource.resource == ResourceType.Light && objectResources.CurrentInternalPower >= resource.amount)
            {
                objectResources.AdjustLight(resource.amount);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Comes under Control of the Control Crystal
        /// </summary>
        /// <param name="value"></param>
        public void ControlOverride(bool value)
        {
            controlOverride = true;
        }

        IEnumerator PowerCheck()
        {
            // DO CHECK
            // Set Temp Variable
            bool _connectedToBase = false;
            List<int> _missingConnections = new List<int>();

            // Calaculate Bool
            if (!controlOverride)
            {
                if (connectedLightLinks.Count > 0)
                {
                    jumpsToBase = -1;
                    int closestJumpsToBase = -1;

                    foreach (LightLink ll in connectedLightLinks)
                    {
                        if (!ll.gameObject.activeInHierarchy || ll == null)
                        {
                            _missingConnections.Add(connectedLightLinks.IndexOf(ll));
                        }

                        if (ll.lightBase)
                        {
                            closestJumpsToBase = 0;

                            // We're connected to the source, we need look no further
                            break;
                        }

                        if (ll.connectedToLightBase)
                        {
                            int newJumps = ll.jumpsToBase + 1;

                            if (closestJumpsToBase == -1 || newJumps < closestJumpsToBase)
                                closestJumpsToBase = ll.jumpsToBase + 1;
                        }
                    }

                    if (closestJumpsToBase != -1)
                    {
                        jumpsToBase = closestJumpsToBase;
                        _connectedToBase = true;
                    }
                }

                // Clear out the Missing Links
                if (_missingConnections.Count > 0)
                {
                    //assignedWork.OrderBy(o => o.workCommand.priority).ToList();
                    _missingConnections = _missingConnections.OrderByDescending(p => p).ToList();

                    for (int i = 0; i < _missingConnections.Count; i++)
                    {
                        connectedLightLinks.RemoveAt(_missingConnections[i]);
                    }
                }

                // If we don't register any active connections, try and find some.
                ConnectToOthers();
            }

            // Assign Variable
            connectedToLightBase = _connectedToBase;

            // Update Max Power
            //GetComponent<ObjectResource>().UpdatePower(); // Redundant?

            // Update Light Beams
            CheckLightBeams();

            // WAIT ONE
            yield return new WaitForSeconds(1f);

            // Restart
            if (GetComponent<ObjectHealth>()?.currentHealth > 0)
                StartCoroutine(PowerCheck());
        }


        // END OF CLASS
    }

    [Serializable]
    public class LightBeam
    {
        [SerializeField] public LightLink targetObject;
        [SerializeField] public LineRenderer targetBeam;
    }
}