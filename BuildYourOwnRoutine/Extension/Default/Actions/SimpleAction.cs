using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TreeRoutine.TreeSharp;

namespace TreeRoutine.Routine.BuildYourOwnRoutine.Extension.Default.Actions
{
    class SimpleAction : ExtensionAction
    {
        private Func<ExtensionParameter, ActionSucceedDelegate> Action { get; set; }

        public SimpleAction(string owner, string name, Func<ExtensionParameter, ActionSucceedDelegate> action) : base(owner, name)
        {
            this.Action = action;
        }

        public override bool CreateConfigurationMenu(ExtensionParameter extensionParameter, ref Dictionary<string, object> Parameters)
        {
            return true;
        }

        public override Composite GetComposite(ExtensionParameter profileParameter)
        {
            return new TreeSharp.Action(Action(profileParameter));
        }

        public override void Initialise(Dictionary<string, object> Parameters)
        {
            
        }

        public override string GetDisplayName(bool isAddingNew)
        {
            return Name;
        }
    }
}
