using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Exile;
using Exile.PoEMemory.MemoryObjects;
using Shared.Helpers;
using PoEMemory;
using PoEMemory.Components;
using Shared.Enums;
using Shared.Interfaces;
using SharpDX;

namespace HealthBars
{
    public class HealthBars : BaseSettingsPlugin<HealthBarsSettings>
    {
        private List<string> Ignored = new List<string>()
        {
            "Metadata/Monsters/Daemon/SilverPoolChillDaemon",
            "Metadata/Monsters/Daemon",
            "Metadata/Monsters/Frog/FrogGod/SilverOrb",
            "Metadata/Monsters/Frog/FrogGod/SilverPool",
            "Metadata/Monsters/Labyrinth/GoddessOfJusticeMapBoss@7",
            "Metadata/Monsters/Labyrinth/GoddessOfJustice@",
            "Metadata/Monsters/LeagueBetrayal/MasterNinjaCop",
        };

        public override void OnLoad()
        {
            CanUseMultiThreading = true;
            Graphics.InitImage("healthbar.png");
        }


        public override bool Initialise() {
            Player = GameController.Player;
            ingameUI = GameController.IngameState.IngameUi;
            PlayerBar = new HealthBar(Player, Settings);
            GameController.EntityListWrapper.PlayerUpdate += (sender, args) =>
            {
                Player = GameController.Player;

                PlayerBar = new HealthBar(Player, Settings);
            };
            ingameUICheckVisible = new TimeCache<bool>(() =>
            {
                windowRectangle = GameController.Window.GetWindowRectangleReal();
                windowSize = new Size2F(windowRectangle.Width / 2560, windowRectangle.Height / 1600);
                camera = GameController.Game.IngameState.Camera;
                return ingameUI.BetrayalWindow.IsVisibleLocal || ingameUI.SellWindow.IsVisibleLocal ||
                       ingameUI.DelveWindow.IsVisibleLocal || ingameUI.IncursionWindow.IsVisibleLocal ||
                       ingameUI.UnveilWindow.IsVisibleLocal || ingameUI.TreePanel.IsVisibleLocal || ingameUI.AtlasPanel.IsVisibleLocal ||
                       ingameUI.CraftBench.IsVisibleLocal;
            }, 250);
            return true;
        }

        public override void AreaChange(AreaInstance area) => ingameUI = GameController.IngameState.IngameUi;

        private bool CanTick = true;
        private List<Element> ElementForSkip = new List<Element>();
        private double time;
        private IngameUIElements ingameUI;
        private CachedValue<bool> ingameUICheckVisible;
        
        public void HpBarWork(HealthBar healthBar) {
            if (!healthBar.Settings.Enable)
            {
                healthBar.Skip = true;
                return;
            }

            if (!healthBar.Entity.IsAlive)
            {
                healthBar.Skip = true;
                return;
            }

            var healthBarDistance = healthBar.Distance;
            if (healthBarDistance > Settings.LimitDrawDistance)
            {
                healthBar.Skip = true;
                return;
            }

            healthBar.HpPercent = healthBar.Life.HPPercentage;
            if (healthBar.HpPercent < 0.001f)
            {
                healthBar.Skip = true;
                return;
            }

            if (healthBar.Type == CreatureType.Minion && healthBar.HpPercent*100 > Settings.ShowMinionOnlyBelowHp)
            {
                healthBar.Skip = true;
                return;
            }

            if (healthBar.Entity.League==LeagueType.Legion && healthBar.Entity.IsHidden && healthBar.Entity.Rarity != MonsterRarity.Unique)
            {
                healthBar.Skip = true;
                return;
            }

            var _ = healthBar.IsHostile;
            var worldCoords = healthBar.Entity.Pos;
            worldCoords.Z += Settings.GlobalZ;
            var mobScreenCoords = camera.WorldToScreen(worldCoords);
            if (mobScreenCoords == Vector2.Zero) return;
            var scaledWidth = healthBar.Settings.Width * windowSize.Width;
            var scaledHeight = healthBar.Settings.Height * windowSize.Height;
            healthBar.BackGround = new RectangleF(mobScreenCoords.X - scaledWidth / 2f, mobScreenCoords.Y - scaledHeight / 2f, scaledWidth,
                                                  scaledHeight);
            if (healthBarDistance > 80 && !windowRectangle.Intersects(healthBar.BackGround))
            {
                healthBar.Skip = true;
                return;
            }

            foreach (var forSkipBar in ElementForSkip)
                if (forSkipBar.IsVisibleLocal && forSkipBar.GetClientRectCache.Intersects(healthBar.BackGround))
                {
                    healthBar.Skip = true;
                    continue;
                }

            healthBar.HpWidth = healthBar.HpPercent * scaledWidth;
            healthBar.EsWidth = healthBar.Life.ESPercentage * scaledWidth;
        }

        public override Job Tick() {
            if (Settings.MultiThreading && GameController.EntityListWrapper.ValidEntitiesByType[EntityType.Monster].Count >= Settings.MultiThreadingCountEntities)
            {
             return new Job(nameof(HealthBars), TickLogic);
              
                 // return GameController.MultiThreadManager.AddJob(TickLogic, nameof(HealthBars));
            }
            TickLogic();
            return null;
        }


        void TickLogic() {
            CanTick = true;
            if (ingameUICheckVisible.Value)
            {
                CanTick = false;
                return;
            }

            if (camera == null)
            {
                CanTick = false;
                return;
            }

            if (GameController.Area.CurrentArea.IsTown &&!Settings.ShowInTown)
            {
                CanTick = false;
                return;
            }
            foreach (var validEntity in GameController.EntityListWrapper.ValidEntitiesByType[EntityType.Monster])
            {
                var healthBar = validEntity.GetHudComponent<HealthBar>();
                if (healthBar != null)
                    HpBarWork(healthBar);
            }

            foreach (var validEntity in GameController.EntityListWrapper.ValidEntitiesByType[EntityType.Player])
            {
                var healthBar = validEntity.GetHudComponent<HealthBar>();
                if (healthBar != null)
                    HpBarWork(healthBar);
            }
        }

        private RectangleF windowRectangle;
        private Size2F windowSize;
        private Camera camera;
        private HealthBar PlayerBar;
        private Entity Player;


        private Vector2 oldplayerCord;
        
        public override void Render() {
            if (!CanTick)
                return;
            foreach (var entity in GameController.EntityListWrapper.ValidEntitiesByType[EntityType.Monster])
            {
                var healthBar = entity.GetHudComponent<HealthBar>();
                if (healthBar == null) continue;
                if (healthBar.Skip)
                {
                    healthBar.Skip = false;
                    continue;
                }

                DrawBar(healthBar);
            }

            foreach (var entity in GameController.EntityListWrapper.ValidEntitiesByType[EntityType.Player])
            {
                var healthBar = entity.GetHudComponent<HealthBar>();
                if (healthBar == null) continue;
                if (healthBar.Skip)
                {
                    healthBar.Skip = false;
                    continue;
                }

                DrawBar(healthBar);
            }

            if (Settings.SelfHealthBarShow)
            {
                var worldCoords = PlayerBar.Entity.Pos;
                worldCoords.Z += Settings.PlayerZ;
                var result = camera.WorldToScreen(worldCoords);
                if (Math.Abs(oldplayerCord.X - result.X) < 40 || Math.Abs(oldplayerCord.X - result.Y) < 40)
                    result = oldplayerCord;
                else
                    oldplayerCord = result;
                var scaledWidth = PlayerBar.Settings.Width * windowSize.Width;
                var scaledHeight = PlayerBar.Settings.Height * windowSize.Height;
                PlayerBar.BackGround = new RectangleF(result.X - scaledWidth / 2f, result.Y - scaledHeight / 2f, scaledWidth,
                    scaledHeight);
                PlayerBar.HpPercent = PlayerBar.Life.HPPercentage;
                PlayerBar.HpWidth = PlayerBar.HpPercent * scaledWidth;
                PlayerBar.EsWidth = PlayerBar.Life.ESPercentage * scaledWidth;
                DrawBar(PlayerBar);
            }
        }


        public void DrawBar(HealthBar bar) {
            if (Settings.ImGuiRender)
            {
                Graphics.DrawBox(bar.BackGround, bar.Settings.BackGround);
                Graphics.DrawBox(new RectangleF(bar.BackGround.X, bar.BackGround.Y, bar.HpWidth, bar.BackGround.Height), bar.Color);
            }
            else
            {
                Graphics.DrawImage("healthbar.png", bar.BackGround, bar.Settings.BackGround);

                Graphics.DrawImage("healthbar.png", new RectangleF(bar.BackGround.X, bar.BackGround.Y, bar.HpWidth, bar.BackGround.Height),
                                   bar.Color);
            }

            Graphics.DrawBox(new RectangleF(bar.BackGround.X, bar.BackGround.Y, bar.EsWidth, bar.BackGround.Height * 0.33f), Color.Aqua);
            bar.BackGround.Inflate(1, 1);
            Graphics.DrawFrame(bar.BackGround, bar.Settings.Outline, 1);
            if (bar.Settings.ShowPercents)
            {
                Graphics.DrawText($"{Math.Floor(bar.HpPercent * 100).ToString(CultureInfo.InvariantCulture)}",
                                  new Vector2(bar.BackGround.Right, bar.BackGround.Center.Y - Graphics.Font.Size / 2f),
                                  bar.Settings.PercentTextColor);
            }


            if (bar.Settings.ShowHealthText)
            {
                var formattableString = $"{bar.Life.CurHP}/{bar.MaxHp}";
                Graphics.DrawText(formattableString,
                                  new Vector2(bar.BackGround.Center.X, bar.BackGround.Center.Y - Graphics.Font.Size / 2f),
                                  bar.Settings.HealthTextColor, FontAlign.Center);
            }
        }

        public override void EntityAdded(Entity Entity) {
            if (Entity.Type != EntityType.Monster && Entity.Type != EntityType.Player || Entity.Address == GameController.Player.Address ||
                Entity.Type == EntityType.Daemon) return;
            if (Entity.GetComponent<Life>() != null && !Entity.IsAlive) return;
            if (Ignored.Any(x => Entity.Path.StartsWith(x))) return;
            Entity.SetHudComponent(new HealthBar(Entity, Settings));
        }

        public override void EntityRemoved(Entity Entity) { }
    }
}