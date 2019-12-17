using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TreeRoutine.Routine.BuildYourOwnRoutine.Extension.Default.Actions
{
    internal class UseFlaskActionFactory : ExtensionActionFactory
    {
        public UseFlaskActionFactory(string owner)
        {
            Owner = owner;
            Name = "UseFlaskActionFactory";
        }

        public override ExtensionAction GetAction()
        {
            return new UseFlaskAction(Owner, Name);
        }

        public override List<string> GetFilterTypes()
        {
            return new List<string>() { ExtensionComponentFilterType.Flask };
        }
    }
}
