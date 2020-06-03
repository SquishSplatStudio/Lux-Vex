/* SquishSplatStudio - https://discord.gg/HVjBM9T
 * You are welcome to use this file or contents for learning and/or non-commercial projects
 * Written by Dubh @Dubh#1508 <Discord> - 2020
 */

using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using UnityEngine.UI;

namespace SquishSplatStudio
{
    public class MenuButton : MonoBehaviour
    {

        [SerializeField] AudioSource _audioSource;
        [SerializeField] AudioClip _hoverSoundEffect;
        [SerializeField] AudioClip _clickSoundEffect;
        [SerializeField] LevelLoader _levelLoader;

        TextMeshProUGUI _tmp;
        string _old;
        string _new;

        void Start()
        {
            _tmp = GetComponentInChildren<TextMeshProUGUI>();
            _old = _tmp.text;
            _new = string.Concat("- ", _old, " -");
        }

        public void OnPointerEnter(BaseEventData data)
        {
            _tmp.text = _new;
            AudioController.Instance.Play(_hoverSoundEffect, AudioController.AudioSourceType.FX);
        }

        public void OnPointerExit(BaseEventData data) => _tmp.text = _old;

        public void StartGame()
        {
            AudioController.Instance.Play(_clickSoundEffect, AudioController.AudioSourceType.FX);
            if (PlayerPrefs.GetInt("CompletedTutorial") != 1)
            {
                _levelLoader.LoadNextLevel();
            }
            else
            {
                _levelLoader.LoadFirstLevel();
            }
        }

        public void NextLevel()
        {
            AudioController.Instance.Play(_clickSoundEffect, AudioController.AudioSourceType.FX);
            DisableButton();
            _levelLoader.LoadNextLevel();
        }

        public void RestartLevel()
        {
            AudioController.Instance.Play(_clickSoundEffect, AudioController.AudioSourceType.FX);
            DisableButton();
            _levelLoader.RestartLevel();
        }

        public void ExitGame()
        {
            AudioController.Instance.Play(_clickSoundEffect, AudioController.AudioSourceType.FX);
            Application.Quit();
        }

        public void Credits()
        {
            AudioController.Instance.Play(_clickSoundEffect, AudioController.AudioSourceType.FX);
            _levelLoader.LoadCreditScreen(true);
        }

        public void ExitCredits()
        {
            AudioController.Instance.Play(_clickSoundEffect, AudioController.AudioSourceType.FX);
            _levelLoader.LoadCreditScreen(false);
        }

        public void MainMenu()
        {
            DisableButton();
            AudioController.Instance.Play(_clickSoundEffect, AudioController.AudioSourceType.FX);

            // Load Menu
            _levelLoader.LoadFirstScreen();
        }

        public void HideMenu()
        {
            AudioController.Instance.Play(_clickSoundEffect, AudioController.AudioSourceType.FX);
            _levelLoader.LoadCreditScreen(false);
        }

        void DisableButton()
        {
            // Reset Time Scale
            Time.timeScale = 1;

            GetComponent<Button>().interactable = false;
        }
    }
}
