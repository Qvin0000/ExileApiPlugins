using TreeRoutine.Routine.BuildYourOwnRoutine.Trigger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TreeRoutine.Routine.BuildYourOwnRoutine.Profile
{
    public class LoadedProfile
    {
        public LoadedProfile()
        {

        }

        public String Name { get; set; } = "";
        public TriggerComposite Composite { get; set; }
    }
}
