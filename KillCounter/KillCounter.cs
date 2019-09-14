using System;
using System.Collections.Generic;
using System.Windows.Forms;
using ExileCore;
using ExileCore.PoEMemory.Components;
using ExileCore.PoEMemory.MemoryObjects;
using ExileCore.Shared.Enums;
using ExileCore.Shared.Helpers;
using ExileCore.Shared.Interfaces;
using ExileCore.Shared.Nodes;
using ExileCore.Shared.Static;
using SharpDX;

namespace KillCounter
{
    public class KillCounter : BaseSettingsPlugin<KillCounterSettings>
    {
        private bool _canRender;
        private Dictionary<uint, HashSet<long>> countedIds;
        private Dictionary<MonsterRarity, int> counters;
        private int sessionCounter;
        private int summaryCounter;

        public override bool Initialise()
        {
            GameController.LeftPanel.WantUse(() => Settings.Enable);
            countedIds = new Dictionary<uint, HashSet<long>>();
            counters = new Dictionary<MonsterRarity, int>();
            Init();
            return true;
        }

        public override void OnLoad()
        {
            CanUseMultiThreading = true;
            Order = -10;
            Graphics.InitImage("preload-new.png");
        }

        private void Init()
        {
            foreach (MonsterRarity rarity in Enum.GetValues(typeof(MonsterRarity)))
            {
                counters[rarity] = 0;
            }
        }

        public override void AreaChange(AreaInstance area)
        {
            if (!Settings.Enable.Value) return;
            countedIds.Clear();
            counters.Clear();
            sessionCounter += summaryCounter;
            summaryCounter = 0;
            Init();
        }

        public override Job Tick()
        {
            if (Settings.MultiThreading)
                return GameController.MultiThreadManager.AddJob(TickLogic, nameof(KillCounter));

            TickLogic();
            return null;
        }

        private void TickLogic()
        {
            foreach (var entity in GameController.EntityListWrapper.ValidEntitiesByType[EntityType.Monster])
            {
                if (entity.IsAlive) continue;
                Calc(entity);
            }
        }

        public override void Render()
        {
            var UIHover = GameController.Game.IngameState.UIHover;
            var miniMap = GameController.Game.IngameState.IngameUi.Map.SmallMiniMap;

            if (Settings.Enable.Value && UIHover.Address != 0x00 && UIHover.Tooltip.Address != 0x00 && UIHover.Tooltip.IsVisibleLocal &&
                UIHover.Tooltip.GetClientRect().Intersects(miniMap.GetClientRect()))
                _canRender = false;

            if (UIHover.Address == 0x00 || UIHover.Tooltip.Address == 0x00 || !UIHover.Tooltip.IsVisibleLocal)
                _canRender = true;

            if (!Settings.Enable || Input.GetKeyState(Keys.F10) || GameController.Area.CurrentArea == null ||
                !Settings.ShowInTown && GameController.Area.CurrentArea.IsTown ||
                !Settings.ShowInTown && GameController.Area.CurrentArea.IsHideout) return;

            if (!_canRender) return;

            var position = GameController.LeftPanel.StartDrawPoint;
            var size = Vector2.Zero;

            if (Settings.ShowDetail) size = DrawCounters(position);
            var session = $"({sessionCounter + summaryCounter})";
            position.Y += size.Y;

            var size2 = Graphics.DrawText($"kills: {summaryCounter} {session}", position.Translate(0, 5), Settings.TextColor,
                FontAlign.Right);

            var width = Math.Max(size.X, size2.X);
            var bounds = new RectangleF(position.X - width - 50, position.Y - size.Y, width + 50, size.Y + size2.Y + 10);
            Graphics.DrawImage("preload-new.png", bounds, Settings.BackgroundColor);
            GameController.LeftPanel.StartDrawPoint = position;
        }

        //TODO Rewrite with use ImGuiRender.DrawMultiColoredText()
        private Vector2 DrawCounters(Vector2 position)
        {
            const int INNER_MARGIN = 15;
            position.Y += 5;
            var drawText = Graphics.DrawText(counters[MonsterRarity.White].ToString(), position, Color.White, FontAlign.Right);
            position.X -= INNER_MARGIN + drawText.X;
            drawText = Graphics.DrawText(counters[MonsterRarity.Magic].ToString(), position, HudSkin.MagicColor, FontAlign.Right);
            position.X -= INNER_MARGIN + drawText.X;
            drawText = Graphics.DrawText(counters[MonsterRarity.Rare].ToString(), position, HudSkin.RareColor, FontAlign.Right);
            position.X -= INNER_MARGIN + drawText.X;
            drawText = Graphics.DrawText(counters[MonsterRarity.Unique].ToString(), position, HudSkin.UniqueColor, FontAlign.Right);

            return drawText.TranslateToNum();
        }

        public override void EntityAdded(Entity Entity)
        {
        }

        private void Calc(Entity Entity)
        {
            var areaHash = GameController.Area.CurrentArea.Hash;

            if (!countedIds.TryGetValue(areaHash, out var monstersHashSet))
            {
                monstersHashSet = new HashSet<long>();
                countedIds[areaHash] = monstersHashSet;
            }

            if (!Entity.HasComponent<ObjectMagicProperties>()) return;
            var hashMonster = Entity.Id;

            if (!monstersHashSet.Contains(hashMonster))
            {
                monstersHashSet.Add(hashMonster);
                var rarity = Entity.Rarity;

                if (Entity.IsHostile && rarity >= MonsterRarity.White && rarity <= MonsterRarity.Unique)
                {
                    counters[rarity]++;
                    summaryCounter++;
                }
            }
        }
    }

    public class KillCounterSettings : ISettings
    {
        public KillCounterSettings()
        {
            ShowDetail = new ToggleNode(true);
            ShowInTown = new ToggleNode(false);
            TextColor = new ColorBGRA(220, 190, 130, 255);
            BackgroundColor = new ColorBGRA(0, 0, 0, 255);
            LabelTextSize = new RangeNode<int>(16, 10, 20);
            KillsTextSize = new RangeNode<int>(16, 10, 20);
        }

        public ToggleNode ShowInTown { get; set; }
        public ToggleNode ShowDetail { get; set; }
        public ColorNode TextColor { get; set; }
        public ColorNode BackgroundColor { get; set; }
        public RangeNode<int> LabelTextSize { get; set; }
        public RangeNode<int> KillsTextSize { get; set; }
        public ToggleNode UseImguiForDraw { get; set; } = new ToggleNode(true);
        public ToggleNode MultiThreading { get; set; } = new ToggleNode(false);
        public ToggleNode Enable { get; set; } = new ToggleNode(false);
    }
}
