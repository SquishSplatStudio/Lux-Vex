/* SquishSplatStudio - https://discord.gg/HVjBM9T
 * You are welcome to use this file or contents for learning and/or non-commercial projects
 * Written by Dubh @Dubh#1508 <Discord> - 2020
 */
using UnityEngine;

namespace SquishSplatStudio
{
    public class Explore : State
    {
        public Explore(AiParameters aiParameters) : base(aiParameters) => name = AgentCommandType.Explore;

        public override void Enter()
        {
            base.Enter();
        }

        public override void Update()
        {
            if (Vector3.Distance(_npc.transform.position, _aiParameters.Point) > 5f)
            {
                _nextState = new Pursue(_aiParameters);
                _stage = EventType.Exit;
            } 
            else
            {
                _nextState = new Guard(_aiParameters);
                _stage = EventType.Exit;
            }
        }

        public override void Exit()
        {
            base.Exit();
        }
    }

}