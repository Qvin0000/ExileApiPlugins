using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TreeRoutine.Routine.BuildYourOwnRoutine.Extension.Default.Conditions
{
    internal class HasFlaskBuffConditionFactory : ExtensionConditionFactory
    {
        public HasFlaskBuffConditionFactory(string owner)
        {
            Owner = owner;
            Name = "HasFlaskBuffConditionFactory";
        }

        public override ExtensionCondition GetCondition()
        {
            return new HasFlaskBuffCondition(Owner, Name);
        }

        public override List<string> GetFilterTypes()
        {
            return new List<string>() { ExtensionComponentFilterType.Flask };
        }
    }
}
