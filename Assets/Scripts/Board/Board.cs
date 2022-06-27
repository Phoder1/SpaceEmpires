using DG.Tweening;
using System;
using System.Collections.Generic;
using UniKit;
using UniKit.Project;
using UniKit.Types;
using UniRx;
using UnityEngine;
using Zenject;
using static UniKit.AStar;

namespace Phoder1.SpaceEmpires
{
    public interface IBoard
    {
        /// <summary>
        /// The map contains all the unit's positions
        /// </summary>
        IReadonlyMap<Vector2Int, IEntity> Map { get; }
        float TileSize { get; }
        RectInt BoardArea { get; }
        bool TileOccupied(Vector2Int tilePos);
        Result CanMove(IEntity unit, Vector2Int to);
        Result TryMove(IEntity unit, Vector2Int to);
        Result<Tween> ForceMove(IEntity unit, Vector2Int to, bool overrunOtherEntities = false);
        Result<IDisposable> Add(IEntity entity, Vector2Int position);
        Result<IDisposable> Add(IEntity entity);
        TEntity FindNearest<TEntity>(IEntity from, Predicate<TEntity> predicate)
            where TEntity : class, IEntity;
        Result<GridPathfindResult> MoveAsCloseAsPossibleTo(IEntity entity, Vector2Int to);
        int CountEntities<T>(Predicate<T> predicate);
        Dictionary<TEntity, int> GetAllInRange<TEntity>(IEntity from, int range, Predicate<TEntity> predicate, bool usePathDistance = false)
            where TEntity : class, IEntity;
        GridPathfindResult GetPath(IEntity entity, Vector2Int to);
    }
    [Serializable]
    public class Board : MonoBehaviour, IBoard
    {
        [SerializeField]
        private RectInt boardArea = new RectInt(Vector2Int.zero, Vector2Int.one * 10);

        [Inject]
        private ITurns turns;

        private readonly Map<Vector2Int, IEntity> map = new Map<Vector2Int, IEntity>();
        private Lazy<float> tileSize = new Lazy<float>(() => ProjectPrefs.GetFloat("Tile size"));

        public IReadonlyMap<Vector2Int, IEntity> Map => map;

        public float TileSize => tileSize.Value;

        public RectInt BoardArea => boardArea;

        public Result<IDisposable> Add(IEntity entity)
            => Add(entity, entity.Transform.position.WorldToGridPosition());
        public Result<IDisposable> Add(IEntity entity, Vector2Int position)
        {
            if (entity == null)
                throw new NullReferenceException();

            if (map.ContainsKey(entity))
                return Result<IDisposable>.Failed($"Entity {entity.Transform.gameObject.name} already exists at position {map[entity]}!");

            entity.Transform.position = position.GridToWorldPosition();
            map.Add(entity, position);
            return Result<IDisposable>.Success(Disposable.Create(Remove));

            void Remove()
            {
                if (map.ContainsKey(entity))
                    map.Remove(entity);
            }
        }

        public Result CanMove(IEntity entity, Vector2Int to)
            => (entity.BoardPosition.TileSteps(to, entity.CanMoveDiagonally) <= entity.MovementSpeed)
            .Assert("Unit is too far!");

        public Result<Tween> ForceMove(IEntity entity, Vector2Int to, bool overrunOtherEntities = false)
            => ForceMove(entity, to, (float)turns.TurnLength.TotalSeconds, overrunOtherEntities);
        public Result<Tween> ForceMove(IEntity entity, Vector2Int to, float duration, bool overrunOtherEntities = false)
        {
            entity.AssertNull($"{nameof(entity)} is null!").Throw();

            if (!overrunOtherEntities && TileOccupied(to))
                return Result<Tween>.Failed("Tile already occupied!");

            map.Remove(entity);
            map.Add(to, entity);

            var tween = entity.Transform.DOMove(to.GridToWorldPosition(), duration);

            return Result.Success((Tween)tween);
        }

        public bool TileOccupied(Vector2Int tilePos)
            => Map.TryGetValue(tilePos, out var occupyingEntity)
            && occupyingEntity != null;
        public bool IsTileTraversable(Vector2Int tilePos)
        {
            if (TileOccupied(tilePos))
                return false;

            if (!boardArea.IsPointInside(tilePos))
                return false;

            return true;
        }
        public Result TryMove(IEntity entity, Vector2Int to)
        {
            var result = CanMove(entity, to);
            if (!result)
                return result;

            return ForceMove(entity, to);
        }

        public Result<GridPathfindResult> MoveAsCloseAsPossibleTo(IEntity entity, Vector2Int to)
        {
            var pathfindingResult = GetPath(entity, to);

            Sequence sequence = DOTween.Sequence();

            int stepsToMove = Mathf.Min(entity.MovementSpeed, pathfindingResult.PathLength);
            float stepDuration = (float)turns.TurnLength.TotalSeconds / stepsToMove;
            for (int i = 0; i < stepsToMove; i++)
            {
                var move = ForceMove(entity, pathfindingResult.Path[i], stepDuration, false);
                if (!move)
                    return move.WithValue(() => pathfindingResult);

                sequence.Append(move.Value);
            }

            return pathfindingResult;
        }

        public GridPathfindResult GetPath(IEntity entity, Vector2Int to)
        {
            var settings = new GridPathfindingSettings(IsTileTraversable, entity.CanMoveDiagonally, false, ReachedTarget);
            return entity.BoardPosition.GetStepsToTarget(to, settings);
        }

        private bool ReachedTarget(Vector2Int targetPosition, Vector2Int currentPosition, GridPathfindingSettings settings)
            => currentPosition.IsNeighborOf(targetPosition, settings.canMoveDiagonaly);
        public TEntity FindNearest<TEntity>(IEntity from, Predicate<TEntity> predicate)
            where TEntity : class, IEntity
        {
            var pos = from.BoardPosition;
            var tiles = new List<KeyValuePair<Vector2Int, IEntity>>(Map);

            //Sort by distance
            tiles.Sort((a, b) => a.Key.TileSteps(pos, from.CanMoveDiagonally).CompareTo(b.Key.TileSteps(pos, from.CanMoveDiagonally)));

            foreach (var tile in tiles)
                if (tile.Value is TEntity target)
                    if (predicate?.Invoke(target) ?? true)
                        return target;

            return null;
        }
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(boardArea.center, (Vector2)boardArea.size);
        }

        public int CountEntities<T>(Predicate<T> predicate = null)
        {
            var tiles = new List<KeyValuePair<Vector2Int, IEntity>>(Map);
            int count = 0;

            foreach (var tile in tiles)
            {
                if (tile.Value is T target
                    && (predicate == null || predicate.Invoke(target)))
                    count++;
            }

            return count;
        }

        public Dictionary<TEntity, int> GetAllInRange<TEntity>(IEntity from, int range, Predicate<TEntity> predicate, bool usePathDistance = false)
            where TEntity : class, IEntity
        {
            var entities = new Dictionary<TEntity, int>();

            foreach (var tile in Map)
            {
                if (tile.Value is TEntity target
                    && (predicate?.Invoke(target) ?? true))
                {
                    int distance;
                    if (usePathDistance)
                        distance = GetPath(from, target.BoardPosition).PathLength;
                    else
                        distance = from.BoardPosition.TileSteps(target.BoardPosition, from.CanMoveDiagonally);

                    if (distance < range)
                        entities.Add(target, distance);
                }
            }

            return entities;
        }
    }

    public static class BoardExt
    {
        public static float TileSize = ProjectPrefs.GetFloat("Tile size");
        public static Vector2 GridToWorldPosition(this Vector2Int gridPos)
            => (Vector2)gridPos * TileSize;
        public static Vector2Int WorldToGridPosition(this Vector3 worldPos)
            => ((Vector2)worldPos).WorldToGridPosition();
        public static Vector2Int WorldToGridPosition(this Vector2 worldPos)
            => Vector2Int.RoundToInt(worldPos / TileSize);
    }
}