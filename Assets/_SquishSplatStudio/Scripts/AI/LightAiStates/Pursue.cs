/* SquishSplatStudio - https://discord.gg/HVjBM9T
 * You are welcome to use this file or contents for learning and/or non-commercial projects
 * Written by Dubh @Dubh#1508 <Discord> - 2020
 */

using UnityEngine;

namespace SquishSplatStudio
{
    public class Pursue : State
    {
        public Pursue(AiParameters aiParameters) : base(aiParameters)
        {
            name = AgentCommandType.Pursue;
            base._agent.speed = 5;
        }

        public override void Enter()
        {
            base.Enter();
        }

        public override void Update()
        {
            _agent.Go(_point);
            if (_aiParameters.Target)
            {
                if (CanAttackTarget())
                {
                    _nextState = new Attack(_aiParameters);
                    _stage = EventType.Exit;
                }
                else if (!CanSeeTarget() && (Vector3.Distance(_aiParameters.Npc.transform.position, _aiParameters.Point) >= 5f))
                {
                    _nextState = new Pursue(_aiParameters);
                    _stage = EventType.Exit;
                }
                else if (Vector3.Distance(_npc.transform.position, _point) <= 5f)
                {
                    _nextState = new Idle(_aiParameters);
                    _stage = EventType.Exit;
                }
            }
            else
            {
                if (CanSeeATarget())
                {
                    _nextState = new Pursue(_aiParameters);
                    _stage = EventType.Exit;
                }

                if (Vector3.Distance(_npc.transform.position, _point) <= 5f)
                {
                    //GetAssignedWork();
                    //_stage = EventType.Exit;

                    _nextState = new Idle(_aiParameters);
                    _stage = EventType.Exit;
                }
            }

            base.Update();
        }

        public override void Exit()
        {
            //_animator.ResetTrigger("isRunning");
            base.Exit();
        }
    }
}