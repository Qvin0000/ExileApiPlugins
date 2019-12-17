using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TreeRoutine.Menu;

namespace TreeRoutine.Routine.BuildYourOwnRoutine.Extension.Default.Conditions
{
    internal class HealthPercentCondition : ExtensionCondition
    {
        private Boolean IsAbove { get; set; }
        private String IsAboveString = "IsAbove";

        private int Percentage { get; set; }
        private String PercentageString = "Percentage";

        public HealthPercentCondition(string owner, string name) : base(owner, name)
        {
            Percentage = 50;
            IsAbove = false;
        }

        public override void Initialise(Dictionary<String, Object> Parameters)
        {
            base.Initialise(Parameters);

            IsAbove = ExtensionComponent.InitialiseParameterBoolean(IsAboveString, IsAbove, ref Parameters);
            Percentage = ExtensionComponent.InitialiseParameterInt32(PercentageString, Percentage, ref Parameters);
        }

        public override bool CreateConfigurationMenu(ExtensionParameter extensionParameter, ref Dictionary<String, Object> Parameters)
        {
            ImGui.TextDisabled("Condition Info");
            ImGuiExtension.ToolTipWithText("(?)", "This condition will return true if the player's health percentage is above/below the specified amount.");

            base.CreateConfigurationMenu(extensionParameter, ref Parameters);

            int radioTarget = IsAbove ? 0 : 1;
            ImGui.RadioButton("Above Percentage", ref radioTarget, 0);
            ImGui.RadioButton("Below Percentage", ref radioTarget, 1);

            IsAbove = radioTarget == 0;
            Parameters[IsAboveString] = IsAbove.ToString();

            Percentage = ImGuiExtension.IntSlider("Health Percentage", Percentage, 1, 100);
            Parameters[PercentageString] = Percentage.ToString();

            return true;
        }

        public override Func<bool> GetCondition(ExtensionParameter extensionParameter)
        {
            return () => !extensionParameter.Plugin.PlayerHelper.isHealthBelowPercentage(Percentage) == IsAbove;
        }

        public override string GetDisplayName(bool isAddingNew)
        {
            string displayName = "Health Percentage";

            if (!isAddingNew)
            {
                displayName += " [";
                if (IsAbove) displayName += ("Above ");
                else displayName += ("Below ");
                displayName += ("Percentage=" + Percentage.ToString());
                displayName += "]";

            }

            return displayName;
        }
    }
}
