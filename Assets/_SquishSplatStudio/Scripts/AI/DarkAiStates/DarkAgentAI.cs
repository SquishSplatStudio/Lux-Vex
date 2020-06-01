/* SquishSplatStudio - https://discord.gg/HVjBM9T
 * You are welcome to use this file or contents for learning and/or non-commercial projects
 * Written by Dubh @Dubh#1508 <Discord> - 2020
 */

using System.Collections.Generic;
using UnityEngine;

namespace SquishSplatStudio
{
    public class DarkAgentAI : MonoBehaviour
    {

        List<DarkAgent> _darkAgents = new List<DarkAgent>();

        [SerializeField] float timeBeforeOnslaught;
        [SerializeField] float OnslaughtIntervals;
        GameObject target;
        GameObject dark;

        void Start() => InvokeRepeating("Setup", timeBeforeOnslaught, OnslaughtIntervals);

        public void AgentSpawned(GameObject agent) => _darkAgents.Add(agent.GetComponent<DarkAgent>());

        GameObject GetRandomStructure()
        {
            if (ControlCrystal.Instance == null) return null;
            var everything = PlacementHandler.Instance.allLightLinks; // ControlCrystal.Instance.playerStructures;
            if (everything.Length == 0) return null; 
            GameObject handPicked = null;

            do
            {
                var i = Random.Range(0, everything.Length);
                var ps = everything[i];
                if (ps == null) continue;
                if (ps.gameObject.activeInHierarchy)
                    handPicked = everything[i].gameObject; // PlayerStructures
            } while (handPicked == null);
            return handPicked;
        }

        void CleanUp()
        {
            var num = new List<int>();
            for (int i = _darkAgents.Count-1; i > 0; i--)
            {
                if (_darkAgents[i] == null)
                    num.Add(i);
            }

            if (num.Count == 0) return;

            for (int i = 0; i > num.Count; i++)
                _darkAgents.RemoveAt(num[i]);
        }

        GameObject GetClosestDarkAgent(GameObject target)
        {
            DarkAgent dark = null;

            CleanUp();

            float distance = Mathf.Infinity;
            for (int i = 0; i < _darkAgents.Count; i++)
            {
                if (_darkAgents[i] == null || !_darkAgents[i].gameObject.activeInHierarchy) continue;
                var ai = _darkAgents[i].GetComponent<DarkAI>();
                if (ai == null) continue;
                if (ai._aiParameters.AssignedWork) continue;

                var dist = Vector3.Distance(_darkAgents[i].transform.position, target.transform.position);
                if (dist < distance)
                {
                    dark = _darkAgents[i];
                    distance = dist;
                }
            }
            return dark?.gameObject;
        }

        void IssueCommand()
        {
            if (!dark) return;
            var ai = dark.GetComponent<DarkAI>();
            if(target != null)
                ai.IssueCommand(AgentCommandType.Attack, target.transform);
        }

        void Setup()
        {
            target = GetRandomStructure();
            if (target == null) return;

            var mobCount = Random.Range(1, 6);
            for (int i = 0; i < mobCount; i++)
            {
                dark = GetClosestDarkAgent(target);
                IssueCommand();
            }
        }
    }
}
