using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TreeRoutine.Routine.BuildYourOwnRoutine.Extension.Default.Conditions
{
    internal class CanUseFlaskConditionFactory : ExtensionConditionFactory
    {
        public CanUseFlaskConditionFactory(string owner)
        {
            Owner = owner;
            Name = "CanUseFlaskConditionFactory";
        }

        public override ExtensionCondition GetCondition()
        {
            return new CanUseFlaskCondition(Owner, Name);
        }

        public override List<string> GetFilterTypes()
        {
            return new List<string>() { ExtensionComponentFilterType.Flask };
        }
    }
}
