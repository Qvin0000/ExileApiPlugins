using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TreeRoutine.Routine.BuildYourOwnRoutine.Extension
{
    public abstract class Extension
    {
        public Extension()
        {

        }

        public String Name { get; set; }

        /// <summary>
        /// Returns a list of available actions within this extension.
        /// </summary>
        /// <returns>List of ExtensionActionFactory objects</returns>
        public abstract List<ExtensionActionFactory> GetActions();

        /// <summary>
        /// Returns a list of available conditions within this extension.
        /// </summary>
        /// <returns>List of ExtensionConditionFactory objects</returns>
        public abstract List<ExtensionConditionFactory> GetConditions();

        /// <summary>
        /// Called before each tick of the behavior tree intended for caching purposes.
        /// </summary>
        /// <param name="extensionParameter">Extension parameter used to populate the cache</param>
        /// <param name="cache">Key of the dictionary should be a unique key for this extension. The value is a key value pair of cached values.</param>
        public abstract void UpdateCache(ExtensionParameter extensionParameter, Dictionary<string, Dictionary<string, object>> cache);
    }
}
