using System;
using System.Collections.Generic;
using UniKit;
using UniRx;
using UnityEngine;
using Zenject;

namespace Phoder1.SpaceEmpires
{
    public interface IEntityTurnsManager
    {
        ITurns Turns { get; }
        IReadOnlyList<ITurnTakeable> EntityTurns { get; }
        IDisposable Subscribe(ITurnTakeable turnTakeable);
        float GetEntityNextTurn(ITurnTakeable turnTakeable);
        void Unsubscribe(ITurnTakeable turnTakeable);

    }
    [Serializable]
    public class EntityTurnsManager : MonoBehaviour, IEntityTurnsManager
    {
        private readonly Dictionary<ITurnTakeable, int> lastTurnPlayed = new Dictionary<ITurnTakeable, int>();

        [Inject]
        private void Constrcut(ITurns turns)
        {
            Turns = turns;
            Turns.TurnNumber.Subscribe(TakeTurn);
            Turns.TurnEnded.Subscribe(TurnEnded);
        }

        private ITurnTakeable currentActive = null;
        private void TakeTurn(int turnNumber)
        {
            if (EntityTurns.Count == 0)
                return;

            SortEntities();

            for (int i = 0; i < EntityTurns.Count; i++)
            {
                var nextEntity = EntityTurns[i];

                if (nextEntity == null)
                {
                    entityTurns.RemoveAt(i);
                    i--;
                    continue;
                }

                PlayEntityTurn(turnNumber, nextEntity);
                break;
            }
        }

        private void PlayEntityTurn(int turnNumber, ITurnTakeable nextEntity)
        {
            var action = nextEntity.TakeAction(turnNumber);
            Debug.Log($"{nextEntity.Transform.gameObject.name} did {action.ActionName}");
            lastTurnPlayed[nextEntity] = turnNumber;
            currentActive = nextEntity;
        }
        private void TurnEnded(int turnNumber)
        {
            if (currentActive == null)
                return;

            currentActive.StopAction();
        }
        public ITurns Turns { get; private set; }

        private readonly List<ITurnTakeable> entityTurns = new List<ITurnTakeable>();
        public IReadOnlyList<ITurnTakeable> EntityTurns => entityTurns;

        public IDisposable Subscribe(ITurnTakeable turnTakeable)
        {
            entityTurns.Add(turnTakeable);
            SortEntities();

            return Disposable.Create(Unsubscribe);

            void Unsubscribe()
            {
                entityTurns.Remove(turnTakeable);
                SortEntities();
            }
        }

        private void SortEntities()
        {
            entityTurns.Sort((a, b) => GetEntityNextTurn(a).CompareTo(GetEntityNextTurn(b)));
        }

        public void Unsubscribe(ITurnTakeable turnTakeable)
        {
            entityTurns.RemoveAll((x) => x == turnTakeable || x == null);
        }

        public float GetEntityNextTurn(ITurnTakeable turnTakeable)
        {
            if (lastTurnPlayed.TryGetValue(turnTakeable, out var lastTurn))
                return lastTurn + turnTakeable.Speed;

            return turnTakeable.Speed;
        }

    }
}
