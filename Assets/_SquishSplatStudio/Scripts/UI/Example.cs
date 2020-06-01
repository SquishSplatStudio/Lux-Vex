/* SquishSplatStudio - https://discord.gg/HVjBM9T
 * You are welcome to use this file or contents for learning and/or non-commercial projects
 * Written by Dubh @Dubh#1508 <Discord> - 2020
 */

using UnityEngine;

namespace SquishSplatStudio
{
    public class Example : MonoBehaviour
    {
        [SerializeField]
        private InfoData _info;
        
        [SerializeField]
        Info _popup;

        public void OnMouseOver()
        {
            _popup.OpenInfoPanel();
            _popup.Name.text = _info.Name;
            _popup.Description.text = _info.Description;
            _popup.Crystals.text = _info.Crystals;
            _popup.Souls.text = _info.Souls;
            _popup.Capacity.text = _info.Capacity;
            _popup.Light.text = _info.Light;
            _popup.Icon.sprite = _info.Icon;
        }

        public void OnMouseExit() => _popup.CloseInfoPanel();
    }
}