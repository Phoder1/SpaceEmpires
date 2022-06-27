using System;
using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;

namespace Phoder1.SpaceEmpires
{
    public interface ITurnTakeable : IEntity
    {
        float Speed { get; }
        ITurnAction TakeAction(int turnNumber);
        void StopAction();
    }
    public interface ITurnAction
    {
        string ActionName { get; }
    }
    public readonly struct TurnAction : ITurnAction
    {
        public TurnAction(string actionName)
        {
            ActionName = actionName ?? throw new ArgumentNullException(nameof(actionName));
        }

        public string ActionName { get; }
    }
    public delegate ITurnAction TurnActionDeleg(int turnNumber);
    public delegate float ActionScoreDeleg();
    public readonly struct ActionOption
    {
        public readonly Func<int, ITurnAction> Action;
        public readonly Func<float> ActionScore;

        public ActionOption(Func<int, ITurnAction> action, Func<float> actionScore)
        {
            Action = action ?? throw new ArgumentNullException(nameof(action));
            ActionScore = actionScore ?? throw new ArgumentNullException(nameof(actionScore));
        }
    }

    [Serializable]
    public class ActionSelector
    {
        [SerializeField]
        private float maxScoreLenience = 4;

        public ActionOption SelectAction(List<ActionOption> actionOptions, Random random)
        {
            float scoreLenience = (float)(random.NextDouble() * maxScoreLenience);

            actionOptions.Sort((a, b) => -a.ActionScore().CompareTo(b.ActionScore()));
            var possibleActions = new List<ActionOption>(actionOptions);
            var topScore = actionOptions[0].ActionScore();
            possibleActions.RemoveAll((x) => x.ActionScore() < Mathf.Max(0, (topScore - scoreLenience)));

            int action = random.Next(possibleActions.Count);
            return possibleActions[action];
        }
    }
}
