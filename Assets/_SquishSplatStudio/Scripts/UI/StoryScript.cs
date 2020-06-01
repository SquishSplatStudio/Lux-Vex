/* SquishSplatStudio - https://discord.gg/HVjBM9T
 * You are welcome to use this file or contents for learning and/or non-commercial projects
 * Written by Dubh @Dubh#1508 <Discord> - 2020
 */

using System.Collections;
using UnityEngine;
using TMPro;

namespace SquishSplatStudio
{
    public class StoryScript : MonoBehaviour
    {
        [SerializeField] float _letterPause = 0.2f;
        [SerializeField] AudioClip _typeSound1;
        [SerializeField] LevelLoader _levelLoader;
        [SerializeField] float _pause;

        string _message;
        TextMeshProUGUI _textComp;

        void Start()
        {
            _textComp = GetComponent<TextMeshProUGUI>();
            _message = _textComp.text;
            _textComp.text = string.Empty;
            StartCoroutine(TypeText());
            StartCoroutine(LoadNextLevel());
        }

        IEnumerator LoadNextLevel()
        {
            yield return new WaitForSeconds(_pause);
            _levelLoader.LoadNextLevel();
        }

        IEnumerator TypeText()
        {
            foreach (char letter in _message.ToCharArray())
            {
                yield return new WaitForSeconds(_letterPause);
                _textComp.text += letter;
                if (_typeSound1)
                    AudioController.Instance.Play(_typeSound1, AudioController.AudioSourceType.FX, .2f);
            }
        }
    }
}
