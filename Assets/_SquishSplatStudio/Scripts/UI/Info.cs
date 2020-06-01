/* SquishSplatStudio - https://discord.gg/HVjBM9T
 * You are welcome to use this file or contents for learning and/or non-commercial projects
 * Written by Dubh @Dubh#1508 <Discord> - 2020
 */

using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace SquishSplatStudio
{
    public class Info : MonoBehaviour
    {
        public GameObject InfoPanel;
        public Image Icon;
        public TextMeshProUGUI Name;
        public TextMeshProUGUI Description;
        public TextMeshProUGUI Crystals;
        public TextMeshProUGUI Souls;
        public TextMeshProUGUI Light;
        public TextMeshProUGUI Capacity;

        public void OpenInfoPanel() => InfoPanel.SetActive(true);

        public void CloseInfoPanel() => InfoPanel.SetActive(false);
    }
}