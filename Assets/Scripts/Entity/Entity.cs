using UniKit.Attributes;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

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

        [Inject(Id = "HpBar")]
        private Image hpBar;
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

        private int initialHp;

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
            initialHp = hp.Value;
            var deathObservable = hp.Select((x) => x <= 0);
            var relativeHpObserv = hp.Select((x) => (float)x / (float)initialHp);

            relativeHpObserv.Subscribe(HpChanged);

            var boardSub = board.Add(this);
            if (boardSub && removeFromBoardOnRuined)
                boardSub.Value.AddTo(onRuinedDisposables);
        }

        private void HpChanged(float hpFill)
        {
            hpBar.gameObject.SetActive(hpFill < 1f);
            hpBar.fillAmount = hpFill;
        }

        protected virtual void Ruine()
        {
            if (ruined.Value)
                return;

            ruined.Value = true;

            MainSpriteRenderer.color = new Color(0.7f, 0.7f, 0.7f, 1f);
            onRuinedDisposables.Dispose();
        }
    }
}
