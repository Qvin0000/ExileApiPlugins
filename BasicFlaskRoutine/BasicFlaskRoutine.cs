using TreeRoutine.DefaultBehaviors.Actions;
using TreeRoutine.DefaultBehaviors.Helpers;
using TreeRoutine.FlaskComponents;
using TreeRoutine.Routine.BasicFlaskRoutine.Flask;
using System;
using System.Collections.Generic;
using SharpDX;
using System.Linq;
using TreeRoutine.Menu;
using ImGuiNET;
using System.Diagnostics;
using ExileCore;
using ExileCore.PoEMemory.Components;
using ExileCore.PoEMemory.MemoryObjects;
using ExileCore.Shared;
using ExileCore.Shared.Enums;
using TreeRoutine.TreeSharp;

namespace TreeRoutine.Routine.BasicFlaskRoutine
{
    public class BasicFlaskRoutine : BaseTreeRoutinePlugin<BasicFlaskRoutineSettings, BaseTreeCache>
    {
        public BasicFlaskRoutine() : base()
        {

        }

        public Composite Tree { get; set; }
        private Coroutine TreeCoroutine { get; set; }
        public Object LoadedMonstersLock { get; set; } = new Object();
        public List<Entity> LoadedMonsters { get; protected set; } = new List<Entity>();


        private KeyboardHelper KeyboardHelper { get; set; } = null;

        private Stopwatch PlayerMovingStopwatch { get; set; } = new Stopwatch();

        private const string basicFlaskRoutineChecker = "BasicFlaskRoutine Checker";



        public override bool Initialise()
        {
            base.Initialise();

            Name = "BasicFlaskRoutine";
            KeyboardHelper = new KeyboardHelper(GameController);

            Tree = CreateTree();

            // Add this as a coroutine for this plugin
            Settings.Enable.OnValueChanged += (sender, b) =>
            {
                if (b)
                {
                    if (Core.ParallelRunner.FindByName(basicFlaskRoutineChecker) == null) InitCoroutine();
                    TreeCoroutine?.Resume();
                }
                else
                    TreeCoroutine?.Pause();

            };
            InitCoroutine();

            Settings.TicksPerSecond.OnValueChanged += (sender, b) =>
            {
                UpdateCoroutineWaitRender();
            };

            return true;
        }

        private void InitCoroutine()
        {
            TreeCoroutine = new Coroutine(() => TickTree(Tree), new WaitTime(1000 / Settings.TicksPerSecond), this, "BasicFlaskRoutine Tree");
            Core.ParallelRunner.Run(TreeCoroutine);
        }

        private void UpdateCoroutineWaitRender()
        {
            TreeCoroutine.UpdateCondtion(new WaitTime(1000 / Settings.TicksPerSecond));
        }

        protected override void UpdateCache()
        {
            base.UpdateCache();

            UpdatePlayerMovingStopwatch();
        }

        private void UpdatePlayerMovingStopwatch()
        {
            var player = GameController.Player.GetComponent<Actor>();
            if (player != null && player.Address != 0 && player.isMoving)
            {
                if (!PlayerMovingStopwatch.IsRunning)
                    PlayerMovingStopwatch.Start();
            }
            else
            {
                PlayerMovingStopwatch.Reset();
            }
        }

        private Composite CreateTree()
        {
            return new Decorator(x => TreeHelper.CanTick() && !PlayerHelper.isPlayerDead() && (!Cache.InHideout || Settings.EnableInHideout) && PlayerHelper.playerDoesNotHaveAnyOfBuffs(new List<string>() { "grace_period" }),
                    new PrioritySelector
                    (
                        new Decorator(x => Settings.AutoFlask,
                        new PrioritySelector(
                            CreateInstantHPPotionComposite(),
                            CreateHPPotionComposite(),
                            CreateInstantManaPotionComposite(),
                            CreateManaPotionComposite()
                            )
                        ),
                        CreateAilmentPotionComposite(),
                        CreateDefensivePotionComposite(),
                        CreateSpeedPotionComposite(),
                        CreateOffensivePotionComposite()
                    )
                );
        }

        private Composite CreateInstantHPPotionComposite()
        {
            return new Decorator((x => PlayerHelper.isHealthBelowPercentage(Settings.InstantHPPotion)),
                new PrioritySelector(
                    CreateUseFlaskAction(FlaskActions.Life, true, true),
                    CreateUseFlaskAction(FlaskActions.Hybrid, true, true),
                    CreateUseFlaskAction(FlaskActions.Life),
                    CreateUseFlaskAction(FlaskActions.Hybrid)
                )
            );

        }

        private Composite CreateHPPotionComposite()
        {
            return new Decorator((x => PlayerHelper.isHealthBelowPercentage(Settings.HPPotion)),
                new Decorator((x => PlayerHelper.playerDoesNotHaveAnyOfBuffs(new List<string>() { "flask_effect_life" })),
                 new PrioritySelector(
                    CreateUseFlaskAction(FlaskActions.Life, false),
                    CreateUseFlaskAction(FlaskActions.Hybrid, false)
                 )
                )
            );
        }

        private Composite CreateInstantManaPotionComposite()
        {
            return new Decorator((x => PlayerHelper.isManaBelowPercentage(Settings.InstantManaPotion)),
                new PrioritySelector(
                    CreateUseFlaskAction(FlaskActions.Mana, true, true),
                    CreateUseFlaskAction(FlaskActions.Hybrid, true, true),
                    CreateUseFlaskAction(FlaskActions.Mana),
                    CreateUseFlaskAction(FlaskActions.Hybrid)

                )
            );
        }

        private Composite CreateManaPotionComposite()
        {
            return new Decorator((x => PlayerHelper.isManaBelowPercentage(Settings.ManaPotion) || PlayerHelper.isManaBelowValue(Settings.MinManaFlask)),
                new Decorator((x => PlayerHelper.playerDoesNotHaveAnyOfBuffs(new List<string>() { "flask_effect_mana" })),
                    new PrioritySelector(
                        CreateUseFlaskAction(FlaskActions.Mana, false),
                        CreateUseFlaskAction(FlaskActions.Hybrid, false)
                    )
                )
            );
        }

        private Composite CreateSpeedPotionComposite()
        {
            return new Decorator((x => Settings.SpeedFlaskEnable && Settings.MinMsPlayerMoving <= PlayerMovingStopwatch.ElapsedMilliseconds && (PlayerHelper.playerDoesNotHaveAnyOfBuffs(new List<string>() { "flask_bonus_movement_speed", "flask_utility_sprint" }) || (!Settings.SilverFlaskEnable || PlayerHelper.playerDoesNotHaveAnyOfBuffs(new List<string>() { "flask_utility_haste" })))),
                new PrioritySelector(
                    new Decorator((x => Settings.QuicksilverFlaskEnable), CreateUseFlaskAction(FlaskActions.Speedrun)),
                    new Decorator((x => Settings.SilverFlaskEnable), CreateUseFlaskAction(FlaskActions.OFFENSE_AND_SPEEDRUN))
                )
            );
        }

        private Composite CreateDefensivePotionComposite()
        {
            return new Decorator((x => Settings.DefensiveFlaskEnable && (PlayerHelper.isHealthBelowPercentage(Settings.HPPercentDefensive) || PlayerHelper.isEnergyShieldBelowPercentage(Settings.ESPercentDefensive) || Settings.DefensiveMonsterCount > 0 && HasEnoughNearbyMonsters(Settings.DefensiveMonsterCount, Settings.DefensiveMonsterDistance, Settings.DefensiveCountNormalMonsters, Settings.DefensiveCountRareMonsters, Settings.DefensiveCountMagicMonsters, Settings.DefensiveCountUniqueMonsters))),
                new PrioritySelector(
                    CreateUseFlaskAction(FlaskActions.Defense),
                    new Decorator((x => Settings.OffensiveAsDefensiveEnable), CreateUseFlaskAction(new List<FlaskActions> { FlaskActions.OFFENSE_AND_SPEEDRUN, FlaskActions.Defense }, ignoreFlasksWithAction: (() => Settings.DisableLifeSecUse ? new List<FlaskActions>() { FlaskActions.Life, FlaskActions.Mana, FlaskActions.Hybrid } : null)))
                )
            );
        }

        private Composite CreateOffensivePotionComposite()
        {
            return new PrioritySelector(
                new Decorator((x => Settings.OffensiveFlaskEnable && (PlayerHelper.isHealthBelowPercentage(Settings.HPPercentOffensive) || PlayerHelper.isEnergyShieldBelowPercentage(Settings.ESPercentOffensive) || Settings.OffensiveMonsterCount > 0 && HasEnoughNearbyMonsters(Settings.OffensiveMonsterCount, Settings.OffensiveMonsterDistance, Settings.OffensiveCountNormalMonsters, Settings.OffensiveCountRareMonsters, Settings.OffensiveCountMagicMonsters, Settings.OffensiveCountUniqueMonsters))),
                    CreateUseFlaskAction(new List<FlaskActions> { FlaskActions.Offense, FlaskActions.OFFENSE_AND_SPEEDRUN }, ignoreFlasksWithAction: (() => Settings.DisableLifeSecUse ?  new List<FlaskActions>() { FlaskActions.Life, FlaskActions.Mana, FlaskActions.Hybrid} : null)))
            );
        }

        private Composite CreateAilmentPotionComposite()
        {
            return new Decorator(x => Settings.RemAilment,
                new PrioritySelector(
                    new Decorator(x => Settings.RemBleed, CreateCurableDebuffDecorator(Cache.DebuffPanelConfig.Bleeding, CreateUseFlaskAction(FlaskActions.BleedImmune, isCleansing:true))),
                    new Decorator(x => Settings.RemBurning, CreateCurableDebuffDecorator(Cache.DebuffPanelConfig.Burning, CreateUseFlaskAction(FlaskActions.IgniteImmune, isCleansing: true))),
                    CreateCurableDebuffDecorator(Cache.DebuffPanelConfig.Corruption, CreateUseFlaskAction(FlaskActions.BleedImmune, isCleansing: true), (() => Settings.CorruptCount)),
                    new Decorator(x => Settings.RemFrozen, CreateCurableDebuffDecorator(Cache.DebuffPanelConfig.Frozen, CreateUseFlaskAction(FlaskActions.FreezeImmune, isCleansing: true))),
                    new Decorator(x => Settings.RemPoison, CreateCurableDebuffDecorator(Cache.DebuffPanelConfig.Poisoned, CreateUseFlaskAction(FlaskActions.PoisonImmune, isCleansing: true))),
                    new Decorator(x => Settings.RemShocked, CreateCurableDebuffDecorator(Cache.DebuffPanelConfig.Shocked, CreateUseFlaskAction(FlaskActions.ShockImmune, isCleansing: true))),
                    new Decorator(x => Settings.RemCurse, CreateCurableDebuffDecorator(Cache.DebuffPanelConfig.WeakenedSlowed, CreateUseFlaskAction(FlaskActions.CurseImmune, isCleansing: true)))
                    )
                );
        }

        private Composite CreateUseFlaskAction(FlaskActions flaskAction, Boolean? instant = null, Boolean ignoreBuffs = false, Func<List<FlaskActions>> ignoreFlasksWithAction = null, Boolean isCleansing = false)
        {
            return CreateUseFlaskAction(new List<FlaskActions> { flaskAction }, instant, ignoreBuffs, ignoreFlasksWithAction, isCleansing);
        }

        private Composite CreateUseFlaskAction(List<FlaskActions> flaskActions, Boolean? instant = null, Boolean ignoreBuffs = false, Func<List<FlaskActions>> ignoreFlasksWithAction = null, Boolean isCleansing = false)
        {
            return new UseHotkeyAction(KeyboardHelper, x =>
            {
                var foundFlask = FindFlaskMatchingAnyAction(flaskActions, instant, ignoreBuffs, ignoreFlasksWithAction, isCleansing);

                if (foundFlask == null)
                {
                    return null;
                }

                return Settings.FlaskSettings[foundFlask.Index].Hotkey;
            });
        }

        private Boolean HasEnoughNearbyMonsters(int minimumMonsterCount, int maxDistance, bool countNormal, bool countRare, bool countMagic, bool countUnique)
        {
            var mobCount = 0;
            var maxDistanceSquare = maxDistance * maxDistance;

            var playerPosition = GameController.Player.Pos;

            if (LoadedMonsters != null)
            {
                List<Entity> localLoadedMonsters = null;
                lock (LoadedMonstersLock)
                {
                    localLoadedMonsters = new List<Entity>(LoadedMonsters);
                }

                // Make sure we create our own list to iterate as we may be adding/removing from the list
                foreach (var monster in localLoadedMonsters)
                {
                    if (!monster.HasComponent<Monster>() || !monster.IsValid || !monster.IsAlive || !monster.IsHostile)
                        continue;

                    var monsterType = monster.GetComponent<ObjectMagicProperties>().Rarity;

                    // Don't count this monster type if we are ignoring it
                    if (monsterType == MonsterRarity.White && !countNormal
                        || monsterType == MonsterRarity.Rare && !countRare
                        || monsterType == MonsterRarity.Magic && !countMagic
                        || monsterType == MonsterRarity.Unique && !countUnique)
                        continue;

                    var monsterPosition = monster.Pos;

                    var xDiff = playerPosition.X - monsterPosition.X;
                    var yDiff = playerPosition.Y - monsterPosition.Y;
                    var monsterDistanceSquare = (xDiff * xDiff + yDiff * yDiff);

                    if (monsterDistanceSquare <= maxDistanceSquare)
                    {
                        mobCount++;
                    }

                    if (mobCount >= minimumMonsterCount)
                    {
                        if (Settings.Debug)
                        {
                            Log("NearbyMonstersCondition returning true because " + mobCount + " mobs valid monsters were found nearby.", 2);
                        }
                        return true;
                    }
                }
            } else if (Settings.Debug)
            {
                Log("NearbyMonstersCondition returning false because mob list was invalid.", 2);
            }

            if (Settings.Debug)
            {
                Log("NearbyMonstersCondition returning false because " + mobCount + " mobs valid monsters were found nearby.", 2);
            }
            return false;
        }

        private PlayerFlask FindFlaskMatchingAnyAction(FlaskActions flaskAction, Boolean? instant = null, Boolean ignoreBuffs = false, Func<List<FlaskActions>> ignoreFlasksWithAction = null, Boolean isCleansing = false)
        {
            return FindFlaskMatchingAnyAction(new List<FlaskActions> { flaskAction }, instant, ignoreBuffs, ignoreFlasksWithAction, isCleansing);
        }

        private PlayerFlask FindFlaskMatchingAnyAction(List<FlaskActions> flaskActions, Boolean? instant = null, Boolean ignoreBuffs = false, Func<List<FlaskActions>> ignoreFlasksWithAction = null, Boolean isCleansing = false)
        {
            var allFlasks = FlaskHelper.GetAllFlaskInfo();

            // We have no flasks or settings for flasks?
            if (allFlasks == null || Settings.FlaskSettings == null)
            {
                if (Settings.Debug)
                {
                    if (allFlasks == null)
                        LogMessage(Name + ": No flasks to match against.", 5);
                    else if (Settings.FlaskSettings == null)
                        LogMessage(Name + ": Flask settings were null. Hopefully doesn't happen frequently.", 5);
                }

                return null;
            }

            if (Settings.Debug)
            {
                foreach (var flask in allFlasks)
                {
                    LogMessage($"{Name}: Flask: {flask.Name} Slot: {flask.Index} Instant: {flask.InstantType} Action1: {flask.Action1} Action2: {flask.Action2}", 5);
                }
            }

            List<FlaskActions> ignoreFlaskActions = ignoreFlasksWithAction == null ? null : ignoreFlasksWithAction();

            var flaskList = allFlasks
                    .Where(x =>
                    Settings.FlaskSettings[x.Index].Enabled
                    && FlaskHasAvailableAction(flaskActions, ignoreFlaskActions, x)
                    && FlaskHelper.CanUsePotion(x, Settings.FlaskSettings[x.Index].ReservedUses, isCleansing)
                    && FlaskMatchesInstant(x, instant)
                    && (ignoreBuffs || MissingFlaskBuff(x))
                    ).OrderByDescending(x => flaskActions.Contains(x.Action1)).ThenByDescending(x => x.TotalUses - Settings.FlaskSettings[x.Index].ReservedUses).ToList();

            if (flaskList == null || !flaskList.Any())
            {
                if (Settings.Debug)
                    LogError(Name + ": No flasks found for action: (instant:" + instant + ") " + flaskActions[0], 1);
                return null;
            }

            if (Settings.Debug)
                LogMessage(Name + ": Flask(s) found for action: " + flaskActions[0] + " Flask Count: " + flaskList.Count(), 1);

            return flaskList.FirstOrDefault();
        }

        private bool FlaskHasAvailableAction(List<FlaskActions> flaskActions, List<FlaskActions> ignoreFlaskActions, PlayerFlask flask)
        {
            return flaskActions.Any(x => x == flask.Action1 || x == flask.Action2)
                    && (ignoreFlaskActions == null || !ignoreFlaskActions.Any(x => x == flask.Action1 || x == flask.Action2));
        }

        private bool FlaskMatchesInstant(PlayerFlask playerFlask, Boolean? instant)
        {
            return instant == null 
                    || instant == false && CanUseFlaskAsRegen(playerFlask)
                    || instant == true && CanUseFlaskAsInstant(playerFlask);
        }

        private bool CanUseFlaskAsInstant(PlayerFlask playerFlask)
        {
            // If the flask is instant, no special logic needed
            return playerFlask.InstantType == FlaskInstantType.Partial
                    || playerFlask.InstantType == FlaskInstantType.Full
                    || playerFlask.InstantType == FlaskInstantType.LowLife && PlayerHelper.isHealthBelowPercentage(35);
        }

        private bool CanUseFlaskAsRegen(PlayerFlask playerFlask)
        {
            return playerFlask.InstantType == FlaskInstantType.None
                    || playerFlask.InstantType == FlaskInstantType.Partial && !Settings.ForceBubblingAsInstantOnly
                    || playerFlask.InstantType == FlaskInstantType.LowLife && !Settings.ForcePanickedAsInstantOnly;
        }

        private bool MissingFlaskBuff(PlayerFlask playerFlask)
        {
            return !PlayerHelper.playerHasBuffs(new List<string> { playerFlask.BuffString1 }) || !PlayerHelper.playerHasBuffs(new List<string> { playerFlask.BuffString2 });
        }

        private Decorator CreateCurableDebuffDecorator(Dictionary<string, int> dictionary, Composite child, Func<int> minCharges = null)
        {
            return new Decorator((x =>
            {
                var buffs = GameController.Game.IngameState.Data.LocalPlayer.GetComponent<Life>().Buffs;
                foreach (var buff in buffs)
                {
                    if (float.IsInfinity(buff.Timer))
                        continue;

                    int filterId = 0;
                    if (dictionary.TryGetValue(buff.Name, out filterId))
                    {
                        // I'm not sure what the values are here, but this is the effective logic from the old plugin
                        return (filterId == 0 || filterId != 1) && (minCharges == null || buff.Charges >= minCharges());
                    }
                }
                return false;
            }), child);
        }

        public override void Render()
        {
            base.Render();
            if (!Settings.Enable.Value) return;
        }

        public override void DrawSettings()
        {
            //base.DrawSettings();

            ImGuiTreeNodeFlags collapsingHeaderFlags = ImGuiTreeNodeFlags.CollapsingHeader;

            if (ImGui.TreeNodeEx("Plugin Options", collapsingHeaderFlags))
            {
                Settings.EnableInHideout.Value = ImGuiExtension.Checkbox("Enable in Hideout", Settings.EnableInHideout);
                ImGui.Separator();
                Settings.TicksPerSecond.Value = ImGuiExtension.IntSlider("Ticks Per Second", Settings.TicksPerSecond); ImGuiExtension.ToolTipWithText("(?)", "Determines how many times the plugin checks flasks every second.\nLower for less resources, raise for faster response (but higher chance to chug potions).");
                ImGui.Separator();
                Settings.Debug.Value = ImGuiExtension.Checkbox("Debug Mode", Settings.Debug);
                ImGui.TreePop();
            }


            if (ImGui.TreeNodeEx("Flask Options", collapsingHeaderFlags))
            {
                if (ImGui.TreeNode("Individual Flask Settings"))
                {
                    for (int i = 0; i < 5; i++)
                    {
                        FlaskSetting currentFlask = Settings.FlaskSettings[i];
                        if (ImGui.TreeNode("Flask " + (i + 1) + " Settings"))
                        {
                            currentFlask.Enabled.Value = ImGuiExtension.Checkbox("Enable", currentFlask.Enabled);
                            currentFlask.Hotkey.Value = ImGuiExtension.HotkeySelector("Hotkey", currentFlask.Hotkey);
                            currentFlask.ReservedUses.Value =
                                ImGuiExtension.IntSlider("Reserved Uses", currentFlask.ReservedUses);
                            ImGuiExtension.ToolTipWithText("(?)",
                                "The absolute number of uses reserved on a flask.\nSet to 1 to always have 1 use of the flask available for manual use.");
                            ImGui.TreePop();
                        }
                    }

                    ImGui.TreePop();
                }

                if (ImGui.TreeNode("Health and Mana"))
                {
                    Settings.AutoFlask.Value = ImGuiExtension.Checkbox("Enable", Settings.AutoFlask);

                    ImGuiExtension.SpacedTextHeader("Settings");
                    Settings.ForceBubblingAsInstantOnly.Value =
                        ImGuiExtension.Checkbox("Force Bubbling as Instant only", Settings.ForceBubblingAsInstantOnly);
                    ImGuiExtension.ToolTipWithText("(?)",
                        "When enabled, flasks with the Bubbling mod will only be used as an instant flask.");
                    Settings.ForcePanickedAsInstantOnly.Value =
                        ImGuiExtension.Checkbox("Force Panicked as Instant only", Settings.ForcePanickedAsInstantOnly);
                    ImGuiExtension.ToolTipWithText("(?)",
                        "When enabled, flasks with the Panicked mod will only be used as an instant flask. \nNote, Panicked will not be used until under 35%% with this enabled."); //
                    ImGuiExtension.SpacedTextHeader("Health Flask");
                    Settings.HPPotion.Value = ImGuiExtension.IntSlider("Min Life % Auto HP Flask", Settings.HPPotion);
                    Settings.InstantHPPotion.Value =
                        ImGuiExtension.IntSlider("Min Life % Auto Instant HP Flask", Settings.InstantHPPotion);
                    Settings.DisableLifeSecUse.Value =
                        ImGuiExtension.Checkbox("Disable Life/Hybrid Flask Offensive/Defensive Usage",
                            Settings.DisableLifeSecUse);

                    ImGuiExtension.SpacedTextHeader("Mana Flask");
                    ImGui.Spacing();
                    Settings.ManaPotion.Value =
                        ImGuiExtension.IntSlider("Min Mana % Auto Mana Flask", Settings.ManaPotion);
                    Settings.InstantManaPotion.Value = ImGuiExtension.IntSlider("Min Mana % Auto Instant MP Flask",
                        Settings.InstantManaPotion);
                    Settings.MinManaFlask.Value =
                        ImGuiExtension.IntSlider("Min Mana Auto Mana Flask", Settings.MinManaFlask);
                    ImGui.TreePop();
                }

                if (ImGui.TreeNode("Remove Ailments"))
                {
                    Settings.RemAilment.Value = ImGuiExtension.Checkbox("Enable", Settings.RemAilment);

                    ImGuiExtension.SpacedTextHeader("Ailments");
                    Settings.RemFrozen.Value = ImGuiExtension.Checkbox("Frozen", Settings.RemFrozen);
                    ImGui.SameLine();
                    Settings.RemBurning.Value = ImGuiExtension.Checkbox("Burning", Settings.RemBurning);
                    Settings.RemShocked.Value = ImGuiExtension.Checkbox("Shocked", Settings.RemShocked);
                    ImGui.SameLine();
                    Settings.RemCurse.Value = ImGuiExtension.Checkbox("Cursed", Settings.RemCurse);
                    Settings.RemPoison.Value = ImGuiExtension.Checkbox("Poison", Settings.RemPoison);
                    ImGui.SameLine();
                    Settings.RemBleed.Value = ImGuiExtension.Checkbox("Bleed", Settings.RemBleed);
                    Settings.CorruptCount.Value =
                        ImGuiExtension.IntSlider("Corrupting Blood Stacks", Settings.CorruptCount);
                    ImGui.TreePop();
                }

                if (ImGui.TreeNode("Speed Flasks"))
                {
                    Settings.SpeedFlaskEnable.Value = ImGuiExtension.Checkbox("Enable", Settings.SpeedFlaskEnable);

                    ImGuiExtension.SpacedTextHeader("Flasks");
                    Settings.QuicksilverFlaskEnable.Value =
                        ImGuiExtension.Checkbox("Quicksilver Flask", Settings.QuicksilverFlaskEnable);
                    Settings.SilverFlaskEnable.Value =
                        ImGuiExtension.Checkbox("Silver Flask", Settings.SilverFlaskEnable);

                    ImGuiExtension.SpacedTextHeader("Settings");
                    Settings.MinMsPlayerMoving.Value =
                        ImGuiExtension.IntSlider("Milliseconds Spent Moving", Settings.MinMsPlayerMoving);
                    ImGuiExtension.ToolTipWithText("(?)",
                        "Milliseconds spent moving before flask will be used.\n1000 milliseconds = 1 second");
                    ImGui.TreePop();
                }

                if (ImGui.TreeNode("Defensive Flasks"))
                {
                    Settings.DefensiveFlaskEnable.Value =
                        ImGuiExtension.Checkbox("Enable", Settings.DefensiveFlaskEnable);
                    ImGui.Spacing();
                    ImGui.Separator();
                    Settings.HPPercentDefensive.Value =
                        ImGuiExtension.IntSlider("Min Life %", Settings.HPPercentDefensive);
                    Settings.ESPercentDefensive.Value =
                        ImGuiExtension.IntSlider("Min ES %", Settings.ESPercentDefensive);
                    Settings.OffensiveAsDefensiveEnable.Value =
                        ImGuiExtension.Checkbox("Use offensive flasks for defense",
                            Settings.OffensiveAsDefensiveEnable);
                    ImGui.Separator();

                    Settings.DefensiveMonsterCount.Value =
                        ImGuiExtension.IntSlider("Monster Count", Settings.DefensiveMonsterCount);
                    Settings.DefensiveMonsterDistance.Value =
                        ImGuiExtension.IntSlider("Monster Distance", Settings.DefensiveMonsterDistance);
                    Settings.DefensiveCountNormalMonsters.Value =
                        ImGuiExtension.Checkbox("Normal Monsters", Settings.DefensiveCountNormalMonsters);
                    Settings.DefensiveCountRareMonsters.Value =
                        ImGuiExtension.Checkbox("Rare Monsters", Settings.DefensiveCountRareMonsters);
                    Settings.DefensiveCountMagicMonsters.Value =
                        ImGuiExtension.Checkbox("Magic Monsters", Settings.DefensiveCountMagicMonsters);
                    Settings.DefensiveCountUniqueMonsters.Value =
                        ImGuiExtension.Checkbox("Unique Monsters", Settings.DefensiveCountUniqueMonsters);
                    ImGui.TreePop();
                }

                if (ImGui.TreeNode("Offensive Flasks"))
                {
                    Settings.OffensiveFlaskEnable.Value =
                        ImGuiExtension.Checkbox("Enable", Settings.OffensiveFlaskEnable);
                    ImGui.Spacing();
                    ImGui.Separator();
                    Settings.HPPercentOffensive.Value =
                        ImGuiExtension.IntSlider("Min Life %", Settings.HPPercentOffensive);
                    Settings.ESPercentOffensive.Value =
                        ImGuiExtension.IntSlider("Min ES %", Settings.ESPercentOffensive);
                    ImGui.Separator();
                    Settings.OffensiveMonsterCount.Value =
                        ImGuiExtension.IntSlider("Monster Count", Settings.OffensiveMonsterCount);
                    Settings.OffensiveMonsterDistance.Value =
                        ImGuiExtension.IntSlider("Monster Distance", Settings.OffensiveMonsterDistance);
                    Settings.OffensiveCountNormalMonsters.Value =
                        ImGuiExtension.Checkbox("Normal Monsters", Settings.OffensiveCountNormalMonsters);
                    Settings.OffensiveCountRareMonsters.Value =
                        ImGuiExtension.Checkbox("Rare Monsters", Settings.OffensiveCountRareMonsters);
                    Settings.OffensiveCountMagicMonsters.Value =
                        ImGuiExtension.Checkbox("Magic Monsters", Settings.OffensiveCountMagicMonsters);
                    Settings.OffensiveCountUniqueMonsters.Value =
                        ImGuiExtension.Checkbox("Unique Monsters", Settings.OffensiveCountUniqueMonsters);

                    ImGui.TreePop();
                }

                ImGui.TreePop();
            }
        }

        public override void EntityAdded(Entity entityWrapper)
        {
            if (entityWrapper.HasComponent<Monster>())
            {
                lock (LoadedMonstersLock)
                {
                    LoadedMonsters.Add(entityWrapper);
                }
            }
        }

        public override void EntityRemoved(Entity entityWrapper)
        {
            lock (LoadedMonstersLock)
            {
                LoadedMonsters.Remove(entityWrapper);
            }
        }
    }
}