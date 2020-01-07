using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TreeRoutine.Routine.BuildYourOwnRoutine.Extension
{
    public class ExtensionParameter
    {
        public ExtensionParameter(BuildYourOwnRoutineCore plugin)
        {
            this.Plugin = plugin;
        }

        /// <summary>
        /// Instance of the plugin that you are running in. This should be used to access game information.
        /// </summary>
        public BuildYourOwnRoutineCore Plugin { get; set; }
    }
}
