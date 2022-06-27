using UnityEngine;
using Zenject;
using UniRx;
using UniKit.Attributes;

namespace Phoder1.SpaceEmpires
{
    public interface IEntity
    {
        Transform Transform { get; }
        Vector2Int BoardPosition { get; }
        bool CanMoveDiagonally { get; }
        int MovementSpeed { get; }
        IReactiveProperty<int> HP { get; }
        SpriteRenderer MainSpriteRenderer { get; }
        IReadOnlyReactiveProperty<bool> Ruined { get; }
    }
    [SelectionBase]
    public class Entity : MonoBehaviour, IEntity
    {
        [SerializeField]
        private bool canMoveDiagonally = true;
        [SerializeField]
        private int movementSpeed = 5;
        [SerializeField]
        private bool removeFromBoardOnRuined = false;
        [SerializeField, Inline(true)]
        private ReactiveProperty<int> hp = new ReactiveProperty<int>(100);


        [Inject]
        protected SpriteRenderer mainSprite;
        [Inject]
        protected IBoard board;

        bool _initialized = false;
        private ReactiveProperty<bool> ruined = new ReactiveProperty<bool>(false);
        public Vector2Int BoardPosition => board.Map[this];
        public Transform Transform => transform;
        public bool CanMoveDiagonally => canMoveDiagonally;
        public int MovementSpeed => movementSpeed;
        public SpriteRenderer MainSpriteRenderer => mainSprite;
        public IReactiveProperty<int> HP => hp;
        public IReadOnlyReactiveProperty<bool> Ruined => ruined;


        protected CompositeDisposable onRuinedDisposables = new CompositeDisposable();

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
            var boardSub = board.Add(this);
            if (boardSub && removeFromBoardOnRuined)
                boardSub.Value.AddTo(onRuinedDisposables);
        }

        protected virtual void Ruine()
        {
            if (ruined.Value)
                return;

            ruined.Value = true;

            MainSpriteRenderer.color = Color.gray;
            onRuinedDisposables.Dispose();
        }
    }
}
