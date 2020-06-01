/* SquishSplatStudio - https://discord.gg/HVjBM9T
 * You are welcome to use this file or contents for learning and/or non-commercial projects
 * Written by Dubh @Dubh#1508 <Discord> - 2020
 */

using UnityEngine;

namespace SquishSplatStudio
{
    public class EventListener : MonoBehaviour
    {
        [SerializeField] Event GameEvent;
        [SerializeField] GameObjectEvent ResponseToGameObjectEvent;
        [SerializeField] LightWorkerEvent ResponseToLightWorkerEvent;
        void OnEnable() => GameEvent.Register(this);
        void OnDisable() => GameEvent.Unregister(this);
        public void OnEventTrigger(GameObject go) => ResponseToGameObjectEvent.Invoke(go);
        public void OnEventTrigger(LightWorker lw) => ResponseToLightWorkerEvent.Invoke(lw);
    }
}