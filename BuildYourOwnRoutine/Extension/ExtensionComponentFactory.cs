using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TreeRoutine.Routine.BuildYourOwnRoutine.Extension
{
    public abstract class ExtensionComponentFactory
    {
        /// <summary>
        /// Extension that owns this component.
        /// </summary>
        public String Owner { get; protected set; }

        /// <summary>
        /// Unique name of this component.
        /// </summary>
        public String Name { get; protected set; }

        /// <summary>
        /// Returns the filter type(s) of this component.
        /// This is used for filtering components in the UI. Use ExtensionComponentFilterType for common types.
        /// </summary>
        /// <returns>List of filter types</returns>
        public virtual List<String> GetFilterTypes()
        {
            return new List<string>() { ExtensionComponentFilterType.General };
        }
    }
}
