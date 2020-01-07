using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TreeRoutine.Menu;

namespace TreeRoutine.Routine.BuildYourOwnRoutine.Extension.Default.Conditions
{
    internal class TimerCondition : ExtensionCondition
    {
        private String TimerName { get; set; } = "";
        private const String TimerNameString = "TimerName";

        private int TimeElapsed { get; set; } = 500;
        private const String TimeElapsedString = "TimeElapsed";

        private Boolean TrueIfStopped { get; set; } = false;
        private const String TrueIfStoppedString = "TrueIfStopped";

        public TimerCondition(string owner, string name) : base(owner, name)
        {

        }

        public override void Initialise(Dictionary<String, Object> Parameters)
        {
            base.Initialise(Parameters);

            TimerName = ExtensionComponent.InitialiseParameterString(TimerNameString, TimerName, ref Parameters);
            TimeElapsed = ExtensionComponent.InitialiseParameterInt32(TimeElapsedString, TimeElapsed, ref Parameters);
            TrueIfStopped = ExtensionComponent.InitialiseParameterBoolean(TrueIfStoppedString, TrueIfStopped, ref Parameters);

        }

        public override bool CreateConfigurationMenu(ExtensionParameter extensionParameter, ref Dictionary<String, Object> Parameters)
        {
            ImGui.TextDisabled("Condition Info");
            ImGuiExtension.ToolTipWithText("(?)", "This condition is used to determine if we can use a specific flask.\nIt will ensure that health/hybrid/mana potions are not used when we are at full health/mana.\nThis will also ensure that we do not use up reserved uses.");

            base.CreateConfigurationMenu(extensionParameter, ref Parameters);

            TimerName = ImGuiExtension.InputText("Timer Name", TimerName, 30, ImGuiInputTextFlags.AlwaysInsertMode);
            ImGuiExtension.ToolTipWithText("(?)", "Name for this timer. Make sure this is unique.");
            Parameters[TimerNameString] = TimerName.ToString();

            var tempTimerLength = ImGuiExtension.InputText("Time Elapsed (ms)", TimeElapsed.ToString(), 30, ImGuiInputTextFlags.AlwaysInsertMode);
            if (Int32.TryParse(tempTimerLength, out int convertedTimerLength))
            {
                TimeElapsed = convertedTimerLength;
            }
            ImGuiExtension.ToolTipWithText("(?)", "True if timer has run longer than specified time\n1000 ms = 1 sec");
            Parameters[TimeElapsedString] = TimeElapsed.ToString();

            TrueIfStopped = ImGuiExtension.Checkbox("True if stopped", TrueIfStopped);
            ImGuiExtension.ToolTipWithText("(?)", "When enabled, returns true if timer is stopped (or never started)");
            Parameters[TrueIfStoppedString] = TrueIfStopped.ToString();

            return true;
        }

        public override Func<bool> GetCondition(ExtensionParameter extensionParameter)
        {
            return () =>
            {
                if (extensionParameter.Plugin.ExtensionCache.Cache.TryGetValue(Owner, out Dictionary<string, object> MyCache))
                {
                    var timerCacheString = DefaultExtension.CustomerTimerPrefix + TimerName;
                    if (MyCache.TryGetValue(timerCacheString, out object value))
                    {
                        Stopwatch stopWatch = (Stopwatch)value;
                        return stopWatch.ElapsedMilliseconds >= TimeElapsed;
                    }
                    else return TrueIfStopped;
                    
                }
                else extensionParameter.Plugin.LogErr(Name + " is unable to get cache!", 5);

                return false;
            };
        }

        public override string GetDisplayName(bool isAddingNew)
        {
            string displayName = "Timer Condition";

            if (!isAddingNew)
            {
                displayName += " [";
                displayName += ("TimerName=" + TimerName.ToString());
                displayName += "]";

            }

            return displayName;
        }
    }
}
