using ImGuiNET;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using TreeRoutine.Menu;
using TreeRoutine.Routine.BuildYourOwnRoutine.Flask;

namespace TreeRoutine.Routine.BuildYourOwnRoutine.UI.MenuItem
{
    internal class ConfigurationMenu
    {
        protected ProfileMenu ProfileMenu { get; set; }
        protected SettingsMenu SettingsMenu { get; set; }

        public ConfigurationMenu(BuildYourOwnRoutineCore plugin)
        {
            this.Plugin = plugin;
            this.ProfileMenu = new ProfileMenu(plugin);
            this.SettingsMenu = new SettingsMenu(plugin);
        }

        private BuildYourOwnRoutineCore Plugin { get; set; }
        private const string OkMenuLabel = "OkMenu";

        #region Menu Cached Values
        private bool RenderSettingsWindow = true;
        #endregion

        public void Render()
        {
            if (RenderSettingsWindow)
            {
                ImGui.Text("Settings");
            }
            else ImGui.TextDisabled("Settings");

            if (ImGui.IsItemHovered(ImGuiHoveredFlags.AnyWindow) && ImGui.IsMouseClicked(0))
            {
                RenderSettingsWindow = true;
            }

            ImGui.SameLine();
            ImGui.Spacing();
            ImGui.SameLine();

            if (!RenderSettingsWindow)
            {
                ImGui.Text("Profile");
            }
            else ImGui.TextDisabled("Profile");

            if (ImGui.IsItemHovered(ImGuiHoveredFlags.AnyWindow) && ImGui.IsMouseClicked(0))
            {
                RenderSettingsWindow = false;
            }

            ImGui.Separator();
            ImGui.Spacing();

            if (RenderSettingsWindow)
            {
                SettingsMenu.Render();
            }
            else
            {
                ProfileMenu.Render();
            }
        }

        #region OKMenu
        private string OKMessage { get; set; }
        private void StartNewOKMenu(string message)
        {
            OKMessage = message;
            ImGui.OpenPopup(OkMenuLabel);
        }

        private void RenderOkMenu()
        {
            bool testOpen = true;
            if (ImGui.BeginPopupModal(OkMenuLabel, ref testOpen, (ImGuiWindowFlags)35))
            {
                ImGui.TextDisabled(OKMessage);
                if (ImGui.Button("OK"))
                {
                    ImGui.CloseCurrentPopup();
                }
                ImGui.EndPopup();
            }
        }
        #endregion
    }
}
