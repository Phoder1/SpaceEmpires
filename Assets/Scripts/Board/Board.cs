using Phoder1.Core;
using Phoder1.Core.Types;
using System;
using UnityEngine;

namespace Phoder1.SpaceEmpires
{
    public interface IBoard
    {
        IReadonlyMap<Vector2Int, IEntity> Map { get; }

        bool TileOccupied(Vector2Int tilePos);
        Result CanMove(IEntity unit, Vector2Int to);
        Result TryMove(IEntity unit, Vector2Int to);
        Result ForceMove(IEntity unit, Vector2Int to, bool overrunOtherEntities = false);
        public Result Add(IEntity entity, Vector2Int position);
    }
    [Serializable]
    public class Board : IBoard
    {
        private readonly Map<Vector2Int, IEntity> map = new Map<Vector2Int, IEntity>();

        public IReadonlyMap<Vector2Int, IEntity> Map => throw new NotImplementedException();

        public Result Add(IEntity entity, Vector2Int position)
        {
            if (entity == null)
                throw new NullReferenceException();

            if (map.ContainsKey(entity))
                return Result.Failed($"Entity {entity.Transform.gameObject.name} already exists at position {map[entity]}!");

            map.Add(entity, position);
            return Result.Success();
        }

        public Result CanMove(IEntity entity, Vector2Int to)
            => (entity.BoardPosition.TileSteps(to, entity.CanMoveDiagonally) > 1)
            .Assert("Unit is too far!");

        public Result ForceMove(IEntity entity, Vector2Int to, bool overrunOtherEntities = false)
        {
            entity.AssertNull().Throw();

            if (!overrunOtherEntities && TileOccupied(to))
                return Result.Failed("Tile already occupied!");

            map.Remove(entity);
            map.Add(to, entity);

            return Result.Success();
        }

        public bool TileOccupied(Vector2Int tilePos)
            => Map.TryGetValue(tilePos, out var occupyingEntity) 
            && occupyingEntity != null;

        public Result TryMove(IEntity entity, Vector2Int to)
        {
            var result = CanMove(entity, to);
            if (!result)
                return result;

            return ForceMove(entity, to);
        }
    }
}