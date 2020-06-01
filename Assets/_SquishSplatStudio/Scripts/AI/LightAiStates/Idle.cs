/* SquishSplatStudio - https://discord.gg/HVjBM9T
 * You are welcome to use this file or contents for learning and/or non-commercial projects
 * Written by Dubh @Dubh#1508 <Discord> - 2020
 */

using UnityEngine;

namespace SquishSplatStudio
{
    public class Idle : State
    {
        public Idle(AiParameters aiParameters) : base(aiParameters) => name = AgentCommandType.Idle;

        public override void Enter()
        {
            _aiParameters.Point = _aiParameters.Npc.transform.position;
            base.Enter();
        }

        public override void Update()
        {
            if (CanSeeATarget())
            {
                _nextState = new Pursue(_aiParameters);
                _stage = EventType.Exit;
            }

            if (Vector3.Distance(_npc.transform.position, _aiParameters.Point) <= 5f)
            {
                GetAssignedWork();
                _stage = EventType.Exit;
            }
            else
            {
                _nextState = new Pursue(_aiParameters);
                _stage = EventType.Exit;
            }

            base.Update();
        }

        public override void Exit()
        {
            base.Exit();
        }
    }
}