using Zenject;
using UnityEngine;
using UniRx;
using System;

namespace Phoder1.SpaceEmpires
{
    public interface IUnit : ITurnTakeable
    {
        IColony Colony { get; }
    }
    public abstract class Unit : Entity, IUnit
    {
        [SerializeField]
        private float speed = 1;

        [Inject]
        private IEntityTurnsManager turnsManager;

        public float Speed => speed;

        public virtual IColony Colony { get; private set; }

        public abstract ITurnAction TakeAction(int turnNumber);

        protected override void Awake()
        {
            base.Awake();

            turnsManager.Subscribe(this);
            Colony.ColonyColor.Subscribe(UpdateColor);
        }

        private void UpdateColor(Color obj)
        {
            MainSpriteRenderer.color = obj;
        }
    }
}
