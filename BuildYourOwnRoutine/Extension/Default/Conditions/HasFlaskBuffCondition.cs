using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TreeRoutine.Menu;

namespace TreeRoutine.Routine.BuildYourOwnRoutine.Extension.Default.Conditions
{
    internal class HasFlaskBuffCondition : ExtensionCondition
    {
        private int FlaskIndex { get; set; } = 1;
        private const String flaskIndexString = "flaskIndex";

        public HasFlaskBuffCondition(string owner, string name) : base(owner, name)
        {

        }

        public override void Initialise(Dictionary<String, Object> Parameters)
        {
            base.Initialise(Parameters);

            FlaskIndex = ExtensionComponent.InitialiseParameterInt32(flaskIndexString, FlaskIndex, ref Parameters);
        }

        public override bool CreateConfigurationMenu(ExtensionParameter extensionParameter, ref Dictionary<String, Object> Parameters)
        {
            ImGui.TextDisabled("Condition Info");
            ImGuiExtension.ToolTipWithText("(?)", "This condition is used to determine if we have the flask buff up already.");

            base.CreateConfigurationMenu(extensionParameter, ref Parameters);

            FlaskIndex = ImGuiExtension.IntSlider("Flask Index", FlaskIndex, 1, 5);
            Parameters[flaskIndexString] = FlaskIndex.ToString();
            return true;
        }

        public override Func<bool> GetCondition(ExtensionParameter extensionParameter)
        {
            return () =>
            {
                var flaskInfo = extensionParameter.Plugin.FlaskHelper.GetFlaskInfo(FlaskIndex - 1);
                if (flaskInfo == null)
                    return false;
                return !extensionParameter.Plugin.PlayerHelper.playerDoesNotHaveAnyOfBuffs(new List<string> { flaskInfo.BuffString1, flaskInfo.BuffString2 });
            };
        }

        public override string GetDisplayName(bool isAddingNew)
        {
            string displayName = "Has Flask Buff";

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
