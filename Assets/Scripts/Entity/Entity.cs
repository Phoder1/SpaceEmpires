using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace Phoder1.SpaceEmpires
{
    public interface IEntity
    {
        Transform Transform { get; }
        Vector2Int BoardPosition { get; }
        bool CanMoveDiagonally { get; }
        int MovementSpeed { get; }
    }
    public class Entity : MonoBehaviour, IEntity
    {
        [SerializeField]
        private bool canMoveDiagonally;
        [SerializeField]
        private int movementSpeed;

        [Inject]
        IBoard board;

        public Vector2Int BoardPosition => board.Map[this];
        public Transform Transform => transform;
        public bool CanMoveDiagonally => canMoveDiagonally;
        public int MovementSpeed => movementSpeed;
    }
}
