using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TreeRoutine.Routine.BuildYourOwnRoutine.Extension.Default.Actions
{
    internal class TimerActionFactory : ExtensionActionFactory
    {
        public TimerActionFactory(string owner)
        {
            Owner = owner;
            Name = "TimerActionFactory";
        }

        public override ExtensionAction GetAction()
        {
            return new TimerAction(Owner, Name);
        }

        public override List<string> GetFilterTypes()
        {
            return new List<string> { ExtensionComponentFilterType.Input };
        }
    }
}
