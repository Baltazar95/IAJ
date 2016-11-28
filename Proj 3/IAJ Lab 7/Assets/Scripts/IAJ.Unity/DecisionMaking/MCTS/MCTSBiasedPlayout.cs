﻿using Assets.Scripts.GameManager;
using System;
using System.Collections.Generic;
using Assets.Scripts.IAJ.Unity.DecisionMaking.GOB;
using UnityEngine;
using Assets.Scripts.IAJ.Unity.Utils;

namespace Assets.Scripts.IAJ.Unity.DecisionMaking.MCTS
{
    public class MCTSBiasedPlayout : MCTS
    {
        public MCTSBiasedPlayout(CurrentStateWorldModel currentStateWorldModel) : base(currentStateWorldModel)
        {
        }

        protected override Reward Playout(WorldModel initialPlayoutState)
        {
            GOB.Action action;
            GOB.Action[] actions;
            List<double> interval = new List<double>();
            double accumulate = 0;
            WorldModel current = initialPlayoutState;
            Reward reward = new Reward();
            double random;

            actions = current.GetExecutableActions();
            if(actions.Length == 0)
            {
                current = current.GenerateChildWorldModel();
                reward.Value = float.MinValue;
                reward.PlayerID = current.GetNextPlayer();
                return reward;
            }
            while (!current.IsTerminal())
            {
                accumulate = 0;
                interval.Clear();
                //if (actions.Length == 0)
                //    break;

                foreach (var a in actions)
                {
                    var child = current.GenerateChildWorldModel();

                    var h = Math.Pow(Math.E, child.CalculateDiscontentment(CurrentStateWorldModel.GetGameManager().autonomousCharacter.Goals));
                    accumulate += h;
                    interval.Add(accumulate);
                }
                //Debug.Log(accumulate);
                //Debug.Log(RandomGenerator.NextDouble());
                random = RandomGenerator.NextDouble() * accumulate;
                for (int j = 0; j < interval.Count; j++)
                {
                    //maybe it gets stuck here

                    if (random < interval[j])
                    {
                        action = actions[j];
                        current = current.GenerateChildWorldModel();
                        action.ApplyActionEffects(current);
                        current.CalculateNextPlayer();
                        break;
                        
                    }

                    if (j == interval.Count - 1)
                    {
                        current = current.GenerateChildWorldModel();
                        reward.Value = float.MinValue;
                        reward.PlayerID = current.GetNextPlayer();
                        return reward;
                    }
                }
            }

            
            reward.PlayerID = current.GetNextPlayer();
            reward.Value = current.GetScore();
            return reward;
        }

        protected override MCTSNode Expand(MCTSNode parent, GOB.Action action)
        {
            WorldModel state = parent.State.GenerateChildWorldModel();
            MCTSNode child = new MCTSNode(state);

            child.Parent = parent;
            action.ApplyActionEffects(state);
            state.CalculateNextPlayer();
           
           

            child.Action = action;
            parent.ChildNodes.Add(child);

            return child;
        }
    }
}
