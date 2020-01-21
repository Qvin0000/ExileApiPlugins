using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using ExileCore;
using ExileCore.PoEMemory;
using ExileCore.PoEMemory.Components;
using ExileCore.Shared;
using ExileCore.Shared.AtlasHelper;
using ImGuiNET;
using PassiveSkillTreePlanter.SkillTreeJson;
using PassiveSkillTreePlanter.UrlDecoders;
using SharpDX;
using Vector2 = System.Numerics.Vector2;

namespace PassiveSkillTreePlanter
{
    public class PassiveSkillTreePlanter : BaseSettingsPlugin<PassiveSkillTreePlanterSettings>
    {
        public const string SkillTreeDataFile = "SkillTreeData.dat";
        public const string SkillTreeDir = "Builds";
        public static int selected;
        public readonly PoESkillTreeJsonDecoder _skillTreeeData = new PoESkillTreeJsonDecoder();


        public bool _bUiRootInitialized;
        public List<SkillNode> _drawNodes = new List<SkillNode>();

        public Element _uiSkillTreeBase;
        public List<ushort> _urlNodes = new List<ushort>(); //List of nodes decoded from URL

        public string AddNewBuildFile = "";
        public string AddNewBuildThreadUrl = "";
        public string AddNewBuildUrl = "";

        private AtlasTexture _ringImage;

        public string CurrentlySelectedBuildFile { get; set; }
        public string CurrentlySelectedBuildFileEdit { get; set; }
        public string CurrentlySelectedBuildUrl { get; set; }
        public string CurrentlySelectedBuildForumThread { get; set; }

        public string SkillTreeUrlFilesDir => DirectoryFullName + @"\" + SkillTreeDir;
        public List<string> BuildFiles { get; set; }
        public IntPtr TextEditCallback { get; set; }
        public static PassiveSkillTreePlanter Core { get; set; }

        public override void OnLoad()
        {
            Core = this;
            if (!Directory.Exists(SkillTreeUrlFilesDir))
            {
                Directory.CreateDirectory(SkillTreeUrlFilesDir);
                LogMessage("PassiveSkillTree: Write your skill tree url to txt file and place it to Build folder.", 10);
                return;
            }

            //OLD
            BuildFiles = TreeConfig.GetBuilds();
            //BuildFiles = TreeConfig.GetBuilds();

            //Read url
            ReadUrlFromSelectedUrl(Settings.SelectedURLFile);
        }
        public override bool Initialise()
        {
            _ringImage = GetAtlasTexture("AtlasMapCircle");//IconArcing or IconArcing.png, doesn't matter, works both
            return true;
        }

        public override void Render()
        {
            base.Render();

            // TODO: let users load party passive trees when we can get it from poehud core
            //PlayerInPartyDraw = PartyElements.GetPlayerInfoElementList(PlayerEntities);
            ExtRender();
            EzTreeChanger();
        }
        public bool InBounds(int index, int arrayLength)
        {
            return (index >= 0 && index <= arrayLength);
        }

        private void EzTreeChanger()
        {
            //Check if setting is on
            if (!Settings.EnableEzTreeChanger)
                return;

            //check if Skill Tree window is open
            var skillTreeElement = GameController.Game.IngameState.IngameUi.TreePanel;
            if (!skillTreeElement.IsVisible)
                return;

            //delacre a few variables
            var topLeftGameWindow = GameController.Window.GetWindowRectangle().TopLeft;

            var isOpen = true;
            //Start of Draw imgui window
            if (ImGuiExtension.BeginWindow("#noTitleTreePlanner", ref isOpen, (int)topLeftGameWindow.X, (int)topLeftGameWindow.Y, 0, 0, ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoMove, true))
            {
                bool pressedBack = false;
                bool pressedNext = false;

                if (ImGui.Button("<"))
                {
                    pressedBack = true;
                    //LogMessage($"Selected index wanting to go from {Settings.SelectedBuild.SelectedIndex} -> {Settings.SelectedBuild.SelectedIndex -1}, Max Index in Build is {Settings.SelectedBuild.Trees.Count}", 10);
                }

                ImGui.SameLine();

                if (ImGui.Button(">"))
                {
                    pressedNext = true;
                    //LogMessage($"Selected index wanting to go from {Settings.SelectedBuild.SelectedIndex} -> {Settings.SelectedBuild.SelectedIndex + 1}, Max Index in Build is {Settings.SelectedBuild.Trees.Count}", 10);
                }
                ImGui.SameLine();

                ImGui.Text(Settings.SelectedTreeName);

                if (pressedBack && InBounds(Settings.SelectedBuild.SelectedIndex - 1, Settings.SelectedBuild.Trees.Count-1))
                {
                    Settings.SelectedBuild.SelectedIndex -= 1;
                    ReadUrlFromSelectedBuild(Settings.SelectedBuild.Trees[Settings.SelectedBuild.SelectedIndex].SkillTreeUrl, Settings.SelectedBuild.Trees[Settings.SelectedBuild.SelectedIndex].Tag);
                }
                if (pressedNext && InBounds(Settings.SelectedBuild.SelectedIndex + 1, Settings.SelectedBuild.Trees.Count-1))
                {
                    Settings.SelectedBuild.SelectedIndex += 1;
                    ReadUrlFromSelectedBuild(Settings.SelectedBuild.Trees[Settings.SelectedBuild.SelectedIndex].SkillTreeUrl, Settings.SelectedBuild.Trees[Settings.SelectedBuild.SelectedIndex].Tag);
                }

                ImGui.EndMenu();
            }
        }

        private void LoadBuildFiles()
        {
            //Update url list variants
            var dirInfo = new DirectoryInfo(SkillTreeUrlFilesDir);
            BuildFiles = dirInfo.GetFiles("*.txt").Select(x => Path.GetFileNameWithoutExtension(x.Name)).ToList();
        }

        private static string CleanFileName(string fileName)
        {
            return Path.GetInvalidFileNameChars()
                .Aggregate(fileName, (current, c) => current.Replace(c.ToString(), string.Empty));
        }

        public void RenameFile(string fileName, string oldFileName)
        {
            fileName = CleanFileName(fileName);
            var newFilePath = Path.Combine(SkillTreeUrlFilesDir, fileName.TrimEnd('\0').TrimEnd('\u0000').Replace("\u0000", "") + ".json");
            var oldFilePath = Path.Combine(SkillTreeUrlFilesDir, oldFileName.TrimEnd('\0').TrimEnd('\u0000').Replace("\u0000", "") + ".json");
            if (File.Exists(newFilePath))
            {
                LogError("PassiveSkillTreePlanter: File already Exists!", 10);
                return;
            }

            //Now Rename the File`
            File.Move(oldFilePath, newFilePath);
            //OLD
            LoadBuildFiles();
            //BuildFiles = TreeConfig.GetBuilds();
            Settings.SelectedURLFile = fileName;
            ReadUrlFromSelectedUrl(Settings.SelectedURLFile);
        }

        public string RemoveAccName(string url)
        {
            // Aim is to remove the string content but keep the info inside the text file incase user wants to revisit that account/char in the future
            if (url.Contains("?accountName"))
                url = url.Split(new[]
                {
                    "?accountName"
                }, StringSplitOptions.None)[0];

            // If string contains characterName but no 
            if (url.Contains("?characterName"))
                url = url.Split(new[]
                {
                    "?characterName"
                }, StringSplitOptions.None)[0];

            return url;
        }

        public void ReplaceUrlContents(string fileName, string newContents)
        {
            fileName = CleanFileName(fileName);
            var filePath = Path.Combine(SkillTreeUrlFilesDir, fileName + ".txt");
            if (!File.Exists(filePath))
            {
                LogError("PassiveSkillTreePlanter: File doesnt exist", 10);
                return;
            }

            if (!IsBase64String(RemoveAccName(newContents)))
            {
                LogError(
                    "PassiveSkillTreePlanter: Invalid URL or you are trying to add something that is not a pathofexile.com build URL",
                    10);
                return;
            }

            //Now Rename the File
            File.WriteAllText(filePath, newContents);
            ReadUrlFromSelectedUrl(Settings.SelectedURLFile);
        }

        public void ReplaceThreadUrlContents(string fileName, string newContents)
        {
            var skillTreeUrlFilePath = Path.Combine(SkillTreeUrlFilesDir, fileName + ".txt");
            if (!File.Exists(skillTreeUrlFilePath))
                return;

            var strings = File.ReadAllLines(skillTreeUrlFilePath);
            var newString = new List<string>
            {
                strings[0],
                newContents
            };
            File.WriteAllLines(skillTreeUrlFilePath, newString);
        }

        public void AddNewBuild(string buildName, string buildUrl, string buildThreadUrl)
        {
            var newFilePath = Path.Combine(SkillTreeUrlFilesDir, buildName + ".txt");
            if (File.Exists(newFilePath))
            {
                LogError("PassiveSkillTreePlanter.Add: File already Exists!", 10);
                return;
            }

            if (!IsBase64String(buildUrl))
            {
                LogError(
                    "PassiveSkillTreePlanter.Add: Invalid URL or you are trying to add something that is not a pathofexile.com build URL OR you need to remove the game version (3.x.x)",
                    10);
                return;
            }

            var newString = new List<string>
            {
                buildUrl
            };
            if (!string.IsNullOrEmpty(buildThreadUrl))
                newString.Add(buildThreadUrl);

            File.WriteAllLines(newFilePath, newString);
            LoadBuildFiles();
        }

        public static bool IsBase64String(string url)
        {
            try
            {
                url = url.Split('/').LastOrDefault()?.Replace("-", "+").Replace("_", "/");
                // If no exception is caught, then it is possibly a base64 encoded string
                var data = Convert.FromBase64String(url);
                // The part that checks if the string was properly padded to the
                return url.Replace(" ", "").Length % 4 == 0;
            }
            catch
            {
                // If exception is caught, then it is not a base64 encoded string
                return false;
            }
        }

        public override void DrawSettings()
        {
            string[] settingName =
            {
                "Build Selection",
                "Build Edit",
                "Build Add",
                "Colors",
                "Sliders",
                "Toggles"
            };
            var newcontentRegionArea = ImGuiNative.igGetContentRegionAvail();
            if (ImGui.BeginChild("LeftSettings", new Vector2(150, newcontentRegionArea.Y), false, ImGuiWindowFlags.None))
                for (var i = 0; i < settingName.Length; i++)
                    if (ImGui.Selectable(settingName[i], selected == i))
                        selected = i;

            ImGui.EndChild();
            ImGui.SameLine();
            ImGui.PushStyleVar(ImGuiStyleVar.ChildRounding, 5.0f);
            newcontentRegionArea = ImGuiNative.igGetContentRegionAvail();
            if (ImGui.BeginChild("RightSettings", new Vector2(newcontentRegionArea.X, newcontentRegionArea.Y), true,
                ImGuiWindowFlags.None))
                switch (settingName[selected])
                {
                    case "Build Selection":
                        // Open Build Folder
                        if (ImGui.Button("Open Build Folder"))
                            Process.Start(SkillTreeUrlFilesDir);

                        ImGui.SameLine();
                        // Populate Build List
                        if (ImGui.Button("(Re)Load List"))
                            BuildFiles = TreeConfig.GetBuilds();

                        ImGui.SameLine();
                        // Open Forum Thread
                        if (ImGui.Button("Open Forum Thread"))
                            Process.Start(Settings.SelectedBuild.BuildLink);

                        Settings.SelectedURLFile = ImGuiExtension.ComboBox("Builds", Settings.SelectedURLFile,
                            BuildFiles, out var buildSelected, ImGuiComboFlags.HeightLarge);
                        if (buildSelected)
                        {
                            Settings.SelectedBuild = TreeConfig.LoadBuild(Settings.SelectedURLFile);
                            ReadUrlFromSelectedUrl(Settings.SelectedURLFile);
                        }
                        var TreeToUse = -1;
                        ImGui.Separator();
                        ImGui.Text($"Currently Selected: {Settings.SelectedTreeName}");
                        ImGui.Separator();
                        ImGui.Columns(2, "LoadColums", true);
                        ImGui.SetColumnWidth(0, 51f);
                        ImGui.Text("");
                        ImGui.NextColumn();
                        ImGui.Text("Tree Name");
                        ImGui.NextColumn();
                        if (Settings.SelectedBuild.Trees.Count != 0)
                            ImGui.Separator();
                        for (var j = 0; j < Settings.SelectedBuild.Trees.Count; j++)
                        {
                            if (ImGui.Button($"LOAD##LOADRULE{j}"))
                            {
                                TreeToUse = j;
                                Settings.SelectedBuild.SelectedIndex = j;
                            }


                            ImGui.NextColumn();
                            ImGui.Text(Settings.SelectedBuild.Trees[j].Tag);
                            ImGui.NextColumn();
                            ImGui.PopItemWidth();
                        }
                        ImGui.Columns(1, "", false);
                        ImGui.Separator();

                        if (TreeToUse != -1)
                            ReadUrlFromSelectedBuild(Settings.SelectedBuild.Trees[TreeToUse].SkillTreeUrl,
                                Settings.SelectedBuild.Trees[TreeToUse].Tag);
                        ImGui.Text("NOTES:");

                        // only way to wrap text with imgui.net without a limit on TextWrap function
                        ImGuiNative.igPushTextWrapPos(0.0f);
                        ImGui.TextUnformatted(Settings.SelectedBuild.Notes);
                        ImGuiNative.igPopTextWrapPos();
                        break;
                    case "Build Edit":
                        if (Settings.SelectedBuild.Trees.Count > 0)
                        {
                            var treesToRemove = new List<int>();
                            var treesToMoveEdit = new List<Tuple<int, bool>>();

                            ImGui.Separator();
                            Settings.SelectedBuild.BuildLink =
                                ImGuiExtension.InputText("Forum Thread", Settings.SelectedBuild.BuildLink,
                                    1024, ImGuiInputTextFlags.None);

                            ImGui.Text("Notes");
                            // Keep at max 4k byte size not sure why it crashes when upped, not going to bother dealing with this either.
                            Settings.SelectedBuild.Notes = ImGuiExtension.MultiLineTextBox("##Notes",
                                Settings.SelectedBuild.Notes, 150000, new Vector2(newcontentRegionArea.X - 20, 200),
                                ImGuiInputTextFlags.Multiline);

                            ImGui.Separator();
                            ImGui.Columns(4, "EditColums", true);
                            ImGui.SetColumnWidth(0, 30f);
                            ImGui.SetColumnWidth(1, 50f);
                            ImGui.Text("");
                            ImGui.NextColumn();
                            ImGui.Text("Move");
                            ImGui.NextColumn();
                            ImGui.Text("Tree Name");
                            ImGui.NextColumn();
                            ImGui.Text("Skill Tree");
                            ImGui.NextColumn();
                            if (Settings.SelectedBuild.Trees.Count != 0)
                                ImGui.Separator();
                            for (var j = 0; j < Settings.SelectedBuild.Trees.Count; j++)
                            {
                                if (ImGui.Button($"X##REMOVERULE{j}"))
                                    treesToRemove.Add(j);

                                ImGui.NextColumn();
                                if (ImGui.Button($"^##MOVERULEUPEDIT{j}"))
                                    treesToMoveEdit.Add(new Tuple<int, bool>(j, true));
                                ImGui.SameLine();
                                if (ImGui.Button($"v##MOVERULEDOWNEDIT{j}"))
                                    treesToMoveEdit.Add(new Tuple<int, bool>(j, false));
                                ImGui.NextColumn();
                                ImGui.PushItemWidth(ImGui.GetContentRegionAvail().X);
                                Settings.SelectedBuild.Trees[j].Tag =
                                    ImGuiExtension.InputText($"##TAG{j}",
                                        Settings.SelectedBuild.Trees[j].Tag, 1024,
                                        ImGuiInputTextFlags.AutoSelectAll).TrimEnd('\0').TrimEnd('\u0000').Replace("\u0000", "");
                                ImGui.PopItemWidth();
                                //ImGui.SameLine();
                                ImGui.NextColumn();
                                ImGui.PushItemWidth(ImGui.GetContentRegionAvail().X);
                                Settings.SelectedBuild.Trees[j].SkillTreeUrl =
                                    ImGuiExtension.InputText($"##GN{j}",
                                        Settings.SelectedBuild.Trees[j].SkillTreeUrl, 1024,
                                        ImGuiInputTextFlags.AutoSelectAll).TrimEnd('\0').TrimEnd('\u0000').Replace("\u0000", "");
                                ImGui.NextColumn();
                                ImGui.PopItemWidth();
                            }

                            foreach (var i in treesToRemove)
                                Settings.SelectedBuild.Trees.Remove(Settings.SelectedBuild.Trees[i]);

                            ImGui.Separator();
                            ImGui.Columns(1, "", false);
                            if (ImGui.Button($"+##AN"))
                                Settings.SelectedBuild.Trees.Add(new TreeConfig.Tree());
                            ImGui.Separator();

                            foreach (var i in treesToMoveEdit)
                                if (i.Item2)
                                {
                                    // Move Up
                                    if (i.Item1 > 0)
                                    {
                                        var itemAbove = Settings.SelectedBuild.Trees[i.Item1 - 1];
                                        var itemMoving = Settings.SelectedBuild.Trees[i.Item1];

                                        Settings.SelectedBuild.Trees[i.Item1 - 1] = itemMoving;
                                        Settings.SelectedBuild.Trees[i.Item1] = itemAbove;
                                    }
                                }
                                else
                                {
                                    // Move Down                                
                                    if (i.Item1 < Settings.SelectedBuild.Trees.Count - 1)
                                    {
                                        var itemBelow = Settings.SelectedBuild.Trees[i.Item1 + 1];
                                        var itemMoving = Settings.SelectedBuild.Trees[i.Item1];

                                        Settings.SelectedBuild.Trees[i.Item1 + 1] = itemMoving;
                                        Settings.SelectedBuild.Trees[i.Item1] = itemBelow;
                                    }
                                }
                            //ImGui.Separator();

                            CurrentlySelectedBuildFileEdit = ImGuiExtension.InputText("##RenameLabel",
                                CurrentlySelectedBuildFileEdit, 200, ImGuiInputTextFlags.None);
                            ImGui.SameLine();
                            if (ImGui.Button("Rename Build"))
                            {
                                RenameFile(CurrentlySelectedBuildFileEdit, Settings.SelectedURLFile);
                                BuildFiles = TreeConfig.GetBuilds();
                            }

                            if (ImGui.Button($"Save Build to File: {Settings.SelectedURLFile}"))
                            {
                                TreeConfig.SaveSettingFile($@"{SkillTreeUrlFilesDir}\{Settings.SelectedURLFile}", Settings.SelectedBuild);
                                BuildFiles = TreeConfig.GetBuilds();
                            }
                        }
                        else
                        {
                            ImGui.Text("No Data Selected");
                        }

                        break;
                    case "Build Add":
                        var treesToRemoveCreation = new List<int>();
                        var treesToMove = new List<Tuple<int, bool>>();

                        ImGui.Separator();
                        Settings.SelectedBuildCreating.BuildLink =
                            ImGuiExtension.InputText("Forum Thread",
                                Settings.SelectedBuildCreating.BuildLink,
                                1024, ImGuiInputTextFlags.None);

                        ImGui.Text("Notes");
                        // Keep at max 4k byte size not sure why it crashes when upped, not going to bother dealing with this either.
                        ImGuiExtension.MultiLineTextBox("##NotesAdd",
                            Settings.SelectedBuildCreating.Notes, 4000, new Vector2(newcontentRegionArea.X - 20, 200),
                            ImGuiInputTextFlags.Multiline);

                        ImGui.Separator();
                        ImGui.Columns(4, "AddColums", true);
                        ImGui.SetColumnWidth(0, 30f);
                        ImGui.SetColumnWidth(1, 50f);
                        ImGui.Text("");
                        ImGui.NextColumn();
                        ImGui.Text("Move");
                        ImGui.NextColumn();
                        ImGui.Text("Tree Name");
                        ImGui.NextColumn();
                        ImGui.Text("Skill Tree");
                        ImGui.NextColumn();
                        if (Settings.SelectedBuildCreating.Trees.Count != 0)
                            ImGui.Separator();
                        for (var j = 0; j < Settings.SelectedBuildCreating.Trees.Count; j++)
                        {
                            if (ImGui.Button($"X##REMOVERULE{j}"))
                                treesToRemoveCreation.Add(j);

                            ImGui.NextColumn();
                            if (ImGui.Button($"^##MOVERULEUP{j}"))
                                treesToMove.Add(new Tuple<int, bool>(j, true));
                            ImGui.SameLine();
                            if (ImGui.Button($"v##MOVERULEDOWN{j}"))
                                treesToMove.Add(new Tuple<int, bool>(j, false));
                            ImGui.NextColumn();
                            ImGui.PushItemWidth(ImGui.GetWindowContentRegionWidth());
                            Settings.SelectedBuildCreating.Trees[j].Tag = ImGuiExtension.InputText($"##TAGADD{j}",
                                    Settings.SelectedBuildCreating.Trees[j].Tag, 1024, ImGuiInputTextFlags.AutoSelectAll);
                            ImGui.PopItemWidth();
                            //ImGui.SameLine();
                            ImGui.NextColumn();
                            ImGui.PushItemWidth(ImGui.GetWindowContentRegionWidth());
                            Settings.SelectedBuildCreating.Trees[j].SkillTreeUrl =
                               ImGuiExtension.InputText($"##GNADD{j}",
                                    Settings.SelectedBuildCreating.Trees[j].SkillTreeUrl, 1024,
                                    ImGuiInputTextFlags.AutoSelectAll);
                            ImGui.PopItemWidth();
                            ImGui.NextColumn();
                        }

                        foreach (var i in treesToRemoveCreation)
                            Settings.SelectedBuildCreating.Trees.Remove(Settings.SelectedBuildCreating.Trees[i]);

                        ImGui.Separator();
                        ImGui.Columns(1, "", false);
                        if (ImGui.Button($"+##AN"))
                            Settings.SelectedBuildCreating.Trees.Add(new TreeConfig.Tree());
                        ImGui.Separator();


                        foreach (var i in treesToMove)
                            if (i.Item2)
                            {
                                // Move Up
                                if (i.Item1 > 0)
                                {
                                    var itemAbove = Settings.SelectedBuildCreating.Trees[i.Item1 - 1];
                                    var itemMoving = Settings.SelectedBuildCreating.Trees[i.Item1];

                                    Settings.SelectedBuildCreating.Trees[i.Item1 - 1] = itemMoving;
                                    Settings.SelectedBuildCreating.Trees[i.Item1] = itemAbove;
                                }
                            }
                            else
                            {
                                // Move Down                                
                                if (i.Item1 < Settings.SelectedBuildCreating.Trees.Count - 1)
                                {
                                    var itemBelow = Settings.SelectedBuildCreating.Trees[i.Item1 + 1];
                                    var itemMoving = Settings.SelectedBuildCreating.Trees[i.Item1];

                                    Settings.SelectedBuildCreating.Trees[i.Item1 + 1] = itemMoving;
                                    Settings.SelectedBuildCreating.Trees[i.Item1] = itemBelow;
                                }
                            }

                        //ImGui.Separator();
                        AddNewBuildFile = ImGuiExtension.InputText("##CreationLabel", AddNewBuildFile, 1024, ImGuiInputTextFlags.EnterReturnsTrue);
                        if (ImGui.Button($"Save Build to File: {AddNewBuildFile}"))
                        {
                            TreeConfig.SaveSettingFile($@"{SkillTreeUrlFilesDir}\{AddNewBuildFile}", Settings.SelectedBuildCreating);
                            BuildFiles = TreeConfig.GetBuilds();
                            Settings.SelectedBuildCreating = new TreeConfig.SkillTreeData();
                        }

                        break;
                    case "Colors":
                        Settings.PickedBorderColor.Value =
                            ImGuiExtension.ColorPicker("Border Color", Settings.PickedBorderColor);
                        Settings.UnpickedBorderColor.Value =
                            ImGuiExtension.ColorPicker("Unpicked Border Color", Settings.UnpickedBorderColor);
                        Settings.WrongPickedBorderColor.Value = ImGuiExtension.ColorPicker("Wrong picked Border Color",
                            Settings.WrongPickedBorderColor);
                        Settings.LineColor.Value = ImGuiExtension.ColorPicker("Line Color", Settings.LineColor);
                        break;
                    case "Sliders":
                        Settings.offsetX.Value = ImGuiExtension.IntSlider("offsetX", Settings.offsetX);
                        Settings.offsetY.Value = ImGuiExtension.IntSlider("offsetY", Settings.offsetY);

                        Settings.PickedBorderWidth.Value = ImGuiExtension.IntSlider("Picked Border Width", Settings.PickedBorderWidth);
                        Settings.UnpickedBorderWidth.Value = ImGuiExtension.IntSlider("Unpicked Border Width", Settings.UnpickedBorderWidth);
                        Settings.WrongPickedBorderWidth.Value = ImGuiExtension.IntSlider("WrongPicked Border Width", Settings.WrongPickedBorderWidth);
                        Settings.LineWidth.Value = ImGuiExtension.IntSlider("Line Width", Settings.LineWidth);
                        break;
                    case "Toggles":
                        Settings.EnableEzTreeChanger.Value = ImGuiExtension.Checkbox("Enable EZ Tree Changer Within Builds", Settings.EnableEzTreeChanger);
                        break;
                }

            ImGui.PopStyleVar();
            ImGui.EndChild();
        }

        private void ReadHtmlLineFromFile(string fileName)
        {
            var skillTreeUrlFilePath = Path.Combine(SkillTreeUrlFilesDir, fileName + ".txt");
            if (!File.Exists(skillTreeUrlFilePath))
            {
                LogMessage("PassiveSkillTree: Select build url from list in options.", 10);
                return;
            }

            var strings = File.ReadAllLines(skillTreeUrlFilePath);
            if (strings.Length == 1 || strings[1] == string.Empty)
            {
                LogMessage("PassiveSkillTree: This build has no saved Forum link.", 10);
                return;
            }

            if (strings[1] != null && strings[1] != string.Empty)
                Process.Start(strings[1]);
        }

        private string GetHtmlLineFromFile(string fileName)
        {
            var skillTreeUrlFilePath = Path.Combine(SkillTreeUrlFilesDir, fileName + ".txt");
            if (!File.Exists(skillTreeUrlFilePath))
                return string.Empty;

            var strings = File.ReadAllLines(skillTreeUrlFilePath);
            if (strings.Length == 1)
                return string.Empty;

            if (strings[1] != null && strings[1] != string.Empty)
                return strings[1];

            return string.Empty;
        }

        private void ReadUrlFromSelectedBuild(string url, string treeName)
        {
            var skillTreeUrl = url;

            // replaces the game tree version "x.x.x/"
            var rgx = new Regex("^https:\\/\\/www.pathofexile.com\\/fullscreen-passive-skill-tree\\/(([0-9]\\W){3})",
                RegexOptions.IgnoreCase);
            var match = rgx.Match(skillTreeUrl);
            if (match.Success)
                skillTreeUrl = skillTreeUrl.Replace(match.Groups[1].Value, "");

            rgx = new Regex("^https:\\/\\/www.pathofexile.com\\/passive-skill-tree\\/(([0-9]\\W){3})",
                RegexOptions.IgnoreCase);
            match = rgx.Match(skillTreeUrl);
            if (match.Success)
                skillTreeUrl = skillTreeUrl.Replace(match.Groups[1].Value, "");

            // remove ?accountName and such off the end of the string
            skillTreeUrl = RemoveAccName(skillTreeUrl);
            if (!DecodeUrl(skillTreeUrl))
            {
                LogMessage("PassiveSkillTree: Can't decode url from file: <NEW FUNCTION>", 10);
                return;
            }
            CurrentlySelectedBuildUrl = url;
            Settings.SelectedURL = url;
            Settings.SelectedTreeName = treeName;
            ProcessNodes();
        }

        private void ReadUrlFromSelectedUrl(string fileName)
        {
            var skillTreeUrl = Settings.SelectedURL;

            // replaces the game tree version "x.x.x/"
            var rgx = new Regex("^https:\\/\\/www.pathofexile.com\\/fullscreen-passive-skill-tree\\/(([0-9]\\W){3})",
                RegexOptions.IgnoreCase);
            var match = rgx.Match(skillTreeUrl);
            if (match.Success)
                skillTreeUrl = skillTreeUrl.Replace(match.Groups[1].Value, "");

            rgx = new Regex("^https:\\/\\/www.pathofexile.com\\/passive-skill-tree\\/(([0-9]\\W){3})",
                RegexOptions.IgnoreCase);
            match = rgx.Match(skillTreeUrl);
            if (match.Success)
                skillTreeUrl = skillTreeUrl.Replace(match.Groups[1].Value, "");

            // remove ?accountName and such off the end of the string
            skillTreeUrl = RemoveAccName(skillTreeUrl);
            if (!DecodeUrl(skillTreeUrl))
            {
                LogMessage("PassiveSkillTree: Can't decode url from file: <TEST>", 10);
                return;
            }

            CurrentlySelectedBuildFile = fileName;
            CurrentlySelectedBuildFileEdit = fileName;
            CurrentlySelectedBuildUrl = Settings.SelectedURL;
            CurrentlySelectedBuildForumThread = Settings.SelectedBuild.BuildLink;
            ProcessNodes();
        }

        private List<string> ReadNotesFromSelectedBuild(string fileName)
        {
            var BuildFilePath = Path.Combine(SkillTreeUrlFilesDir, fileName + ".txt");
            var tempList = new List<string>();
            if (!File.Exists(BuildFilePath))
                return tempList;

            var strings = File.ReadAllLines(BuildFilePath);
            if (strings.Length <= 2) return tempList;
            for (var i = 2; i < strings.Length; i++)
            {
                var line = strings[i];
                var replace = line.Replace('–', '-');
                tempList.Add(replace);
            }

            return tempList;
        }

        private void ProcessNodes()
        {
            _drawNodes = new List<SkillNode>();

            //Read data
            var skillTreeDataPath = DirectoryFullName + @"\" + SkillTreeDataFile;
            if (!File.Exists(skillTreeDataPath))
            {
                LogMessage("PassiveSkillTree: Can't find file " + SkillTreeDataFile + " with skill tree data.", 10);
                return;
            }

            var skillTreeJson = File.ReadAllText(skillTreeDataPath);
            _skillTreeeData.Decode(skillTreeJson);
            foreach (var urlNodeId in _urlNodes)
            {
                if (!_skillTreeeData.Skillnodes.ContainsKey(urlNodeId))
                {
                    LogError("PassiveSkillTree: Can't find passive skill tree node with id: " + urlNodeId, 5);
                    continue;
                }

                var node = _skillTreeeData.Skillnodes[urlNodeId];
                node.Init();
                _drawNodes.Add(node);
                var dontDrawLinesTwice = new List<ushort>();
                foreach (var lNodeId in node.linkedNodes)
                {
                    if (!_urlNodes.Contains(lNodeId))
                        continue;

                    if (dontDrawLinesTwice.Contains(lNodeId))
                        continue;

                    if (!_skillTreeeData.Skillnodes.ContainsKey(lNodeId))
                    {
                        LogError(
                            "PassiveSkillTree: Can't find passive skill tree node with id: " + lNodeId +
                            " to draw the link", 5);
                        continue;
                    }

                    var lNode = _skillTreeeData.Skillnodes[lNodeId];
                    node.DrawNodeLinks.Add(lNode.Position);
                }

                dontDrawLinesTwice.Add(urlNodeId);
            }
        }


        private bool DecodeUrl(string url)
        {
            if (PoePlannerUrlDecoder.UrlMatch(url))
            {
                _urlNodes = PoePlannerUrlDecoder.Decode(url);
                return true;
            }

            if (PathOfExileUrlDecoder.UrlMatch(url))
            {
                _urlNodes = PathOfExileUrlDecoder.Decode(url);
                return true;
            }

            return false;
        }

        private async void DownloadTree()
        {
            var skillTreeDataPath = DirectoryFullName + @"\" + SkillTreeDataFile;
            await PassiveSkillTreeJson_Downloader.DownloadSkillTreeToFileAsync(skillTreeDataPath);
            LogMessage("Skill tree updated!", 3);
        }

        private void ExtRender()
        {
            if (_drawNodes.Count == 0) return;
            if (!GameController.InGame ||
                !WinApi.IsForegroundWindow(GameController.Window.Process.MainWindowHandle)) return;
            var treePanel = GameController.Game.IngameState.IngameUi.TreePanel;
            if (!treePanel.IsVisible)
            {
                _bUiRootInitialized = false;
                _uiSkillTreeBase = null;
                return;
            }

            if (!_bUiRootInitialized)
            {
                _bUiRootInitialized = true;
                //I still can't find offset for Skill Data root, so I made it by checking
                foreach (var child in treePanel.Children) //Only 8 childs for check
                    if (child.Width > 20000 && child.Width < 30000)
                    {
                        _uiSkillTreeBase = child;
                        break;
                    }
            }

            if (_uiSkillTreeBase == null)
            {
                LogError("Can't find UiSkillTreeBase root!", 0);
                return;
            }

            var scale = _uiSkillTreeBase.Scale;

            //Hand-picked values
            var passives = GameController.Game.IngameState.ServerData.PassiveSkillIds;

            var totalNodes = _drawNodes.Count;
            var pickedNodes = passives.Count;
            var wrongPicked = 0;

            foreach (var node in _drawNodes)
            {
                var drawSize = node.DrawSize * scale;
                var posX = (_uiSkillTreeBase.X + node.DrawPosition.X + Settings.offsetX) * scale;
                var posY = (_uiSkillTreeBase.Y + node.DrawPosition.Y + Settings.offsetY) * scale;

                var color = Settings.PickedBorderColor;
                var vWidth = Settings.PickedBorderWidth.Value;
                if (!passives.Contains(node.Id))
                {
                    vWidth = Settings.UnpickedBorderWidth.Value;
                    color = Settings.UnpickedBorderColor;
                }
                else
                {
                    passives.Remove(node.Id);
                }

                //Graphics.DrawPluginImage(Path.Combine(DirectoryFullName, "images/AtlasMapCircle.png"),
                //new RectangleF(posX - drawSize / 2, posY - drawSize / 2, drawSize, drawSize), color);
                Graphics.DrawImage(_ringImage, new RectangleF(posX - drawSize / 2, posY - drawSize / 2, drawSize, drawSize), color);

                if (Settings.LineWidth > 0)
                    foreach (var link in node.DrawNodeLinks)
                    {
                        var linkDrawPosX = (_uiSkillTreeBase.X + link.X + Settings.offsetX) * scale;
                        var linkDrawPosY = (_uiSkillTreeBase.Y + link.Y + Settings.offsetY) * scale;

                        Graphics.DrawLine(new SharpDX.Vector2(posX, posY),
                            new SharpDX.Vector2(linkDrawPosX, linkDrawPosY), Settings.LineWidth, Settings.LineColor);
                    }
            }

            wrongPicked = passives.Count;
            foreach (var passiveId in passives)
                if (_skillTreeeData.Skillnodes.TryGetValue(passiveId, out var node))
                {
                    node.Init();
                    var drawSize = node.DrawSize * scale;
                    var posX = (_uiSkillTreeBase.X + node.DrawPosition.X + Settings.offsetX) * scale;
                    var posY = (_uiSkillTreeBase.Y + node.DrawPosition.Y + Settings.offsetY) * scale;

                    Graphics.DrawLine(new SharpDX.Vector2(posX, posY), new SharpDX.Vector2(posX, posY),
                        Settings.LineWidth, Settings.WrongPickedBorderColor);
                    //Graphics.DrawPluginImage(Path.Combine(DirectoryFullName, "images/AtlasMapCircle.png"),
                    //new RectangleF(posX - drawSize / 2, posY - drawSize / 2, drawSize, drawSize),
                    //Settings.WrongPickedBorderColor);
                    Graphics.DrawImage(_ringImage, new RectangleF(posX - drawSize / 2, posY - drawSize / 2, drawSize, drawSize), Settings.WrongPickedBorderColor);
                }


            var textPos = new SharpDX.Vector2(50, 300);
            Graphics.DrawText("Total Tree Nodes: " + totalNodes, textPos, Color.White, 15);
            textPos.Y += 20;
            Graphics.DrawText("Picked Nodes: " + pickedNodes, textPos, Color.Green, 15);
            textPos.Y += 20;
            Graphics.DrawText("Wrong Picked Nodes: " + wrongPicked, textPos, Color.Red, 15);
        }

        //public bool IsAllocated(ulong passiveID)
        //{
        //    foreach (var passive in GameController.Player.GetComponent<Player>().AllocatedPassives)
        //        if ((int) passiveID == passive.PassiveId)
        //            return true;

        //    return false;
        //}
    }
}