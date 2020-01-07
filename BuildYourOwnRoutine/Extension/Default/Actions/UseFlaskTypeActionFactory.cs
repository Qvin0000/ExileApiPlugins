using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TreeRoutine.Routine.BuildYourOwnRoutine.Extension.Default.Actions
{
    internal class UseFlaskTypeActionFactory : ExtensionActionFactory
    {
        public UseFlaskTypeActionFactory(string owner)
        {
            Owner = owner;
            Name = "UseFlaskTypeActionFactory";
        }

        public override ExtensionAction GetAction()
        {
            return new UseFlaskTypeAction(Owner, Name);
        }

        public override List<string> GetFilterTypes()
        {
            return new List<string>() { ExtensionComponentFilterType.Flask };
        }
    }
}
