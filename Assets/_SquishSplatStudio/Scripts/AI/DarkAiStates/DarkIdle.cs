/* SquishSplatStudio - https://discord.gg/HVjBM9T
 * You are welcome to use this file or contents for learning and/or non-commercial projects
 * Written by Dubh @Dubh#1508 <Discord> - 2020
 */

using UnityEngine;

namespace SquishSplatStudio
{
    public class DarkIdle : DarkState
    {
        public DarkIdle(DarkAiParameters aiParameters) : base(aiParameters) => name = AgentCommandType.Idle;

        public override void Enter() => base.Enter();

        public override void Update()
        {

            if (CanSeeATarget())
            {
                _nextState = new DarkPursue(_aiParameters);
                _stage = EventType.Exit;
            }
            else
            {
                // find a place random place and move there...
                Vector3 pos;
                while (GameEnvironment.Instance.InCrystalRadius(pos = GameEnvironment.Instance.GenerateRandomLocation())) { }

                _aiParameters.Point = pos;
                _nextState = new DarkPursue(_aiParameters);
                _stage = EventType.Exit;
            }

            base.Update();
        }

        public override void Exit() => base.Exit();
    }
}