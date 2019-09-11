using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Exile;
using Exile.PoEMemory.MemoryObjects;
using JM.LinqFaster;
using Shared;
using Shared.Helpers;
using PoEMemory.Components;
using Shared.Enums;
using Shared.Interfaces;
using Shared.Nodes;
using Shared.Static;
using SharpDX;
using Input = Exile.Input;
using Vector2N = System.Numerics.Vector2;

namespace MiscInformation
{
    public class MiscInformation : BaseSettingsPlugin<MiscInformationSettings>
    {
        private Dictionary<int, float> ArenaEffectiveLevels = new Dictionary<int, float>()
        {
            {71, 70.94f},
            {72, 71.82f},
            {73, 72.64f},
            {74, 73.4f},
            {75, 74.1f},
            {76, 74.74f},
            {77, 75.32f},
            {78, 75.84f},
            {79, 76.3f},
            {80, 76.7f},
            {81, 77.04f},
            {82, 77.32f},
            {83, 77.54f},
            {84, 77.7f}
        };

        public MiscInformation() => Order = 1;

        private bool CanRender = false;

        public override bool Initialise() {
            Input.RegisterKey(Keys.F10);
            Input.ReleaseKey += (sender, keys) =>
            {
                if (keys == Keys.F10) Settings.Enable.Value = !Settings.Enable;
            };
            GameController.LeftPanel.WantUse(() => Settings.Enable);
            Graphics.InitImage("preload-start.png");
            Graphics.InitImage("preload-end.png");
            Graphics.InitImage("preload-new.png");
            CalcXp = new TimeCache<bool>(() =>
            {
                partytime += time;
                time = 0;
                CalculateXp();
                var areaCurrentArea = GameController.Area.CurrentArea;
                if (areaCurrentArea == null)
                    return false;
                timeSpan = DateTime.UtcNow - areaCurrentArea.TimeEntered;
                // Time = $"{timeSpan.TotalMinutes:00}:{timeSpan.Seconds:00}";
                Time = AreaInstance.GetTimeString(timeSpan);
                xpReceivingText = $"{xpRate}  *{levelXpPenalty * partyXpPenalty:p0}";
                xpGetLeft =
                    $"Got: {ConvertHelper.ToShorten(getXp, "0.00")} ({percentGot:P3})  Left: {ConvertHelper.ToShorten(xpLeftQ, "0.00")}";
                maxX = MathHepler.Max(Graphics.MeasureText(fps).X, Graphics.MeasureText(ping).X, Graphics.MeasureText(latency).X,
                                      Graphics.MeasureText(areaName).X, Graphics.MeasureText(xpReceivingText).X) * 1.5f;
                if (partytime > 4900)
                {
                    var levelPenaltyValue = LevelPenalty.Value;
                }
                return true;
            }, 1000);
            LevelPenalty = new TimeCache<bool>(() =>
            {
                partyXpPenalty = PartyXpPenalty();
                levelXpPenalty = LevelXpPenalty();
                return true;
            }, 5000);
            GameController.EntityListWrapper.PlayerUpdate += OnEntityListWrapperOnPlayerUpdate;
            OnEntityListWrapperOnPlayerUpdate(this,GameController.Player);

            debugInformation = new DebugInformation("Game FPS","Collect game fps",false);
            return true;
        }
        DebugInformation debugInformation;

        private void OnEntityListWrapperOnPlayerUpdate(object sender, Entity entity) {
            percentGot = 0;
            xpRate = "0.00 xp/h";
            timeLeft = "-h -m -s  to level up";
            getXp = 0;
            xpLeftQ = 0;

            startTime = lastTime = DateTime.UtcNow;
            startXp = entity.GetComponent<Player>().XP;
            levelXpPenalty = LevelXpPenalty();
        }

        private TimeCache<bool> CalcXp;
        private TimeCache<bool> LevelPenalty;
        private Vector2 leftPanelStartDrawPoint = Vector2.Zero;
        private RectangleF leftPanelStartDrawRect = RectangleF.Empty;
        private string fps = "";
        private string latency = "";
        private string areaName = "";
        private string xpReceivingText = "";
        private string xpGetLeft = "";
        private string ping = "";
        private string xpRate = "";
        private string timeLeft = "";
        private DateTime startTime, lastTime;
        private TimeSpan timeSpan;
        private string Time = "";
        private float maxX, maxY, percentGot;
        private long startXp, getXp, xpLeftQ;
        private double levelXpPenalty, partyXpPenalty;
        private Vector2N drawTextVector2;
        private Vector2 leftEndVector2, rightEndVector2;
        private float startY;
        private double time;
        private double partytime = 4000;


        public override void AreaChange(AreaInstance area)
        {
            LevelPenalty.ForceUpdate();
        }

        public override Job Tick() {
      
            TickLogic();
            return null;

        }

        void TickLogic() {
            time += GameController.DeltaTime;
            var gameUi = GameController.Game.IngameState.IngameUi;
            if (GameController.Area.CurrentArea == null || gameUi.InventoryPanel.IsVisible || gameUi.SynthesisWindow.IsVisibleLocal ||
                gameUi.BetrayalWindow.IsVisibleLocal)
            {
                CanRender = false;
                return;
            }

            var UIHover = GameController.Game.IngameState.UIHover;
            var miniMap = GameController.Game.IngameState.IngameUi.Map.SmallMiniMap;
            if (UIHover.Tooltip != null && UIHover.Tooltip.IsVisibleLocal &&
                UIHover.Tooltip.GetClientRectCache.Intersects(leftPanelStartDrawRect))
            {
                CanRender = false;
                return;
            }


            CanRender = true;

            var calcXpValue = CalcXp.Value;
            var ingameStateCurFps = GameController.Game.IngameState.CurFps;
            debugInformation.Tick = ingameStateCurFps;
            fps = $"fps:({ingameStateCurFps})";
            areaName = $"{GameController.Area.CurrentArea.DisplayName}";
            latency = $"({GameController.Game.IngameState.CurLatency})";
            ping = $"ping:({GameController.Game.IngameState.CurLatency})";
        }

        private void CalculateXp() {
            var level = GameController.Player.GetComponent<Player>().Level;
            if (level >= 100)
            {
                // player can't level up, just show fillers
                xpRate = "0.00 xp/h";
                timeLeft = "--h--m--s";
                return;
            }

            long currentXp = GameController.Player.GetComponent<Player>().XP;
            getXp = currentXp - startXp;
            var rate = (currentXp - startXp) / (DateTime.UtcNow - startTime).TotalHours;
            xpRate = $"{ConvertHelper.ToShorten(rate, "0.00")} xp/h";
            if (level >= 0 && level + 1 < Constants.PlayerXpLevels.Length && rate > 1)
            {
                var xpLeft = Constants.PlayerXpLevels[level + 1] - currentXp;
                xpLeftQ = xpLeft;
                var time = TimeSpan.FromHours(xpLeft / rate);
                timeLeft = $"{time.Hours:0}h {time.Minutes:00}m {time.Seconds:00}s to level up";
                if (getXp == 0)
                    percentGot = 0;
                else
                {
                    percentGot = getXp / ((float) Constants.PlayerXpLevels[level + 1] - (float) Constants.PlayerXpLevels[level]);
                    if (percentGot < -100) percentGot = 0;
                }
            }
        }

        private double LevelXpPenalty() {
            var arenaLevel = GameController.Area.CurrentArea.RealLevel;
            var characterLevel = GameController.Player.GetComponent<Player>().Level;

            var effectiveArenaLevel = arenaLevel < 71 ? arenaLevel : ArenaEffectiveLevels[arenaLevel];
            var safeZone = Math.Floor(Convert.ToDouble(characterLevel) / 16) + 3;
            var effectiveDifference = Math.Max(Math.Abs(characterLevel - effectiveArenaLevel) - safeZone, 0);
            double xpMultiplier;

            xpMultiplier = Math.Pow((characterLevel + 5) / (characterLevel + 5 + Math.Pow(effectiveDifference, 2.5)), 1.5);

            if (characterLevel >= 95) //For player levels equal to or higher than 95:
                xpMultiplier *= 1d / (1 + 0.1 * (characterLevel - 94));

            xpMultiplier = Math.Max(xpMultiplier, 0.01);

            return xpMultiplier;
        }

        private double PartyXpPenalty() {
            var entities = GameController.EntityListWrapper.ValidEntitiesByType[EntityType.Player];

            if (entities.Count == 0)
                return 1;
            var levels = entities.SelectF(y => y.GetComponent<Player>().Level).ToList();
            var characterLevel = GameController.Player.GetComponent<Player>().Level;
            var partyXpPenalty = Math.Pow(characterLevel + 10, 2.71) / levels.SumF(level => Math.Pow(level + 10, 2.71));
            return partyXpPenalty * levels.Count;
        }


        private RectangleF bounds = RectangleF.Empty;

        public override void Render() {
            if (!CanRender)
                return;
            
            leftPanelStartDrawPoint = GameController.LeftPanel.StartDrawPoint;
            leftPanelStartDrawRect = new RectangleF(leftPanelStartDrawPoint.X, leftPanelStartDrawPoint.Y, 1, 1);

            leftPanelStartDrawPoint.X -= maxX;
            startY = leftPanelStartDrawPoint.Y;
            //00:00
            drawTextVector2 = Graphics.DrawText(Time, leftPanelStartDrawPoint, Settings.TimerTextColor);
            leftPanelStartDrawPoint.Y += drawTextVector2.Y;
            //fps:(100)
            drawTextVector2 = Graphics.DrawText(fps, leftPanelStartDrawPoint, Settings.FpsTextColor);
            leftPanelStartDrawPoint.Y += drawTextVector2.Y;
            //ping:(100)
            drawTextVector2 = Graphics.DrawText(ping, leftPanelStartDrawPoint, Settings.FpsTextColor);
            leftPanelStartDrawPoint.Y += drawTextVector2.Y;
            leftEndVector2 = leftPanelStartDrawPoint;
            leftPanelStartDrawPoint.X += maxX;
            leftPanelStartDrawPoint.Y = startY;
            //NameArea
            drawTextVector2 = Graphics.DrawText(areaName, leftPanelStartDrawPoint, GameController.Area.CurrentArea.AreaColorName,
                                                FontAlign.Right);
            leftPanelStartDrawPoint.Y += drawTextVector2.Y;
            //-h-m-s to level
            drawTextVector2 = Graphics.DrawText(timeLeft, leftPanelStartDrawPoint, Settings.TimeLeftColor, FontAlign.Right);
            leftPanelStartDrawPoint.Y += drawTextVector2.Y;
            //0,00 xph*%
            drawTextVector2 = Graphics.DrawText(xpReceivingText, leftPanelStartDrawPoint, Settings.XphTextColor, FontAlign.Right);
            leftPanelStartDrawPoint.Y += drawTextVector2.Y;
            //GotLeft
            drawTextVector2 = Graphics.DrawText(xpGetLeft, leftPanelStartDrawPoint, Settings.XphTextColor, FontAlign.Right);
            leftPanelStartDrawPoint.Y += drawTextVector2.Y;
            rightEndVector2 = leftPanelStartDrawPoint;
            var max = Math.Max(rightEndVector2.Y, leftEndVector2.Y + 5);
            bounds = new RectangleF(leftEndVector2.X, startY - 2, rightEndVector2.X - leftEndVector2.X, max);
            // Graphics.DrawImage("preload-start.png", bounds, Settings.BackgroundColor);
            // Graphics.DrawImage("preload-end.png", bounds, Settings.BackgroundColor);
            Graphics.DrawImage("preload-new.png", bounds, Settings.BackgroundColor);
            GameController.LeftPanel.StartDrawPoint = new Vector2(leftPanelStartDrawPoint.X, max + 10);
        }
    }
}