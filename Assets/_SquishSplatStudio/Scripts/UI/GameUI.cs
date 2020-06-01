/* SquishSplatStudio - https://discord.gg/HVjBM9T
 * You are welcome to use this file or contents for learning and/or non-commercial projects
 * Written by Dubh @Dubh#1508 <Discord> - 2020
 */

#region ---[ directives ]---

using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


#endregion

namespace SquishSplatStudio
{
    public sealed class GameUI : MonoBehaviour
    {

        #region ---[ singleton code base ]---

        static GameUI() { }

        private GameUI() { }

        public static GameUI Instance { get; private set; }

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            //DontDestroyOnLoad(gameObject);
        }

        #endregion

        [SerializeField] Button BuildPurifierButton;
        [SerializeField] Button BuildLightWellButton;
        [SerializeField] Button BuildLightTowerButton;
        [SerializeField] Button PurifySoulButton;
        [SerializeField] TextMeshProUGUI Crystal;
        [SerializeField] TextMeshProUGUI Light;
        [SerializeField] TextMeshProUGUI Souls;
        [SerializeField] TextMeshProUGUI Workers;
        [SerializeField] TextMeshProUGUI Capacity;
        [SerializeField] TextMeshProUGUI MaxCapacity;
        [SerializeField] TextMeshProUGUI LuxLevel;
        [SerializeField] Slider sunTimer;
        float _optimizer = 1f;

        void LateUpdate()
        {
            if (_optimizer > 0f)
            {
                _optimizer -= Time.deltaTime;
                return;
            }

            _optimizer = GameHandler.Instance.TickRate;
            UpdateStats();
            UpdateButtons();
            AdjustLuxLevel();
        }

        void UpdateStats()
        {
            Crystal.text = ResourceType.Crystal.GetValue().ToString();
            Light.text = ResourceType.Light.GetValue().ToString();
            Souls.text = ResourceType.Soul.GetValue().ToString();
            Workers.text = ResourceType.Worker.GetValue().ToString();
            Capacity.text = ResourceType.Capacity.GetValue().ToString();
            MaxCapacity.text = string.Concat("/ ", ResourceType.Capacity.GetMaxValue().ToString());
        }

        internal void ShowCanvas()
        {
            var cg = gameObject.GetComponent<CanvasGroup>();
            cg.alpha = 1;

            GameHandler.Instance.DisplayOverlayElements();
        }

        void UpdateButtons()
        {
            BuildPurifierButton.interactable = ObjectType.Purifier.HasResources();
            BuildLightWellButton.interactable = ObjectType.LightWell.HasResources();
            BuildLightTowerButton.interactable = ObjectType.LightTower.HasResources();
            PurifySoulButton.interactable = PlacementType.Agent.HasResources() && ObjectType.Purifier.AnyActiveOnScreen();
        }

        void AdjustLuxLevel()
        {
            float luxLevel = 100f * GameHandler.Instance.timeLeftPercent;
            string luxText = luxLevel.ToString("F1") + "K";

            if (luxLevel < 10f)
                luxText = luxLevel.ToString("F2") + "K";

            if (luxLevel < 1f)
                luxText = luxLevel.ToString("F2");

            LuxLevel.text = luxText;
        }
    }
}