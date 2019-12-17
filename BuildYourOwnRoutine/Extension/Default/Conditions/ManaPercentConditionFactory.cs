using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TreeRoutine.Routine.BuildYourOwnRoutine.Extension.Default.Conditions
{
    internal class ManaPercentConditionFactory : ExtensionConditionFactory
    {
        public ManaPercentConditionFactory(string owner)
        {
            Owner = owner;
            Name = "ManaPercentConditionFactory";
        }

        public override ExtensionCondition GetCondition()
        {
            return new ManaPercentCondition(Owner, Name);
        }

        public override List<string> GetFilterTypes()
        {
            return new List<string>() { ExtensionComponentFilterType.Player };
        }
    }
}
