using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TreeRoutine.Menu;

namespace TreeRoutine.Routine.BuildYourOwnRoutine.Extension.Default.Conditions
{
    internal class PlayerMovingCondition : ExtensionCondition
    {
        private Int64 MsMoving { get; set; } = 0;
        private const String msMovingString = "msMoving";


        public PlayerMovingCondition(string owner, string name) : base(owner, name)
        {

        }

        public override void Initialise(Dictionary<String, Object> Parameters)
        {
            base.Initialise(Parameters);

            MsMoving = ExtensionComponent.InitialiseParameterInt32(msMovingString, (int)MsMoving, ref Parameters);
        }

        public override bool CreateConfigurationMenu(ExtensionParameter extensionParameter, ref Dictionary<String, Object> Parameters)
        {
            ImGui.TextDisabled("Condition Info");
            ImGuiExtension.ToolTipWithText("(?)", "This condition will return true if the player has been moving longer than the specified time.");

            base.CreateConfigurationMenu(extensionParameter, ref Parameters);

            MsMoving = ImGuiExtension.IntSlider("Time spent moving (ms)", (int)MsMoving, 0, 10000);
            ImGuiExtension.ToolTipWithText("(?)", "Player must remain moving for this configured number of milliseconds (1000ms = 1 sec) before this condition returns true");
            Parameters[msMovingString] = MsMoving.ToString();
            return true;
        }

        public override Func<bool> GetCondition(ExtensionParameter extensionParameter)
        {
            return () =>
            {
                if (!extensionParameter.Plugin.ExtensionCache.Cache.TryGetValue(Owner, out Dictionary<string, object> myCache))
                {
                    return false;
                }

                if (!myCache.TryGetValue(DefaultExtension.CacheStartedMoving, out object o))
                    return false;
                if (o is Int64)
                {
                    return ((Int64)o) >= MsMoving;
                }
                extensionParameter.Plugin.LogErr("The cached value " + DefaultExtension.CacheStartedMoving + " is not an int. Type: " + o.GetType(), 5);
                return false;
            };
        }

        public override string GetDisplayName(bool isAddingNew)
        {
            string displayName = "Player Moving";

            if (!isAddingNew)
            {
                displayName += " [";
                displayName += ("MsMoving=" + MsMoving.ToString());
                displayName += "]";

            }

            return displayName;
        }
    }
}
