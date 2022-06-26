using Zenject;
using UnityEngine;
using UniRx;
using System;

namespace Phoder1.SpaceEmpires
{
    public interface IUnit : ITurnTakeable
    {
        IColony Colony { get; }
        bool CanMine { get; }
        bool CanAttack { get; }
    }
    public class Unit : Entity, IUnit
    {
        [SerializeField]
        private float speed = 1;
        [SerializeField]
        private bool canMine;
        [SerializeField]
        private bool canAttack;

        [Inject]
        private IEntityTurnsManager turnsManager;


        public float Speed => speed;

        public virtual IColony Colony { get; private set; }

        public bool CanMine => canMine;

        public bool CanAttack => canAttack;

        public virtual ITurnAction TakeAction(int turnNumber)
        {
            return new TurnAction("Wait");
        }

        protected override void OnInit()
        {
            base.OnInit();

            turnsManager.Subscribe(this);
            Colony.ColonyColor.Subscribe(UpdateColor);
        }

        private void UpdateColor(Color obj)
        {
            MainSpriteRenderer.color = obj;
        }

        public static Unit Instantiate(Unit prefab, IColony colony, Vector2Int position, DiContainer container, Transform parent = null)
        {
            prefab.gameObject.SetActive(false);
            var newUnit = container.InstantiatePrefabForComponent<Unit>(prefab, parent);
            prefab.gameObject.SetActive(true);

            //Zenject insists on parenting the object to it's container transform if no parent has been set...
            newUnit.transform.SetParent(parent);

            newUnit.Colony = colony;
            newUnit.Transform.position = position.GridToWorldPosition();

            newUnit.gameObject.SetActive(true);
            newUnit.Init();
            return newUnit;
        }
    }
}
