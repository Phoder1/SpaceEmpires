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
    [SelectionBase]
    public abstract class Entity : MonoBehaviour, IEntity
    {
        [SerializeField]
        private bool canMoveDiagonally = true;
        [SerializeField]
        private int movementSpeed = 5;

        [Inject]
        protected SpriteRenderer mainSprite;
        [Inject]
        protected IBoard board;

        bool _initialized = false;

        public Vector2Int BoardPosition => board.Map[this];
        public Transform Transform => transform;
        public bool CanMoveDiagonally => canMoveDiagonally;
        public int MovementSpeed => movementSpeed;
        public SpriteRenderer MainSpriteRenderer => mainSprite;

        void Awake()
        {
            Init();
        }

        protected void Init()
        {
            if (_initialized)
                return;

            _initialized = true;

            OnInit();
        }
        protected virtual void OnInit()
        {
            board.Add(this);
        }
    }
}
