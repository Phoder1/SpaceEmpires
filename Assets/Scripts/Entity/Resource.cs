using UniKit;
using UnityEngine;

namespace Phoder1.SpaceEmpires
{
    public interface IInteractable
    {
        bool CanInteract(IUnit entity);
        void Interact(IUnit entity);
    }
    public class Resource : Entity, IInteractable
    {
        [SerializeField]
        private int resourcesValue = 25;
        public bool CanInteract(IUnit unit)
            => unit.CanMine
            && unit.BoardPosition.IsNeighborOf(BoardPosition, true);

        public void Interact(IUnit unit)
        {
            unit.Colony.AddResources(resourcesValue);
        }
    }
}
