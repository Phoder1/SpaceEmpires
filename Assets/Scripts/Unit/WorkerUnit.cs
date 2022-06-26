using UnityEngine;
using Zenject;

namespace Phoder1.SpaceEmpires
{
    public class WorkerUnit : Unit, ITurnTakeable
    {
        public override ITurnAction TakeAction(int turnNumber)
        {
            board.TryMove(this, BoardPosition + Vector2Int.right);
            return new TurnAction("Move");
        }
    }
}
