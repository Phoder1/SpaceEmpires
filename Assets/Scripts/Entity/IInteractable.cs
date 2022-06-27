namespace Phoder1.SpaceEmpires
{
    public interface IInteractable
    {
        bool IsInteractable();
        bool CanInteract(IUnit entity);
        void Interact(IUnit entity);
    }
}
