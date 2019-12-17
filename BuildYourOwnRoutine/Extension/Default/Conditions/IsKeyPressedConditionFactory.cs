using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TreeRoutine.Routine.BuildYourOwnRoutine.Extension.Default.Conditions
{
    internal class IsKeyPressedConditionFactory : ExtensionConditionFactory
    {
        public IsKeyPressedConditionFactory(string owner)
        {
            Owner = owner;
            Name = "IsKeyPressedConditionFactory";
        }

        public override ExtensionCondition GetCondition()
        {
            return new IsKeyPressedCondition(Owner, Name);
        }

        public override List<string> GetFilterTypes()
        {
            return new List<string>() { ExtensionComponentFilterType.Input };
        }
    }
}
