using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TreeRoutine.Routine.BuildYourOwnRoutine.Extension.Default.Conditions
{
    internal class NearbyMonstersConditionFactory : ExtensionConditionFactory
    {
        public NearbyMonstersConditionFactory(string owner)
        {
            Owner = owner;
            Name = "NearbyMonstersConditionFactory";
        }

        public override ExtensionCondition GetCondition()
        {
            return new NearbyMonstersCondition(Owner, Name);
        }

        public override List<string> GetFilterTypes()
        {
            return new List<string>() { ExtensionComponentFilterType.Monster };
        }
    }
}
