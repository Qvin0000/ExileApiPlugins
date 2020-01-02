using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TreeRoutine.DefaultBehaviors.Actions;
using TreeRoutine.Menu;
using TreeRoutine.TreeSharp;

namespace TreeRoutine.Routine.BuildYourOwnRoutine.Extension.Default.Actions
{
    internal class TimerAction : ExtensionAction
    {
        private String TimerName { get; set; } = "";
        private const String TimerNameString = "TimerName";



        private bool ForceStop { get; set; } = false;
        private const String ForceStopString = "ForceStop";


        public TimerAction(string owner, string name) : base(owner, name)
        {

        }

        public override void Initialise(Dictionary<String, Object> Parameters)
        {
            TimerName = ExtensionComponent.InitialiseParameterString(TimerNameString, TimerName, ref Parameters);
            ForceStop = ExtensionComponent.InitialiseParameterBoolean(ForceStopString, ForceStop, ref Parameters);

        }

        public override bool CreateConfigurationMenu(ExtensionParameter extensionParameter, ref Dictionary<String, Object> Parameters)
        {
            ImGui.TextDisabled("Action Info");
            ImGuiExtension.ToolTipWithText("(?)", "This timer is used to begin or end a specific timer");
            TimerName = ImGuiExtension.InputText("Timer Name", TimerName, 30, ImGuiInputTextFlags.AlwaysInsertMode);
            ImGuiExtension.ToolTipWithText("(?)", "Name for this timer");
            Parameters[TimerNameString] = TimerName.ToString();

            var selectedOption = ForceStop ? 1 : 0;
            ImGui.RadioButton("Start Timer", ref selectedOption, 0);
            ImGui.RadioButton("End Timer", ref selectedOption, 1);
            ForceStop = selectedOption == 1;
            Parameters[ForceStopString] = ForceStop.ToString();

            return true;
        }

        public override Composite GetComposite(ExtensionParameter profileParameter)
        {
            return new TreeSharp.Action((x) =>
            {
                if (profileParameter.Plugin.ExtensionCache.Cache.TryGetValue(Owner, out Dictionary<string, object> MyCache))
                {
                    var timerCacheString = DefaultExtension.CustomerTimerPrefix + TimerName;
                    Stopwatch stopWatch = null;
                    if (!MyCache.TryGetValue(timerCacheString, out object value))
                    {
                        stopWatch = new Stopwatch();
                        MyCache[timerCacheString] = stopWatch;
                    } else stopWatch = (Stopwatch)value;

                    if (ForceStop)
                    {
                        // We need to start the timer, and it isn't in the cache already
                        stopWatch.Stop();
                    }
                    else
                    {
                        stopWatch.Restart();
                    }
                }
                else profileParameter.Plugin.LogErr(Name + " is unable to get cache!", 5);
            });
        }

        public override string GetDisplayName(bool isAddingNew)
        {
            string displayName = "Timer Action";

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
