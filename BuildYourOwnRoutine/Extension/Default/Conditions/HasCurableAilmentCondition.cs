using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ExileCore.PoEMemory.Components;
using TreeRoutine.Menu;

namespace TreeRoutine.Routine.BuildYourOwnRoutine.Extension.Default.Conditions
{
    internal class HasCurableAilmentCondition : ExtensionCondition
    {
        public bool RemFrozen { get; set; } = false;
        public string RemFrozenString { get; set; } = "RemFrozen";

        public bool RemBurning { get; set; } = false;
        public string RemBurningString { get; set; } = "RemBurning";

        public bool RemShocked { get; set; } = false;
        public string RemShockedString { get; set; } = "RemShocked";

        public bool RemCurse { get; set; } = false;
        public string RemCurseString { get; set; } = "RemCurse";

        public bool RemPoison { get; set; } = false;
        public string RemPoisonString { get; set; } = "RemPoison";

        public bool RemBleed { get; set; } = false;
        public string RemBleedString { get; set; } = "RemBleed";

        public int CorruptCount { get; set; } = 0;
        public string CorruptCountString { get; set; } = "CorruptCount";

        public bool IgnoreInfiniteTimer { get; set; } = false;
        public string IgnoreInfiniteTimerString { get; set; } = "IgnoreInfiniteTimer";



        public HasCurableAilmentCondition(string owner, string name) : base(owner, name)
        {

        }

        public override void Initialise(Dictionary<String, Object> Parameters)
        {
            base.Initialise(Parameters);

            RemFrozen = ExtensionComponent.InitialiseParameterBoolean(RemFrozenString, RemFrozen, ref Parameters);
            RemBurning = ExtensionComponent.InitialiseParameterBoolean(RemBurningString, RemBurning, ref Parameters);
            RemShocked = ExtensionComponent.InitialiseParameterBoolean(RemShockedString, RemShocked, ref Parameters);
            RemCurse = ExtensionComponent.InitialiseParameterBoolean(RemCurseString, RemCurse, ref Parameters);
            RemPoison = ExtensionComponent.InitialiseParameterBoolean(RemPoisonString, RemPoison, ref Parameters);
            RemBleed = ExtensionComponent.InitialiseParameterBoolean(RemBleedString, RemBleed, ref Parameters);
            CorruptCount = ExtensionComponent.InitialiseParameterInt32(CorruptCountString, CorruptCount, ref Parameters);
            IgnoreInfiniteTimer = ExtensionComponent.InitialiseParameterBoolean(IgnoreInfiniteTimerString, IgnoreInfiniteTimer, ref Parameters);
        }

        public override bool CreateConfigurationMenu(ExtensionParameter extensionParameter, ref Dictionary<String, Object> Parameters)
        {
            ImGui.TextDisabled("Condition Info");
            ImGuiExtension.ToolTipWithText("(?)", "This condition will return true if the player has any of the selected ailments or a minimum of the specified corrupted blood stacks.");

            base.CreateConfigurationMenu(extensionParameter, ref Parameters);

            RemFrozen = ImGuiExtension.Checkbox("Frozen", RemFrozen);
            Parameters[RemFrozenString] = RemFrozen.ToString();

            RemBurning = ImGuiExtension.Checkbox("Burning", RemBurning);
            Parameters[RemBurningString] = RemBurning.ToString();

            RemShocked = ImGuiExtension.Checkbox("Shocked", RemShocked);
            Parameters[RemShockedString] = RemShocked.ToString();

            RemCurse = ImGuiExtension.Checkbox("Curse", RemCurse);
            Parameters[RemCurseString] = RemCurse.ToString();

            RemPoison = ImGuiExtension.Checkbox("Poison", RemPoison);
            Parameters[RemPoisonString] = RemPoison.ToString();

            RemBleed = ImGuiExtension.Checkbox("Bleed", RemBleed);
            Parameters[RemBleedString] = RemBleed.ToString();

            CorruptCount = ImGuiExtension.IntSlider("Corruption Count", CorruptCount, 0, 20);
            Parameters[CorruptCountString] = CorruptCount.ToString();

            IgnoreInfiniteTimer = ImGuiExtension.Checkbox("Ignore Infinite Timer", IgnoreInfiniteTimer);
            Parameters[IgnoreInfiniteTimerString] = IgnoreInfiniteTimer.ToString();
            return true;
        }

        public override Func<bool> GetCondition(ExtensionParameter extensionParameter)
        {
            return () =>
            {
                if (RemFrozen && hasAilment(extensionParameter, extensionParameter.Plugin.Cache.DebuffPanelConfig.Frozen))
                    return true;
                if (RemBurning && hasAilment(extensionParameter, extensionParameter.Plugin.Cache.DebuffPanelConfig.Burning))
                    return true;
                if (RemShocked && hasAilment(extensionParameter, extensionParameter.Plugin.Cache.DebuffPanelConfig.Shocked))
                    return true;
                if (RemCurse && hasAilment(extensionParameter, extensionParameter.Plugin.Cache.DebuffPanelConfig.WeakenedSlowed))
                    return true;
                if (RemPoison && hasAilment(extensionParameter, extensionParameter.Plugin.Cache.DebuffPanelConfig.Poisoned))
                    return true;
                if (RemBleed && hasAilment(extensionParameter, extensionParameter.Plugin.Cache.DebuffPanelConfig.Bleeding))
                    return true;
                if (CorruptCount > 0 && hasAilment(extensionParameter, extensionParameter.Plugin.Cache.DebuffPanelConfig.Corruption, () => CorruptCount))
                    return true;

                return false;
            };
        }

        private bool hasAilment(ExtensionParameter profileParameter, Dictionary<string, int> dictionary, Func<int> minCharges = null)
        {
            var buffs = profileParameter.Plugin.GameController.Game.IngameState.Data.LocalPlayer.GetComponent<Life>().Buffs;
            foreach (var buff in buffs)
            {
                if (!IgnoreInfiniteTimer && float.IsInfinity(buff.Timer))
                    continue;

                int filterId = 0;
                if (dictionary.TryGetValue(buff.Name, out filterId))
                {
                    // I'm not sure what the values are here, but this is the effective logic from the old plugin
                    return (filterId == 0 || filterId != 1) && (minCharges == null || buff.Charges >= minCharges());
                }
            }
            return false;
        }

        public override string GetDisplayName(bool isAddingNew)
        {
            string displayName = "Has Curable Ailment";

            if (!isAddingNew)
            {
                displayName += " [";
                if (RemFrozen) displayName += ("Frozen,");
                if (RemBurning) displayName += ("Burn,");
                if (RemShocked) displayName += ("Shock,");
                if (RemCurse) displayName += ("Curse,");
                if (RemPoison) displayName += ("Poison,");
                if (RemBleed) displayName += ("Bleed,");
                if (CorruptCount > 0) displayName += ("Corrupt,");
                displayName += "]";

            }

            return displayName;
        }
    }
}
