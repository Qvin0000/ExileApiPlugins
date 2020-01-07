using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ExileCore.PoEMemory.Components;
using TreeRoutine.Buffs;
using TreeRoutine.Menu;
using TreeRoutine.Routine.BuildYourOwnRoutine.UI;

namespace TreeRoutine.Routine.BuildYourOwnRoutine.Extension.Default.Conditions
{
    internal class PlayerHasBuffCondition : ExtensionCondition
    {
        public bool HasBuff { get; set; } = false;
        public string HasBuffString { get; set; } = "HasBuff";
        public string SearchString { get; set; } = "ignited";
        public string SearchingBuff { get; set; } = "SearchingBuff";
        public int HasBuffSelecter { get; set; } = -1;
        public List<String> HasBuffList { get; set; } 


        public int CorruptCount { get; set; } = 0;
        public string HasBuffReady { get; set; } = "ignited";
        public string SearchStringString { get; set; } = "SearchStringString";
        public string CorruptCountString { get; set; } = "CorruptCount";

        public int RemainingDuration { get; set; } = 0;
        public string RemainingDurationString { get; set; } = "RemainingDuration";
        public int MinimumCharges { get; set; } = 0;
        public string MinimumChargesString { get; set; } = "MinimumCharges";

        public static List<string> GetEnumList<T>()
        {
            // validate that T is in fact an enum
            if (!typeof(T).IsEnum)
            {
                throw new InvalidOperationException();
            }

            return Enum.GetNames(typeof(T)).ToList();
        }


        public PlayerHasBuffCondition(string owner, string name) : base(owner, name)
        {

        }

        public override void Initialise(Dictionary<String, Object> Parameters)
        {
            base.Initialise(Parameters);


            HasBuffReady = ExtensionComponent.InitialiseParameterString(SearchingBuff, HasBuffReady, ref Parameters);
            SearchString = ExtensionComponent.InitialiseParameterString(SearchStringString, SearchString, ref Parameters);
            RemainingDuration = ExtensionComponent.InitialiseParameterInt32(RemainingDurationString, RemainingDuration, ref Parameters);
            MinimumCharges = ExtensionComponent.InitialiseParameterInt32(MinimumChargesString, MinimumCharges, ref Parameters);

        }

        public override bool CreateConfigurationMenu(ExtensionParameter extensionParameter, ref Dictionary<String, Object> Parameters)
        {
            ImGui.TextDisabled("Condition Info");
            ImGuiExtension.ToolTipWithText("(?)", "This condition will return true if the player has any of the selected ailments or a minimum of the specified corrupted blood stacks.");

            base.CreateConfigurationMenu(extensionParameter, ref Parameters);
            var buffList = GetEnumList<BuffEnums>().ToList();
            var buffListSearch = buffList.Where(x => x.Contains(SearchString)).ToList();

            HasBuffReady = ImGuiExtension.ComboBox("Buff List", HasBuffReady, buffListSearch);
            Parameters[SearchingBuff] = HasBuffReady.ToString();

            SearchString = ImGuiExtension.InputText("Filter Buffs", SearchString, 32, ImGuiInputTextFlags.AllowTabInput);
            Parameters[SearchStringString] = SearchString.ToString();

            RemainingDuration = ImGuiExtension.IntSlider("Remaining Duration", RemainingDuration, 0, 4000);
            ImGuiExtension.ToolTipWithText("(?)", "Includes buffs with duration longer than specified. Set to 0 to ignore duration.");
            Parameters[RemainingDurationString] = RemainingDuration.ToString();

            MinimumCharges = ImGuiExtension.IntSlider("Minimum Charges", MinimumCharges, 0, 20);
            ImGuiExtension.ToolTipWithText("(?)", "Includes buffs with charges greater or equal to specified value. Set to 0 to ignore charges.");
            Parameters[MinimumChargesString] = MinimumCharges.ToString();
            return true;
        }

        public override Func<bool> GetCondition(ExtensionParameter extensionParameter)
        {
            return () =>
            {
                if (extensionParameter.Plugin.Settings.Debug)
                {
                    var foundBuff = extensionParameter.Plugin.GameController.Game.IngameState.Data.LocalPlayer.Buffs.FirstOrDefault(x =>
                        x.Name == HasBuffReady);
                    if (foundBuff != null)
                    {
                        extensionParameter.Plugin.LogMessage(
                            $"Found buff {HasBuffReady}. Timer: {foundBuff.Timer} (eval:{(foundBuff.Timer >= RemainingDuration * 1000)}) Charges: {foundBuff.Charges} (eval:{(foundBuff.Charges >= MinimumCharges)})");
                    }
                    else extensionParameter.Plugin.LogMessage($"Buff {HasBuffReady} not found.");

                }

                return extensionParameter.Plugin.GameController.Game.IngameState.Data.LocalPlayer.Buffs.Any(x => x.Name == HasBuffReady && (x.Timer >= (1.0 * RemainingDuration / 1000)) && x.Charges >= MinimumCharges);
            };
        }

        public override string GetDisplayName(bool isAddingNew)
        {
            string displayName = "Has Buff";

            if (!isAddingNew)
            {
                displayName += " [";
                //  if (RemFrozen) displayName += ("Frozen,");
                //    if (CorruptCount > 0) displayName += ("Corrupt,");
                displayName += "]";

            }

            return displayName;
        }
    }
}
