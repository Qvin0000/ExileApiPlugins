using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TreeRoutine.TreeSharp;

namespace TreeRoutine.Routine.BuildYourOwnRoutine.Extension
{
    public abstract class ExtensionAction : ExtensionComponent
    {
        public ExtensionAction()
        {
            Owner = "Owner not configured";
            Name = "Name not configured";
        }

        public ExtensionAction(string owner, string name)
        {
            Owner = owner;
            Name = name;
        }

        /// <summary>
        /// Returns the behavior tree composite for this action.
        /// This composite can be as simple or complex as necessary.
        /// </summary>
        /// <param name="profileParameter">A parameter containing everything needed for creating the composite</param>
        /// <returns>Composite tree to be run</returns>
        public abstract Composite GetComposite(ExtensionParameter profileParameter);
    }
}
