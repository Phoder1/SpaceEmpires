using System;
using System.Collections.Generic;
using System.Linq;
using UniKit;

namespace Phoder1.SpaceEmpires
{
    public class SoldierUnit : Unit
    {
        protected override IEnumerable<ActionOption> ActionOptions
        {
            get
            {
                foreach (var item in base.ActionOptions)
                    yield return item;

                //yield return new ActionOption(GroupUp, GroupUpScore);
                yield return new ActionOption(Attack, AttackScore);
            }
        }

        private float AttackScore()
        {
            Dictionary<IUnit, int> entitiesInRange = GetTargetsInRange();

            if (entitiesInRange.Any((x) => x.Key.HP.Value <= AttackDamage))
                return 1000;

            var target = GetNearestTarget();

            if (target == null)
                return -1;

            //Add danger level
            return 10;
        }

        private ITurnAction Attack(int arg)
        {
            //Attacks nearest lowest HP
            var targetsInRange = GetTargetsInRange();

            IUnit target;
            if (targetsInRange == null || targetsInRange.Count == 0)
                target = GetNearestTarget();
            else
                target = targetsInRange.GetMin((x) => x.Key.HP.Value, (a, b) => a.Value.CompareTo(b.Value)).Key;

            board.MoveAsCloseAsPossibleTo(this, target.BoardPosition);
            target.Interact(this);

            return new TurnAction($"Attacked {target}!");
        }

        private ITurnAction GroupUp(int arg)
        {
            throw new NotImplementedException();
        }

        private float GroupUpScore()
        {
            throw new NotImplementedException();
        }

        public Dictionary<IUnit, int> GetTargetsInRange()
            => board.GetAllInRange<IUnit>(this, MovementSpeed, IsTarget, true);
        public IUnit GetNearestTarget()
            => board.FindNearest<IUnit>(this, IsTarget);

        public bool IsTarget(IUnit unit)
            => unit.Colony != Colony
            && !unit.Ruined.Value;
    }
}
