using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TreeRoutine.Menu;

namespace TreeRoutine.Routine.BuildYourOwnRoutine.Extension.Default.Conditions
{
    internal class SimpleCondition : ExtensionCondition
    {
        private Func<ExtensionParameter, Func<bool>> Condition { get; set; }

        public SimpleCondition(string owner, string name, Func<ExtensionParameter, Func<bool>> condition) : base(owner, name)
        {
            this.Condition = condition;
        }

        public override Func<bool> GetCondition(ExtensionParameter extensionParameter)
        {
            return Condition(extensionParameter);
        }

        public override string GetDisplayName(bool isAddingNew)
        {
            return Name;
        }
    }
}
