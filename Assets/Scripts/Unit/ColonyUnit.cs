using System;
using System.Collections.Generic;
using UniKit;
using UniKit.Attributes;
using UniKit.Project;
using UniRx;
using UnityEngine;
using Zenject;
using Random = System.Random;

namespace Phoder1.SpaceEmpires
{
    public interface IColony : IUnit
    {
        IReadOnlyReactiveProperty<Color> ColonyColor { get; }
        IReadOnlyReactiveProperty<int> Resources { get; }
        void AddResources(int amount);
    }

    public class ColonyUnit : Unit, IColony
    {
        private const int WorkerCost = 25;
        private const int SoldierCost = 25;

        [SerializeField, Inline]
        private ActionSelector actionSelector;

        [Inject]
        protected ColonyColorPool colonyColorPool;
        [Inject]
        protected DiContainer container;
        [Inject]
        private Random random;

        private ReactiveProperty<int> resources = new ReactiveProperty<int>();
        public IReadOnlyReactiveProperty<Color> ColonyColor { get; private set; }
        public override IColony Colony => this;

        private Vector2Int[] localSpawnPositions = new Vector2Int[]
        {
            Vector2Int.up,
            Vector2Int.down,
            Vector2Int.left,
            Vector2Int.right
        };

        public IReadOnlyReactiveProperty<int> Resources => resources;

        private List<ActionOption> actionOptions;

        protected override void OnInit()
        {
            resources.Value = ProjectPrefs.GetInt("Initial resources");
            ColonyColor = colonyColorPool.AddColony(this);

            actionOptions = new List<ActionOption>()
            {
                new ActionOption(WaitAction, () => 0),
                new ActionOption(SpawnWorker, WorkerScore),
                new ActionOption(SpawnSoldier, SoldierScore)
            };
            base.OnInit();
        }

        private float SoldierScore()
        {
            if (GetFreeSpawnPosition() == null || resources.Value < SoldierCost)
                return -1;

            return 2;
        }
        private ITurnAction SpawnSoldier(int arg)
        {
            throw new NotImplementedException();
        }


        protected override ITurnAction Action(int turnNumber)
        {
            var action = actionSelector.SelectAction(actionOptions, random);

            return action.Action(turnNumber);
        }
        private float WorkerScore()
        {
            if (GetFreeSpawnPosition() == null || resources.Value < WorkerCost)
                return -1;

            return 2;
        }

        private ITurnAction SpawnWorker(int arg)
        {
            var spawnPos = GetFreeSpawnPosition();
            var prefab = ProjectPrefs.GetGameObject("Worker prefab").GetComponent<Unit>();
            Instantiate(prefab, this, spawnPos.Value, container);
            resources.Value -= WorkerCost;
            return new TurnAction("Spawn");
        }

        private static ITurnAction WaitAction(int arg) => new TurnAction("Wait");



        private Vector2Int? GetFreeSpawnPosition()
        {
            localSpawnPositions.Shuffle();

            foreach (var localPos in localSpawnPositions)
            {
                var boardPos = BoardPosition + localPos;
                if (!board.Map.ContainsKey(boardPos))
                    return boardPos;
            }

            return null;
        }

        public void AddResources(int amount)
        {
            if (amount > 0)
                resources.Value += amount;
        }
    }
}
