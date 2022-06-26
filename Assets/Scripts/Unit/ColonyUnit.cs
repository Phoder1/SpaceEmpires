using UniKit;
using UniKit.Project;
using UniRx;
using UnityEngine;
using Zenject;

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
        [Inject]
        protected ColonyColorPool colonyColorPool;
        [Inject]
        protected DiContainer container;

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

        protected override void OnInit()
        {
            resources.Value = ProjectPrefs.GetInt("Initial resources");
            ColonyColor = colonyColorPool.AddColony(this);

            base.OnInit();
        }

        public override ITurnAction TakeAction(int turnNumber)
        {
            if (resources.Value > WorkerCost)
            {
                var spawnPos = GetFreeSpawnPosition();
                if (!spawnPos.HasValue)
                    return new TurnAction("Wait");

                var prefab = ProjectPrefs.GetGameObject("Worker prefab").GetComponent<Unit>();
                Instantiate(prefab, this, spawnPos.Value, container);

                resources.Value -= WorkerCost;

                return new TurnAction("Spawn");
            }
            return new TurnAction("Wait");
        }

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
