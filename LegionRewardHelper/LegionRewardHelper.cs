using System;
using System.Collections.Generic;
using ExileCore;
using ExileCore.PoEMemory.MemoryObjects;
using ExileCore.PoEMemory.Components;
using ExileCore.Shared.Cache;
using ExileCore.Shared.Enums;
using ExileCore.Shared.Helpers;
using ExileCore.Shared.Interfaces;
using SharpDX;

namespace LegionRewardHelper
{
    public class LegionRewardHelper : BaseSettingsPlugin<LegionRewardHelperSettings>
    {
        private List<(Entity, Func<bool>, MapIconsIndex)> Entities = new List<(Entity, Func<bool>, MapIconsIndex)>(20);

        public override void EntityAdded(Entity Entity) {
            if (Entity.League==LeagueType.Legion)
            {
                if (Entity.Rarity == MonsterRarity.Unique)
                {
                    if (Entity.Path.StartsWith("Metadata/Monsters/LegionLeague/MonsterChest", StringComparison.Ordinal))
                    {
                        Entities.Add((Entity, () => Entity.IsValid && Entity.GetComponent<Life>().HPPercentage > 0.02,
                                      MapIconsIndex.StashGuild));
                    }
                    else
                    {
                        Entities.Add((Entity, () => Entity.IsValid && Entity.GetComponent<Life>().HPPercentage >= 0.02,
                                      MapIconsIndex.LegionGeneric));
                    }
                }
                else
                {
                    var statDictionary = Entity.GetComponent<Stats>().StatDictionary;
                    if (statDictionary.TryGetValue(GameStat.MonsterMinimapIcon, out var indexMinimapIcon))
                    {
                        var index = (MapIconsIndex) indexMinimapIcon;
                        var frozenCheck = new TimeCache<bool>(() =>
                        {
                            var stats = Entity.GetComponent<Stats>().StatDictionary;
                            if (stats.Count == 0) return false;
                            stats.TryGetValue(GameStat.FrozenInTime, out var FrozenInTime);
                            stats.TryGetValue(GameStat.MonsterHideMinimapIcon, out var MonsterHideMinimapIcon);
                            return FrozenInTime == 1 && MonsterHideMinimapIcon == 1;
                        }, 75);
                        Entities.Add((Entity, () => Entity.IsValid && Entity.IsAlive && frozenCheck.Value, index));
                    }
                }
            }
        }

        private const string iconsPng = "Icons.png";

        public override void Render() {
            var camera = GameController.IngameState.Camera;
            foreach ((var entity, var show, var mapIconsIndex) in Entities)
            {
                if (!show()) continue;
                var worldCoords = entity.Pos;
                worldCoords.Z += Settings.Z;
                var ScreenCoords = camera.WorldToScreen(worldCoords);
                var settingsSize = Settings.Size / 2f;
                var drawRect = new RectangleF(ScreenCoords.X - settingsSize, ScreenCoords.Y - settingsSize, Settings.Size, Settings.Size);
                Graphics.DrawImage(iconsPng, drawRect, SpriteHelper.GetUV(mapIconsIndex));
            }
        }

        public override void AreaChange(AreaInstance area) => Entities.Clear();
    }
}
