using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SquishSplatStudio
{
    [Serializable]
    public class ObjectResource : MonoBehaviour
    {
        [SerializeField] public LightLink lightLink = null;
        [SerializeField] public float baseLightIntensity = 2f;
        [SerializeField] Light _objectLight = null;

        [SerializeField] int _currentInternalPower = 100;
        public int CurrentInternalPower
        {
            get { return _currentInternalPower; }
        }
        [SerializeField] int _maxInternalPower = 100;
        public int MaxInternalPower
        {
            get { return _maxInternalPower; }
        }

        [SerializeField] public List<ResourceRequirement> consumes;
        [SerializeField] public List<ResourceRequirement> produces;
        [Tooltip("How much of the Resource does it consume per Tick (Per Second)")]

        float _nextTick = 0f;


        // Primary Loop
        private void Update()
        {
            // Tick down until 0
            if (_nextTick > 0)
            {
                _nextTick -= Time.deltaTime;
            }

            // Trigger Tick Action
            if (_nextTick <= 0)
            {
                if (GetComponent<BuildRequirements>())
                {
                    if (GetComponent<BuildRequirements>().objectBuilt)
                        AdjustResources();
                }
            }
        }

        /// <summary>
        /// Does actions based on current power level
        /// </summary>
        public void LightAdjusted()
        {
            // Adjust the Light Intensity
            if (_objectLight != null)
                AdjustLightIntensity();

            // If we've run out of power
            if (_currentInternalPower == 0)
            {
                if (GetComponent<ObjectHealth>())
                    GetComponent<ObjectHealth>().DoDeath();
            }
        }

        /// <summary>
        /// Deducts a given amount of the internal light resource
        /// </summary>
        /// <param name="amount"></param>
        public void AdjustLight(int amount)
        {
            _currentInternalPower += amount;
            _currentInternalPower = Mathf.Clamp(_currentInternalPower, 0, _maxInternalPower);

            // Do Adjustments
            LightAdjusted();
        }

        /// <summary>
        /// Adjusts the Light Workers light Intensity
        /// </summary>
        void AdjustLightIntensity()
        {
            // Deduct from light radius
            if (_objectLight.intensity > 0)
            {
                _objectLight.intensity = (_currentInternalPower / _maxInternalPower) * baseLightIntensity;
            }

            // Ensure it remains Zero - Just in case ;)
            if (_objectLight.intensity < 0)
            {
                _objectLight.intensity = 0;
            }
        }

        /// <summary>
        /// Consumes the amount of resources in the consume list
        /// </summary>
        void AdjustResources()
        {
            // Set New Tick Rate
            _nextTick = GameHandler.Instance.TickRate; // 

            // Loop through the resource production
            foreach (ResourceRequirement pr in produces)
            {
                if (pr.resource == ResourceType.Light)
                {
                    ResourceHandler.Instance.AdjustLight(pr.amount);
                }
            }

            // Loop through Resources and Deduct amount
            foreach (ResourceRequirement res in consumes)
            {
                // Consume
                if (res.resource == ResourceType.Light)
                {
                    ConsumeLight(res.amount);
                }
            }

            // Recover
            if (!lightLink.controlOverride)
            {
                if (CurrentInternalPower < _maxInternalPower)
                {
                    bool structureConnected = false;
                    if (GetComponent<PlayerStructure>())
                    {
                        if (lightLink.connectedToLightBase || lightLink.lightBase)
                            structureConnected = true;
                    }
                    bool agentLightInRange = GetComponent<BuildRequirements>().ObjectType == PlacementType.Agent && CommonUtilities.DistanceFromTo(PlacementHandler.Instance.GetClosestLightBase(transform.position).gameObject, gameObject) < lightLink.linkRange;

                    if (structureConnected || agentLightInRange)
                        Recover(1);
                }
            }
        }

        /// <summary>
        /// Consumes a given resource from Parent or Global if able to
        /// </summary>
        void ConsumeLight(int amount)
        {
            if (lightLink.connectedToLightBase && ResourceHandler.Instance.Light >= amount)
            {
                ResourceHandler.Instance.AdjustLight(-amount);
            }
            if (lightLink.controlOverride || !lightLink.connectedToLightBase)
            {
                if (CurrentInternalPower > 0)
                    AdjustLight(-amount);
            }
        }

        /// <summary>
        /// Recovers a given resource from Parent or Global if able to
        /// </summary>
        void Recover(int amount)
        {
            int recoverAmount = amount * 2;
            if (ResourceHandler.Instance.Light >= recoverAmount)
            {
                AdjustLight(recoverAmount);
                ResourceHandler.Instance.AdjustLight(-recoverAmount);
            }

            // Trigger healing if needed
            if (GetComponent<ObjectHealth>().currentHealth < GetComponent<ObjectHealth>().maxHealth)
                GetComponent<ObjectHealth>().AdjustHealth(amount);
        }

        /// <summary>
        /// Updates the Maximum power and inverse Current power
        /// </summary>
        public void UpdatePower()
        {
            /*
            int _newMaxPower = (int)(100f - (100 *(0.1f * _lightLink.jumpsToBase)));

            if (_maxInternalPower != _newMaxPower)
            {
                _maxInternalPower = _newMaxPower;

                if (_currentInternalPower > _maxInternalPower)
                    _currentInternalPower = _maxInternalPower;
            }
            */
        }

        /// <summary>
        /// Cascades the power down to the Child or Inversely up to the parent.
        /// </summary>
        /// <param name="power"></param>
        public void PowerCascade(GameObject sender, int power)
        {
            // Set my new Power level
            //_currentInternalPower = power - 5;

            // Trickle Power either Direction
            /*
            if (_lightLink.linkParent != null && sender != _lightLink.linkParent && _lightLink.linkParent.objectResources.currentInternalPower < _currentInternalPower)
            {
                _lightLink.linkParent.objectResources.PowerCascade(gameObject, power);
            }

            // Power Up Children if they need it
            for (int i = 0; i < _lightLink.childList.Count; i++)
            {
                if (_lightLink.childList[i] != sender)
                {
                    _lightLink.linkParent.objectResources.PowerCascade(gameObject, power);
                }
            }
            */
            // Do actions based on power
            //AdjustLight(_currentInternalPower);
        }

        // END OF OBJECTHEALTH
    }
}
