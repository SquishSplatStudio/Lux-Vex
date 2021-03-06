﻿/* SquishSplatStudio - https://discord.gg/HVjBM9T
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
        [SerializeField] bool IntroText = false;
        [SerializeField] float _letterPause = 0.2f;
        [SerializeField] AudioClip _typeSound1;
        [SerializeField] LevelLoader _levelLoader;
        [SerializeField] float _pause;
        [SerializeField] public bool TextFinished = false;

        string _message;
        TextMeshProUGUI _textComp;

        void Start()
        {
            Init();
        }

        public void Init()
        {
            _textComp = GetComponent<TextMeshProUGUI>();
            _message = _textComp.text;
            _textComp.text = string.Empty;

            // Type Text
            StartCoroutine(TypeText());

            // Load Next Level if Intro Text
            if (_levelLoader != null && IntroText)
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

            TextFinished = true;
        }
    }
}
