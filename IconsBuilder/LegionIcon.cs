using System;
using System.Collections.Generic;
using ExileCore;
using ExileCore.PoEMemory.Components;
using ExileCore.PoEMemory.MemoryObjects;
using ExileCore.Shared;
using ExileCore.Shared.Abstract;
using ExileCore.Shared.Cache;
using ExileCore.Shared.Enums;
using ExileCore.Shared.Helpers;
using JM.LinqFaster;
using SharpDX;

namespace IconsBuilder
{
    public class LegionIcon : BaseIcon
    {
        public LegionIcon(Entity entity, GameController gameController, IconsBuilderSettings settings, Dictionary<string, Size2> modIcons) :
            base(entity, settings)
        {
            Update(entity, settings, modIcons);
        }

        public override string ToString()
        {
            return $"{Entity.Metadata} : {Entity.Type} ({Entity.Address}) T: {Text}";
        }

        public void Update(Entity entity, IconsBuilderSettings settings, Dictionary<string, Size2> modIcons)
        {
            MainTexture = new HudTexture("Icons.png");
            if (!_HasIngameIcon) MainTexture = new HudTexture("Icons.png");

            switch (Rarity)
            {
                case MonsterRarity.White:
                    MainTexture.Size = settings.SizeEntityWhiteIcon;
                    break;
                case MonsterRarity.Magic:
                    MainTexture.Size = settings.SizeEntityMagicIcon;
                    break;
                case MonsterRarity.Rare:
                    MainTexture.Size = settings.SizeEntityRareIcon;
                    break;
                case MonsterRarity.Unique:
                    MainTexture.Size = settings.SizeEntityUniqueIcon;
                    Text = entity.RenderName;
                    break;
                default:
                    throw new ArgumentException("Legion icon rarity corrupted.");
                    break;
            }

            if (entity.Path.StartsWith("Metadata/Monsters/LegionLeague/MonsterChest", StringComparison.Ordinal) || Rarity == MonsterRarity.Unique)
            {
                MainTexture.UV = SpriteHelper.GetUV(MapIconsIndex.LootFilterLargeGreenSquare);
                MainTexture.Color = Color.LimeGreen;
                Hidden = () => false;
                Text = entity.RenderName;

                Show = () =>
                {
                    if (Entity.IsValid)
                        return Entity.GetComponent<Life>().HPPercentage > 0.02;

                    return Entity.IsAlive;
                };
            }
            else
            {
                string modName = null;

                if (entity.HasComponent<ObjectMagicProperties>())
                {
                    var objectMagicProperties = entity.GetComponent<ObjectMagicProperties>();

                    var mods = objectMagicProperties.Mods;
                    if (mods.Contains("MonsterConvertsOnDeath_")) Show = () => entity.IsValid && entity.IsAlive && entity.IsHostile;

                    modName = mods.FirstOrDefaultF(modIcons.ContainsKey);

                    if (modName != null)
                    {
                        MainTexture = new HudTexture("sprites.png");
                        MainTexture.UV = SpriteHelper.GetUV(modIcons[modName], new Size2F(7, 8));
                        Priority = IconPriority.VeryHigh;
                    }
                    else
                    {
                        switch (Rarity)
                        {
                            case MonsterRarity.White:
                                MainTexture.UV = SpriteHelper.GetUV(MapIconsIndex.LootFilterLargeRedCircle);
                                break;
                            case MonsterRarity.Magic:
                                MainTexture.UV = SpriteHelper.GetUV(MapIconsIndex.LootFilterLargeBlueCircle);

                                break;
                            case MonsterRarity.Rare:
                                MainTexture.UV = SpriteHelper.GetUV(MapIconsIndex.LootFilterLargeYellowCircle);
                                break;
                            case MonsterRarity.Unique:
                                MainTexture.UV = SpriteHelper.GetUV(MapIconsIndex.LootFilterLargeCyanHexagon);
                                MainTexture.Color = Color.DarkOrange;
                                break;
                            default:
                                throw new ArgumentOutOfRangeException();
                        }
                    }
                }

                var statDictionary = Entity.Stats;

                if (statDictionary.Count == 0)
                {
                    statDictionary = entity.GetComponentFromMemory<Stats>().ParseStats();
                    if (statDictionary.Count == 0) Text = "Error";
                }

                if (statDictionary.TryGetValue(GameStat.MonsterMinimapIcon, out var indexMinimapIcon))
                {
                    var name = (MapIconsIndex) indexMinimapIcon;
                    Text = name.ToString().Replace("Legion", "");
                    Priority = IconPriority.Critical;

                    var frozenCheck = new TimeCache<bool>(() =>
                    {
                        var stats = Entity.Stats;
                        if (stats.Count == 0) return false;
                        stats.TryGetValue(GameStat.FrozenInTime, out var FrozenInTime);
                        stats.TryGetValue(GameStat.MonsterHideMinimapIcon, out var MonsterHideMinimapIcon);
                        return FrozenInTime == 1 && MonsterHideMinimapIcon == 1 || FrozenInTime == 0 && MonsterHideMinimapIcon == 0;
                    }, 75);

                    Show = () => Entity.IsAlive && frozenCheck.Value;
                }
                else
                    Show = () => !Hidden() && Entity.GetComponent<Life>().HPPercentage > 0.02;
            }
        }
    }
}
