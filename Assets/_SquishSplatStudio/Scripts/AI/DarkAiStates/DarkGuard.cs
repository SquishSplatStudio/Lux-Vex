/* SquishSplatStudio - https://discord.gg/HVjBM9T
 * You are welcome to use this file or contents for learning and/or non-commercial projects
 * Written by Dubh @Dubh#1508 <Discord> - 2020
 */

using UnityEngine;

namespace SquishSplatStudio
{
    public class DarkGuard : DarkState
    {
        public DarkGuard(DarkAiParameters aiParameters) : base(aiParameters) => name = AgentCommandType.Guard;

        float _actionInSeconds = .75f;
        float _lastAction;
        float wanderJitter = 1f;
        float _optimizer;

        public override void Enter()
        {
            _optimizer = GameHandler.Instance.TickRate;
            base.Enter();
        }

        public override void Update()
        {

            if (_optimizer > 0f)
            {
                _optimizer -= Time.deltaTime;
                return;
            }

            _optimizer = GameHandler.Instance.TickRate;

            if (_aiParameters.Npc.gameObject.activeInHierarchy)
            {
                _lastAction += Time.deltaTime;
                if (_lastAction > _actionInSeconds)
                {
                    _lastAction = 0f;
                    var wanderTarget = _aiParameters.Point;
                    wanderTarget += new Vector3(Random.Range(-1.0f, 1.0f) * wanderJitter, 0, Random.Range(-1.0f, 1.0f) * wanderJitter);
                    _aiParameters.Agent.Go(wanderTarget);
                }

                if (CanSeeATarget())
                {
                    _nextState = new DarkPursue(_aiParameters);
                    _stage = EventType.Exit;
                }
            }
            
            base.Update();
        }

        public override void Exit() => base.Exit();
    }
}