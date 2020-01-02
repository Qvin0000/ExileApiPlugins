using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TreeRoutine.Routine.BuildYourOwnRoutine.Extension.Default.Conditions
{
    internal class LoweredResistanceConditionFactory : ExtensionConditionFactory
    {
        public LoweredResistanceConditionFactory(string owner)
        {
            Owner = owner;
            Name = "LowerResistanceConditionFactory";
        }

        public override ExtensionCondition GetCondition()
        {
            return new LoweredResistanceCondition(Owner, Name);
        }

        public override List<string> GetFilterTypes()
        {
            return new List<string>() { ExtensionComponentFilterType.Player };
        }
    }
}
