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
        SpriteRenderer MainSpriteRenderer { get; }
    }
    public abstract class Entity : MonoBehaviour, IEntity
    {
        [SerializeField]
        private bool canMoveDiagonally = true;
        [SerializeField]
        private int movementSpeed = 5;
        [SerializeField]
        private SpriteRenderer mainSprite;

        [Inject]
        IBoard board;

        public Vector2Int BoardPosition => board.Map[this];
        public Transform Transform => transform;
        public bool CanMoveDiagonally => canMoveDiagonally;
        public int MovementSpeed => movementSpeed;
        public SpriteRenderer MainSpriteRenderer => mainSprite;

        protected virtual void Awake()
        {
            board.Add(this);
        }
    }
}
