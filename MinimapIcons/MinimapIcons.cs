using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
using Map = ExileCore.PoEMemory.Elements.Map;

namespace MinimapIcons
{
    public class MinimapIcons : BaseSettingsPlugin<MapIconsSettings>
    {
        private const string ALERT_CONFIG = "config\\new_mod_alerts.txt";
        private readonly Dictionary<string, Size2> modIcons = new Dictionary<string, Size2>();
        private CachedValue<float> _diag;
        private CachedValue<RectangleF> _mapRect;

        private List<string> ignoreEntites = new List<string>
        {
            "Metadata/Monsters/Frog/FrogGod/SilverPool",
            "Metadata/MiscellaneousObjects/WorldItem",
            "Metadata/Pet/Weta/Basic",
            "Metadata/Monsters/Daemon/SilverPoolChillDaemon",
            "Metadata/Monsters/Daemon",
            "Metadata/Monsters/Frog/FrogGod/SilverOrbFromMonsters"
        };

        private IngameUIElements ingameStateIngameUi;
        private float k;
        private bool largeMap;
        private float scale;
        private Vector2 screentCenterCache;
        private RectangleF MapRect => _mapRect?.Value ?? (_mapRect = new TimeCache<RectangleF>(() => mapWindow.GetClientRect(), 100)).Value;
        private Map mapWindow => GameController.Game.IngameState.IngameUi.Map;
        private Camera camera => GameController.Game.IngameState.Camera;
        private float diag =>
            _diag?.Value ?? (_diag = new TimeCache<float>(() =>
            {
                if (ingameStateIngameUi.Map.SmallMiniMap.IsVisibleLocal)
                {
                    var mapRect = ingameStateIngameUi.Map.SmallMiniMap.GetClientRect();
                    return (float) (Math.Sqrt(mapRect.Width * mapRect.Width + mapRect.Height * mapRect.Height) / 2f);
                }

                return (float) Math.Sqrt(camera.Width * camera.Width + camera.Height * camera.Height);
            }, 100)).Value;
        private Vector2 screenCenter =>
            new Vector2(MapRect.Width / 2, MapRect.Height / 2 - 20) + new Vector2(MapRect.X, MapRect.Y) +
            new Vector2(mapWindow.LargeMapShiftX, mapWindow.LargeMapShiftY);

        public override void OnLoad()
        {
            LoadConfig();
            Graphics.InitImage("sprites.png");
            Graphics.InitImage("Icons.png");
            CanUseMultiThreading = true;
        }

        public override bool Initialise()
        {
            return true;
        }

        private void LoadConfig()
        {
            var readAllLines = File.ReadAllLines(ALERT_CONFIG);

            foreach (var readAllLine in readAllLines)
            {
                if (readAllLine.StartsWith("#")) continue;
                var s = readAllLine.Split(';');
                var sz = s[2].Trim().Split(',');
                modIcons[s[0]] = new Size2(int.Parse(sz[0]), int.Parse(sz[1]));
            }
        }

        public override Job Tick()
        {
            if (Settings.MultiThreading)
                return GameController.MultiThreadManager.AddJob(TickLogic, nameof(MinimapIcons));

            TickLogic();
            return null;
        }

        private void TickLogic()
        {
            ingameStateIngameUi = GameController.Game.IngameState.IngameUi;

            if (ingameStateIngameUi.Map.SmallMiniMap.IsVisibleLocal)
            {
                var mapRect = ingameStateIngameUi.Map.SmallMiniMap.GetClientRectCache;
                screentCenterCache = new Vector2(mapRect.X + mapRect.Width / 2, mapRect.Y + mapRect.Height / 2);
                largeMap = false;
            }
            else if (ingameStateIngameUi.Map.LargeMap.IsVisibleLocal)
            {
                screentCenterCache = screenCenter;
                largeMap = true;
            }

            k = camera.Width < 1024f ? 1120f : 1024f;
            scale = k / camera.Height * camera.Width * 3f / 4f / mapWindow.LargeMapZoom;
        }

        public override void Render()
        {
            if (!Settings.Enable.Value || !GameController.InGame || Settings.DrawOnlyOnLargeMap && !largeMap) return;

            if (ingameStateIngameUi.AtlasPanel.IsVisibleLocal || ingameStateIngameUi.DelveWindow.IsVisibleLocal ||
                ingameStateIngameUi.TreePanel.IsVisibleLocal)
                return;

            var playerPos = GameController.Player.GetComponent<Positioned>().GridPos;
            var posZ = GameController.Player.GetComponent<Render>().Pos.Z;
            var mapWindowLargeMapZoom = mapWindow.LargeMapZoom;

            var baseIcons = GameController.EntityListWrapper.OnlyValidEntities
                .SelectWhereF(x => x.GetHudComponent<BaseIcon>(), icon => icon != null).OrderByF(x => x.Priority)
                .ToList();

            foreach (var icon in baseIcons)
            {
                if (icon.Entity.Type == EntityType.WorldItem)
                    continue;

                if (!Settings.DrawMonsters && icon.Entity.Type == EntityType.Monster)
                    continue;

                if (icon.HasIngameIcon)
                    continue;

                if (!icon.Show())
                    continue;

                var component = icon?.Entity?.GetComponent<Render>();
                if (component == null) continue;
                var iconZ = component.Pos.Z;
                Vector2 position;

                if (largeMap)
                {
                    position = screentCenterCache + MapIcon.DeltaInWorldToMinimapDelta(
                                   icon.GridPosition() - playerPos, diag, scale, (iconZ - posZ) / (9f / mapWindowLargeMapZoom));
                }
                else
                {
                    position = screentCenterCache +
                               MapIcon.DeltaInWorldToMinimapDelta(icon.GridPosition() - playerPos, diag, 240f, (iconZ - posZ) / 20);
                }

                HudTexture iconValueMainTexture;
                iconValueMainTexture = icon.MainTexture;
                var size = iconValueMainTexture.Size;
                var halfSize = size / 2f;
                icon.DrawRect = new RectangleF(position.X - halfSize, position.Y - halfSize, size, size);
                Graphics.DrawImage(iconValueMainTexture.FileName, icon.DrawRect, iconValueMainTexture.UV, iconValueMainTexture.Color);

                if (icon.Hidden())
                {
                    var s = icon.DrawRect.Width * 0.5f;
                    icon.DrawRect.Inflate(-s, -s);

                    Graphics.DrawImage(icon.MainTexture.FileName, icon.DrawRect,
                        SpriteHelper.GetUV(MapIconsIndex.LootFilterSmallCyanCircle), Color.White);

                    icon.DrawRect.Inflate(s, s);
                }

                if (!string.IsNullOrEmpty(icon.Text))
                    Graphics.DrawText(icon.Text, position.Translate(0, Settings.ZForText), FontAlign.Center);
            }

            if (Settings.DrawNotValid)
            {
                for (var index = 0; index < GameController.EntityListWrapper.NotOnlyValidEntities.Count; index++)
                {
                    var entity = GameController.EntityListWrapper.NotOnlyValidEntities[index];
                    if (entity.Type == EntityType.WorldItem) continue;
                    var icon = entity.GetHudComponent<BaseIcon>();

                    if (icon != null && !entity.IsValid && icon.Show())
                    {
                        if (icon.Entity.Type == EntityType.WorldItem)
                            continue;

                        if (!Settings.DrawMonsters && icon.Entity.Type == EntityType.Monster)
                            continue;

                        if (icon.HasIngameIcon)
                            continue;

                        if (!icon.Show())
                            continue;

                        var iconZ = icon.Entity.Pos.Z;
                        Vector2 position;

                        if (largeMap)
                        {
                            position = screentCenterCache + MapIcon.DeltaInWorldToMinimapDelta(
                                           icon.GridPosition() - playerPos, diag, scale, (iconZ - posZ) / (9f / mapWindowLargeMapZoom));
                        }
                        else
                        {
                            position = screentCenterCache +
                                       MapIcon.DeltaInWorldToMinimapDelta(icon.GridPosition() - playerPos, diag, 240f, (iconZ - posZ) / 20);
                        }

                        HudTexture iconValueMainTexture;
                        iconValueMainTexture = icon.MainTexture;
                        var size = iconValueMainTexture.Size;
                        var halfSize = size / 2f;
                        icon.DrawRect = new RectangleF(position.X - halfSize, position.Y - halfSize, size, size);
                        Graphics.DrawImage(iconValueMainTexture.FileName, icon.DrawRect, iconValueMainTexture.UV, iconValueMainTexture.Color);

                        if (icon.Hidden())
                        {
                            var s = icon.DrawRect.Width * 0.5f;
                            icon.DrawRect.Inflate(-s, -s);

                            Graphics.DrawImage(icon.MainTexture.FileName, icon.DrawRect,
                                SpriteHelper.GetUV(MapIconsIndex.LootFilterSmallCyanCircle), Color.White);

                            icon.DrawRect.Inflate(s, s);
                        }

                        if (!string.IsNullOrEmpty(icon.Text))
                            Graphics.DrawText(icon.Text, position.Translate(0, Settings.ZForText), FontAlign.Center);
                    }
                }
            }
        }
    }
}
