using System;
using UniRx;
using UnityEngine;

namespace Phoder1.SpaceEmpires
{
    public interface ITurnsManager
    {
        IReactiveProperty<float> TurnsPerSecond { get; }
        IReadOnlyReactiveProperty<int> TurnCount { get; }
        public float TurnLength { get; }
    }
    [Serializable]
    public class TurnsManager : ITurnsManager
    {
        [SerializeField]
        private ReactiveProperty<float> turnsPerSecond;

        private readonly ReactiveProperty<int> turnCount = new ReactiveProperty<int>(0);

        public IReadOnlyReactiveProperty<int> TurnCount => turnCount;

        public IReactiveProperty<float> TurnsPerSecond => TurnsPerSecond;

        public float TurnLength { get; }
    }
}
