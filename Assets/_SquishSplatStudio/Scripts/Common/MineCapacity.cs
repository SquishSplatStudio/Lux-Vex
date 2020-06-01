using Boo.Lang;
using UnityEngine;

namespace SquishSplatStudio
{
    public class MineCapacity : MonoBehaviour
    {
        [SerializeField] int _maxCrystals;
        [SerializeField] Vector3 _origScale;
        [SerializeField] float _scaleModifier;
        List<WorkCommand> _commands = new List<WorkCommand>();
        public int CurrentCrystals;

        void Start() => _origScale = gameObject.transform.localScale;

        public void ConsumeCrystal(int amount)
        {
            CurrentCrystals = Mathf.Clamp(CurrentCrystals - amount, 0, _maxCrystals);
            if (CurrentCrystals <= 0)
            {
                CleanUpObject();
            }
            else
            {
                _scaleModifier = (float)CurrentCrystals / _maxCrystals;
                gameObject.transform.localScale = _origScale * _scaleModifier;
            }
        }

        void CleanUpObject()
        {
            for (var i = 0; i < _commands.Count; i++)
            {
                if (_commands[i] == null) continue;
                WaypointHandler.Instance.RemoveMarker(_commands[i]);
            }
            GameObject.Destroy(gameObject);
        }

        internal void RegisterWorkerCommands(WorkCommand wc)
        {
            if (_commands.Contains(wc)) return;
            _commands.Add(wc);
        }
    }
}