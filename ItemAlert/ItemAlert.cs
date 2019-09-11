using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Antlr4.Runtime;
using ExileCore;
using ExileCore.PoEMemory.Components;
using ExileCore.PoEMemory.Elements;
using ExileCore.PoEMemory.MemoryObjects;
using ExileCore.Shared;
using ExileCore.Shared.Abstract;
using ExileCore.Shared.Enums;
using ExileCore.Shared.Helpers;
using PoeFilterParser;
using PoeFilterParser.Model;
using SharpDX;

namespace ItemAlert
{
    public class ItemAlert : BaseSettingsPlugin<ItemAlertSettings>
    {
        private const int BOTTOM_MARGIN = 2;
        private readonly object locker = new object();
        private readonly Queue<Entity> queueOfWorldItems = new Queue<Entity>();
        private readonly ConcurrentQueue<Entity> queueUpdatedItems = new ConcurrentQueue<Entity>();
        private Dictionary<long, LabelOnGround> currentLabels = new Dictionary<long, LabelOnGround>();
        private bool shouldUpdate = true;
        private Task task;
        private bool TownOrHideout;
        private PoeFilterVisitor visitor;

        public ItemAlert()
        {
            Order = 5;
        }

        public override void OnLoad()
        {
            task = Task.Run(() => PoeFilterInit(Settings.FilePath));
        }

        public override bool Initialise()
        {
            GameController.UnderPanel.WantUse(() => Settings.Enable);

            Settings.FilePath.OnFileChanged += (sender, s) =>
            {
                PoeFilterInit(s);
                var coroutine = new Coroutine(RestartParseItemsAfterFilterChange(), this, "Check items after filter change");
                Core.MainRunner.Run(coroutine);
            };

            Graphics.InitImage("currency.png");
            Graphics.InitImage("item_icons.png");
            Graphics.InitImage("directions.png");
            task.Wait();
            return true;
        }

        private IEnumerator RestartParseItemsAfterFilterChange()
        {
            foreach (var entity in GameController.EntityListWrapper.ValidEntitiesByType[EntityType.WorldItem])
            {
                EntityAdded(entity);
            }

            yield return false;
        }

        public override Job Tick()
        {
            if (Settings.MultiThreading && queueUpdatedItems.Count >= Settings.MultiThreadingWhenItemMoreThan)
                return GameController.MultiThreadManager.AddJob(TickLogic, nameof(ItemAlert));

            TickLogic();
            return null;
        }

        private void TickLogic()
        {
            while (queueUpdatedItems.TryDequeue(out var entity))
            {
                queueOfWorldItems.Enqueue(entity);
            }

            while (queueOfWorldItems.Count > 0)
            {
                var entity = queueOfWorldItems.Dequeue();

                var item = entity.GetComponent<WorldItem>()?.ItemEntity;

                if (!string.IsNullOrEmpty(Settings.FilePath) && item?.Path != null)
                {
                    if (item.Path != null && !item.Path.StartsWith("Meta")) continue;

                    var result = visitor.Visit(item);

                    if (result != null)
                    {
                        entity.SetHudComponent(result);
                        PrepareForDrawingAndPlaySound(entity, result);
                    }
                }
            }

            if (shouldUpdate)
            {
                currentLabels = GameController.Game.IngameState.IngameUi.ItemsOnGroundLabels?.Where(x => x.ItemOnGround != null)
                    .GroupBy(y => y.ItemOnGround.Address)
                    .ToDictionary(y => y.Key, y => y.First());

                shouldUpdate = false;
            }
        }

        public override void Render()
        {
            var playerPos = GameController.Player.GridPos;
            var position = GameController.UnderPanel.StartDrawPoint;

            foreach (var entity in GameController.EntityListWrapper.ValidEntitiesByType[EntityType.WorldItem])
            {
                var alertDrawStyle = entity.GetHudComponent<AlertDrawStyle>();
                if (alertDrawStyle?.Text == null) continue;

                if (!currentLabels.TryGetValue(entity.Address, out var entityLabel))
                {
                    shouldUpdate = true;
                    continue;
                }

                if (!Settings.ShowText) continue;

                if (Settings.HideOthers)
                {
                    if (entityLabel.CanPickUp || entityLabel.MaxTimeForPickUp.TotalSeconds == 0)
                        position = DrawText(playerPos, position, BOTTOM_MARGIN, alertDrawStyle, entity);
                }
                else
                {
                    if (entityLabel.CanPickUp || entityLabel.MaxTimeForPickUp.TotalSeconds == 0)
                        position = DrawText(playerPos, position, BOTTOM_MARGIN, alertDrawStyle, entity);
                    else
                    {
                        // get current values
                        var TextColor = alertDrawStyle.TextColor;
                        var BorderColor = alertDrawStyle.BorderColor;
                        var BackgroundColor = alertDrawStyle.BackgroundColor;

                        if (Settings.DimOtherByPercentToggle)
                        {
                            // edit values to new ones
                            var ReduceByPercent = (double) Settings.DimOtherByPercent / 100;
                            TextColor = ReduceNumbers(TextColor, ReduceByPercent);
                            BorderColor = ReduceNumbers(BorderColor, ReduceByPercent);
                            BackgroundColor = ReduceNumbers(BackgroundColor, ReduceByPercent);

                            // backgrounds with low alpha start to look a little strange when dark so im adding an alpha threshold
                            if (BackgroundColor.A < 210) BackgroundColor.A = 210;
                        }

                        // Complete new KeyValuePair with new stuff
                        var ModifiedDrawStyle = new AlertDrawStyle(alertDrawStyle.Text, TextColor, alertDrawStyle.BorderWidth, BorderColor,
                            BackgroundColor, alertDrawStyle.IconIndex);

                        position = DrawText(playerPos, position, BOTTOM_MARGIN, ModifiedDrawStyle, entity);
                    }
                }
            }
        }

        private Color ReduceNumbers(Color oldColor, double percent)
        {
            var newColor = oldColor;
            newColor.R = (byte) (oldColor.R - oldColor.R * percent);
            newColor.G = (byte) (oldColor.G - oldColor.G * percent);
            newColor.B = (byte) (oldColor.B - oldColor.B * percent);
            newColor.A = (byte) (oldColor.A - (double) oldColor.A / 10 * percent);
            return newColor;
        }

        private Vector2 DrawText(Vector2 playerPos, Vector2 position, int BOTTOM_MARGIN, AlertDrawStyle kv, Entity entity)
        {
            var padding = new Vector2(5, 2);
            var positioned = entity.GetComponent<Positioned>();
            if (positioned == null) return position;
            var delta = positioned.GridPos - playerPos;
            var itemSize = DrawItem(kv, delta, position, padding, kv.Text);
            if (itemSize != new Vector2()) position.Y += itemSize.Y + BOTTOM_MARGIN;

            return position;
        }

        private Vector2 DrawItem(AlertDrawStyle drawStyle, Vector2 delta, Vector2 position, Vector2 padding, string text)
        {
            padding.X -= drawStyle.BorderWidth;
            padding.Y -= drawStyle.BorderWidth;
            double phi;
            var distance = delta.GetPolarCoordinates(out phi);
            float compassOffset = Settings.TextSize + 8;
            var textPos = position.Translate(-padding.X - compassOffset, padding.Y);
            var textSize = Graphics.DrawText(text, textPos, drawStyle.TextColor, FontAlign.Right);
            var iconSize = drawStyle.IconIndex >= 0 ? textSize.Y : 0;
            var fullHeight = textSize.Y + 2 * padding.Y + 2 * drawStyle.BorderWidth;
            var fullWidth = textSize.X + 2 * padding.X + iconSize + 2 * drawStyle.BorderWidth + compassOffset;
            var boxRect = new RectangleF(position.X - fullWidth, position.Y, fullWidth - compassOffset, fullHeight);
            Graphics.DrawBox(boxRect, drawStyle.BackgroundColor);
            var rectUV = MathHepler.GetDirectionsUV(phi, distance);
            var rectangleF = new RectangleF(position.X - padding.X - compassOffset + 6, position.Y + padding.Y, textSize.Y, textSize.Y);
            Graphics.DrawImage("directions.png", rectangleF, rectUV);

            if (iconSize > 0)
            {
                const float ICONS_IN_SPRITE = 4;
                var iconX = drawStyle.IconIndex / ICONS_IN_SPRITE;
                var topLeft = new System.Numerics.Vector2(textPos.X - iconSize - textSize.X, textPos.Y);
                var topLeftUv = new System.Numerics.Vector2(iconX, 0);

                Graphics.DrawImageGui("item_icons.png", topLeft, topLeft.Translate(iconSize, iconSize), topLeftUv,
                    topLeftUv.Translate((drawStyle.IconIndex + 1) / ICONS_IN_SPRITE - iconX, 1));
            }

            if (drawStyle.BorderWidth > 0) Graphics.DrawFrame(boxRect, drawStyle.BorderColor, drawStyle.BorderWidth);

            return new Vector2(fullWidth, fullHeight);
        }

        public override void EntityAdded(Entity Entity)
        {
            if (Settings.Enable && !TownOrHideout && Entity.Type == EntityType.WorldItem)
            {
                Entity.OnUpdate += (sender, entity) =>
                {
                    var item = entity.GetComponent<WorldItem>()?.ItemEntity;

                    if (!string.IsNullOrEmpty(Settings.FilePath) && item != null)
                    {
                        if (!item.Path.StartsWith("Meta")) return;

                        queueUpdatedItems.Enqueue(entity);
                    }
                };

                queueOfWorldItems.Enqueue(Entity);
            }
        }

        public override void AreaChange(AreaInstance area)
        {
            lock (locker)
            {
                TownOrHideout = area.IsHideout || area.IsTown;
                queueOfWorldItems.Clear();

                while (queueUpdatedItems.TryDequeue(out _))
                {
                }

                currentLabels.Clear();
            }
        }

        private void PrepareForDrawingAndPlaySound(Entity entity, AlertDrawStyle drawStyle)
        {
            ItemIcon icon;

            if (Settings.UseDiamond)
            {
                icon = new ItemIcon(
                    entity,
                    new HudTexture("Icons.png")
                    {
                        Color = Settings.LootIconBorderColor ? drawStyle.BackgroundColor : drawStyle.TextColor,
                        UV = SpriteHelper.GetUV(MapIconsIndex.LootFilterLargeCyanStar)
                    }, () => Settings.ShowItemOnMap, GameController, Settings);
            }
            else
            {
                icon = new ItemIcon(
                    entity,
                    new HudTexture("currency.png") {Color = Settings.LootIconBorderColor ? drawStyle.BackgroundColor : drawStyle.TextColor},
                    () => Settings.ShowItemOnMap, GameController, Settings);
            }

            entity.SetHudComponent<BaseIcon>(icon);

            if (Settings.WithSound)
                GameController.SoundController.PlaySound("alert");
        }

        private void PoeFilterInit(string path)
        {
            using (new PerformanceTimer("@@PoeFilterInit"))
            {
                try
                {
                    if (!string.IsNullOrEmpty(path))
                    {
                        using (var fileStream = new StreamReader(path))
                        {
                            var input = new AntlrInputStream(fileStream.ReadToEnd());
                            var lexer = new PoeFilterLexer(input);
                            var tokens = new CommonTokenStream(lexer);
                            var parser = new PoeFilterParser.Model.PoeFilterParser(tokens);
                            parser.RemoveErrorListeners();
                            parser.AddErrorListener(new ErrorListener());

                            using (new PerformanceTimer("@@@Tree Visitor Main"))
                            {
                                var tree = parser.main();
                                visitor = new PoeFilterVisitor(tree, GameController, Settings);
                            }
                        }
                    }
                    else
                        Settings.Alternative.Value = false;
                }
                catch (SyntaxErrorException ex)
                {
                    Settings.FilePath.Value = string.Empty;
                    Settings.Alternative.Value = false;

                    MessageBox.Show($"Line: {ex.Line}:{ex.CharPositionInLine}, " + $"{ex.Message}", "Error", MessageBoxButtons.OK,
                        MessageBoxIcon.Error);

                    visitor = null;
                }
                catch (Exception ex)
                {
                    Settings.FilePath.Value = string.Empty;
                    Settings.Alternative.Value = false;
                    MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    }
}
