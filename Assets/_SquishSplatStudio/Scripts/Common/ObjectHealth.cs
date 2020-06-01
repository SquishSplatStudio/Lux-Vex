using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace SquishSplatStudio
{
    [Serializable]
    public class ObjectHealth : MonoBehaviour
    {
        [SerializeField] public bool adjustVisuals = true;
        [SerializeField] int _currentHealth = 100;
        public int currentHealth
        {
            get { return _currentHealth; }
        }

        [SerializeField] int _maxHealth = 100;
        public int maxHealth
        {
            get { return _maxHealth; }
        }
        List<Light> _myLights = new List<Light>();
        List<Collider> _myColliders = new List<Collider>();
        LightLink _myLL;
        bool _isDying = false;
        float _newPerc = 1f;
        float _setPerc = 1f;
        public bool _deathDone = false;
        float _withinLightTimer = 0f;
        bool _underAttack;
        float _lastAttack;
        float _tickTimer;

        // Start Loop
        private void Start()
        {
            foreach (Collider col in GetComponentsInChildren<Collider>())
            {
                _myColliders.Add(col);
            }

            foreach (Light l in GetComponentsInChildren<Light>())
            {
                _myLights.Add(l);
            }

            if (GetComponent<LightLink>())
                _myLL = GetComponent<LightLink>();
        }

        // Primary Loop
        private void Update()
        {
            // Tick Timer
            if (_tickTimer > 0)
                _tickTimer -= Time.deltaTime;

            if (_tickTimer <= 0)
            {
                // Reset Tick
                _tickTimer = GameHandler.Instance.TickRate;

                // Check Health
                if (!_underAttack && currentHealth < maxHealth && !_deathDone)
                    RecoverHealth();
            }


            // For Dark Agents Dying in the Light
            if (GetComponent<BuildRequirements>()?.ObjectType == PlacementType.Shade)
            {
                if (_withinLightTimer > 0)
                    _withinLightTimer -= Time.deltaTime;

                if (_withinLightTimer <= 0)
                    CheckForLightDamage();
            }

            // Adjust last Attack time
            if (_lastAttack < 5f)
            {
                _lastAttack += Time.deltaTime;
            }
            else
            {
                _underAttack = false;
            }

        }

        /// <summary>
        /// Handles health recovery if needed
        /// </summary>
        void RecoverHealth()
        {
            if (_myLL != null)
            {
                bool structureConnected = false;
                if (GetComponent<PlayerStructure>())
                {
                    if (_myLL.connectedToLightBase || _myLL.lightBase)
                        structureConnected = true;
                }
                bool agentLightInRange = GetComponent<BuildRequirements>().ObjectType == PlacementType.Agent && CommonUtilities.DistanceFromTo(PlacementHandler.Instance.GetClosestLightBase(transform.position).gameObject, gameObject) < _myLL.linkRange;

                if (structureConnected || agentLightInRange)
                    AdjustHealth(1);
            }
        }

        /// <summary>
        /// Adjusts the current Health
        /// </summary>
        /// <param name="value"></param>
        public void AdjustHealth(int value)
        {
            // If we've lost health, mark as being under attack
            if (value < 0)
            {
                if (!_underAttack)
                    _underAttack = true;

                // Update Last Attack Time
                _lastAttack = 0f;
            }

            _currentHealth += value;
            _currentHealth = Mathf.Clamp(_currentHealth, 0, _maxHealth);

            // Set Global Perc
            _newPerc = (float)_currentHealth / (float)_maxHealth;

            // Set Dying Value
            if (value < 0)
                _isDying = true;

            // Set Healing Value
            if (value > 0)
                _isDying = false;

            // Adjust Visuals
            if (adjustVisuals)
                AdjustObjectVisuals(_newPerc, _isDying);

            // Do Death
            if (_currentHealth == 0 && !_deathDone)
                DoDeath();
        }

        /// <summary>
        /// Adjusts the visual materials/look of the object
        /// </summary>
        void AdjustObjectVisuals(float newLerpPercent, bool isDying)
        {
            // Adjust the Lights
            foreach (Light l in _myLights)
            {
                float _newLightIntensity = (newLerpPercent * GetComponent<ObjectResource>().baseLightIntensity) * (GetComponent<ObjectResource>().CurrentInternalPower / GetComponent<ObjectResource>().MaxInternalPower);
                l.intensity = Mathf.Lerp(l.intensity, _newLightIntensity, Time.deltaTime * 1f);
            }

            // Loop through the Mesh Renders and do changes
            foreach (MeshRenderer mr in GetComponent<BuildRequirements>().listOfMeshRenderers)
            {
                if (mr.GetComponent<AdjustObjectVisuals>())
                    mr.GetComponent<AdjustObjectVisuals>().AdjustMaterials(newLerpPercent, isDying);
            }
        }

        /// <summary>
        /// Performs actions associated with Death
        /// </summary>
        public void DoDeath()
        {
            // Prevent Loop
            _deathDone = true;

            // Play Death Sound
            PlayDeathSound();

            // Spawn in Broken Version
            if (GetComponent<PlayerStructure>())
            {
                GetComponent<BuildRequirements>().ReturnCapacity();
                ControlCrystal.Instance.RemoveStructure(gameObject);
                if (GetComponent<BuildRequirements>().snapPlacement && GetComponent<BuildRequirements>().snapObject != null)
                    GetComponent<BuildRequirements>().snapObject.SetActive(true);

                foreach (MeshRenderer mr in GetComponent<BuildRequirements>().listOfMeshRenderers)
                {
                    mr.enabled = false;
                }
            }

            // Disable Animator
            if (GetComponent<Animator>())
                GetComponent<Animator>().enabled = false;

            // Disable NavAgent
            if (GetComponent<NavMeshAgent>())
                GetComponent<NavMeshAgent>().enabled = false;

            // Disable AI
            if (GetComponent<AI>())
                GetComponent<AI>().enabled = false;

            // Disable Light Worker
            if (GetComponent<LightWorker>())
            {
                // Decrease Worker Count
                ResourceType.Worker.DecreaseBy(1);

                // Remove Worker from Work List
                WaypointHandler.Instance.RemoveLightWorker(GetComponent<LightWorker>());

                // Rebuild Light Link Array
                PlacementHandler.Instance.RebuildLightLinkArray();

                // Disable Component
                GetComponent<LightWorker>().enabled = false;
            }

            // Disable Object Resources
            if (GetComponent<ObjectResource>())
                GetComponent<ObjectResource>().enabled = false;

            // Disable All Particle Emitters
            foreach (ParticleSystem ps in GetComponentsInChildren<ParticleSystem>())
            {
                ParticleSystem.MainModule mm = ps.main;
                mm.loop = false;
            }

            // Enable all Mesh Colliders
            foreach (Collider col in _myColliders)
            {
                col.enabled = true;
            }

            // Enable all Rigid Bodies
            foreach (Rigidbody rb in GetComponentsInChildren<Rigidbody>())
            {
                rb.useGravity = true;
                rb.isKinematic = false;
            }

            // Disable all Lights
            foreach (Light l in _myLights)
            {
                l.intensity = 0;
            }

            // If we were a structure, spawn death object
            if (GetComponent<PlayerStructure>())
                GetComponent<PlayerStructure>().SpawnDeathObject();

            // Give a soul to the Player
            if (GetComponent<BuildRequirements>().ObjectType == PlacementType.Shade)
            {
                var ll = PlacementHandler.Instance.GetClosestLightBase(transform.position);
                float distToLL = Vector3.Distance(transform.position, ll.transform.position);
                if (distToLL <= ll.linkRange)
                    ResourceType.Soul.IncreaseBy(1);
            }

            // Enable Time to Destroy GameObject
            StartCoroutine(DeathCleanup());
        }

        /// <summary>
        /// Plays a sound for the object
        /// </summary>
        void PlayDeathSound()
        {
            switch (GetComponent<BuildRequirements>()?.ObjectType)
            {
                case ObjectType.LightTower:
                case ObjectType.LightWell:
                case ObjectType.Purifier:
                    AudioController.Instance.PlayStructureDestroyed();
                    break;
                case PlacementType.Agent:
                    AudioController.Instance.PlayWorkerDeath();
                    break;
                case PlacementType.Shade:
                    AudioController.Instance.PlayEnemyVanquished();
                    break;
            }
        }

        // Checks to See if within Light Radius of a LL Object that isn't a Light Worker and start taking tick damage
        void CheckForLightDamage()
        {
            bool withinLightRadius = false;

            foreach (LightLink ll in PlacementHandler.Instance.lightLinkArray) // was all light links
            {
                if (ll == null) continue;
                if (ll.GetComponent<BuildRequirements>().objectBuilt) //ll.GetComponent<BuildRequirements>().ObjectType != PlacementType.Agent && 
                {
                    float distToLL = Vector3.Distance(transform.position, ll.transform.position);
                    if (distToLL <= ll.linkRange)
                        withinLightRadius = true;
                }
            }

            // Take Damage
            if (withinLightRadius)
            {
                AdjustHealth(-GameHandler.Instance.LightToShadeDamage);
            }

            // Reset Timer
            _withinLightTimer = GameHandler.Instance.TickRate;
        }

        // Death Cleanup
        IEnumerator DeathCleanup()
        {
            yield return new WaitForSeconds(2f);
            if (GetComponent<LightLink>())
                GetComponent<LightLink>().Destroyed();

            gameObject.SetActive(false);
            PlacementHandler.Instance.RebuildLightLinkArray();

            DestroyImmediate(gameObject);
        }

        // END OF OBJECTHEALTH
    }
}