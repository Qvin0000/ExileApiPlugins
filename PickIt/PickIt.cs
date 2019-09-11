using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Exile;
using Exile.PoEMemory.MemoryObjects;
using Shared;
using Shared.Helpers;
using PoEMemory;
using PoEMemory.Components;
using Shared.Enums;
using SharpDX;
using Input = Exile.Input;

namespace PickIt
{
    public class PickIt : BaseSettingsPlugin<PickItSettings>
    {
        //https://stackoverflow.com/questions/826777/how-to-have-an-auto-incrementing-version-number-visual-studio
        public Version Version { get; } = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
        public string PluginVersion { get; set; }
        public DateTime buildDate;
        private const string PickitRuleDirectory = "Pickit Rules";
        private readonly List<Entity> _entities = new List<Entity>();
        private readonly Stopwatch _pickUpTimer = Stopwatch.StartNew();
        private readonly Stopwatch DebugTimer = Stopwatch.StartNew();
        private Vector2 _clickWindowOffset;
        private HashSet<string> _magicRules;
        private HashSet<string> _normalRules;
        private HashSet<string> _rareRules;
        private HashSet<string> _uniqueRules;
        private Dictionary<string, int> _weightsRules = new Dictionary<string, int>();
        public string MagicRuleFile;
        public string NormalRuleFile;
        public string RareRuleFile;
        public string UniqueRuleFile;
        private bool CustomRulesExists = true;

        public PickIt() => Name = "Pickit";
        private List<string> PickitFiles { get; set; }


        public override bool Initialise() {
            buildDate = new DateTime(2000, 1, 1).AddDays(Version.Build).AddSeconds(Version.Revision * 2);
            PluginVersion = $"{Version}";
            pickItCoroutine = new Coroutine(MainWorkCoroutine(), this, "Pick It");
            Core.ParallelRunner.Run(pickItCoroutine);
            pickItCoroutine.Pause();
            DebugTimer.Reset();
            Settings.MouseSpeed.OnValueChanged += (sender, f) => { Mouse.speedMouse = Settings.MouseSpeed.Value; };
            _workCoroutine = new WaitTime(Settings.ExtraDelay);
            Settings.ExtraDelay.OnValueChanged += (sender, i) => _workCoroutine = new WaitTime(i);
            LoadRuleFiles();
            return true;
        }

        private Coroutine pickItCoroutine;

        private bool FullWork = true;
        private uint coroutineCounter = 0;
        private Vector2 cursorBeforePickIt;

        private WaitTime mainWorkCoroutine = new WaitTime(5);
        private WaitTime tryToPick = new WaitTime(7);
        private WaitTime wait3ms = new WaitTime(3);
        private WaitTime toPick = new WaitTime(10);
        private WaitTime waitPlayerMove = new WaitTime(10);
        private WaitTime waitForNextTry = new WaitTime(33);
        private WaitTime _workCoroutine;

        private IEnumerator MainWorkCoroutine() {
            while (true)
            {
                yield return FindItemToPick();

                coroutineCounter++;
                pickItCoroutine.UpdateTicks(coroutineCounter);
                yield return _workCoroutine;
            }
        }

        /*public override void DrawSettings()
        {
            ImGui.BulletText($"v{PluginVersion}");
            ImGui.BulletText($"Last Updated: {buildDate}");
            Settings.PickUpKey = ImGuiExtension.HotkeySelector("Pickup Key", Settings.PickUpKey);
            Settings.LeftClickToggleNode.Value = ImGuiExtension.Checkbox("Mouse Button: " + (Settings.LeftClickToggleNode ? "Left" : "Right"), Settings.LeftClickToggleNode);
            Settings.LeftClickToggleNode.Value = ImGuiExtension.Checkbox("Return Mouse To Position Before Click", Settings.ReturnMouseToBeforeClickPosition);
            Settings.GroundChests.Value = ImGuiExtension.Checkbox("Click Chests If No Items Around", Settings.GroundChests);
            Settings.PickupRange.Value = ImGuiExtension.IntSlider("Pickup Radius", Settings.PickupRange);
            Settings.ChestRange.Value = ImGuiExtension.IntSlider("Chest Radius", Settings.ChestRange);
            Settings.ExtraDelay.Value = ImGuiExtension.IntSlider("Extra Click Delay", Settings.ExtraDelay);
            Settings.MouseSpeed.Value = ImGuiExtension.FloatSlider("Mouse speed", Settings.MouseSpeed);
            Settings.TimeBeforeNewClick.Value = ImGuiExtension.IntSlider("Time wait for new click", Settings.TimeBeforeNewClick);
            //Settings.OverrideItemPickup.Value = ImGuiExtension.Checkbox("Item Pickup Override", Settings.OverrideItemPickup); ImGui.SameLine();
            //ImGuiExtension.ToolTip("Override item.CanPickup\n\rDO NOT enable this unless you know what you're doing!");
            
            var tempRef = false;
            if (ImGui.CollapsingHeader("Pickit Rules", TreeNodeFlags.Framed | TreeNodeFlags.DefaultOpen))
            {
                if (ImGui.Button("Reload All Files")) LoadRuleFiles();
                Settings.NormalRuleFile = ImGuiExtension.ComboBox("Normal Rules", Settings.NormalRuleFile, PickitFiles, out tempRef);
                if (tempRef) _normalRules = LoadPickit(Settings.NormalRuleFile);
                Settings.MagicRuleFile = ImGuiExtension.ComboBox("Magic Rules", Settings.MagicRuleFile, PickitFiles, out tempRef);
                if (tempRef) _magicRules = LoadPickit(Settings.MagicRuleFile);
                Settings.RareRuleFile = ImGuiExtension.ComboBox("Rare Rules", Settings.RareRuleFile, PickitFiles, out tempRef);
                if (tempRef) _rareRules = LoadPickit(Settings.RareRuleFile);
                Settings.UniqueRuleFile = ImGuiExtension.ComboBox("Unique Rules", Settings.UniqueRuleFile, PickitFiles, out tempRef);
                if (tempRef) _uniqueRules = LoadPickit(Settings.UniqueRuleFile);
            }

            if (ImGui.CollapsingHeader("Item Logic", TreeNodeFlags.Framed | TreeNodeFlags.DefaultOpen))
            {
                Settings.ShaperItems.Value = ImGuiExtension.Checkbox("Pickup Shaper Items", Settings.ShaperItems);
                ImGui.SameLine();
                Settings.ElderItems.Value = ImGuiExtension.Checkbox("Pickup Elder Items", Settings.ElderItems);
                if (ImGui.TreeNode("Links/Sockets/RGB"))
                {
                    Settings.RGB.Value = ImGuiExtension.Checkbox("RGB Items", Settings.RGB);
                    Settings.TotalSockets.Value = ImGuiExtension.IntSlider("##Sockets", Settings.TotalSockets);
                    ImGui.SameLine();
                    Settings.Sockets.Value = ImGuiExtension.Checkbox("Sockets", Settings.Sockets);
                    Settings.LargestLink.Value = ImGuiExtension.IntSlider("##Links", Settings.LargestLink);
                    ImGui.SameLine();
                    Settings.Links.Value = ImGuiExtension.Checkbox("Links", Settings.Links);
                    ImGui.Separator();
                    ImGui.TreePop();
                }

                if (ImGui.TreeNode("Overrides"))
                {
                    Settings.PickUpEverything.Value = ImGuiExtension.Checkbox("Pickup Everything", Settings.PickUpEverything);
                    Settings.AllDivs.Value = ImGuiExtension.Checkbox("All Divination Cards", Settings.AllDivs);
                    Settings.AllCurrency.Value = ImGuiExtension.Checkbox("All Currency", Settings.AllCurrency);
                    Settings.AllUniques.Value = ImGuiExtension.Checkbox("All Uniques", Settings.AllUniques);
                    Settings.QuestItems.Value = ImGuiExtension.Checkbox("Quest Items", Settings.QuestItems);
                    Settings.Maps.Value = ImGuiExtension.Checkbox("##Maps", Settings.Maps);
                    ImGui.SameLine();
                    if (ImGui.TreeNode("Maps"))
                    {
                        Settings.MapTier.Value = ImGuiExtension.IntSlider("Lowest Tier", Settings.MapTier);
                        Settings.UniqueMap.Value = ImGuiExtension.Checkbox("All Unique Maps", Settings.UniqueMap);
                        Settings.MapFragments.Value = ImGuiExtension.Checkbox("Fragments", Settings.MapFragments);
                        ImGui.Spacing();
                        ImGui.TreePop();
                    }

                    Settings.GemQuality.Value = ImGuiExtension.IntSlider("##Gems", "Lowest Quality", Settings.GemQuality);
                    ImGui.SameLine();
                    Settings.Gems.Value = ImGuiExtension.Checkbox("Gems", Settings.Gems);
                    ImGui.Separator();
                    ImGui.TreePop();
                }

                Settings.Rares.Value = ImGuiExtension.Checkbox("##Rares", Settings.Rares);
                ImGui.SameLine();
                if (ImGui.TreeNode("Rares##asd"))
                {
                    Settings.RareJewels.Value = ImGuiExtension.Checkbox("Jewels", Settings.RareJewels);
                    Settings.RareRingsilvl.Value = ImGuiExtension.IntSlider("##RareRings", "Lowest iLvl", Settings.RareRingsilvl);
                    ImGui.SameLine();
                    Settings.RareRings.Value = ImGuiExtension.Checkbox("Rings", Settings.RareRings);
                    Settings.RareAmuletsilvl.Value = ImGuiExtension.IntSlider("##RareAmulets", "Lowest iLvl", Settings.RareAmuletsilvl);
                    ImGui.SameLine();
                    Settings.RareAmulets.Value = ImGuiExtension.Checkbox("Amulets", Settings.RareAmulets);
                    Settings.RareBeltsilvl.Value = ImGuiExtension.IntSlider("##RareBelts", "Lowest iLvl", Settings.RareBeltsilvl);
                    ImGui.SameLine();
                    Settings.RareBelts.Value = ImGuiExtension.Checkbox("Belts", Settings.RareBelts);
                    Settings.RareGlovesilvl.Value = ImGuiExtension.IntSlider("##RareGloves", "Lowest iLvl", Settings.RareGlovesilvl);
                    ImGui.SameLine();
                    Settings.RareGloves.Value = ImGuiExtension.Checkbox("Gloves", Settings.RareGloves);
                    Settings.RareBootsilvl.Value = ImGuiExtension.IntSlider("##RareBoots", "Lowest iLvl", Settings.RareBootsilvl);
                    ImGui.SameLine();
                    Settings.RareBoots.Value = ImGuiExtension.Checkbox("Boots", Settings.RareBoots);
                    Settings.RareHelmetsilvl.Value = ImGuiExtension.IntSlider("##RareHelmets", "Lowest iLvl", Settings.RareHelmetsilvl);
                    ImGui.SameLine();
                    Settings.RareHelmets.Value = ImGuiExtension.Checkbox("Helmets", Settings.RareHelmets);
                    Settings.RareArmourilvl.Value = ImGuiExtension.IntSlider("##RareArmours", "Lowest iLvl", Settings.RareArmourilvl);
                    ImGui.SameLine();
                    Settings.RareArmour.Value = ImGuiExtension.Checkbox("Armours", Settings.RareArmour);
                    ImGui.Spacing();
                    Settings.RareWeaponilvl.Value = ImGuiExtension.IntSlider("##RareWeapons", "Lowest iLvl", Settings.RareWeaponilvl);
                    ImGui.SameLine();
                    Settings.RareWeapon.Value = ImGuiExtension.Checkbox("Weapons", Settings.RareWeapon);
                    Settings.RareShieldilvl.Value = ImGuiExtension.IntSlider("##Shields", "Lowest iLvl", Settings.RareWeaponilvl);
                    ImGui.SameLine();
                    Settings.RareShield.Value = ImGuiExtension.Checkbox("Shields", Settings.RareWeapon);
                    Settings.RareWeaponWidth.Value = ImGuiExtension.IntSlider("Maximum Width##RareWeaponWidth", Settings.RareWeaponWidth);
                    Settings.RareWeaponHeight.Value = ImGuiExtension.IntSlider("Maximum Height##RareWeaponHeight", Settings.RareWeaponHeight);
                    Settings.ItemCells.Value = ImGuiExtension.IntSlider("Maximum Cells##RareWeaponCell", Settings.ItemCells);
                    ImGui.TreePop();
                }
            }
        }*/

        public override Job Tick() {
            if (Input.GetKeyState(Keys.Escape)) pickItCoroutine.Pause();

            if (Input.GetKeyState(Settings.PickUpKey.Value))
            {
                DebugTimer.Restart();
                if (pickItCoroutine.IsDone)
                {
                    var firstOrDefault = Core.ParallelRunner.Coroutines.FirstOrDefault(x => x.OwnerName == nameof(PickIt));
                    if (firstOrDefault != null)
                        pickItCoroutine = firstOrDefault;
                }

                pickItCoroutine.Resume();
                FullWork = false;
            }
            else
            {
                if (FullWork)
                {
                    pickItCoroutine.Pause();
                    DebugTimer.Reset();
                }
            }

            if (DebugTimer.ElapsedMilliseconds > 2000)
            {
                FullWork = true;
                LogMessage("Error pick it stop after time limit 2000 ms", 1);
                DebugTimer.Reset();
            }
            return null;
        }

        public bool InCustomList(HashSet<string> checkList, CustomItem itemEntity, ItemRarity rarity) {
            if (checkList.Contains(itemEntity.BaseName) && itemEntity.Rarity == rarity) return true;
            return false;
        }


        public bool OverrideChecks(CustomItem item) {
            try
            {
                #region Currency

                if (Settings.AllCurrency && item.ClassName.EndsWith("Currency"))
                    return !item.Path.Equals("Metadata/Items/Currency/CurrencyIdentification", StringComparison.Ordinal) ||
                           !Settings.IgnoreScrollOfWisdom;

                #endregion

                #region Shaper & Elder

                if (Settings.ElderItems)
                {
                    if (item.IsElder)
                        return true;
                }

                if (Settings.ShaperItems)
                {
                    if (item.IsShaper)
                        return true;
                }

                if (Settings.FracturedItems)
                {
                    if (item.IsFractured)
                        return true;
                }

                #endregion

                #region Rare Overrides

                if (Settings.Rares && item.Rarity == ItemRarity.Rare)
                {
                    if (Settings.RareJewels && (item.ClassName == "Jewel" || item.ClassName == "AbyssJewel")) return true;
                    if (Settings.RareRings && item.ClassName == "Ring" && item.ItemLevel >= Settings.RareRingsilvl) return true;
                    if (Settings.RareAmulets && item.ClassName == "Amulet" && item.ItemLevel >= Settings.RareAmuletsilvl) return true;
                    if (Settings.RareBelts && item.ClassName == "Belt" && item.ItemLevel >= Settings.RareBeltsilvl) return true;
                    if (Settings.RareGloves && item.ClassName == "Gloves" && item.ItemLevel >= Settings.RareGlovesilvl) return true;
                    if (Settings.RareBoots && item.ClassName == "Boots" && item.ItemLevel >= Settings.RareBootsilvl) return true;
                    if (Settings.RareHelmets && item.ClassName == "Helmet" && item.ItemLevel >= Settings.RareHelmetsilvl) return true;
                    if (Settings.RareArmour && item.ClassName == "Body Armour" && item.ItemLevel >= Settings.RareArmourilvl) return true;
                    if (Settings.RareWeapon && item.IsWeapon && item.ItemLevel >= Settings.RareWeaponilvl &&
                        item.Width * item.Height <= Settings.ItemCells) return true;
                    if (Settings.RareWeapon && item.IsWeapon && item.ItemLevel >= Settings.RareWeaponilvl &&
                        item.Width <= Settings.RareWeaponWidth && item.Height <= Settings.RareWeaponHeight) return true;
                    if (Settings.RareShield && item.ClassName == "Shield" && item.ItemLevel >= Settings.RareShieldilvl &&
                        item.Width * item.Height <= Settings.ItemCells) return true;
                }

                #endregion

                #region Sockets/Links/RGB

                if (Settings.Sockets && item.Sockets >= Settings.TotalSockets.Value) return true;
                if (Settings.Links && item.LargestLink >= Settings.LargestLink) return true;
                if (Settings.RGB && item.IsRGB) return true;

                #endregion

                #region Divination Cards

                if (Settings.AllDivs && item.ClassName == "DivinationCard") return true;

                #endregion


                #region Maps

                if (Settings.Maps && item.MapTier >= Settings.MapTier.Value) return true;
                if (Settings.Maps && Settings.UniqueMap && item.MapTier >= 1 && item.Rarity == ItemRarity.Unique) return true;
                if (Settings.Maps && Settings.MapFragments && item.ClassName == "MapFragment") return true;

                #endregion

                #region Quest Items

                if (Settings.QuestItems && item.ClassName == "QuestItem") return true;

                #endregion

                #region Skill Gems

                if (Settings.Gems && item.Quality >= Settings.GemQuality.Value && item.ClassName.Contains("Skill Gem")) return true;

                #endregion

                #region Uniques

                if (Settings.AllUniques && item.Rarity == ItemRarity.Unique) return true;

                #endregion
            }
            catch (Exception e)
            {
               LogError($"{nameof(OverrideChecks)} error: {e}");
            }

            return false;
        }

        public bool DoWePickThis(CustomItem itemEntity) {
            var pickItemUp = false;

            #region Force Pickup All

            if (Settings.PickUpEverything) return true;

            #endregion

            #region Rarity Rule Switch

            if (CustomRulesExists)
            {
                switch (itemEntity.Rarity)
                {
                    case ItemRarity.Normal:
                        if (_normalRules != null)
                            if (InCustomList(_normalRules, itemEntity, itemEntity.Rarity))
                                pickItemUp = true;
                        break;
                    case ItemRarity.Magic:
                        if (_magicRules != null)
                            if (InCustomList(_magicRules, itemEntity, itemEntity.Rarity))
                                pickItemUp = true;
                        break;
                    case ItemRarity.Rare:
                        if (_rareRules != null)
                            if (InCustomList(_rareRules, itemEntity, itemEntity.Rarity))
                                pickItemUp = true;
                        break;
                    case ItemRarity.Unique:
                        if (_uniqueRules != null)
                            if (InCustomList(_uniqueRules, itemEntity, itemEntity.Rarity))
                                pickItemUp = true;
                        break;
                }
            }

            #endregion

            #region Override Rules

            if (OverrideChecks(itemEntity)) pickItemUp = true;

            #endregion

            return pickItemUp;
        }

        private IEnumerator FindItemToPick() {
            if (!Input.GetKeyState(Settings.PickUpKey.Value)) yield break;
            var window = GameController.Window.GetWindowRectangleTimeCache;
            var rect = new RectangleF(window.X, window.X, window.X + window.Width, window.Y + window.Height);
            var playerPos = GameController.Player.GridPos;

            List<CustomItem> currentLabels;

            if (Settings.UseWeight)
            {
                currentLabels = GameController.Game.IngameState.IngameUi.ItemsOnGroundLabels
                    .Where(x => x.Address != 0 &&
                                x.ItemOnGround?.GetComponent<WorldItem>()?.ItemEntity.Path != null &&
                                x.IsVisible && x.Label.GetClientRectCache.Center.PointInRectangle(rect) &&
                                (x.CanPickUp || x.MaxTimeForPickUp.TotalSeconds <= 0))
                    .Select(x => new CustomItem(x, GameController.Files,
                        x.ItemOnGround.GetComponent<Positioned>().GridPos
                            .Distance(playerPos), _weightsRules))
                    .OrderByDescending(x => x.Weight).ThenBy(x => x.Distance).ToList();

             
            }
            else
            {
                currentLabels = GameController.Game.IngameState.IngameUi.ItemsOnGroundLabels
                    .Where(x => x.Address != 0 &&
                                x.ItemOnGround?.GetComponent<WorldItem>()?.ItemEntity.Path != null &&
                                x.IsVisible  &&
                                (x.CanPickUp || x.MaxTimeForPickUp.TotalSeconds <= 0))
                    .Select(x => new CustomItem(x, GameController.Files,
                        x.ItemOnGround.GetComponent<Positioned>().GridPos
                            .Distance(playerPos), _weightsRules))
                    .OrderBy(x => x.Distance).ToList();
            }
            
             GameController.Debug["PickIt"] = currentLabels;
            var pickUpThisItem = currentLabels.FirstOrDefault(x => DoWePickThis(x) && x.Distance < Settings.PickupRange);
            if (pickUpThisItem?.GroundItem != null) yield return TryToPickV2(pickUpThisItem);
            FullWork = true;
        }


        private IEnumerator TryToPickV2(CustomItem pickItItem) {
            if (!pickItItem.IsValid)
            {
                FullWork = true;
                LogMessage("PickItem is not valid.", 5, Color.Red);
                yield break;
            }

            var centerOfItemLabel = pickItItem.LabelOnGround.Label.GetClientRectCache.Center;
            var rectangleOfGameWindow = GameController.Window.GetWindowRectangleTimeCache;
            var oldMousePosition = Mouse.GetCursorPositionVector();
            _clickWindowOffset = rectangleOfGameWindow.TopLeft;
            rectangleOfGameWindow.Inflate(-25, -25);
            centerOfItemLabel.X += rectangleOfGameWindow.Left;
            centerOfItemLabel.Y += rectangleOfGameWindow.Top;
            if (!rectangleOfGameWindow.Intersects(new RectangleF(centerOfItemLabel.X, centerOfItemLabel.Y, 3, 3)))
            {
                FullWork = true;
                LogMessage($"Label outside game window. Label: {centerOfItemLabel} Window: {rectangleOfGameWindow}", 5, Color.Red);
                yield break;
            }

            var tryCount = 0;
            while (!pickItItem.IsTargeted() && tryCount < 5)
            {
                var completeItemLabel = pickItItem.LabelOnGround?.Label;
                if (completeItemLabel == null)
                {
                    if (tryCount > 0)
                    {
                        LogMessage("Probably item already picked.", 3);
                        yield break;
                    }

                    LogError("Label for item not found.", 5);
                    yield break;
                }

                /*while (GameController.Player.GetComponent<Actor>().isMoving)
                {
                    yield return waitPlayerMove;
                }*/
                var clientRect = completeItemLabel.GetClientRect();

                var clientRectCenter = clientRect.Center;


                var vector2 = clientRectCenter + _clickWindowOffset;

                Mouse.MoveCursorToPosition(vector2);
                yield return wait3ms;
                Mouse.MoveCursorToPosition(vector2);
                yield return wait3ms;
                yield return Mouse.LeftClick();
                yield return toPick;
                tryCount++;
            }

            if (pickItItem.IsTargeted()) 
                Input.Click(MouseButtons.Left);
            tryCount = 0;
            while (GameController.Game.IngameState.IngameUi.ItemsOnGroundLabels.FirstOrDefault(
                       x => x.Address == pickItItem.LabelOnGround.Address) != null && tryCount < 6)
            {
                tryCount++;
                yield return waitForNextTry;
            }

            yield return waitForNextTry;
            //   Mouse.MoveCursorToPosition(oldMousePosition);
        }

        private Element LastLabelClick;

        #region (Re)Loading Rules

        private void LoadRuleFiles()
        {
            var PickitConfigFileDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Plugins", "Compiled",nameof(PickIt),
                PickitRuleDirectory);


            if (!Directory.Exists(PickitConfigFileDirectory))
            {
                Directory.CreateDirectory(PickitConfigFileDirectory);
                CustomRulesExists = false;
                return;
            }

            var dirInfo = new DirectoryInfo(PickitConfigFileDirectory);

            PickitFiles = dirInfo.GetFiles("*.txt").Select(x => Path.GetFileNameWithoutExtension(x.Name)).ToList();
            _normalRules = LoadPickit(Settings.NormalRuleFile);
            _magicRules = LoadPickit(Settings.MagicRuleFile);
            _rareRules = LoadPickit(Settings.RareRuleFile);
            _uniqueRules = LoadPickit(Settings.UniqueRuleFile);
            _weightsRules = LoadWeights("Weights");
        }

        public HashSet<string> LoadPickit(string fileName) {
            var hashSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            if (fileName == string.Empty)
            {
                CustomRulesExists = false;
                return hashSet;
            }

            var pickitFile = $@"{DirectoryFullName}\{PickitRuleDirectory}\{fileName}.txt";
            if (!File.Exists(pickitFile))
            {
                CustomRulesExists = false;
                return hashSet;
            }

            var lines = File.ReadAllLines(pickitFile);
            foreach (var x in lines.Where(x => !string.IsNullOrWhiteSpace(x) && !x.StartsWith("#"))) hashSet.Add(x.Trim());
            LogMessage($"PICKIT :: (Re)Loaded {fileName}", 5, Color.GreenYellow);
            return hashSet;
        }

        public Dictionary<string, int> LoadWeights(string fileName) {
            var result = new Dictionary<string, int>();
            var filePath = $@"{DirectoryFullName}\{PickitRuleDirectory}\{fileName}.txt";
            if (!File.Exists(filePath)) return result;

            var lines = File.ReadAllLines(filePath);

            foreach (var x in lines.Where(x => !string.IsNullOrEmpty(x) && !x.StartsWith("#") && x.IndexOf('=') > 0))
            {
                try
                {
                    var s = x.Split('=');
                    if (s.Length == 2) result[s[0].Trim()] = int.Parse(s[1]);
                }
                catch (Exception e)
                {
                    DebugWindow.LogError($"{nameof(PickIt)} => Error when parse weight.");
                }
            }

            return result;
        }

        public override void OnPluginDestroyForHotReload() => pickItCoroutine.Done(true);

        #endregion

        #region Adding / Removing Entities

        public override void EntityAdded(Entity Entity) { }

        public override void EntityRemoved(Entity Entity) { }

        #endregion
    }
}