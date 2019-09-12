using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using ExileCore;
using ExileCore.PoEMemory.Components;
using ExileCore.PoEMemory.Elements;
using ExileCore.PoEMemory.MemoryObjects;
using ExileCore.Shared.Enums;
using ExileCore.Shared.Helpers;
using SharpDX;
using Vector2N = System.Numerics.Vector2;

namespace AdvancedTooltip
{
    public class AdvancedTooltip : BaseSettingsPlugin<AdvancedTooltipSettings>
    {
        private string affix;
        private bool canRender;
        private Entity itemEntity;
        private List<ModValue> mods = new List<ModValue>();
        private long ModsHash;
        private Vector2N nextLine = Vector2N.Zero;
        private readonly string symbol = " *";
        private Color TColor;
        private Dictionary<int, Color> TColors;

        public override bool Initialise()
        {
            TColors = new Dictionary<int, Color>
            {
                {1, Settings.ItemMods.T1Color}, {2, Settings.ItemMods.T2Color}, {3, Settings.ItemMods.T3Color}
            };

            Input.RegisterKey(Keys.F9);

            Input.ReleaseKey += (sender, keys) =>
            {
                if (keys == Keys.F9) Settings.ItemMods.Enable.Value = !Settings.ItemMods.Enable.Value;
            };

            Graphics.InitImage("menu-colors.png");
            Graphics.InitImage("preload-end.png");
            return true;
        }

        public override Job Tick()
        {
            canRender = true;

            if (GameController.IsLoading)
            {
                canRender = false;
                return null;
            }

            return null;
        }

        public override void Render()
        {
            var uiHover = GameController.Game.IngameState.UIHover;
            if (uiHover?.Address == 0) return;
            var inventoryItemIcon = uiHover?.AsObject<HoverItemIcon>();

            if (inventoryItemIcon?.Tooltip == null)
                return;

            var tooltip = inventoryItemIcon.Tooltip;
            var poeEntity = inventoryItemIcon.Item;

            if (tooltip == null || poeEntity == null || poeEntity.Address == 0) return;

            itemEntity = null;
            mods = null;
            var tooltipRect = tooltip.GetClientRect();

            var modsComponent = poeEntity.GetComponent<Mods>();
            var id = poeEntity.Id;
            var hash = modsComponent?.Hash;

            if (itemEntity == null || itemEntity.Id != id || modsComponent != null && hash != modsComponent.Hash)
            {
                var itemMods = modsComponent?.ItemMods;

                if (itemMods == null || itemMods.Any(x => string.IsNullOrEmpty(x.RawName) && string.IsNullOrEmpty(x.Name)))
                    return;

                mods = itemMods?.Select(item => new ModValue(item, GameController.Files, modsComponent.ItemLevel,
                    GameController.Files.BaseItemTypes.Translate(poeEntity.Path))).ToList();

                itemEntity = poeEntity;
            }

            if (mods != null)
            {
                var startPosition = tooltipRect.TopLeft.TranslateToNum(20, 56);
                var t1 = mods.Count(item => item.CouldHaveTiers() && item.Tier == 1);
                Graphics.DrawText(string.Concat(Enumerable.Repeat(symbol, t1)), startPosition, Settings.ItemMods.T1Color /*, "DFPT_B5_POE:15"*/);

                var t2 = mods.Count(item => item.CouldHaveTiers() && item.Tier == 2);

                Graphics.DrawText(string.Concat(Enumerable.Repeat(symbol, t2)), startPosition.Translate(t1 * 14),
                    Settings.ItemMods.T2Color /*,"DFPT_B5_POE:15"*/);

                var t3 = mods.Count(item => item.CouldHaveTiers() && item.Tier == 3);

                Graphics.DrawText(string.Concat(Enumerable.Repeat(symbol, t3)), startPosition.Translate(t1 * 14 + t2 * 14),
                    Settings.ItemMods.T3Color /*, "DFPT_B5_POE:15"*/);
            }

            if (Settings.ItemLevel.Enable.Value)
            {
                var itemLevel = Convert.ToString(modsComponent?.ItemLevel ?? 0);
                var imageSize = Settings.ItemLevel.TextSize + 10;
                Graphics.DrawText(itemLevel, tooltipRect.TopLeft.Translate(2, 2), Settings.ItemLevel.TextColor);

                Graphics.DrawImage("menu-colors.png",
                    new RectangleF(tooltipRect.TopLeft.X - 2, tooltipRect.TopLeft.Y - 2, imageSize, imageSize),
                    Settings.ItemLevel.BackgroundColor);
            }

            if (Settings.ItemMods.Enable.Value)
            {
                var bottomTooltip = tooltipRect.Bottom + 5;
                Vector2 modPosition;
                modPosition = new Vector2(tooltipRect.X + 20, bottomTooltip + 4);

                var height = mods?.Where(x => x.Record.StatNames.Count(y => y != null) > 0)
                                 .Aggregate(modPosition, (position, item) => DrawMod(item, position)).Y - bottomTooltip ?? 0;

                if (height > 4)
                {
                    var modsRect = new RectangleF(tooltipRect.X + 1, bottomTooltip, tooltipRect.Width, height);
                    Graphics.DrawBox(modsRect, Settings.ItemMods.BackgroundColor);
                }
            }

            if (Settings.WeaponDps.Enable && poeEntity.HasComponent<Weapon>()) DrawWeaponDps(tooltipRect);
        }

        private Vector2 DrawMod(ModValue item, Vector2 position)
        {
            const float EPSILON = 0.001f;
            const int MARGIN_BOTTOM = 4, MARGIN_LEFT = 50;
            var oldPosition = position;
            var settings = Settings.ItemMods;
            var x = 0f;

            switch (item.AffixType)
            {
                case ModType.Prefix:
                    nextLine = Graphics.DrawText("[P]", position, settings.PrefixColor);
                    break;
                case ModType.Suffix:
                    nextLine = Graphics.DrawText("[S]", position, settings.SuffixColor);
                    break;
                case ModType.Corrupted:
                    nextLine = Graphics.DrawText("[C]", position, new Color(220, 20, 60));
                    break;
                case ModType.Unique:
                    nextLine = Graphics.DrawText("[U]", position, new Color(255, 140, 0));
                    break;
                case ModType.Enchantment:
                    nextLine = Graphics.DrawText("[E]", position, new Color(255, 0, 255));
                    break;
                default:
                    nextLine = Graphics.DrawText("[?]", position, new Color(211, 211, 211));
                    break;
            }

            x += nextLine.X;

            if (item.AffixType != ModType.Unique)
            {
                if (item.TotalTiers > 0)
                {
                    affix = $" T{item.Tier}({item.TotalTiers}) ";

                    switch (item.AffixType)
                    {
                        case ModType.Prefix:
                            nextLine = Graphics.DrawText(affix, position.Translate(x), settings.PrefixColor);
                            if (!TColors.TryGetValue(item.Tier, out TColor)) TColor = settings.PrefixColor;

                            break;
                        case ModType.Suffix:
                            nextLine = Graphics.DrawText(affix, position.Translate(x), settings.SuffixColor);
                            if (!TColors.TryGetValue(item.Tier, out TColor)) TColor = settings.SuffixColor;

                            break;
                    }
                }

                var textSize = Graphics.DrawText(item.AffixText, position.Translate(x + nextLine.X), TColor);
                position.Y += textSize.Y;
            }

            {
                for (var i = 0; i < 4; i++)
                {
                    var range = item.Record.StatRange[i];
                    if (range.Min == 0 && range.Max == 0) continue;

                    var stat = item.Record.StatNames[i];
                    var value = item.StatValue[i];
                    if (value <= -1000 || stat == null) continue;

                    var noSpread = !range.HasSpread();
                    var line2 = string.Format(noSpread ? "{0}" : "[{1}] {0}", stat, range);
                    var statText = stat.ValueToString(value);
                    Vector2N txSize;

                    if (item.AffixType == ModType.Unique)
                    {
                        txSize = Graphics.DrawText(statText, position.Translate(x + 30), Color.Gainsboro, FontAlign.Right);
                        var drawText = Graphics.DrawText(line2, position.Translate(x + 40), Color.Gainsboro);
                    }
                    else
                    {
                        txSize = Graphics.DrawText(statText, position.Translate(x), Color.Gainsboro, FontAlign.Right);
                        var drawText = Graphics.DrawText(line2, position.Translate(+40), Color.Gainsboro);
                    }

                    position.Y += txSize.Y;
                }

                return Math.Abs(position.Y - oldPosition.Y) > EPSILON ? position.Translate(0, MARGIN_BOTTOM) : oldPosition;
            }

            return position;
        }

        private void DrawWeaponDps(RectangleF clientRect)
        {
            var weapon = itemEntity.GetComponent<Weapon>();
            if (weapon == null) return;
            if (!itemEntity.IsValid) return;
            var aSpd = (float) Math.Round(1000f / weapon.AttackTime, 2);
            var cntDamages = Enum.GetValues(typeof(DamageType)).Length;
            var doubleDpsPerStat = new float[cntDamages];
            float physDmgMultiplier = 1;
            var PhysHi = weapon.DamageMax;
            var PhysLo = weapon.DamageMin;

            foreach (var mod in mods)
            {
                for (var iStat = 0; iStat < 4; iStat++)
                {
                    var range = mod.Record.StatRange[iStat];
                    if (range.Min == 0 && range.Max == 0) continue;

                    var theStat = mod.Record.StatNames[iStat];
                    if (theStat == null) continue;
                    var value = mod.StatValue[iStat];

                    switch (theStat.Key)
                    {
                        case "physical_damage_+%":
                        case "local_physical_damage_+%":
                            physDmgMultiplier += value / 100f;
                            break;

                        case "local_attack_speed_+%":
                            aSpd *= (100f + value) / 100;
                            break;

                        case "local_minimum_added_physical_damage":
                            PhysLo += value;
                            break;
                        case "local_maximum_added_physical_damage":
                            PhysHi += value;
                            break;

                        case "local_minimum_added_fire_damage":
                        case "local_maximum_added_fire_damage":
                        case "unique_local_minimum_added_fire_damage_when_in_main_hand":
                        case "unique_local_maximum_added_fire_damage_when_in_main_hand":
                            doubleDpsPerStat[(int) DamageType.Fire] += value;
                            break;

                        case "local_minimum_added_cold_damage":
                        case "local_maximum_added_cold_damage":
                        case "unique_local_minimum_added_cold_damage_when_in_off_hand":
                        case "unique_local_maximum_added_cold_damage_when_in_off_hand":
                            doubleDpsPerStat[(int) DamageType.Cold] += value;
                            break;

                        case "local_minimum_added_lightning_damage":
                        case "local_maximum_added_lightning_damage":
                            doubleDpsPerStat[(int) DamageType.Lightning] += value;
                            break;

                        case "unique_local_minimum_added_chaos_damage_when_in_off_hand":
                        case "unique_local_maximum_added_chaos_damage_when_in_off_hand":
                        case "local_minimum_added_chaos_damage":
                        case "local_maximum_added_chaos_damage":
                            doubleDpsPerStat[(int) DamageType.Chaos] += value;
                            break;
                    }
                }
            }

            var settings = Settings.WeaponDps;

            Color[] elementalDmgColors =
            {
                Color.White, settings.DmgFireColor, settings.DmgColdColor, settings.DmgLightningColor, settings.DmgChaosColor
            };

            var component = itemEntity.GetComponent<Quality>();
            if (component == null) return;
            physDmgMultiplier += component.ItemQuality / 100f;
            PhysLo = (int) Math.Round(PhysLo * physDmgMultiplier);
            PhysHi = (int) Math.Round(PhysHi * physDmgMultiplier);
            doubleDpsPerStat[(int) DamageType.Physical] = PhysLo + PhysHi;

            aSpd = (float) Math.Round(aSpd, 2);
            var pDps = doubleDpsPerStat[(int) DamageType.Physical] / 2 * aSpd;
            float eDps = 0;
            var firstEmg = 0;
            Color DpsColor = settings.pDamageColor;

            for (var i = 1; i < cntDamages; i++)
            {
                eDps += doubleDpsPerStat[i] / 2 * aSpd;
                if (!(doubleDpsPerStat[i] > 0)) continue;

                if (firstEmg == 0)
                {
                    firstEmg = i;
                    DpsColor = elementalDmgColors[i];
                }
                else
                    DpsColor = settings.eDamageColor;
            }

            var textPosition = new Vector2(clientRect.Right - 15, clientRect.Y + 1);
            var pDpsSize = pDps > 0 ? Graphics.DrawText(pDps.ToString("#.#"), textPosition, FontAlign.Right) : Vector2N.Zero;

            var eDpsSize = eDps > 0
                ? Graphics.DrawText(eDps.ToString("#.#"), textPosition.Translate(0, pDpsSize.Y), DpsColor, FontAlign.Right)
                : Vector2N.Zero;

            var dps = pDps + eDps;

            var dpsSize = dps > 0
                ? Graphics.DrawText(dps.ToString("#.#"), textPosition.Translate(0, pDpsSize.Y + eDpsSize.Y), Color.White, FontAlign.Right)
                : Vector2N.Zero;

            var dpsTextPosition = textPosition.Translate(0, pDpsSize.Y + eDpsSize.Y + dpsSize.Y);
            Graphics.DrawText("dps", dpsTextPosition, settings.TextColor, FontAlign.Right);

            Graphics.DrawImage("preload-end.png", new RectangleF(textPosition.X - 100, textPosition.Y - 6, 100, 78),
                settings.BackgroundColor);
        }
    }
}
