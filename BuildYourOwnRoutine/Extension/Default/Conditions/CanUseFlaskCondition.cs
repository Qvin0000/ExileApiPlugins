using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TreeRoutine.Menu;

namespace TreeRoutine.Routine.BuildYourOwnRoutine.Extension.Default.Conditions
{
    internal class CanUseFlaskCondition : ExtensionCondition
    {
        private int FlaskIndex { get; set; } = 1;
        private const String flaskIndexString = "flaskIndex";

        private int ReservedUses { get; set; } = 0;
        private const String reserveUsesString = "reserveUses";


        public CanUseFlaskCondition(string owner, string name) : base(owner, name)
        {

        }

        public override void Initialise(Dictionary<String, Object> Parameters)
        {
            base.Initialise(Parameters);

            FlaskIndex = ExtensionComponent.InitialiseParameterInt32(flaskIndexString, FlaskIndex, ref Parameters);
            ReservedUses = ExtensionComponent.InitialiseParameterInt32(reserveUsesString, ReservedUses, ref Parameters);
        }

        public override bool CreateConfigurationMenu(ExtensionParameter extensionParameter, ref Dictionary<String, Object> Parameters)
        {
            ImGui.TextDisabled("Condition Info");
            ImGuiExtension.ToolTipWithText("(?)", "This condition is used to determine if we can use a specific flask.\nIt will ensure that health/hybrid/mana potions are not used when we are at full health/mana.\nThis will also ensure that we do not use up reserved uses.");

            base.CreateConfigurationMenu(extensionParameter, ref Parameters);

            FlaskIndex = ImGuiExtension.IntSlider("Flask Index", FlaskIndex, 1, 5);
            Parameters[flaskIndexString] = FlaskIndex.ToString();
            ReservedUses = ImGuiExtension.IntSlider("Reserved Uses", ReservedUses, 0, 5);
            Parameters[reserveUsesString] = ReservedUses.ToString();
            return true;
        }

        public override Func<bool> GetCondition(ExtensionParameter extensionParameter)
        {
            return () => extensionParameter.Plugin.FlaskHelper.CanUsePotion(FlaskIndex - 1, ReservedUses);
        }

        public override string GetDisplayName(bool isAddingNew)
        {
            string displayName = "Can Use Flask";

            if (!isAddingNew)
            {
                displayName += " [";
                displayName += ("FlaskIndex=" + FlaskIndex.ToString());
                displayName += (",Reserved=" + ReservedUses.ToString());
                displayName += "]";

            }

            return displayName;
        }
    }
}
