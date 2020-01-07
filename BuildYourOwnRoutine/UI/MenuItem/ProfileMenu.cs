using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TreeRoutine.Routine.BuildYourOwnRoutine.Profile;
using TreeRoutine.Routine.BuildYourOwnRoutine.Extension;
using System.IO;
using TreeRoutine.Menu;
using TreeRoutine.Routine.BuildYourOwnRoutine.Trigger;
using System.Numerics;

namespace TreeRoutine.Routine.BuildYourOwnRoutine.UI.MenuItem
{
    public class ProfileMenu
    {
        internal enum ProfileMenuAction
        {
            None,
            Remove,
            Edit,
            MoveUp,
            MoveDown,
            ImportTree,
            ExportTree
        }

        public ProfileMenu(BuildYourOwnRoutineCore plugin)
        {
            this.Plugin = plugin;
        }

        private BuildYourOwnRoutineCore Plugin { get; set; }

        private const string TriggerMenuLabel = "TriggerMenu";
        private const string OkMenuLabel = "OkMenu";
        private static Vector4 GreenColor = new Vector4(0.0f, 1.0f, 0.0f, 1.0f);
        private static Vector4 RedColor = new Vector4(1.0f, 0.0f, 0.0f, 1.0f);


        #region Menu Cache

        private TriggerMenu NewTriggerMenu = null;
        private int currentlySelectedProfile = -1;
        private string currentFileName = "";

        #endregion
    

        public void Render()
        {
            if (ImGui.SmallButton("Reload"))
            {
                // Validate current tree
                var root = Plugin.Settings.LoadedProfile.Composite;
                if (root == null)
                {
                    // Warn the user that there is no profile to reload
                    StartNewOKMenu("No profile to reload.");
                }
                else
                {
                    if (!ValidateTree(Plugin.Settings.LoadedProfile.Composite, out string error))
                    {
                        StartNewOKMenu(error);
                    }
                    else
                    {
                        // Everything seems good. Load the tree
                        Plugin.CreateAndStartTreeFromLoadedProfile();
                    }
                }
            }
            ImGuiExtension.ToolTipWithText("(?)", "The tree MUST be reloaded in order for changes to take effect.");

            ImGui.SameLine();
            ImGui.Spacing();
            ImGui.SameLine();

            if (ImGui.SmallButton("Save") || forceOpenSave)
            {
                ImGui.OpenPopup("Save Profile");
            }

            RenderSaveProfileMenu();

            ImGui.SameLine();
            ImGui.Spacing();
            ImGui.SameLine();

            if (ImGui.SmallButton("Load") || forceOpenLoad)
            {
                ImGui.OpenPopup("Load Profile");
            }

            RenderLoadProfileMenu();

            ImGui.SameLine();
            ImGui.Spacing();
            ImGui.SameLine();

            if (ImGui.SmallButton("Clear") || Plugin.Settings.LoadedProfile == null)
            {
                Plugin.Settings.LoadedProfile = new Profile.LoadedProfile();
            }
            ImGuiExtension.ToolTipWithText("(?)", "Clear the tree to start over.");

            RenderOkMenu();

            ImGui.Spacing();
            ImGui.Separator();
            ImGui.Text("Loaded Tree");
            ImGui.Spacing();

            if (Plugin.Settings.LoadedProfile.Composite == null)
            {
                if (ImGui.Button("+"))
                {
                    ImGui.OpenPopup(TriggerMenuLabel);
                    NewTriggerMenu = new TriggerMenu(Plugin.ExtensionParameter, null);
                }
                ImGuiExtension.ToolTipWithText("(?)", "Add root");


                // If start profile is clicked, trigger menu is rendered
                RenderTriggerMenu();
            }
            else
            {
                var menuAction = CreateTreeForComposite(null, Plugin.Settings.LoadedProfile.Composite, 0);
                if (menuAction == ProfileMenuAction.Remove)
                {
                    // Was asked to remove the root node.
                    Plugin.Settings.LoadedProfile.Composite = null;
                }
            }

        }

        private ProfileMenuAction CreateTreeForComposite(TriggerComposite parent, TriggerComposite composite, int depth)
        {
            String typeTag = "";
            if (composite.Type == TriggerType.Action)
                typeTag = "[A]";
            else if (composite.Type == TriggerType.Decorator)
                typeTag = "[D]";
            else if (composite.Type == TriggerType.PrioritySelector)
                typeTag = "[P]";
            else if (composite.Type == TriggerType.Sequence)
                typeTag = "[S]";

            string label = depth + ":" + typeTag + composite.Name;

            ImGui.PushID("Composite" + depth + label);
            try
            {
                Vector4 chosenColor = GreenColor;
                if (!ValidateTree(composite, out string error))
                {
                    chosenColor = RedColor;
                }

                if (composite.Type == TriggerType.Action)
                {
                    // Push an ID so everything below remains unique
                    ImGuiNative.igIndent(1.0f);

                    var profileAction = CreateTriggerTextWithPopupMenu(label, chosenColor, parent, composite);
                    if (profileAction == ProfileMenuAction.Edit)
                    {
                        ImGui.OpenPopup(TriggerMenuLabel);
                        NewTriggerMenu = new TriggerMenu(Plugin.ExtensionParameter, parent, composite);
                    }
                    else if (profileAction == ProfileMenuAction.ExportTree)
                    {
                        LoadSaveTrigger = composite;
                        forceOpenSave = true;
                    }
                    else if (profileAction != ProfileMenuAction.None)
                    {
                        // Pass it up, maybe someone above knows how to deal with it
                        return profileAction;
                    }

                    RenderTriggerMenu();
                    ImGuiNative.igUnindent(1.0f);
                }
                else if (ImGui.TreeNodeEx("", ImGuiTreeNodeFlags.OpenOnArrow | ImGuiTreeNodeFlags.DefaultOpen))
                {
                    ImGui.SameLine();

                    var profileAction = CreateTriggerTextWithPopupMenu(label, chosenColor, parent, composite);
                    if (profileAction == ProfileMenuAction.Edit)
                    {
                        ImGui.OpenPopup(TriggerMenuLabel);
                        NewTriggerMenu = new TriggerMenu(Plugin.ExtensionParameter, parent, composite);
                    }
                    else if (profileAction == ProfileMenuAction.ImportTree)
                    {
                        forceOpenLoad = true;
                        LoadSaveTrigger = composite;
                    }
                    else if (profileAction == ProfileMenuAction.ExportTree)
                    {
                        forceOpenSave = true;
                        LoadSaveTrigger = composite;
                    }
                    else if (profileAction != ProfileMenuAction.None)
                    {
                        // Pass it up, maybe someone above knows how to deal with it
                        return profileAction;
                    }

                    // Decorators can only have one child
                    if (composite.Type != TriggerType.Decorator || (composite.Children == null || composite.Children.Count == 0))
                    {
                        if (ImGui.Button("+"))
                        {
                            ImGui.OpenPopup(TriggerMenuLabel);
                            NewTriggerMenu = new TriggerMenu(Plugin.ExtensionParameter, composite);
                        }
                        ImGuiExtension.ToolTipWithText("(?)", "Add Child");
                    }

                    if (composite.Children != null && composite.Children.Any())
                    {
                        List<TriggerComposite> childrenToRemove = new List<TriggerComposite>();
                        List<TriggerComposite> childrenToMoveUp = new List<TriggerComposite>();
                        List<TriggerComposite> childrenToMoveDown = new List<TriggerComposite>();
                        foreach (var child in composite.Children)
                        {
                            var childAction = CreateTreeForComposite(composite, child, depth + 1);
                            if (childAction == ProfileMenuAction.Remove)
                                childrenToRemove.Add(child);
                            else if (childAction == ProfileMenuAction.MoveUp)
                                childrenToMoveUp.Add(child);
                            else if (childAction == ProfileMenuAction.MoveDown)
                                childrenToMoveDown.Add(child);
                        }
                        // Remove all children who were requested deletion
                        childrenToRemove.ForEach(x => composite.Children.Remove(x));
                        childrenToMoveUp.ForEach(x =>
                        {
                            var index = composite.Children.IndexOf(x);
                            composite.Children.Remove(x);
                            composite.Children.Insert(Math.Max(0, index - 1), x);
                        });
                        childrenToMoveDown.ForEach(x =>
                        {
                            var index = composite.Children.IndexOf(x);
                            composite.Children.Remove(x);
                            composite.Children.Insert(index + 1, x);
                        });
                    }

                    RenderTriggerMenu();

                    ImGui.TreePop();
                }
                else
                {
                    // Tree is closed, but we still want to display the text.
                    ImGui.SameLine();
                    var profileAction = CreateTriggerTextWithPopupMenu(label, chosenColor, parent, composite);
                    if (profileAction == ProfileMenuAction.Edit)
                    {
                        ImGui.OpenPopup(TriggerMenuLabel);
                        NewTriggerMenu = new TriggerMenu(Plugin.ExtensionParameter, parent, composite);
                    }
                    else if (profileAction == ProfileMenuAction.ImportTree)
                    {
                        forceOpenLoad = true;
                        LoadSaveTrigger = composite;
                    }
                    else if (profileAction == ProfileMenuAction.ExportTree)
                    {
                        forceOpenSave = true;
                        LoadSaveTrigger = composite;
                    }
                    else if (profileAction != ProfileMenuAction.None)
                    {
                        // Pass it up, maybe someone above knows how to deal with it
                        return profileAction;
                    }

                    RenderTriggerMenu();
                }
            }
            finally
            {
                // Just to make sure we pop the ID, no matter if we return early
                ImGui.PopID();
            }

            return ProfileMenuAction.None;
        }

        private ProfileMenuAction CreateTriggerTextWithPopupMenu(string label, Vector4 color, TriggerComposite parent, TriggerComposite composite)
        {
            ImGui.Text(label);

            if (ImGui.IsItemHovered(ImGuiHoveredFlags.AnyWindow) && ImGui.IsMouseClicked(1))
            {
                ImGui.OpenPopup("Trigger Menu Context");
            }

            var selectedAction = ProfileMenuAction.None;
            if (ImGui.BeginPopup("Trigger Menu Context"))
            {
                if (ImGui.Selectable("Remove"))
                {
                    selectedAction = ProfileMenuAction.Remove;
                }
                if (ImGui.Selectable("Edit"))
                {
                    selectedAction = ProfileMenuAction.Edit;
                }
                // Only show move up/down if our parent can have more than one child
                if (parent != null && (parent.Type == TriggerType.PrioritySelector || parent.Type == TriggerType.Sequence))
                {
                    if (ImGui.Selectable("Move Up"))
                    {
                        selectedAction = ProfileMenuAction.MoveUp;
                    }
                    if (ImGui.Selectable("Move Down"))
                    {
                        selectedAction = ProfileMenuAction.MoveDown;
                    }
                }

                // We can't import on decorators with 1 child or actions
                if (composite != null && composite.Type != TriggerType.Action && (composite.Type != TriggerType.Decorator || composite.Children != null && composite.Children.Count == 0))
                {
                    if (ImGui.Selectable("Import"))
                    {
                        selectedAction = ProfileMenuAction.ImportTree;
                    }
                }

                if (ImGui.Selectable("Export"))
                {
                    selectedAction = ProfileMenuAction.ExportTree;
                }

                ImGui.EndPopup();
                return selectedAction;
            }

            return selectedAction;
        }

        private void RenderTriggerMenu()
        {
            bool open = true;
            if (ImGui.BeginPopupModal(TriggerMenuLabel, ref open, ImGuiWindowFlags.AlwaysAutoResize))
            {
                if (!NewTriggerMenu.Render())
                {
                    if (NewTriggerMenu.TriggerComposite != null)
                    {
                        // We saved, not canceled.
                        if (NewTriggerMenu.Parent == null)
                        {
                            // We are saving the root
                            Plugin.Settings.LoadedProfile.Composite = NewTriggerMenu.TriggerComposite;
                        }
                        else
                        {
                            // Add the saved trigger to the parent
                            NewTriggerMenu.Parent.Children.Add(NewTriggerMenu.TriggerComposite);
                        }
                    }

                    NewTriggerMenu = null;
                    ImGui.CloseCurrentPopup();
                }
                ImGui.EndPopup();
            }
        }

        private bool ValidateTree(TriggerComposite composite, out string error)
        {
            error = null;
            if (composite == null)
            {
                error = "No profile loaded";
                return false;
            }
            if (composite.Type != TriggerType.Action && (composite.Children == null || composite.Children.Count == 0))
            {
                error = "Composite must have at least one child.\nName: " + composite.Name;
                return false;
            }

            if (composite.Children != null && composite.Children.Count > 0)
            {
                foreach(var child in composite.Children)
                {
                    if (!ValidateTree(child, out string retVal))
                    {
                        error = retVal;
                        return false;
                    }
                }
            }

            return true;
        }

        // TODO: I hate imgui. Gotta figure out the right way to not do this
        private Boolean forceOpenLoad = false;
        private Boolean forceOpenSave = false;
        private TriggerComposite LoadSaveTrigger = null;

        private void RenderLoadProfileMenu()
        {
            bool open = true;
            if (ImGui.BeginPopupModal("Load Profile", ref open, ImGuiWindowFlags.AlwaysAutoResize))
            {
                string[] files = Directory.GetFiles(Plugin.ProfileDirectory);

                if (files.Length == 0)
                {
                    ImGui.Text($"No profiles in profile directory: {Plugin.ProfileDirectory}");
                    currentlySelectedProfile = -1;
                    LoadSaveTrigger = null;
                    forceOpenLoad = false;
                }
                else
                {
                    ImGui.Combo("Files", ref currentlySelectedProfile, files.Select(x => Path.GetFileName(x)).ToArray(), files.Length);
                }

                if (currentlySelectedProfile >= 0 && ImGui.Button("Load"))
                {
                    Profile.LoadedProfile loadedProfile = Plugin.LoadSettingFile<Profile.LoadedProfile>(files[currentlySelectedProfile]);
                    if (loadedProfile == null || loadedProfile.Name == null)
                    {
                        StartNewOKMenu("Profile did not load profile properly");
                    }
                    else
                    {
                        if (LoadSaveTrigger != null)
                        {
                            if (loadedProfile.Composite != null)
                                LoadSaveTrigger.Children.Add((loadedProfile.Composite));

                            LoadSaveTrigger = null;
                            forceOpenLoad = false;
                        }
                        else
                        {
                            Plugin.Settings.LoadedProfile = loadedProfile;
                            Plugin.CreateAndStartTreeFromLoadedProfile();

                            currentlySelectedProfile = -1;
                            LoadSaveTrigger = null;
                            forceOpenLoad = false;
                        }

                        ImGui.CloseCurrentPopup();
                    }

                }
                // Render the menu from loading profile
                RenderOkMenu();

                if (ImGui.Button("Cancel"))
                {
                    currentlySelectedProfile = -1;
                    LoadSaveTrigger = null;
                    forceOpenLoad = false;

                    ImGui.CloseCurrentPopup();
                }

                ImGui.EndPopup();
            }
        }

        private void RenderSaveProfileMenu()
        {
            bool open = true;
            if (ImGui.BeginPopupModal($"Save Profile", ref open, ImGuiWindowFlags.AlwaysAutoResize))
            {

                currentFileName = ImGuiExtension.InputText("File Name", currentFileName, 100, ImGuiInputTextFlags.AlwaysInsertMode);
                if (!String.IsNullOrEmpty(currentFileName))
                {
                    if (ImGui.Button("Save"))
                    {

                        LoadedProfile profileToSave = LoadSaveTrigger != null
                            ? new LoadedProfile() {Composite = LoadSaveTrigger }
                            : Plugin.Settings.LoadedProfile;

                        BaseTreeRoutinePlugin<BuildYourOwnRoutineSettings, BaseTreeCache>.SaveSettingFile<Profile.LoadedProfile>(Plugin.ProfileDirectory + currentFileName, profileToSave);

                        currentFileName = "";
                        forceOpenSave = false;
                        ImGui.CloseCurrentPopup();
                    }
                    ImGui.SameLine();
                }

                if (ImGui.Button("Cancel"))
                {
                    currentFileName = "";
                    forceOpenSave = false;
                    ImGui.CloseCurrentPopup();
                }
                ImGui.EndPopup();
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
            bool open = true;
            if (ImGui.BeginPopupModal(OkMenuLabel, ref open, (ImGuiWindowFlags)35))
            {
                ImGui.TextDisabled(OKMessage);
                ImGui.Spacing();
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
