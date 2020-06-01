/* SquishSplatStudio - https://discord.gg/HVjBM9T
 * You are welcome to use this file or contents for learning and/or non-commercial projects
 * Written by Dubh @Dubh#1508 <Discord> - 2020
 */

using System.Collections.Generic;
using UnityEngine;

namespace SquishSplatStudio
{
    [CreateAssetMenu(fileName = "newGameEvent", menuName = "Game Event", order = 52)]
    public class Event : ScriptableObject
    {
        List<EventListener> eventListeners = new List<EventListener>();
        public void Register(EventListener listener) => eventListeners.Add(listener);
        public void Unregister(EventListener listener) => eventListeners.Remove(listener);
        public void Trigger(GameObject go)
        {
            for (int i = 0; i < eventListeners.Count; i++)
                eventListeners[i].OnEventTrigger(go);
        }

        public void Trigger(LightWorker lw)
        {
            for (int i = 0; i < eventListeners.Count; i++)
                eventListeners[i].OnEventTrigger(lw);
        }
    }
}