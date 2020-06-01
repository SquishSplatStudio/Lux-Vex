/* SquishSplatStudio - https://discord.gg/HVjBM9T
 * You are welcome to use this file or contents for learning and/or non-commercial projects
 * Written by Dubh @Dubh#1508 <Discord> - 2020
 */

using System.Collections;
using UnityEngine;

namespace SquishSplatStudio
{
    public class Pause : MonoBehaviour
    {

        [SerializeField] LevelLoader _levelLoader;
        [SerializeField] float _pause;

        void Start() => StartCoroutine(LoadNextLevel());

        IEnumerator LoadNextLevel()
        {
            yield return new WaitForSeconds(_pause);
            _levelLoader.LoadNextLevel();
        }
    }
}