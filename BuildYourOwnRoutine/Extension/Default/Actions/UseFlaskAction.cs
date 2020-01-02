using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TreeRoutine.DefaultBehaviors.Actions;
using TreeRoutine.Menu;
using TreeRoutine.TreeSharp;

namespace TreeRoutine.Routine.BuildYourOwnRoutine.Extension.Default.Actions
{
    internal class UseFlaskAction : ExtensionAction
    {
        private int FlaskIndex { get; set; } = 1;
        private const String flaskIndexString = "flaskIndex";

        public UseFlaskAction(string owner, string name) : base(owner, name)
        {

        }

        public override void Initialise(Dictionary<String, Object> Parameters)
        {
            FlaskIndex = ExtensionComponent.InitialiseParameterInt32(flaskIndexString, FlaskIndex, ref Parameters);
        }

        public override bool CreateConfigurationMenu(ExtensionParameter extensionParameter, ref Dictionary<String, Object> Parameters)
        {
            ImGui.TextDisabled("Action Info");
            ImGuiExtension.ToolTipWithText("(?)", "This action is used to use a specific flask.\nFlask Hotkey will be pulled from plugin settings.");

            FlaskIndex = ImGuiExtension.IntSlider("Flask Index", FlaskIndex, 1, 5);
            ImGuiExtension.ToolTipWithText("(?)", "Index for flask to be used (1= farthest left, 5 = farthest right)");
            Parameters[flaskIndexString] = FlaskIndex.ToString();
            return true;
        }

        public override Composite GetComposite(ExtensionParameter profileParameter)
        {
            return new UseHotkeyAction(profileParameter.Plugin.KeyboardHelper, x => profileParameter.Plugin.Settings.FlaskSettings[FlaskIndex - 1].Hotkey);
        }

        public override string GetDisplayName(bool isAddingNew)
        {
            string displayName = "Use Flask";

            if (!isAddingNew)
            {
                displayName += " [";
                displayName += ("FlaskIndex=" + FlaskIndex.ToString());
                displayName += "]";

            }

            return displayName;
        }
    }
}
