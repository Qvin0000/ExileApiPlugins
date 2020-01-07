using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TreeRoutine.Routine.BuildYourOwnRoutine.Extension
{
    public abstract class ExtensionActionFactory : ExtensionComponentFactory
    {
        /// <summary>
        /// Creates an instance of an action
        /// </summary>
        /// <returns>ExtensionAction instance</returns>
        public abstract ExtensionAction GetAction();
    }
}
