using System;

namespace Phoder1.SpaceEmpires
{
    public interface ITurnTakeable : IEntity
    {
        float Speed { get; }
        ITurnAction TakeAction(int turnNumber);
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
}
