using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TreeRoutine.Routine.BuildYourOwnRoutine.Extension.Default.Conditions
{
    internal class InTownConditionFactory : ExtensionConditionFactory
    {
        public InTownConditionFactory(string owner)
        {
            Owner = owner;
            Name = "InTownConditionFactory";
        }

        public override ExtensionCondition GetCondition()
        {

            return new SimpleCondition(Owner, Name, x => (() => x.Plugin.Cache.InTown));
        }

        public override List<string> GetFilterTypes()
        {
            return new List<string>() { ExtensionComponentFilterType.Player };
        }

    }
}
