using System.Collections.Generic;
using TMPro;
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

        [SerializeField]
        private float maxSpawnScore;
        [SerializeField]
        private int maxSpawnScoreAmount;
        [SerializeField]
        private float aggressivness = 0;
        [SerializeField]
        private TextMeshProUGUI resourcesText;

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
        protected override IEnumerable<ActionOption> ActionOptions
        {
            get
            {
                foreach (var item in base.ActionOptions)
                    yield return item;

                yield return new ActionOption(SpawnWorker, WorkerScore);
                yield return new ActionOption(SpawnSoldier, SoldierScore);
            }
        }

        protected override void OnInit()
        {
            resources.Value = SpaceEmpiresSettings.InitialResources;
            resources.Subscribe((x) => resourcesText.text = x.ToString());
            ColonyColor = colonyColorPool.AddColony(this);
            base.OnInit();
        }

        private float SoldierScore()
        {
            if (GetFreeSpawnPosition() == null || resources.Value < SoldierCost)
                return -1;

            int enemiesCount = board.CountEntities<IUnit>((x) => x.Colony != Colony);
            return Mathf.Lerp(0, maxSpawnScore, (float)enemiesCount / (float)maxSpawnScoreAmount) + aggressivness;
        }
        private ITurnAction SpawnSoldier(int arg)
        {
            var spawnPos = GetFreeSpawnPosition();
            var prefab = SpaceEmpiresSettings.SoldierPrefab.GetComponent<Unit>();
            Instantiate(prefab, this, spawnPos.Value, container);
            resources.Value -= SoldierCost;
            return new TurnAction("Spawned Soldier");
        }
        private float WorkerScore()
        {
            if (GetFreeSpawnPosition() == null || resources.Value < WorkerCost)
                return -1;

            int resourcesCount = board.CountEntities<Resource>((x) => x.IsInteractable());
            return Mathf.Lerp(0, maxSpawnScore, (float)resourcesCount / (float)maxSpawnScoreAmount) - aggressivness;
        }

        private ITurnAction SpawnWorker(int arg)
        {
            var spawnPos = GetFreeSpawnPosition();
            var prefab = SpaceEmpiresSettings.WorkerPrefab.GetComponent<Unit>();
            Instantiate(prefab, this, spawnPos.Value, container);
            resources.Value -= WorkerCost;
            return new TurnAction("Spawned Worker");
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
