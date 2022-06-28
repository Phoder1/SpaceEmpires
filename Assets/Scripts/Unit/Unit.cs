using Zenject;
using UnityEngine;
using UniRx;
using System;
using UniKit.Attributes;
using System.Collections.Generic;
using Random = System.Random;
using UniKit;
using UniKit.Project;

namespace Phoder1.SpaceEmpires
{
    public interface IUnit : ITurnTakeable, IInteractable
    {
        IColony Colony { get; }
        bool CanMine { get; }
        int AttackDamage { get; }
    }
    public class Unit : Entity, IUnit
    {
        [SerializeField]
        private float speed = 1;
        [SerializeField]
        private bool canMine;
        [SerializeField]
        private int attackDamage = 0;
        [SerializeField]
        private SpriteRenderer activeSymbol;
        [SerializeField, Inline]
        protected ActionSelector actionSelector;

        [Inject]
        protected IEntityTurnsManager turnsManager;
        [Inject]
        protected Random random;

        private int KillReward => SpaceEmpiresSettings.KillReward;
        public float Speed => speed;
        public virtual IColony Colony { get; private set; }
        public bool CanMine => canMine;
        protected virtual IEnumerable<ActionOption> ActionOptions
        {
            get
            {
                yield return new ActionOption(WaitAction, () => 0);
            }
        }
        private static ITurnAction WaitAction(int arg) => new TurnAction("Wait");

        public int AttackDamage => attackDamage;

        public ITurnAction TakeAction(int turnNumber)
        {
            activeSymbol.enabled = true;
            return Action(turnNumber);
        }
        protected virtual ITurnAction Action(int turnNumber)
        {
            var action = actionSelector.SelectAction(ActionOptions, random);

            return action.Action(turnNumber);
        }
        public void StopAction() => activeSymbol.enabled = false;
        protected override void OnInit()
        {
            onRuinedDisposables.AddTo(gameObject);
            onRuinedDisposables.Add(turnsManager.Subscribe(this));
            onRuinedDisposables.Add(Colony.ColonyColor.Subscribe(UpdateColor));

            base.OnInit();
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


        public bool IsInteractable()
            => !Ruined.Value;

        public bool CanInteract(IUnit entity)
            => IsInteractable()
            && !entity.Ruined.Value
            && entity.AttackDamage > 0
            && entity.BoardPosition.IsNeighborOf(BoardPosition, entity.CanMoveDiagonally);

        public void Interact(IUnit entity)
        {
            if (!CanInteract(entity))
                return;

            HP.Value -= entity.AttackDamage;

            if (HP.Value <= 0)
            {
                entity.Colony.AddResources(KillReward);
                Ruine();
            }
        }
    }
}
