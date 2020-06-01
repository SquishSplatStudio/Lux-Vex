/* SquishSplatStudio - https://discord.gg/HVjBM9T
 * You are welcome to use this file or contents for learning and/or non-commercial projects
 * Written by Dubh @Dubh#1508 <Discord> - 2020
 */
using UnityEngine;

namespace SquishSplatStudio
{
    public class Deposit : State
    {
        public Deposit(AiParameters aiParameters) : base(aiParameters) => name = AgentCommandType.Mine;

        GameObject _homeBase;

        public override void Enter()
        {

            if (_homeBase == null)
                _homeBase = GameObject.FindGameObjectWithTag("ControlCrystal");

            base._agent.speed = 5;

            base.Enter();
        }

        public override void Update()
        {
            if (_homeBase != null)
                _agent.Go(_homeBase.transform.position);

            if (Vector3.Distance(_aiParameters.Npc.transform.position, _homeBase.transform.position) <= 5f)
            {
                ResourceType.Crystal.IncreaseBy(5);

                _nextState = new Guard(_aiParameters);
                _stage = EventType.Exit;
            }

            if (CanSeeATarget())
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