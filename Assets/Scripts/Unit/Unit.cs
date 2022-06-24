using Zenject;
using UnityEngine;

namespace Phoder1.SpaceEmpires
{
    public abstract class Unit : Entity, ITurnTakeable
    {
        [SerializeField]
        private float speed = 1;

        [Inject]
        private IEntityTurnsManager turnsManager;

        public float Speed => speed;

        public abstract ITurnAction TakeAction(int turnNumber);

        protected override void Awake()
        {
            base.Awake();

            turnsManager.Subscribe(this);
        }
    }
}
