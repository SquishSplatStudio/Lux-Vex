/* SquishSplatStudio - https://discord.gg/HVjBM9T
 * You are welcome to use this file or contents for learning and/or non-commercial projects
 * Written by Dubh @Dubh#1508 <Discord> - 2020
 */

using SquishSplatStudio;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelLoader : MonoBehaviour
{

    [SerializeField] Animator _transition;
    [SerializeField] float _transitionTime = 1f;
    public Canvas CreditCanvas;
    AsyncOperation _levelToLoad;
    bool _sceneReady;

    private void Update()
    {
        if (_levelToLoad.isDone)
        {
            _levelToLoad.allowSceneActivation = true;

            ResetSceneLoad();
        }
    }

    public void LoadFirstScreen() => StartLevelLoad(0);

    public void LoadFirstLevel() => StartLevelLoad(3);

    public void LoadNextLevel() => StartLevelLoad(SceneManager.GetActiveScene().buildIndex + 1);

    public void LoadPreviousLevel() => StartLevelLoad(SceneManager.GetActiveScene().buildIndex - 1);

    public void RestartLevel() => StartLevelLoad(SceneManager.GetActiveScene().buildIndex);

    void StartLevelLoad(int levelIndex)
    {
        _levelToLoad = SceneManager.LoadSceneAsync(levelIndex, LoadSceneMode.Single);

        StartCoroutine(InitLevelLoad());
    }

    private IEnumerator LoadLevel(int levelIndex)
    {
        _transition.SetTrigger("Start");
        _levelToLoad = SceneManager.LoadSceneAsync(levelIndex, LoadSceneMode.Single);
        _levelToLoad.allowSceneActivation = false;
        
        yield return new WaitForSeconds(_transitionTime);

        while (!_levelToLoad.isDone)
        {
            if (_levelToLoad.progress >= 0.9f)
                _sceneReady = true;
                //_levelToLoad.allowSceneActivation = true;

            yield return null;
        }

        ResetSceneLoad();
        //SceneManager.LoadScene(levelIndex);
    }

    private IEnumerator InitLevelLoad()
    {
        _transition.SetTrigger("Start");

        yield return new WaitForSeconds(_transitionTime);

        _levelToLoad.allowSceneActivation = true;
    }

    private IEnumerator LoadEmpty(bool visible = false)
    {
        _transition.SetTrigger("Start");
        if (!visible)
            CreditCanvas.gameObject.SetActive(visible);
        yield return new WaitForSeconds(_transitionTime);
        CreditCanvas.gameObject.SetActive(visible);
        if (visible)
        {
            if (GameUI.Instance)
                GameUI.Instance.ToggleSun(false);
            Time.timeScale = 0;
        }
    }

    internal void LoadCreditScreen(bool visible)
    {
        if (InputHandler.Instance != null)
            InputHandler.Instance.AdjustInput(visible);

        if (!visible)
        {
            if (GameUI.Instance)
                GameUI.Instance.ToggleSun(true);
            Time.timeScale = 1;
        }
        StartCoroutine(LoadEmpty(visible));
    }

    void ResetSceneLoad()
    {
        _levelToLoad.allowSceneActivation = false;
        _levelToLoad = null;
        _sceneReady = false;
    }
}