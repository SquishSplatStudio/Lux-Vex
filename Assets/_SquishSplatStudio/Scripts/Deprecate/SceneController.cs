using System;
using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.UIElements;

namespace SquishSplatStudio
{

    public sealed class SceneController : MonoBehaviour
    {

        #region ---[base singleton stuff]---

        static SceneController() { }
        public static SceneController Instance { get; private set; }

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        #endregion

        public event Action BeforeSceneUnload;
        public event Action AfterSceneLoad;

        public CanvasGroup faderCanvasGroup;
        public float fadeDuration = 1f;
        public string startingSceneName = "";
        public string initialStartingPositionName = "";
        public UnityEngine.UI.Slider slider;
        private bool _isFading;
        private AsyncOperation operation;

        public bool isShowing;
        public GameObject Panel;

        public Camera _camera;

        public void ToggleCamera(bool toggle)
        {
            if (_camera)
            {
                _camera.enabled = toggle;
            }
        }

        private IEnumerator Start()
        {
            faderCanvasGroup.alpha = 1f;

            //if (playerSaveData)
            //{
            //    playerSaveData.Save(PlayerMovement.startingPositionKey, initialStartingPositionName);
            //}

            yield return StartCoroutine(LoadSceneAndSetActive(startingSceneName));
            StartCoroutine(Fade(0f));
        }

        internal void ShowMenu(bool toggle)
        {
            Panel.SetActive(toggle);
            isShowing = toggle;
            faderCanvasGroup.alpha = (toggle) ? 1f : 0f;
        }

        //public void FadeAndLoadScene(SceneReaction sceneReaction)
        //{
        //    FadeAndLoadScene(sceneReaction.sceneName);
        //}

        public void FadeAndLoadScene(string sceneName)
        {
            if (!_isFading)
            {
                StartCoroutine(FadeAndSwitchScenes(sceneName));
            }
        }


        private IEnumerator FadeAndSwitchScenes(string sceneName)
        {
            yield return StartCoroutine(Fade(1f));

            BeforeSceneUnload?.Invoke();

            yield return SceneManager.UnloadSceneAsync(SceneManager.GetActiveScene().buildIndex);

            yield return StartCoroutine(LoadSceneAndSetActive(sceneName));

            AfterSceneLoad?.Invoke();

            yield return StartCoroutine(Fade(0f));
        }


        private IEnumerator LoadSceneAndSetActive(string sceneName)
        {
            yield return SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);


            //if (slider != null)
            //{
            //    float timePassed = 0f;
            //    while (!operation.isDone && timePassed < 5)
            //    {
            //        timePassed += Time.deltaTime;
            //        slider.value = Mathf.Clamp01(operation.progress);
            //        yield return null;
            //    }
            //}

            Scene newlyLoadedScene = SceneManager.GetSceneAt(SceneManager.sceneCount - 1);

            SceneManager.SetActiveScene(newlyLoadedScene);
        }


        private IEnumerator Fade(float finalAlpha)
        {

            _isFading = true;
            faderCanvasGroup.blocksRaycasts = true;
            float fadeSpeed = Mathf.Abs(faderCanvasGroup.alpha - finalAlpha) / fadeDuration;

            while (!Mathf.Approximately(faderCanvasGroup.alpha, finalAlpha))
            {
                faderCanvasGroup.alpha = Mathf.MoveTowards(faderCanvasGroup.alpha, finalAlpha,
                    fadeSpeed * Time.deltaTime);

                yield return null;
            }

            _isFading = false;

            faderCanvasGroup.blocksRaycasts = false;
        }
    }
}