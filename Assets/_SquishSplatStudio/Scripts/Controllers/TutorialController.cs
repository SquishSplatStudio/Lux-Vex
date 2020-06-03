using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SquishSplatStudio
{
    public class TutorialController : MonoBehaviour
    {
        #region Singleton Reference
        // Singleton Reference
        private static TutorialController _instance;
        public static TutorialController Instance { get { return _instance; } }

        // Setup Variables
        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(this.gameObject);
            }
            else
            {
                _instance = this;
            }
        }
        #endregion

        [SerializeField] GameObject tutorialsParent;
        [SerializeField] List<TutorialSteps> tutorialSteps;
        [SerializeField] GameObject textBackground;
        [SerializeField] ShadeSpawner shadeSpawner;
        [SerializeField] int CurrentStep = 0;

        bool _init = false;
        TutorialSteps _currentStep;
        TutorialSteps _previousStep;
        bool _checkConditions;
        float _displayTimer = -99f;
        bool _conditionTimerStarted = false;
        float _conditionCheckTimer = -99f;

        private void Update()
        {
            if (_init)
            {
                /// INPUT CONTROL \\\
                /// Continue Button \\\
                if (Input.GetKeyDown(KeyCode.Return))
                    ContinueTutorial();

                // Timer Wait
                if (_displayTimer > 0)
                    _displayTimer -= Time.deltaTime;

                // Condition Check Loop
                if (_conditionCheckTimer > 0)
                    _conditionCheckTimer -= Time.deltaTime;

                // Trigger Display
                if (_displayTimer <= 0 && _displayTimer != -99f)
                    ShowTutorials();

                // Condition Tigger Check
                if (_conditionCheckTimer <= 0 && _conditionCheckTimer != -99f)
                    ContinueTutorial();
            }
        }

        // Initialize the Tutorials
        public void Init()
        {
            _init = true;

            _currentStep = tutorialSteps[0];

            textBackground.SetActive(true);
            tutorialsParent.SetActive(true);

            DisplayTutorialText();
        }

        /// <summary>
        /// Displays the Tutorial Step Text
        /// </summary>
        void DisplayTutorialText()
        {
            // ReDisplay Tutorials
            ShowTutorials();

            // Hide Old Display text if Visible;
            if (CurrentStep > 0)
            {
                if (_currentStep.CurrentStepInfo - 1 > -1)
                {
                    _currentStep.StepText[tutorialSteps[CurrentStep].CurrentStepInfo - 1].gameObject.SetActive(false);
                    if (_currentStep.Highlighter != null && _currentStep.Highlighter.activeInHierarchy)
                        _currentStep.Highlighter.SetActive(false);
                }
                else
                {
                    _previousStep.StepText[_previousStep.CurrentStepInfo].gameObject.SetActive(false);
                    if (_previousStep.Highlighter != null && _previousStep.Highlighter.activeInHierarchy)
                        _previousStep.Highlighter.SetActive(false);
                }
            }

            // Show Current Display text.
            _currentStep.StepText[_currentStep.CurrentStepInfo].gameObject.SetActive(true);

            // Do we need to Trigger the Step Highlighter?
            if (_currentStep.CurrentStepInfo == _currentStep.HighlightOnStepInfo && _currentStep.Highlighter != null)
                _currentStep.Highlighter.SetActive(true);

            // Special Trigger
            if (_currentStep.Condition.TriggerShade && !shadeSpawner.enabled)
                shadeSpawner.enabled = true;
        }

        /// <summary>
        /// Continues the Tutorial Along
        /// </summary>
        public void ContinueTutorial()
        {
            // Check for Step Text
            if (_currentStep.CurrentStepInfo + 1 < _currentStep.StepText.Count)
            {
                _currentStep.CurrentStepInfo++;
                DisplayTutorialText();
            }
            else
            {
                if (!_currentStep.StepComplete)
                {
                    // Check Conditions
                    if (CheckStepConditions(_currentStep.Condition))
                    {
                        // Mark as Set
                        _currentStep.StepComplete = true;

                        // Stop Condition Check Timer
                        if (_conditionTimerStarted)
                        {
                            _conditionTimerStarted = false;
                            _conditionCheckTimer = -99f;
                        }

                        if (CurrentStep + 1 < tutorialSteps.Count)
                        {
                            // Store Previous Step
                            _previousStep = _currentStep;

                            // Hide Previous Items
                            _previousStep.StepText[_previousStep.CurrentStepInfo].SetActive(false);
                            if (_previousStep.Highlighter != null && _previousStep.Highlighter.activeInHierarchy)
                                _previousStep.Highlighter.SetActive(false);

                            // Increment Step
                            CurrentStep++;

                            // Store Current Step
                            _currentStep = tutorialSteps[CurrentStep];

                            // Display Next Tutorial
                            DisplayTutorialText();

                            /*
                            if (_currentStep.TimeToWaitForDisplay > 0)
                            {
                                _displayTimer = _currentStep.TimeToWaitForDisplay;
                                HideTutorials();
                            }
                            else
                            {
                                DisplayTutorialText();
                            }
                            */
                        }
                        else
                        {
                            HideTutorials();
                            PlayerPrefs.SetInt("CompletedTutorial", 1);
                        }
                    }
                    else
                    {
                        HideTutorials();

                        if (!_conditionTimerStarted)
                        {
                            _conditionTimerStarted = true;
                            _conditionCheckTimer = 1f;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Shows the Tutorials
        /// </summary>
        void ShowTutorials()
        {
            textBackground.SetActive(true);
            tutorialsParent.SetActive(true);
        }
        /// <summary>
        /// Hides the Tutorials (AKA Finsished)
        /// </summary>
        void HideTutorials()
        {
            textBackground.SetActive(false);
            tutorialsParent.SetActive(false);
        }

        /// <summary>
        /// Checks whether a Tutorial can proceed
        /// </summary>
        bool CheckStepConditions(TutorialCondition cond)
        {
            // Quick Out for no Conditions to Proceed
            if (cond.BuildingType == PlacementType.None && !cond.BuildObject && !cond.HaveFiveWorkers && !cond.PlaceWaypoint && !cond.TriggerShade && cond.WaypointType == AgentCommandType.None && !cond.HaveOneShadeSoul)
                return true;    // Basically a null check
            /*
            if (cond == new TutorialCondition() || cond == null)
                return true;
            */

            // Check for Build Requirement
            if (cond.BuildObject)
            {
                BuildRequirements[] structureArray = FindObjectsOfType<BuildRequirements>();

                foreach (BuildRequirements br in structureArray)
                {
                    if (br.ObjectType.HasFlag(cond.BuildingType) && br.objectBuilt)
                        return true;
                }
            }

            // Check for Waypoint Placement
            if (cond.PlaceWaypoint)
            {
                WorkCommandHelper[] wchArray = FindObjectsOfType<WorkCommandHelper>();

                foreach (WorkCommandHelper wch in wchArray)
                {
                    if (wch.AssignedWork.WorkType == cond.WaypointType)
                    {
                        if (wch.GetComponent<BuildRequirements>().objectBuilt)
                            return true;
                    }
                }
            }

            // Check Worker Count
            if (cond.HaveFiveWorkers)
            {
                if (ResourceType.Worker.GetValue() >= 5)
                    return true;
            }

            // Check if we need a Shade Soul
            if (cond.HaveOneShadeSoul)
            {
                if (ResourceType.Soul.GetValue() >= 1)
                    return true;
            }

            // Didn't find a Positive return Path
            _conditionCheckTimer = 1f;
            return false;
        }
    }

    [Serializable]
    public class TutorialSteps
    {
        [SerializeField] public string Name;
        [SerializeField] public List<GameObject> StepText;
        [SerializeField] public GameObject Highlighter;
        [SerializeField] public int HighlightOnStepInfo;
        [SerializeField] public TutorialCondition Condition;
        [SerializeField] public bool ContinueText;
        [SerializeField] public bool StepComplete;
        [SerializeField] public int CurrentStepInfo = 0;
        [SerializeField] public float TimeToWaitForDisplay = 0f;
    }

    [Serializable]
    public class TutorialCondition
    {
        [SerializeField] public bool BuildObject;
        [SerializeField] public PlacementType BuildingType;
        [SerializeField] public bool PlaceWaypoint;
        [SerializeField] public AgentCommandType WaypointType;
        [SerializeField] public bool TriggerShade;
        [SerializeField] public bool HaveOneShadeSoul;
        [SerializeField] public bool HaveFiveWorkers;

        public TutorialCondition()
        {
            BuildObject = false;
            BuildingType = PlacementType.None;
            PlaceWaypoint = false;
            WaypointType = AgentCommandType.None;
            TriggerShade = false;
            HaveFiveWorkers = false;
        }
    }

}