/* SquishSplatStudio - https://discord.gg/HVjBM9T
 * You are welcome to use this file or contents for learning and/or non-commercial projects
 * Written by Dubh @Dubh#1508 <Discord> - 2020
 */

using UnityEngine;

namespace SquishSplatStudio
{
    public class DarkAgent : MonoBehaviour
    {
        [SerializeField] Event _spawned;
        void Start() => _spawned.Trigger(gameObject);
    }
}