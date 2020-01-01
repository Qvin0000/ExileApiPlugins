using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using ExileCore;
using ExileCore.PoEMemory.Components;
using ExileCore.PoEMemory.MemoryObjects;
using ExileCore.Shared;
using ExileCore.Shared.Enums;
using ExileCore.Shared.Helpers;
using SharpDX;

namespace PickIt
{
    public class PickIt : BaseSettingsPlugin<PickItSettings>
    {
        private const string PickitRuleDirectory = "Pickit Rules";
        private readonly Stopwatch _debugTimer = Stopwatch.StartNew();
        private readonly WaitTime _toPick = new WaitTime(10);
        private readonly WaitTime _wait3Ms = new WaitTime(3);
        private readonly WaitTime _waitForNextTry = new WaitTime(33);
        private Vector2 _clickWindowOffset;
        private HashSet<string> _magicRules;
        private HashSet<string> _normalRules;
        private HashSet<string> _rareRules;
        private HashSet<string> _uniqueRules;
        private Dictionary<string, int> _weightsRules = new Dictionary<string, int>();
        private WaitTime _workCoroutine;
        public DateTime BuildDate;
        private uint _coroutineCounter;
        private bool _customRulesExists = true;
        private bool _fullWork = true;
        private Coroutine _pickItCoroutine;
        private List<string> _ignoredCurrency;
        public PickIt()
        {
            Name = "Pickit";
        }

        //https://stackoverflow.com/questions/826777/how-to-have-an-auto-incrementing-version-number-visual-studio
        public Version Version { get; } = Assembly.GetExecutingAssembly().GetName().Version;
        public string PluginVersion { get; set; }
        private List<string> PickitFiles { get; set; }

        public override bool Initialise()
        {
            BuildDate = new DateTime(2000, 1, 1).AddDays(Version.Build).AddSeconds(Version.Revision * 2);
            PluginVersion = $"{Version}";
            _pickItCoroutine = new Coroutine(MainWorkCoroutine(), this, "Pick It");
            Core.ParallelRunner.Run(_pickItCoroutine);
            _pickItCoroutine.Pause();
            _debugTimer.Reset();
            Settings.MouseSpeed.OnValueChanged += (sender, f) => { Mouse.speedMouse = Settings.MouseSpeed.Value; };
            _workCoroutine = new WaitTime(Settings.ExtraDelay);
            Settings.ExtraDelay.OnValueChanged += (sender, i) => _workCoroutine = new WaitTime(i);
            LoadRuleFiles();
            LoadIgnoredCurrency();
            return true;
        }

        private void LoadIgnoredCurrency()
        {
            var cfgPath = Path.Combine(DirectoryFullName, "ignored_currency.txt");

            if (!File.Exists(cfgPath))
            {
                File.WriteAllLines(cfgPath, new []
                {
                    "Portal Scroll",
                    "Scroll of Wisdom",
                    "Orb of Transmutation",
                    "Orb of Augmentation",
                    "Orb of Alteration",
                    "Armourer's Scrap",
                    "Blacksmith's Whetstone"
                });
            }

            _ignoredCurrency = File.ReadAllLines(cfgPath).Where(x => !string.IsNullOrEmpty(x)).ToList();
        }

        private IEnumerator MainWorkCoroutine()
        {
            while (true)
            {
                yield return FindItemToPick();

                _coroutineCounter++;
                _pickItCoroutine.UpdateTicks(_coroutineCounter);
                yield return _workCoroutine;
            }
        }

        private bool ChatOpen
        {
            get
            {
                var chatBox = GameController?.Game?.IngameState?.UIRoot.GetChildAtIndex(1)?.GetChildAtIndex(37)?.GetChildAtIndex(3);
                if (chatBox.IsVisibleLocal)
                {
                    return true;
                }
                else return false;
            }
        }

        public override Job Tick()
        {
            if (!ChatOpen)
            {
                if (Input.GetKeyState(Keys.Escape)) _pickItCoroutine.Pause();

                if (Input.GetKeyState(Settings.PickUpKey.Value))
                {
                    _debugTimer.Restart();

                    if (_pickItCoroutine.IsDone)
                    {
                        var firstOrDefault = Core.ParallelRunner.Coroutines.FirstOrDefault(x => x.OwnerName == nameof(PickIt));

                        if (firstOrDefault != null)
                            _pickItCoroutine = firstOrDefault;
                    }

                    _pickItCoroutine.Resume();
                    _fullWork = false;
                }
                else
                {
                    if (_fullWork)
                    {
                        _pickItCoroutine.Pause();
                        _debugTimer.Reset();
                    }
                }

                if (_debugTimer.ElapsedMilliseconds > 2000)
                {
                    _fullWork = true;
                    LogMessage("Error pick it stop after time limit 2000 ms");
                    _debugTimer.Reset();
                }
            }

            return null;
        }

        public bool InCustomList(HashSet<string> checkList, CustomItem itemEntity, ItemRarity rarity)
        {
            if (checkList.Contains(itemEntity.BaseName) && itemEntity.Rarity == rarity) return true;
            return false;
        }

        public bool OverrideChecks(CustomItem item)
        {
            try
            {
                #region Currency

                if (Settings.AllCurrency && item.ClassName.EndsWith("Currency"))
                {
                    return !_ignoredCurrency.Contains(item.BaseName);
                }

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

        public bool DoWePickThis(CustomItem itemEntity)
        {
            var pickItemUp = false;

            #region Force Pickup All

            if (Settings.PickUpEverything) return true;

            #endregion

            #region Rarity Rule Switch

            if (_customRulesExists)
            {
                switch (itemEntity.Rarity)
                {
                    case ItemRarity.Normal:
                        if (_normalRules != null)
                        {
                            if (InCustomList(_normalRules, itemEntity, itemEntity.Rarity))
                                pickItemUp = true;
                        }

                        break;
                    case ItemRarity.Magic:
                        if (_magicRules != null)
                        {
                            if (InCustomList(_magicRules, itemEntity, itemEntity.Rarity))
                                pickItemUp = true;
                        }

                        break;
                    case ItemRarity.Rare:
                        if (_rareRules != null)
                        {
                            if (InCustomList(_rareRules, itemEntity, itemEntity.Rarity))
                                pickItemUp = true;
                        }

                        break;
                    case ItemRarity.Unique:
                        if (_uniqueRules != null)
                        {
                            if (InCustomList(_uniqueRules, itemEntity, itemEntity.Rarity))
                                pickItemUp = true;
                        }

                        break;
                }
            }

            #endregion

            #region Override Rules

            if (OverrideChecks(itemEntity)) pickItemUp = true;

            #endregion

            return pickItemUp;
        }

        private IEnumerator FindItemToPick()
        {
            if (!Input.GetKeyState(Settings.PickUpKey.Value) || !GameController.Window.IsForeground()) yield break;
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
                                x.IsVisible &&
                                (x.CanPickUp || x.MaxTimeForPickUp.TotalSeconds <= 0))
                    .Select(x => new CustomItem(x, GameController.Files,
                        x.ItemOnGround.GetComponent<Positioned>().GridPos
                            .Distance(playerPos), _weightsRules))
                    .OrderBy(x => x.Distance).ToList();
            }

            GameController.Debug["PickIt"] = currentLabels;
            var pickUpThisItem = currentLabels.FirstOrDefault(x => DoWePickThis(x) && x.Distance < Settings.PickupRange);
            if (pickUpThisItem?.GroundItem != null) yield return TryToPickV2(pickUpThisItem);
            _fullWork = true;
        }

        private IEnumerator TryToPickV2(CustomItem pickItItem)
        {
            if (!pickItItem.IsValid)
            {
                _fullWork = true;
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
                _fullWork = true;
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
                yield return _wait3Ms;
                Mouse.MoveCursorToPosition(vector2);
                yield return _wait3Ms;
                yield return Mouse.LeftClick();
                yield return _toPick;
                tryCount++;
            }

            if (pickItItem.IsTargeted())
                Input.Click(MouseButtons.Left);

            tryCount = 0;

            while (GameController.Game.IngameState.IngameUi.ItemsOnGroundLabels.FirstOrDefault(
                       x => x.Address == pickItItem.LabelOnGround.Address) != null && tryCount < 6)
            {
                tryCount++;
                yield return _waitForNextTry;
            }

            yield return _waitForNextTry;

            //   Mouse.MoveCursorToPosition(oldMousePosition);
        }

        #region (Re)Loading Rules

        private void LoadRuleFiles()
        {
            var PickitConfigFileDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Plugins", "Compiled", nameof(PickIt),
                PickitRuleDirectory);

            if (!Directory.Exists(PickitConfigFileDirectory))
            {
                Directory.CreateDirectory(PickitConfigFileDirectory);
                _customRulesExists = false;
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

        public HashSet<string> LoadPickit(string fileName)
        {
            var hashSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            if (fileName == string.Empty)
            {
                _customRulesExists = false;
                return hashSet;
            }

            var pickitFile = $@"{DirectoryFullName}\{PickitRuleDirectory}\{fileName}.txt";

            if (!File.Exists(pickitFile))
            {
                _customRulesExists = false;
                return hashSet;
            }

            var lines = File.ReadAllLines(pickitFile);

            foreach (var x in lines.Where(x => !string.IsNullOrWhiteSpace(x) && !x.StartsWith("#")))
            {
                hashSet.Add(x.Trim());
            }

            LogMessage($"PICKIT :: (Re)Loaded {fileName}", 5, Color.GreenYellow);
            return hashSet;
        }

        public Dictionary<string, int> LoadWeights(string fileName)
        {
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

        public override void OnPluginDestroyForHotReload()
        {
            _pickItCoroutine.Done(true);
        }

        #endregion

        #region Adding / Removing Entities

        public override void EntityAdded(Entity Entity)
        {
        }

        public override void EntityRemoved(Entity Entity)
        {
        }

        #endregion
    }
}
