using System;
using UniRx;
using UnityEngine;
using Zenject;

namespace Phoder1.SpaceEmpires
{
    public interface IColony : IUnit
    {
        IReadOnlyReactiveProperty<Color> ColonyColor { get; }
    }
    public class ColonyUnit : Unit, IColony
    {
        [Inject]
        private ColonyColorPool colonyColorPool;
        public IReadOnlyReactiveProperty<Color> ColonyColor { get; private set; }
        public override IColony Colony => this;
        protected override void Awake()
        {
            ColonyColor = colonyColorPool.AddColony(this);

            base.Awake();
        }

        public override ITurnAction TakeAction(int turnNumber)
        {
            return new TurnAction("Wait");
        }
    }
}
