using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TreeRoutine.Routine.BuildYourOwnRoutine.Extension.Default.Actions
{
    internal class SendKeyActionFactory : ExtensionActionFactory
    {
        public SendKeyActionFactory(string owner)
        {
            Owner = owner;
            Name = "SendKeyActionFactory";
        }

        public override ExtensionAction GetAction()
        {
            return new SendKeyAction(Owner, Name);
        }

        public override List<string> GetFilterTypes()
        {
            return new List<string> { ExtensionComponentFilterType.Input };
        }
    }
}
