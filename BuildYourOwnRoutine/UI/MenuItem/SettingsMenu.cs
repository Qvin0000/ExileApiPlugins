using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TreeRoutine.Menu;
using TreeRoutine.Routine.BuildYourOwnRoutine.Flask;

namespace TreeRoutine.Routine.BuildYourOwnRoutine.UI.MenuItem
{
    internal class SettingsMenu
    {
        public SettingsMenu(BuildYourOwnRoutineCore plugin)
        {
            this.Plugin = plugin;
        }

        private BuildYourOwnRoutineCore Plugin { get; set; }

        public void Render()
        {
            Plugin.Settings.TicksPerSecond.Value = ImGuiExtension.IntSlider("Ticks Per Second", Plugin.Settings.TicksPerSecond);
            Plugin.Settings.Debug.Value = ImGuiExtension.Checkbox("Debug", Plugin.Settings.Debug.Value);

            if (ImGui.TreeNodeEx("Individual Flask Settings", ImGuiTreeNodeFlags.DefaultOpen))
            {
                for (int i = 0; i < 5; i++)
                {
                    FlaskSetting currentFlask = Plugin.Settings.FlaskSettings[i];
                    if (ImGui.TreeNodeEx("Flask " + (i + 1) + " Settings", ImGuiTreeNodeFlags.DefaultOpen))
                    {
                        currentFlask.Hotkey.Value = ImGuiExtension.HotkeySelector("Hotkey", currentFlask.Hotkey);
                        ImGui.TreePop();
                    }
                }

                ImGui.TreePop();
            }
        }
    }
}
