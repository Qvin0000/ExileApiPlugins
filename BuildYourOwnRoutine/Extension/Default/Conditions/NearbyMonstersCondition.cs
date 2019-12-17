using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ExileCore.PoEMemory.Components;
using ExileCore.PoEMemory.MemoryObjects;
using ExileCore.Shared.Enums;
using TreeRoutine.Menu;

namespace TreeRoutine.Routine.BuildYourOwnRoutine.Extension.Default.Conditions
{
    internal class NearbyMonstersCondition : ExtensionCondition
    {
        private int MinimumMonsterCount { get; set; }
        private readonly String MinimumMonsterCountString = "MinimumMonsterCount";

        private float MaxDistance { get; set; }
        private readonly String MaxDistanceString = "MaxDistance";

        private int ColdResistanceThreshold { get; set; }
        private readonly String ColdResistanceThresholdString = "ColdResistanceThreshold";


        private int FireResistanceThreshold { get; set; }
        private readonly String FireResistanceThresholdString = "FireResistanceThreshold";

        private int LightningResistanceThreshold { get; set; }
        private readonly String LightningResistanceThresholdString = "LightningResistanceThreshold";

        private int ChaosResistanceThreshold { get; set; }
        private readonly String ChaosResistanceThresholdString = "ChaosResistanceThreshold";

        private bool CountWhiteMonsters { get; set; }
        private readonly String CountWhiteMonstersString = "CountWhiteMonsters";

        private bool CountRareMonsters { get; set; }
        private readonly String CountRareMonstersString = "CountRareMonsters";

        private bool CountMagicMonsters { get; set; }
        private readonly String CountMagicMonstersString = "CountMagicMonsters";

        private bool CountUniqueMonsters { get; set; }
        private readonly String CountUniqueMonstersString = "CountUniqueMonsters";

        private int MonsterHealthPercentThreshold { get; set; }
        private readonly String MonsterHealthPercentThresholdString = "MonsterHealthPercentThreshold";

        private bool MonsterAboveHealthThreshold { get; set; }
        private readonly String MonsterAboveHealthThresholdString = "MonsterAboveHealthThreshold";

        // Local, non-config value
        private bool PreviewDistance { get; set; }

        public static Dictionary<string, Tuple<GameStat, GameStat>> ResistanceTypes { get; } = new Dictionary<string, Tuple<GameStat, GameStat>>()
        {
            { "Cold", Tuple.Create(GameStat.ColdDamageResistancePct, GameStat.MaximumColdDamageResistancePct) },
            { "Fire", Tuple.Create(GameStat.FireDamageResistancePct, GameStat.MaximumFireDamageResistancePct) },
            { "Lightning",Tuple.Create(GameStat.LightningDamageResistancePct, GameStat.LightningDamageResistancePct) },
            { "Chaos", Tuple.Create(GameStat.ChaosDamageResistancePct, GameStat.MaximumChaosDamageResistancePct) }
        };

        public NearbyMonstersCondition(string owner, string name) : base(owner, name)
        {
            MinimumMonsterCount = 0;
            MaxDistance = 0;
            ColdResistanceThreshold = 0;
            FireResistanceThreshold = 0;
            LightningResistanceThreshold = 0;
            ChaosResistanceThreshold = 0;
            CountWhiteMonsters = true;
            CountRareMonsters = true;
            CountMagicMonsters = true;
            CountUniqueMonsters = true;
            MonsterHealthPercentThreshold = 0;
            MonsterAboveHealthThreshold = false;

            PreviewDistance = false;
        }

        public override void Initialise(Dictionary<String, Object> Parameters)
        {
            base.Initialise(Parameters);

            MinimumMonsterCount = ExtensionComponent.InitialiseParameterInt32(MinimumMonsterCountString, MinimumMonsterCount, ref Parameters);
            MaxDistance = ExtensionComponent.InitialiseParameterSingle(MaxDistanceString, MaxDistance, ref Parameters);

            ColdResistanceThreshold = ExtensionComponent.InitialiseParameterInt32(ColdResistanceThresholdString, ColdResistanceThreshold, ref Parameters);
            FireResistanceThreshold = ExtensionComponent.InitialiseParameterInt32(FireResistanceThresholdString, FireResistanceThreshold, ref Parameters);
            LightningResistanceThreshold = ExtensionComponent.InitialiseParameterInt32(LightningResistanceThresholdString, LightningResistanceThreshold, ref Parameters);
            ChaosResistanceThreshold = ExtensionComponent.InitialiseParameterInt32(ChaosResistanceThresholdString, ChaosResistanceThreshold, ref Parameters);

            CountWhiteMonsters = ExtensionComponent.InitialiseParameterBoolean(CountWhiteMonstersString, CountWhiteMonsters, ref Parameters);
            CountRareMonsters = ExtensionComponent.InitialiseParameterBoolean(CountRareMonstersString, CountRareMonsters, ref Parameters);
            CountMagicMonsters = ExtensionComponent.InitialiseParameterBoolean(CountMagicMonstersString, CountMagicMonsters, ref Parameters);
            CountUniqueMonsters = ExtensionComponent.InitialiseParameterBoolean(CountUniqueMonstersString, CountUniqueMonsters, ref Parameters);

            MonsterHealthPercentThreshold = ExtensionComponent.InitialiseParameterInt32(MonsterHealthPercentThresholdString, MonsterHealthPercentThreshold, ref Parameters);
            MonsterAboveHealthThreshold = ExtensionComponent.InitialiseParameterBoolean(MonsterAboveHealthThresholdString, MonsterAboveHealthThreshold, ref Parameters);
        }

        public override bool CreateConfigurationMenu(ExtensionParameter extensionParameter, ref Dictionary<String, Object> Parameters)
        {
            ImGui.TextDisabled("Condition Info");
            ImGuiExtension.ToolTipWithText("(?)", "This condition will return true if any of the selected player's resistances\nare reduced by more than or equal to the specified amount.\nReduced max resistance modifiers are taken into effect automatically (e.g. -res map mods).");


            base.CreateConfigurationMenu(extensionParameter, ref Parameters);

            MinimumMonsterCount = ImGuiExtension.IntSlider("Minimum Monster Count", MinimumMonsterCount, 1, 50);
            Parameters[MinimumMonsterCountString] = MinimumMonsterCount.ToString();

            MaxDistance = ImGuiExtension.FloatSlider("Maximum Distance", MaxDistance, 1.0f, 5000.0f);
            Parameters[MaxDistanceString] = MaxDistance.ToString();

            PreviewDistance = ImGuiExtension.Checkbox("Preview distance", PreviewDistance);

            if (PreviewDistance)
            {
                //extensionParameter.Plugin.PlayerHelper.DrawSquareToWorld(extensionParameter.Plugin.GameController.Player.Pos, MaxDistance);
            }

            ImGui.Spacing();
            ImGui.Separator();
            ImGui.Spacing();

            ColdResistanceThreshold = ImGuiExtension.IntSlider("Cold Resist Above", ColdResistanceThreshold, 0, 75);
            Parameters[ColdResistanceThresholdString] = ColdResistanceThreshold.ToString();

            FireResistanceThreshold = ImGuiExtension.IntSlider("Fire Resist Above", FireResistanceThreshold, 0, 75);
            Parameters[FireResistanceThresholdString] = FireResistanceThreshold.ToString();

            LightningResistanceThreshold = ImGuiExtension.IntSlider("Lightning Resist Above", LightningResistanceThreshold, 0, 75);
            Parameters[LightningResistanceThresholdString] = LightningResistanceThreshold.ToString();

            ChaosResistanceThreshold = ImGuiExtension.IntSlider("Chaos Resist Above", ChaosResistanceThreshold, 0, 75);
            Parameters[ChaosResistanceThresholdString] = ChaosResistanceThreshold.ToString();

            ImGui.Spacing();
            ImGui.Separator();
            ImGui.Spacing();

            CountWhiteMonsters = ImGuiExtension.Checkbox("White Monsters", CountWhiteMonsters);
            Parameters[CountWhiteMonstersString] = CountWhiteMonsters.ToString();

            CountRareMonsters = ImGuiExtension.Checkbox("Rare Monsters", CountRareMonsters);
            Parameters[CountRareMonstersString] = CountRareMonsters.ToString();

            CountMagicMonsters = ImGuiExtension.Checkbox("Magic Monsters", CountMagicMonsters);
            Parameters[CountMagicMonstersString] = CountMagicMonsters.ToString();

            CountUniqueMonsters = ImGuiExtension.Checkbox("Unique Monsters", CountUniqueMonsters);
            Parameters[CountUniqueMonstersString] = CountUniqueMonsters.ToString();

            ImGui.Spacing();
            ImGui.Separator();
            ImGui.Spacing();

            MonsterHealthPercentThreshold = ImGuiExtension.IntSlider("Monster Health Percent", MonsterHealthPercentThreshold, 0, 100);
            Parameters[MonsterHealthPercentThresholdString] = MonsterHealthPercentThreshold.ToString();

            MonsterAboveHealthThreshold = ImGuiExtension.Checkbox("Above Health Threshold", MonsterAboveHealthThreshold);
            Parameters[MonsterAboveHealthThresholdString] = MonsterAboveHealthThreshold.ToString();

            return true;
        }

        public override Func<bool> GetCondition(ExtensionParameter extensionParameter)
        {
            return () =>
            {
                var mobCount = 0;
                var maxDistanceSquare = MaxDistance * MaxDistance;

                var playerPosition = extensionParameter.Plugin.GameController.Player.Pos;

                // Make sure we create our own list to iterate as we may be adding/removing from the list
                foreach (var monster in new List<Entity>(extensionParameter.Plugin.LoadedMonsters))
                {
                    if (!monster.HasComponent<Monster>() || !monster.IsValid || !monster.IsAlive || !monster.IsHostile)
                        continue;

                    var monsterType = monster.GetComponent<ObjectMagicProperties>().Rarity;

                    // Don't count this monster type if we are ignoring it
                    if (monsterType == MonsterRarity.White && !CountWhiteMonsters
                        || monsterType == MonsterRarity.Rare && !CountRareMonsters
                        || monsterType == MonsterRarity.Magic && !CountMagicMonsters
                        || monsterType == MonsterRarity.Unique && !CountUniqueMonsters)
                        continue;

                    if (MonsterHealthPercentThreshold > 0)
                    {
                        // If the monster is still too healthy, don't count it
                        var monsterLife = monster.GetComponent<Life>();
                        if (monsterLife != null && monsterLife.Address > 0 && ((monsterLife.CurHP / Math.Max(1, monsterLife.MaxHP)) * 100 >= MonsterHealthPercentThreshold) != MonsterAboveHealthThreshold)
                            continue;
                    }

                    var monsterPosition = monster.Pos;

                    var xDiff = playerPosition.X - monsterPosition.X;
                    var yDiff = playerPosition.Y - monsterPosition.Y;
                    var monsterDistanceSquare = (xDiff * xDiff + yDiff * yDiff);

                    if (monsterDistanceSquare <= maxDistanceSquare)
                    {
                        //if (extensionParameter.Plugin.Settings.Debug)
                        //{
                        //    extensionParameter.Plugin.Log("Enemy was in range. Mobs so far: " + mobCount + " monsterDistanceSquare:" + monsterDistanceSquare + " maxDistanceSquare:" + maxDistanceSquare, 2);
                        //}

                        if (ColdResistanceThreshold > 0 || FireResistanceThreshold > 0 || LightningResistanceThreshold > 0 || ChaosResistanceThreshold > 0)
                        {
                            // We care about resists. Only increment IF we are above the threshold
                            var monsterStats = monster.GetComponent<Stats>();
                            int tempRes = 0;
                            if (ColdResistanceThreshold > 0 && monsterStats.StatDictionary.TryGetValue((GameStat)extensionParameter.Plugin.GameController.Files.Stats.records["cold_damage_resistance_%"].ID, out tempRes) && tempRes >= ColdResistanceThreshold)
                            {
                                mobCount++;
                            }
                            else if (FireResistanceThreshold > 0 && monsterStats.StatDictionary.TryGetValue((GameStat)extensionParameter.Plugin.GameController.Files.Stats.records["fire_damage_resistance_%"].ID, out tempRes) && tempRes >= FireResistanceThreshold)
                            {
                                mobCount++;
                            }
                            else if (LightningResistanceThreshold > 0 && monsterStats.StatDictionary.TryGetValue((GameStat)extensionParameter.Plugin.GameController.Files.Stats.records["lightning_damage_resistance_%"].ID, out tempRes) && tempRes >= LightningResistanceThreshold)
                            {
                                mobCount++;
                            }
                            else if (ChaosResistanceThreshold > 0 && monsterStats.StatDictionary.TryGetValue((GameStat)extensionParameter.Plugin.GameController.Files.Stats.records["chaos_damage_resistance_%"].ID, out tempRes) && tempRes >= ChaosResistanceThreshold)
                            {
                                mobCount++;
                            }
                        }
                        else mobCount++;

                    }

                    if (mobCount >= MinimumMonsterCount)
                    {
                        if (extensionParameter.Plugin.Settings.Debug)
                        {
                            extensionParameter.Plugin.Log("NearbyMonstersCondition returning true because " + mobCount + " mobs valid monsters were found nearby.", 2);
                        }
                        return true;
                    }
                }

                if (extensionParameter.Plugin.Settings.Debug)
                {
                    extensionParameter.Plugin.Log("NearbyMonstersCondition returning false because " + mobCount + " mobs valid monsters were found nearby.", 2);
                }
                return false;
            };
        }

        public override string GetDisplayName(bool isAddingNew)
        {
            string displayName = "Nearby Monsters";

            if (!isAddingNew)
            {
                displayName += " [";
                displayName += ("Types=");
                if (CountWhiteMonsters) displayName += ("White,");
                if (CountRareMonsters) displayName += ("Rare,");
                if (CountMagicMonsters) displayName += ("Magic,");
                if (CountUniqueMonsters) displayName += ("Unique");

                displayName += ("MaxDistance=" + MaxDistance.ToString());
                if (ColdResistanceThreshold > 0 || FireResistanceThreshold > 0 || LightningResistanceThreshold > 0 || ChaosResistanceThreshold > 0)
                    displayName += ("Resist restrictions");
                displayName += "]";

            }

            return displayName;
        }
    }
}