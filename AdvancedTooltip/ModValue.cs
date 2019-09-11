using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Basic.Models;
using Exile;
using Shared.Helpers;
using PoEMemory;
using PoEMemory.FilesInMemory;
using Shared.Enums;
using SharpDX;

namespace AdvancedTooltip
{
    public class ModValue
    {
        private readonly int totalTiers = 1;

        public ModValue(ItemMod mod, FilesContainer fs, int iLvl, BaseItemType baseItem) {
            var baseClassName = baseItem.ClassName.ToLower().Replace(' ', '_');
            Record = fs.Mods.records[mod.RawName];
            AffixType = Record.AffixType;
            AffixText = string.IsNullOrEmpty(Record.UserFriendlyName) ? Record.Key : Record.UserFriendlyName;
            IsCrafted = Record.Domain == ModDomain.Master;
            StatValue = new[] {mod.Value1, mod.Value2, mod.Value3, mod.Value4};
            Tier = -1;
            var subOptimalTierDistance = 0;

            if (fs.Mods.recordsByTier.TryGetValue(Tuple.Create(Record.Group, Record.AffixType), out var allTiers))
            {
                var tierFound = false;
                totalTiers = 0;
                var keyRcd = Record.Key.Where(char.IsLetter).ToArray();
                var optimizedListTiers = allTiers.Where(x => x.Key.StartsWith(new string(keyRcd))).ToList();
                foreach (var tmp in optimizedListTiers)
                {
                    /*if(Math.Abs(Record.Key.Length-tmp.Key.Length)>2)
                        continue;*/
                    var keyrcd = tmp.Key.Where(char.IsLetter).ToArray();
                    if (!keyrcd.SequenceEqual(keyRcd))
                        continue;

                    int baseChance;
                    if (!tmp.TagChances.TryGetValue(baseClassName, out baseChance))
                        baseChance = -1;

                    int defaultChance;
                    if (!tmp.TagChances.TryGetValue("default", out defaultChance))
                        defaultChance = 0;

                    var tagChance = -1;
                    foreach (var tg in baseItem.Tags)
                        if (tmp.TagChances.ContainsKey(tg))
                            tagChance = tmp.TagChances[tg];

                    var moreTagChance = -1;
                    foreach (var tg in baseItem.MoreTagsFromPath)
                        if (tmp.TagChances.ContainsKey(tg))
                            moreTagChance = tmp.TagChances[tg];

                    #region GetOnlyValidMods

                    switch (baseChance)
                    {
                        case 0:
                            break;
                        case -1: //baseClass name not found in mod tags.
                            switch (tagChance)
                            {
                                case 0:
                                    break;
                                case -1: //item tags not found in mod tags.
                                    switch (moreTagChance)
                                    {
                                        case 0:
                                            break;
                                        case -1: //more item tags not found in mod tags.
                                            if (defaultChance > 0)
                                            {
                                                totalTiers++;
                                                if (tmp.Equals(Record))
                                                {
                                                    Tier = totalTiers;
                                                    tierFound = true;
                                                }

                                                if (!tierFound && tmp.MinLevel <= iLvl) subOptimalTierDistance++;
                                            }

                                            break;
                                        default:
                                            totalTiers++;
                                            if (tmp.Equals(Record))
                                            {
                                                Tier = totalTiers;
                                                tierFound = true;
                                            }

                                            if (!tierFound && tmp.MinLevel <= iLvl) subOptimalTierDistance++;
                                            break;
                                    }

                                    break;
                                default:
                                    totalTiers++;
                                    if (tmp.Equals(Record))
                                    {
                                        Tier = totalTiers;
                                        tierFound = true;
                                    }

                                    if (!tierFound && tmp.MinLevel <= iLvl) subOptimalTierDistance++;
                                    break;
                            }

                            break;
                        default:
                            totalTiers++;
                            if (tmp.Equals(Record))
                            {
                                Tier = totalTiers;
                                tierFound = true;
                            }

                            if (!tierFound && tmp.MinLevel <= iLvl) subOptimalTierDistance++;
                            break;
                    }

                    #endregion
                }

                if (Tier == -1 && !string.IsNullOrEmpty(Record.Tier))
                {
                    /*var tierNumber = Record.Tier.Split(' ')[1];
                     tierNumber = tierNumber.Replace('M', ' ');*/
                    var tierNumber = new string(Record.Tier.Where(x => char.IsDigit(x)).ToArray());
                    if (int.TryParse(tierNumber, out var result))
                    {
                        Tier = result;
                        totalTiers = optimizedListTiers.Count;
                    }
                }

                /*else if (string.IsNullOrEmpty(Record.Tier))
                {
                    Tier = -1;
                    totalTiers = 0;
                }*/
            }

            double hue = totalTiers == 1 ? 180 : 120 - Math.Min(subOptimalTierDistance, 3) * 40;
            Color = ConvertHelper.ColorFromHsv(hue, totalTiers == 1 ? 0 : 1, 1);
        }

        public ModType AffixType { get; private set; }
        public bool IsCrafted { get; private set; }
        public string AffixText { get; private set; }
        public Color Color { get; private set; }
        public ModsDat.ModRecord Record { get; }
        public int[] StatValue { get; private set; }
        public int Tier { get; private set; }

        public int TotalTiers => totalTiers;

        public bool CouldHaveTiers() => totalTiers > 0;
    }
}