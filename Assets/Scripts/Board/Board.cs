using DG.Tweening;
using System;
using System.Collections.Generic;
using UniKit;
using UniKit.Project;
using UniKit.Types;
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
        Result ForceMove(IEntity unit, Vector2Int to, bool overrunOtherEntities = false);
        Result Add(IEntity entity, Vector2Int position);
        Result Add(IEntity entity);
        TEntity FindNearest<TEntity>(IEntity from)
            where TEntity : class, IEntity;
        GridPathfindResult MoveAsCloseAsPossibleTo(IEntity entity, Vector2Int to);
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

        public Result Add(IEntity entity)
            => Add(entity, entity.Transform.position.WorldToGridPosition());
        public Result Add(IEntity entity, Vector2Int position)
        {
            if (entity == null)
                throw new NullReferenceException();

            if (map.ContainsKey(entity))
                return Result.Failed($"Entity {entity.Transform.gameObject.name} already exists at position {map[entity]}!");

            entity.Transform.position = position.GridToWorldPosition();
            map.Add(entity, position);
            return Result.Success();
        }

        public Result CanMove(IEntity entity, Vector2Int to)
            => (entity.BoardPosition.TileSteps(to, entity.CanMoveDiagonally) <= entity.MovementSpeed)
            .Assert("Unit is too far!");

        public Result ForceMove(IEntity entity, Vector2Int to, bool overrunOtherEntities = false)
        {
            entity.AssertNull($"{nameof(entity)} is null!").Throw();

            if (!overrunOtherEntities && TileOccupied(to))
                return Result.Failed("Tile already occupied!");

            map.Remove(entity);
            map.Add(to, entity);

            entity.Transform.DOMove(to.GridToWorldPosition(), (float)turns.TurnLength.TotalSeconds);

            return Result.Success();
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

        public GridPathfindResult MoveAsCloseAsPossibleTo(IEntity entity, Vector2Int to)
            => GetPath(entity, to);

        private GridPathfindResult GetPath(IEntity entity, Vector2Int to)
        {
            var settings = new GridPathfindingSettings(IsTileTraversable, entity.CanMoveDiagonally, false, ReachedTarget);
            return entity.BoardPosition.GetStepsToTarget(to, settings);
        }

        private bool ReachedTarget(Vector2Int targetPosition, Vector2Int currentPosition, GridPathfindingSettings settings)
            => currentPosition.IsNeighborOf(targetPosition, settings.canMoveDiagonaly);
        public TEntity FindNearest<TEntity>(IEntity from)
            where TEntity : class, IEntity
        {
            var pos = from.BoardPosition;
            var tiles = new List<KeyValuePair<Vector2Int, IEntity>>(Map);

            //Sort by distance
            tiles.Sort((a, b) => a.Key.TileSteps(pos, from.CanMoveDiagonally).CompareTo(b.Key.TileSteps(pos, from.CanMoveDiagonally)));

            foreach (var tile in tiles)
                if (tile.Value is TEntity target)
                    return target;

            return null;
        }
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(boardArea.center, (Vector2)boardArea.size);
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