using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TreeRoutine.Routine.BuildYourOwnRoutine.Trigger
{
    public enum TriggerConditionType
    {
        And = 0,
        Or = 1
    }

    public class TriggerCondition : Trigger
    {
        public TriggerCondition() : this("", "")
        {

        }

        public TriggerCondition(String owner, String name) : base(owner, name)
        {

        }

        public TriggerCondition(TriggerCondition condition) : base(condition)
        {
            Linker = condition.Linker;
        }

        public TriggerConditionType? Linker { get; set; }
    }
}
