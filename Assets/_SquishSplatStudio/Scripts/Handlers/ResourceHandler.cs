/* SquishSplatStudio - https://discord.gg/HVjBM9T
 * You are welcome to use this file or contents for learning and/or non-commercial projects
 * Written by Dubh @Dubh#1508 <Discord> - 2020
 */

using System.Collections.Generic;
using UnityEngine;

namespace SquishSplatStudio
{

    public class ResourceHandler : MonoBehaviour
    {

        #region ---[ singleton code base ]---

        static ResourceHandler() { }

        private ResourceHandler() { }

        public static ResourceHandler Instance { get; private set; }

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        #endregion

        public int MaximumSouls { get; private set; }
        public int Souls { get; private set; }
        
        public int MaximumCrystal { get; private set; }
        public int Crystal { get; private set; }
        
        public int MaximumLight { get; private set; }
        public int Light { get; private set; }
        
        public int MaximumWorkers { get; private set; }
        public int Workers { get; private set; }

        public int Capacity { get; private set; }
        public int MaximumCapacity { get; private set; }

        private bool _wasStatChanged;
        public bool AnyStatChanged
        {
            get
            {
                var changed = _wasStatChanged;
                _wasStatChanged = false;
                return changed;
            }
        }

        public void AdjustSouls(int amount)
        {
            Souls = Mathf.Clamp(Souls + amount, 0, MaximumSouls);
            _wasStatChanged = true;
        }

        public void AdjustMaximumSouls(int amount)
        {
            MaximumSouls = amount;
            _wasStatChanged = true;
        }

        public void AdjustLight(int amount)
        {
            Light = Mathf.Clamp(Light + amount, 0, MaximumLight);
            _wasStatChanged = true;
        }

        public void AdjustMaximumLight(int amount)
        {
            MaximumLight = amount;
            _wasStatChanged = true;
        }

        internal void AdjustStats(List<ResourceRequirement> resourceRequirements, ResourceType resourceType = ResourceType.Any)
        {
            for (var i = 0; i < resourceRequirements.Count; i++)
            {
                if (resourceType != resourceRequirements[i].resource)
                    resourceRequirements[i].resource.DecreaseBy(resourceRequirements[i].amount);
            }
        }

        internal void RefundStats(List<ResourceRequirement> resourceRequirements, ResourceType resourceType = ResourceType.Any)
        {
            for (var i = 0; i < resourceRequirements.Count; i++)
            {
                if (resourceType != resourceRequirements[i].resource)
                    resourceRequirements[i].resource.IncreaseBy(resourceRequirements[i].amount);
            }
        }

        public void AdjustCrystal(int amount)
        {
            Crystal = Mathf.Clamp(Crystal + amount, 0, MaximumCrystal);
            _wasStatChanged = true;
        }

        public void AdjustMaximumCrystal(int amount)
        {
            MaximumCrystal = amount;
            _wasStatChanged = true;
        }

        public void AdjustCapacity(int amount)
        {
            Capacity = Mathf.Clamp(Capacity + amount, -MaximumCapacity, MaximumCapacity);
            _wasStatChanged = true;
        }

        public void AdjustMaximumCapacity(int amount)
        {
            MaximumCapacity = Mathf.Clamp(MaximumCapacity + amount, 0, int.MaxValue);
            _wasStatChanged = true;
        }

        public void AdjustWorkers(int amount)
        {
            Workers = Mathf.Clamp(Workers + amount, 0, MaximumWorkers);
            _wasStatChanged = true;
        }

        public void AdjustMaximumWorkers(int amount)
        {
            MaximumWorkers = amount;
            _wasStatChanged = true;
        }
    }
}
