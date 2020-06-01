/* SquishSplatStudio - https://discord.gg/HVjBM9T
 * You are welcome to use this file or contents for learning and/or non-commercial projects
 * Written by Dubh @Dubh#1508 <Discord> - 2020
 */

using UnityEngine;

namespace SquishSplatStudio
{
    public class DarkPursue : DarkState
    {

        float _optimizer;
        public DarkPursue(DarkAiParameters aiParameters) : base(aiParameters)
        {
            name = AgentCommandType.Pursue;
            base._aiParameters.Agent.speed = 5;
        }

        public override void Enter()
        {
            _optimizer = GameHandler.Instance.TickRate * .5f;
            _aiParameters.Agent.Freeze();
            base.Enter();
        }

        public override void Update()
        {

            if (_optimizer > 0f)
            {
                _optimizer -= Time.deltaTime;
                return;
            }

            _optimizer = GameHandler.Instance.TickRate * .5f;

            _aiParameters.Agent.Go(_aiParameters.Point);

            

            if (_target)
            {
                _aiParameters.Npc.transform.LookAt(_target);

                if (CanAttackTarget())
                {
                    _nextState = new DarkAttack(_aiParameters);
                    _stage = EventType.Exit;
                }
            } 
            else if (CanSeeATarget())
            {
                _nextState = new DarkPursue(_aiParameters);
                _stage = EventType.Exit;
            }

            if (Vector3.Distance(_aiParameters.Npc.transform.position, _aiParameters.Point) <= 5f)
            {
                GetAssignedWork();
                _stage = EventType.Exit;
            }
            base.Update();
        }

        public override void Exit() => base.Exit();

    }
}