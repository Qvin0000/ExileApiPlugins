using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TreeRoutine.Routine.BuildYourOwnRoutine.Trigger
{
    public class TriggerComposite
    {
        public TriggerComposite()
        {
            ConditionList = new List<TriggerCondition>();
            Children = new List<TriggerComposite>();
        }

        public TriggerComposite(TriggerComposite trigger) : this()
        {
            this.Name = trigger.Name;
            this.Type = trigger.Type;
            // TODO: This is only a shallow copy. It should be a deep copy.
            if (trigger.Children != null && trigger.Children.Count > 0)
            {
                trigger.Children.ForEach(x => this.Children.Add(x));
            }

            if (trigger.ConditionList != null && trigger.ConditionList.Count > 0)
            {
                trigger.ConditionList.ForEach(x => this.ConditionList.Add(x));
            }

            if (trigger.Action != null)
            {
                this.Action = new TriggerAction(trigger.Action);
            }
        }

        public String Name { get; set; } = "";

        // What type of composite are we? (Decorator, Sequence, PrioritySelector, Action)
        public TriggerType Type { get; set; }

        // Always return RunStatus.Success, even if children return RunStatus.Failure
        // Only applies to Decorators and Actions
        public bool AlwaysContinue { get; set; }

        // Condition/Delegate for running the composite
        // -- Should be able to be a list of conditions (e.g. hp below, mana below) with ability to and/or
        public List<TriggerCondition> ConditionList { get; set; } = new List<TriggerCondition>();


        // List of TriggerConfig (for children, Decorator MUST have only 1)
        public List<TriggerComposite> Children { get; set; } = new List<TriggerComposite>();

        // Action to perform (ACTION ONLY) [Store by owner/name so we can load external actions?]
        public TriggerAction Action { get; set; }
    }
}
