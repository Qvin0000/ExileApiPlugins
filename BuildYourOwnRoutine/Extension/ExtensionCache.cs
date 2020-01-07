using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TreeRoutine.Routine.BuildYourOwnRoutine.Extension.Default;

namespace TreeRoutine.Routine.BuildYourOwnRoutine.Extension
{
    public class ExtensionCache
    {
        public ExtensionCache()
        {
            LoadedExtensions = new List<Extension>();
            ConditionList = new List<ExtensionConditionFactory>();
            ActionList = new List<ExtensionActionFactory>();
            Cache = new Dictionary<string, Dictionary<string, object>>();
            ActionFilterList = new HashSet<string>();
            ConditionFilterList = new HashSet<string>();

            LoadDefaultExtensions();
        }

        private void LoadDefaultExtensions()
        {
            // Load the default extension
            LoadedExtensions.Add(new DefaultExtension());
        }

        public List<Extension> LoadedExtensions { get; set; }

        public List<ExtensionConditionFactory> ConditionList { get; set; }
        public HashSet<string> ConditionFilterList { get; set; }

        public List<ExtensionActionFactory> ActionList { get; set; }
        public HashSet<string> ActionFilterList { get; set; }

        public Dictionary<string, Dictionary<string, object>> Cache { get; }
    }
}
