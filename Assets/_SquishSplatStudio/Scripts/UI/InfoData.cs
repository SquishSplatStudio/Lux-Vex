/* SquishSplatStudio - https://discord.gg/HVjBM9T
 * You are welcome to use this file or contents for learning and/or non-commercial projects
 * Written by Dubh @Dubh#1508 <Discord> - 2020
 */

using UnityEngine;

namespace SquishSplatStudio
{
    [CreateAssetMenu(fileName = "infoData", menuName = "Info Data", order = 61)]
    public class InfoData : ScriptableObject
    {
        public Sprite Icon;
        public string Name;
        public string Description;
        public string Crystals;
        public string Souls;
        public string Light;
        public string Capacity;
    }
}