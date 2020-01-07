using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TreeRoutine.Routine.BuildYourOwnRoutine.Extension.Default.Conditions
{
    internal class PlayerMovingConditionFactory : ExtensionConditionFactory
    {
        public PlayerMovingConditionFactory(string owner)
        {
            Owner = owner;
            Name = "PlayerMovingConditionFactory";
        }

        public override ExtensionCondition GetCondition()
        {
            return new PlayerMovingCondition(Owner, Name);
        }

        public override List<string> GetFilterTypes()
        {
            return new List<string>() { ExtensionComponentFilterType.Player };
        }
    }
}
