using UnityEngine;

namespace Phoder1.SpaceEmpires
{
    public interface IUnitBehaviour
    {
        ITurnAction TakeAction(IUnit unit);
    }
    public class UnitBehaviour : MonoBehaviour, IUnitBehaviour
    {
        public virtual ITurnAction TakeAction(IUnit unit)
        {
            throw new System.NotImplementedException();
        }
    }
}
