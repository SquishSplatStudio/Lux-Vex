/* SquishSplatStudio - https://discord.gg/HVjBM9T
 * You are welcome to use this file or contents for learning and/or non-commercial projects
 * Written by Dubh @Dubh#1508 <Discord> - 2020
 */
using UnityEngine;

namespace SquishSplatStudio
{
    public class Build : State
    {
        public Build(AiParameters aiParameters) : base(aiParameters) => name = AgentCommandType.Build;

        BuildRequirements _workingBuild;

        float _actionInSeconds = 1f;
        float _lastAction;
        float wanderJitter = 1f;

        public override void Enter()
        {
            _workingBuild = CommonUtilities.ClosestBuild(_aiParameters.Npc.transform.position);
            base.Enter();
        }

        public override void Update()
        {
            if(!_aiParameters.LightWorker.HasWork())
            {
                _aiParameters.Point = _aiParameters.Npc.transform.position;
                _nextState = new Guard(_aiParameters);
                _stage = EventType.Exit;
            }
            else
            {
                if (Vector3.Distance(_npc.transform.position, _point) > 5f)
                {
                    //GetAssignedWork();
                    _nextState = new Pursue(_aiParameters);
                    _stage = EventType.Exit;
                }
                else if (_workingBuild != null && _workingBuild.canContribute)
                {
                    _lastAction += Time.deltaTime;
                    if (_lastAction <= _actionInSeconds)
                    {
                        base.Update();
                        return;
                    }
                    
                    _lastAction = 0f;
                    _workingBuild.AddToBuild();

                    var wanderTarget = _point;
                    wanderTarget += new Vector3(Random.Range(-1.0f, 1.0f) * wanderJitter, 0, Random.Range(-1.0f, 1.0f) * wanderJitter);
                    _agent.Go(wanderTarget);

                }
                else if (_workingBuild == null)
                {
                    _nextState = new Pursue(_aiParameters); // todo: monitor, should be guard?
                    _stage = EventType.Exit;
                } 
                else 
                {
                    _nextState = new Idle(_aiParameters);
                    _stage = EventType.Exit;
                }
            }
            base.Update();
        }

        public override void Exit()
        {
            base.Exit();
        }
    }

}