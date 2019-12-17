using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TreeRoutine.Routine.BuildYourOwnRoutine.Extension.Default.Actions
{
    internal class UseVaalSkillActionFactory : ExtensionActionFactory
    {
        public UseVaalSkillActionFactory(string owner)
        {
            Owner = owner;
            Name = "UseVaalSkillActionFactory";
        }

        public override ExtensionAction GetAction()
        {
            return new UseVaalSkillAction(Owner, Name);
        }

        public override List<string> GetFilterTypes()
        {
            return new List<string> { ExtensionComponentFilterType.Input };
        }
    }
}
