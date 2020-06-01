/* SquishSplatStudio - https://discord.gg/HVjBM9T
 * You are welcome to use this file or contents for learning and/or non-commercial projects
 * Written by Dubh @Dubh#1508 <Discord> - 2020
 */

using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

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
            _levelLoader.LoadNextLevel();
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
            AudioController.Instance.Play(_clickSoundEffect, AudioController.AudioSourceType.FX);
            _levelLoader.LoadFirstScreen();
        }

        public void HideMenu()
        {
            AudioController.Instance.Play(_clickSoundEffect, AudioController.AudioSourceType.FX);
            _levelLoader.LoadCreditScreen(false);
        }
    }
}
