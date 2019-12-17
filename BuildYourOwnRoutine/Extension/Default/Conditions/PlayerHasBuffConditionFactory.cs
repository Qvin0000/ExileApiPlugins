using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TreeRoutine.Routine.BuildYourOwnRoutine.Extension.Default.Conditions
{
    internal class PlayerHasBuffConditionFactory : ExtensionConditionFactory
    {
        public PlayerHasBuffConditionFactory(string owner)
        {
            Owner = owner;
            Name = "PlayerHasBuffConditionFactory";
        }

        public override ExtensionCondition GetCondition()
        {
            return new PlayerHasBuffCondition(Owner, Name);
        }

        public override List<string> GetFilterTypes()
        {
            return new List<string>() { ExtensionComponentFilterType.Player };
        }
    }
}
