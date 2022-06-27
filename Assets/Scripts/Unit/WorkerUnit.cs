using System.Collections.Generic;
using UniKit;

namespace Phoder1.SpaceEmpires
{
    public class WorkerUnit : Unit
    {
        protected override IEnumerable<ActionOption> ActionOptions
        {
            get
            {
                foreach (var item in base.ActionOptions)
                    yield return item;


            }
        }
        protected override ITurnAction Action(int turnNumber)
        {
            var nearestResource = board.FindNearest<Resource>(this, (x) => x.IsInteractable());
            if (nearestResource == null)
                return Fallback();

            if (BoardPosition.IsNeighborOf(nearestResource.BoardPosition, CanMoveDiagonally))
                return Mine(nearestResource);

            board.MoveAsCloseAsPossibleTo(this, nearestResource.BoardPosition);
            return new TurnAction("Going to resource");
        }

        private ITurnAction Mine(Resource resource)
        {
            resource.Interact(this);
            return new TurnAction("Mining");
        }

        private ITurnAction Fallback()
        {
            board.MoveAsCloseAsPossibleTo(this, Colony.BoardPosition);
            return new TurnAction("Falling back");
        }
    }
}
