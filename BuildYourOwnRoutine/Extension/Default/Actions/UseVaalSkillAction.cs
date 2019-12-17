using ExileCore.PoEMemory.Components;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TreeRoutine.DefaultBehaviors.Actions;
using TreeRoutine.Menu;
using TreeRoutine.TreeSharp;

namespace TreeRoutine.Routine.BuildYourOwnRoutine.Extension.Default.Actions
{
    internal class UseVaalSkillAction : ExtensionAction
    {
        private bool useVaalHaste { get; set; } = false;
        private const String useHasteString = "useVaalHaste";

        private bool useVaalGrace { get; set; } = false;
        private const String useDodgeString = "useVaalGrace";

        private bool useVaalClarity { get; set; } = false;
        private const String useNoManaString = "useVaalClarity";

        private bool useVaalReave { get; set; } = false;
        private const String useAoeExtenderString = "useVaalReave";

        private int Key { get; set; }
            private const String keyString = "key";

        public UseVaalSkillAction(string owner, string name) : base(owner, name)
        {

        }

        public override void Initialise(Dictionary<String, Object> Parameters)
        {
            Key = ExtensionComponent.InitialiseParameterInt32(keyString, Key, ref Parameters);
            useVaalHaste = ExtensionComponent.InitialiseParameterBoolean(useHasteString, useVaalHaste, ref Parameters);
            useVaalGrace = ExtensionComponent.InitialiseParameterBoolean(useDodgeString, useVaalGrace, ref Parameters);
            useVaalClarity = ExtensionComponent.InitialiseParameterBoolean(useNoManaString, useVaalClarity, ref Parameters);
            useVaalReave = ExtensionComponent.InitialiseParameterBoolean(useAoeExtenderString, useVaalReave, ref Parameters);
        }

        public override bool CreateConfigurationMenu(ExtensionParameter extensionParameter, ref Dictionary<String, Object> Parameters)
        {
            ImGui.TextDisabled("Vaal Skills");

            ImGui.Spacing();
            ImGui.Separator();
            ImGui.Spacing();

            useVaalHaste = ImGuiExtension.Checkbox("Vaal Haste", useVaalHaste);
            Parameters[useHasteString] = useVaalHaste.ToString();
            ImGui.SameLine();

            useVaalGrace = ImGuiExtension.Checkbox("Vaal Grace", useVaalGrace);
            Parameters[useDodgeString] = useVaalGrace.ToString();
            ImGui.SameLine();

            useVaalClarity = ImGuiExtension.Checkbox("Vaal Clarity", useVaalClarity);
            Parameters[useNoManaString] = useVaalClarity.ToString();
            ImGui.SameLine();

            useVaalReave = ImGuiExtension.Checkbox("Vaal Reave", useVaalReave);
            Parameters[useAoeExtenderString] = useVaalReave.ToString();
            ImGui.SameLine();


            ImGui.Spacing();
            ImGui.Separator();
            ImGui.Spacing();
            ImGuiExtension.ToolTipWithText("(?)", "This action is used to configure Vaal Skills");
            Key = (int)ImGuiExtension.HotkeySelector("Hotkey", (Keys)Key);
            ImGuiExtension.ToolTipWithText("(?)", "Hotkey to press for the first Vaal Skill.");
            Parameters[keyString] = Key.ToString();
            return true;
        }

        public override Composite GetComposite(ExtensionParameter profileParameter)
        {
            return null;
            /*
            var playerBuff = profileParameter.Plugin.GameController.Game.IngameState.Data.LocalPlayer.GetComponent<Life>();
            var playerBuffs = profileParameter.Plugin.GameController.Game.IngameState.Data.LocalPlayer.GetComponent<Life>().Buffs;
            var player = profileParameter.Plugin.PlayerHelper;
            var localPlayer = profileParameter.Plugin.GameController.Game.IngameState.Data.LocalPlayer;
            var playeraccess = localPlayer.GetComponent<Actor>().ActorVaalSkills;

            #region Vaal Haste
            bool VaalHasteUseable = false;
            var VaalHasteUsable = playeraccess.Any(x => x.VaalSkillSkillName == "VaalHaste" && x.CurrVaalSouls >= x.VaalSoulsPerUse);

            if (VaalHasteUsable && !playerBuff.HasBuff("vaal_aura_speed"))
            {
                 VaalHasteUseable = true;
            }
            else
            { 
                 VaalHasteUseable = false;
            }
            #endregion

            #region Vaal Grace
            bool VaalGraceUseable = false;
            var VaalGraceUsable = playeraccess.Any(x => x.VaalSkillSkillName == "VaalGrace" && x.CurrVaalSouls >= x.VaalSoulsPerUse);

            if (VaalGraceUsable && !playerBuff.HasBuff("vaal_aura_dodge"))
            {
                VaalGraceUseable = true;
            }
            else
            {
                VaalGraceUseable = false;
            }
            #endregion

            #region Vaal Clarity
            bool VaalClarityUseable = false;
            var VaalClarityUsable = playeraccess.Any(x => x.VaalSkillSkillName == "VaalClarity" && x.CurrVaalSouls >= x.VaalSoulsPerUse);

            if (VaalClarityUsable && !playerBuff.HasBuff("vaal_aura_no_mana_cost"))
            {
                VaalClarityUseable = true;
            }
            else
            {
                VaalClarityUseable = false;
            }
            #endregion

            #region Vaal Reave
            bool VaalReaveUseable = false;
            var reavePlayerCount = playerBuffs.Exists(x => x.Name == "reave_counter" && x.Charges == 4);
            var VaalReaveUsable = playeraccess.Exists(x => x.VaalSkillSkillName == "VaalReave" && x.CurrVaalSouls >= x.VaalSoulsPerUse);

            if(reavePlayerCount)
            {
                profileParameter.Plugin.Log("WTF", 500);
            }


            if (VaalReaveUsable && reavePlayerCount)
            {
                VaalReaveUseable = true;
            }
            else
            {
                VaalReaveUseable = false;
            }
            #endregion


            return new PrioritySelector(
         new Decorator(x => ((VaalHasteUseable && useVaalHaste) || (VaalGraceUseable && useVaalGrace) || (VaalClarityUseable && useVaalClarity) ||
                             (VaalReaveUseable && useVaalReave)),
             new UseHotkeyAction(profileParameter.Plugin.KeyboardHelper, x => (Keys)Key)
        )); */
        }
                
        public override string GetDisplayName(bool isAddingNew)
        {
            string displayName = "Send Key Press";

            if (!isAddingNew)
            {
                displayName += " [";
                displayName += ("Key=" + Key.ToString());
                displayName += "]";

            }

            return displayName;
        }
    }
}
