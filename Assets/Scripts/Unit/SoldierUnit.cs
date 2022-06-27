using System;
using System.Collections.Generic;

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

                yield return new ActionOption(FinishOffUnit, FinishOffUnitScore);
                yield return new ActionOption(GroupUp, GroupUpScore);
                yield return new ActionOption(Attack, AttackScore);
            }
        }

        private float AttackScore()
        {
            throw new NotImplementedException();
        }

        private ITurnAction Attack(int arg)
        {
            throw new NotImplementedException();
        }

        private ITurnAction GroupUp(int arg)
        {
            throw new NotImplementedException();
        }

        private float GroupUpScore()
        {
            throw new NotImplementedException();
        }

        private float FinishOffUnitScore()
        {
            throw new NotImplementedException();
        }

        private ITurnAction FinishOffUnit(int arg)
        {
            throw new NotImplementedException();
        }

        //DoGroupingLogic
    }
}
