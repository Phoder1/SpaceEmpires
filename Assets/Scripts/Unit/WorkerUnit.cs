using UniKit;

namespace Phoder1.SpaceEmpires
{
    public class WorkerUnit : Unit, ITurnTakeable
    {
        public override ITurnAction TakeAction(int turnNumber)
        {
            var nearestResource = board.FindNearest<Resource>(this);
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
