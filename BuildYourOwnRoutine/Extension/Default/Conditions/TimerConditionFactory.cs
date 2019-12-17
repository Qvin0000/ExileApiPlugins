using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TreeRoutine.Routine.BuildYourOwnRoutine.Extension.Default.Conditions
{
    internal class TimerConditionFactory : ExtensionConditionFactory
    {
        public TimerConditionFactory(string owner)
        {
            Owner = owner;
            Name = "TimerConditionFactory";
        }

        public override ExtensionCondition GetCondition()
        {
            return new TimerCondition(Owner, Name);
        }

        public override List<string> GetFilterTypes()
        {
            return new List<string>() { ExtensionComponentFilterType.Input };
        }
    }
}
