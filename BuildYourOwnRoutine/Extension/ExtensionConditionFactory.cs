using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TreeRoutine.Routine.BuildYourOwnRoutine.Extension
{
    public abstract class ExtensionConditionFactory : ExtensionComponentFactory
    {
        public abstract ExtensionCondition GetCondition();
    }
}
