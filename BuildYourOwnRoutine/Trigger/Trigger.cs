using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TreeRoutine.Routine.BuildYourOwnRoutine.Trigger
{
    public abstract class Trigger
    {
        public Trigger(string owner, string name)
        {
            this.Owner = owner;
            this.Name = name;
            this.Parameters = new Dictionary<String, Object>();
        }

        public Trigger(Trigger trigger)
        {
            this.Owner = trigger.Owner;
            this.Name = trigger.Name;

            // TODO: This is a very shallow copy. This should be a deep copy because value could be an object
            if (trigger.Parameters != null && trigger.Parameters.Any())
            {
                this.Parameters = trigger.Parameters.ToDictionary(entry => entry.Key, entry => entry.Value);
            }
        }

        // Which action to run, owner/name so we can have custom actions
        public string Owner { get; set; }
        public string Name { get; set; }


        public Dictionary<String, Object> Parameters;
    }
}
