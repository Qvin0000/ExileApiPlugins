using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TreeRoutine.Routine.BuildYourOwnRoutine.Trigger
{
    public class TriggerAction : Trigger
    {
        public TriggerAction() : this("", "")
        {

        }

        public TriggerAction(string owner, string name) : base(owner, name)
        {

        }

        public TriggerAction(TriggerAction action) : base(action)
        {

        }
    }
}
