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

    public void LoadFirstScreen() => StartCoroutine(LoadLevel(0));

    public void LoadFirstLevel() => StartCoroutine(LoadLevel(3));

    public void LoadNextLevel() => StartCoroutine(LoadLevel(SceneManager.GetActiveScene().buildIndex + 1));

    public void LoadPreviousLevel() => StartCoroutine(LoadLevel(SceneManager.GetActiveScene().buildIndex - 1));

    public void RestartLevel() => StartCoroutine(LoadLevel(SceneManager.GetActiveScene().buildIndex));

    private IEnumerator LoadLevel(int levelIndex)
    {
        _transition.SetTrigger("Start");
        _levelToLoad = SceneManager.LoadSceneAsync(levelIndex, LoadSceneMode.Single);
        _levelToLoad.allowSceneActivation = false;
        
        yield return new WaitForSeconds(_transitionTime);
        _levelToLoad.allowSceneActivation = true;
        //SceneManager.LoadScene(levelIndex);
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
}