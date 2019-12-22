using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using ExileCore;
using ExileCore.PoEMemory.Components;
using ExileCore.PoEMemory.Elements;
using ExileCore.PoEMemory.Elements.InventoryElements;
using ExileCore.PoEMemory.MemoryObjects;
using ExileCore.Shared;
using ExileCore.Shared.Enums;
using ExileCore.Shared.Helpers;
using ImGuiNET;
using SharpDX;
using Map = ExileCore.PoEMemory.Components.Map;

namespace MapsExchange
{
    public class MapsExchange : BaseSettingsPlugin<MapsExchangeSettings>
    {
        private readonly Dictionary<int, float> ArenaEffectiveLevels = new Dictionary<int, float>
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
            {84, 77.7f},
            {85, 77.85f},
            {86, 77.99f}
        };

        private readonly List<int> InventShapersOrbs = new List<int>();
        private readonly PoeTradeProcessor TradeProcessor = new PoeTradeProcessor();
        private IList<WorldArea> BonusCompletedMaps;
        private Dictionary<string, int> CachedDropLvl = new Dictionary<string, int>();
        private IList<WorldArea> CompletedMaps;
        private long CurrentStashAddr;
        private bool LastVisible;
        private List<MapItem> MapItems = new List<MapItem>();
        private Color[] SelectColors;
        private IList<WorldArea> ShapeUpgradedMaps;

        public override bool Initialise()
        {
            SelectColors = new[]
            {
                Color.Aqua,
                Color.Blue,
                Color.BlueViolet,
                Color.Brown,
                Color.BurlyWood,
                Color.CadetBlue,
                Color.Chartreuse,
                Color.Chocolate,
                Color.Coral,
                Color.CornflowerBlue,
                Color.Cornsilk,
                Color.Crimson,
                Color.Cyan,
                Color.DarkBlue,
                Color.DarkCyan,
                Color.DarkGoldenrod,
                Color.DarkGray,
                Color.DarkGreen,
                Color.DarkKhaki,
                Color.DarkMagenta,
                Color.DarkOliveGreen,
                Color.DarkOrange,
                Color.DarkOrchid,
                Color.DarkRed,
                Color.DarkSalmon,
                Color.DarkSeaGreen,
                Color.DarkSlateBlue,
                Color.DarkSlateGray,
                Color.DarkTurquoise,
                Color.DarkViolet,
                Color.DeepPink,
                Color.DeepSkyBlue,
                Color.DimGray,
                Color.DodgerBlue,
                Color.Firebrick,
                Color.FloralWhite,
                Color.ForestGreen,
                Color.Fuchsia,
                Color.Gainsboro,
                Color.GhostWhite,
                Color.Gold,
                Color.Goldenrod,
                Color.Gray,
                Color.Green,
                Color.GreenYellow,
                Color.Honeydew,
                Color.HotPink,
                Color.IndianRed,
                Color.Indigo,
                Color.Ivory,
                Color.Khaki,
                Color.Lavender,
                Color.LavenderBlush,
                Color.LawnGreen,
                Color.LemonChiffon,
                Color.LightBlue,
                Color.LightCoral,
                Color.LightCyan,
                Color.LightGoldenrodYellow,
                Color.LightGray,
                Color.LightGreen,
                Color.LightPink,
                Color.LightSalmon,
                Color.LightSeaGreen,
                Color.LightSkyBlue,
                Color.LightSlateGray,
                Color.LightSteelBlue,
                Color.LightYellow,
                Color.Lime,
                Color.LimeGreen,
                Color.Linen,
                Color.Magenta,
                Color.Maroon,
                Color.MediumAquamarine,
                Color.MediumBlue,
                Color.MediumOrchid,
                Color.MediumPurple,
                Color.MediumSeaGreen,
                Color.MediumSlateBlue,
                Color.MediumSpringGreen,
                Color.MediumTurquoise,
                Color.MediumVioletRed,
                Color.MidnightBlue,
                Color.MintCream,
                Color.MistyRose,
                Color.Moccasin,
                Color.NavajoWhite,
                Color.Navy,
                Color.OldLace,
                Color.Olive,
                Color.OliveDrab,
                Color.Orange,
                Color.OrangeRed,
                Color.Orchid,
                Color.PaleGoldenrod,
                Color.PaleGreen,
                Color.PaleTurquoise,
                Color.PaleVioletRed,
                Color.PapayaWhip,
                Color.PeachPuff,
                Color.Peru,
                Color.Pink,
                Color.Plum,
                Color.PowderBlue,
                Color.Purple,
                Color.Red,
                Color.RosyBrown,
                Color.RoyalBlue,
                Color.SaddleBrown,
                Color.Salmon,
                Color.SandyBrown,
                Color.SeaGreen,
                Color.SeaShell,
                Color.Sienna,
                Color.Silver,
                Color.SkyBlue,
                Color.SlateBlue,
                Color.SlateGray,
                Color.Snow,
                Color.SpringGreen,
                Color.SteelBlue,
                Color.Tan,
                Color.Teal,
                Color.Thistle,
                Color.Tomato,
                Color.Transparent,
                Color.Turquoise,
                Color.Violet,
                Color.Wheat,
                Color.White,
                Color.WhiteSmoke,
                Color.Yellow,
                Color.YellowGreen
            };

            var initImage = Graphics.InitImage(Path.Combine(DirectoryFullName, "images", "ImagesAtlas.png"), false);

            if (!initImage)
                return false;

            Input.RegisterKey(Keys.LControlKey);
            return true;
        }

        public override void Render()
        {
            DrawPlayerInvMaps();
            DrawNpcInvMaps();
            DrawAtlasMaps();

            var stash = GameController.IngameState.IngameUi.StashElement;

            if (stash.IsVisible)
            {
                var visibleStash = stash.VisibleStash;

                if (visibleStash != null)
                {
                    var items = visibleStash.VisibleInventoryItems;

                    if (items != null)
                    {
                        HiglightExchangeMaps();
                        HiglightAllMaps(items);

                        if (CurrentStashAddr != visibleStash.Address)
                        {
                            CurrentStashAddr = visibleStash.Address;
                            var updateMapsCount = Settings.MapTabNode.Value == stash.IndexVisibleStash;
                            UpdateData(items, updateMapsCount);
                        }
                    }
                    else
                        CurrentStashAddr = -1;
                }
            }
            else
                CurrentStashAddr = -1;
        }

        private void DrawAtlasMaps()
        {
            if (!Settings.ShowOnAtlas.Value) return;

            var atlas = GameController.Game.IngameState.IngameUi.AtlasPanel;

            if (LastVisible != atlas.IsVisible || CompletedMaps == null)
            {
                LastVisible = atlas.IsVisible;

                if (LastVisible)
                {
                    CompletedMaps = GameController.Game.IngameState.ServerData.CompletedAreas;
                    BonusCompletedMaps = GameController.Game.IngameState.ServerData.BonusCompletedAreas;
                    ShapeUpgradedMaps = GameController.Game.IngameState.ServerData.ShapedMaps;
                    ScanPlayerInventForShapersOrb();
                }
            }

            if (!atlas.IsVisible) return;

            var root = atlas.GetChildAtIndex(0);
            var rootPos = new Vector2(root.X, root.Y);
            var scale = root.Scale;

            foreach (var atlasMap in GameController.Files.AtlasNodes.EntriesList)
            {
                var area = atlasMap.Area;
                var mapName = area.Name;
                if (mapName.Contains("Realm")) continue;

                var centerPos = (atlasMap.Pos * 5.69f + rootPos) * scale;
                var textRect = centerPos;
                textRect.Y -= 30 * scale;
                var testSize = (int) Math.Round(Settings.TextSize.Value * scale);
                var fontFlags = FontAlign.Center;

                byte textTransp;
                Color textBgColor;
                bool fill;
                Color fillColor;

                if (BonusCompletedMaps.Contains(area))
                {
                    textTransp = Settings.BonusCompletedTextTransparency.Value;
                    textBgColor = Settings.BonusCompletedTextBg.Value;
                    fill = Settings.BonusCompletedFilledCircle.Value;
                    fillColor = Settings.BonusCompletedFillColor.Value;
                }
                else if (CompletedMaps.Contains(area))
                {
                    textTransp = Settings.CompletedTextTransparency.Value;
                    textBgColor = Settings.CompletedTextBg.Value;
                    fill = Settings.CompletedFilledCircle.Value;
                    fillColor = Settings.CompletedFillColor.Value;
                }
                else
                {
                    textTransp = Settings.UnCompletedTextTransparency.Value;
                    textBgColor = Settings.UnCompletedTextBg.Value;
                    fill = Settings.UnCompletedFilledCircle.Value;
                    fillColor = Settings.UnCompletedFillColor.Value;
                }

                var textColor = Settings.WhiteMapColor.Value;

                if (area.AreaLevel >= 78)
                    textColor = Settings.RedMapColor.Value;
                else if (area.AreaLevel >= 73)
                    textColor = Settings.YellowMapColor.Value;

                textColor.A = textTransp;

                Graphics.DrawText(mapName, textRect.Translate(0, -15), textColor, testSize, fontFlags);

                var mapNameSize = Graphics.MeasureText(mapName, testSize);
                mapNameSize.X += 5;

                var nameBoxRect = new RectangleF(textRect.X - mapNameSize.X / 2, textRect.Y - mapNameSize.Y, mapNameSize.X,
                    mapNameSize.Y);

                Graphics.DrawBox(nameBoxRect, textBgColor);

                if (Input.IsKeyDown(Keys.LControlKey))
                {
                    var upgraded = ShapeUpgradedMaps.Contains(area);
                    var areaLvlColor = Color.White;
                    var areaLvl = area.AreaLevel;

                    if (upgraded)
                    {
                        areaLvl += 5;
                        areaLvlColor = Color.Orange;
                    }

                    var penalty = LevelXpPenalty(areaLvl);
                    var penaltyTextColor = Color.Lerp(Color.Red, Color.Green, (float) penalty);
                    var labelText = $"{penalty:p0}";
                    var textSize = Graphics.MeasureText(labelText, testSize);
                    textSize.X += 6;
                    var penaltyRect = new RectangleF(textRect.X + mapNameSize.X / 2, textRect.Y - textSize.Y, textSize.X, textSize.Y);
                    Graphics.DrawBox(penaltyRect, Color.Black);
                    Graphics.DrawText(labelText, penaltyRect.Center.Translate(0, -8), penaltyTextColor, testSize, FontAlign.Center);

                    labelText = $"{areaLvl}";
                    textSize = Graphics.MeasureText(labelText, testSize);

                    penaltyRect = new RectangleF(textRect.X - mapNameSize.X / 2 - textSize.X, textRect.Y - textSize.Y, textSize.X,
                        textSize.Y);

                    Graphics.DrawBox(penaltyRect, Color.Black);
                    Graphics.DrawText(labelText, penaltyRect.Center.Translate(3, -8), areaLvlColor, testSize, FontAlign.Center);

                    if (Settings.ShowBuyButton.Value)
                    {
                        var butTextWidth = 50 * scale;
                        var buyButtonRect = new RectangleF(textRect.X - butTextWidth / 2, textRect.Y - testSize * 2, butTextWidth, testSize);

                        Graphics.DrawImage("ImagesAtlas.png", buyButtonRect, new RectangleF(.367f, .731f, .184f, .223f),
                            new Color(255, 255, 255, 255));

                        //buyButtonRect
                        //ImGui.Button()
                        //TradeProcessor.OpenBuyMap(BuyAtlasNode.Area.Name, IsUniq(BuyAtlasNode), GameController.Game.IngameState.ServerData.League);
                    }
                }

                var imgRectSize = 60 * scale;
                var imgDrawRect = new RectangleF(centerPos.X - imgRectSize / 2, centerPos.Y - imgRectSize / 2, imgRectSize, imgRectSize);

                if (InventShapersOrbs.Count > 0)
                {
                    var areaLvl = area.AreaLevel;

                    if (!ShapeUpgradedMaps.Contains(area))
                    {
                        var tier = areaLvl - 67;

                        if (InventShapersOrbs.Contains(tier))
                        {
                            var shapedRect = imgDrawRect;
                            var sizeOffset = 30 * scale;
                            shapedRect.Left -= sizeOffset;
                            shapedRect.Right += sizeOffset;
                            shapedRect.Top -= sizeOffset;
                            shapedRect.Bottom += sizeOffset;

                            Graphics.DrawImage("ImagesAtlas.png", shapedRect, new RectangleF(0, 0, .5f, .731f), new Color(155, 0, 255, 255));
                        }
                    }
                }

                if (fill)
                    Graphics.DrawImage("ImagesAtlas.png", imgDrawRect, new RectangleF(.5f, 0, .5f, .731f), fillColor);

                Graphics.DrawImage("ImagesAtlas.png", imgDrawRect, new RectangleF(0, 0, .5f, .731f), Color.Black);

                if (Settings.ShowAmount.Value)
                {
                    if (IsUniq(atlasMap))
                        mapName += ":Uniq";

                    if (Settings.MapStashAmount.TryGetValue(mapName, out var amount))
                    {
                        var mapCountSize = Graphics.MeasureText(amount.ToString(), testSize);
                        mapCountSize.X += 6;

                        Graphics.DrawBox(
                            new RectangleF(centerPos.X - mapCountSize.X / 2, centerPos.Y - mapCountSize.Y / 2, mapCountSize.X,
                                mapCountSize.Y), Color.Black);

                        textColor.A = 255;
                        Graphics.DrawText(amount.ToString(), centerPos, textColor, testSize, FontAlign.Center);
                    }
                }
            }
        }

        private bool IsUniq(AtlasNode atlasMap)
        {
            var uniqTest = GameController.Memory.ReadStringU(GameController.Memory.Read<long>(atlasMap.Address + 0x3c, 0));

            return !string.IsNullOrEmpty(uniqTest) && uniqTest.Contains("Uniq") ||
                   Vector2.Distance(atlasMap.Pos, new Vector2(294.979f, 386.641f)) < 5;
        }

        private void ScanPlayerInventForShapersOrb()
        {
            InventShapersOrbs.Clear();
            var playerInvent = GameController.Game.IngameState.ServerData.GetPlayerInventoryByType(InventoryTypeE.MainInventory);

            foreach (var item in playerInvent.Items)
            {
                var path = item.Path;
                if (string.IsNullOrEmpty(path) || !path.Contains("/MapUpgrades/")) continue;
                if (!item.HasComponent<Base>()) continue;

                var baseName = item.GetComponent<Base>().Name;
                if (string.IsNullOrEmpty(baseName) || !baseName.Contains("Shaper's Orb")) continue;

                var tierStr = baseName.Replace("Shaper's Orb (Tier ", string.Empty).Replace(")", string.Empty);
                int tierValue;

                if (int.TryParse(tierStr, out tierValue))
                    InventShapersOrbs.Add(tierValue);
            }
        }

        private void DrawPlayerInvMaps()
        {
            var ingameState = GameController.Game.IngameState;

            if (ingameState.IngameUi.InventoryPanel.IsVisible)
            {
                var inventoryZone = ingameState.IngameUi.InventoryPanel[InventoryIndex.PlayerInventory].VisibleInventoryItems;
                HiglightAllMaps(inventoryZone);
            }
        }

        private void DrawNpcInvMaps()
        {
            var ingameState = GameController.Game.IngameState;

            var serverData = ingameState.ServerData;
            var npcInv = serverData.NPCInventories;

            if (npcInv == null || npcInv.Count == 0) return;

            var bonusComp = serverData.BonusCompletedAreas;
            var comp = serverData.CompletedAreas;
            var shapered = serverData.ShaperElderAreas;

            var drawListPos = new Vector2(200, 200);

            foreach (var inv in npcInv)
            {
                foreach (var item in inv.Inventory.Items)
                {
                    var mapComponent = item.GetComponent<Map>();

                    if (mapComponent == null)
                        continue;

                    var mapArea = mapComponent.Area;
                    var shaper = shapered.Contains(mapArea);

                    if (bonusComp.Contains(mapArea) && !shaper) continue;

                    var color = Color.Yellow;

                    if (!comp.Contains(mapArea))
                        color = Color.Red;

                    Graphics.DrawText(mapArea.Name + (shaper ? " (shaper/elder)" : ""), drawListPos, color, 20);
                    drawListPos.Y += 20;
                }
            }

            /*
          if (ingameState.IngameUi.InventoryPanel.IsVisible)
          {
              List<NormalInventoryItem> playerInvItems = new List<NormalInventoryItem>();
              var inventoryZone = ingameState.IngameUi.InventoryPanel[PoeHUD.Models.Enums.InventoryIndex.PlayerInventory].VisibleInventoryItems;//.InventoryUiElement;

              foreach (Element element in inventoryZone.Children)
              {
                  var inventElement = element.AsObject<NormalInventoryItem>();

                  if (inventElement.InventPosX < 0 || inventElement.InventPosY < 0)
                  {
                      continue;
                  }
                  playerInvItems.Add(inventElement);
              }
            
            HiglightAllMaps(playerInvItems);
            }
              */
        }

        private void UpdateData(IList<NormalInventoryItem> items, bool checkAmount)
        {
            MapItems = new List<MapItem>();

            if (checkAmount)
                Settings.MapStashAmount.Clear();

            foreach (var invItem in items)
            {
                var item = invItem.Item;
                if (item == null) continue;

                var bit = GameController.Files.BaseItemTypes.Translate(item.Path);
                if (bit == null) continue;

                if (bit.ClassName != "Map") continue;

                float width = Settings.BordersWidth;
                float spacing = Settings.Spacing;

                var drawRect = invItem.GetClientRect();
                drawRect.X += width / 2 + spacing;
                drawRect.Y += width / 2 + spacing;
                drawRect.Width -= width + spacing * 2;
                drawRect.Height -= width + spacing * 2;

                var baseName = bit.BaseName;

                var mapItem = new MapItem(baseName, drawRect);
                var mapComponent = item.GetComponent<Map>();

                if (checkAmount)
                {
                    var areaName = mapComponent.Area.Name;
                    var mods = item.GetComponent<Mods>();

                    if (mods.ItemRarity == ItemRarity.Unique)
                    {
                        areaName = mods.UniqueName.Replace(" Map", string.Empty);
                        areaName += ":Uniq";
                    }

                    if (!Settings.MapStashAmount.ContainsKey(areaName))
                        Settings.MapStashAmount.Add(areaName, 0);

                    Settings.MapStashAmount[areaName]++;
                }

                mapItem.Penalty = LevelXpPenalty(mapComponent.Area.AreaLevel);
                MapItems.Add(mapItem);
            }

            var sortedMaps = (from demoClass in MapItems

                    //where demoClass.Tier >= Settings.MinTier && demoClass.Tier <= Settings.MaxTier
                    //TODO: Check tiers (or nobody need it?)
                    group demoClass by demoClass.Name
                    into groupedDemoClass
                    select groupedDemoClass
                ).ToDictionary(gdc => gdc.Key, gdc => gdc.ToList());

            var colorCounter = 0;

            foreach (var group in sortedMaps)
            {
                var count = group.Value.Count;
                var take = count / 3;
                take *= 3;

                var grabMaps = group.Value.Take(take);

                foreach (var dropMap in grabMaps)
                {
                    dropMap.DrawColor = SelectColors[colorCounter];
                }

                colorCounter++;
            }
        }

        private void HiglightExchangeMaps()
        {
            foreach (var drapMap in MapItems)
            {
                Graphics.DrawFrame(drapMap.DrawRect, drapMap.DrawColor, Settings.BordersWidth.Value);
            }
        }

        private void HiglightAllMaps(IList<NormalInventoryItem> items)
        {
            var ingameState = GameController.Game.IngameState;
            var serverData = ingameState.ServerData;
            var bonusComp = serverData.BonusCompletedAreas;
            var comp = serverData.CompletedAreas;
            var shEld = serverData.ShaperElderAreas;

            var disableOnHover = false;
            var disableOnHoverRect = new RectangleF();

            var inventoryItemIcon = ingameState.UIHover.AsObject<HoverItemIcon>();

            var tooltip = inventoryItemIcon?.Tooltip;

            if (tooltip != null)
            {
                disableOnHover = true;
                disableOnHoverRect = tooltip.GetClientRect();
            }

            foreach (var item in items)
            {
                var entity = item?.Item;
                if (entity == null) continue;

                var bit = GameController.Files.BaseItemTypes.Translate(entity.Path);

                if (bit == null) continue;
                if (bit.ClassName != "Map") continue;
                var mapComponent = entity.GetComponent<Map>();

                var drawRect = item.GetClientRect();

                if (disableOnHover && disableOnHoverRect.Intersects(drawRect))
                    continue;

                var offset = 3;
                drawRect.Top += offset;
                drawRect.Bottom -= offset;
                drawRect.Right -= offset;
                drawRect.Left += offset;

                var completed = 0;

                var area = mapComponent.Area;

                if (comp.Contains(area))
                    completed++;

                if (bonusComp.Contains(area))
                    completed++;

                var shaperElder = shEld.Contains(area);

                if (completed == 0)
                {
                    Graphics.DrawImage("ImagesAtlas.png", drawRect, new RectangleF(.184f, .731f, .184f, .269f), Color.Red);
                }
                else if (completed == 1)
                {
                    Graphics.DrawImage("ImagesAtlas.png", drawRect, new RectangleF(.184f, .731f, .184f, .269f), Color.Yellow);
                }

                if (shaperElder)
                {
                    var bgRect = drawRect;
                    bgRect.Left = bgRect.Right - 25;
                    bgRect.Bottom = bgRect.Top + 17;

                    Graphics.DrawBox(bgRect, Color.Black);
                    Graphics.DrawText("S/E", drawRect.TopRight, Color.Yellow, 15, FontAlign.Right);
                }

                if (Settings.ShowPenalty.Value)
                {
                    var mods = entity.GetComponent<Mods>();
                    var areaLvl = mapComponent.Tier + 67; // mapComponent.Area.AreaLevel;

                    if (mods.ItemMods.Any(x => x.Name == "MapShaped")) //Shaper orb upgraded
                        areaLvl += 5;

                    var penalty = LevelXpPenalty(areaLvl);
                    var textColor = Color.Lerp(Color.Red, Color.Green, (float) penalty);

                    //textColor.A = (byte)(255f * (1f - (float)penalty) * 20f);

                    var labelText = $" {penalty:p0} ";
                    var textSize = Graphics.MeasureText(labelText, 20);

                    Graphics.DrawBox(
                        new RectangleF(
                            drawRect.X + drawRect.Width / 2 - textSize.X / 2,
                            drawRect.Y + drawRect.Height - textSize.Y - 3,
                            textSize.X,
                            textSize.Y),
                        Color.Black);

                    Graphics.DrawText(labelText, new Vector2(drawRect.Center.X, drawRect.Bottom - 17), textColor, 20, FontAlign.Center);
                }
            }
        }

        private double LevelXpPenalty(int arenaLevel)
        {
            var characterLevel = GameController.Player.GetComponent<Player>().Level;

            float effectiveArenaLevel;

            if (arenaLevel < 71)
                effectiveArenaLevel = arenaLevel;
            else
            {
                if (!ArenaEffectiveLevels.TryGetValue(arenaLevel, out var scale))
                {
                    LogError($"Can't calc ArenaEffectiveLevels from arenaLevel: {arenaLevel}", 2);
                    return 0;
                }

                effectiveArenaLevel = scale;
            }

            var safeZone = Math.Floor(Convert.ToDouble(characterLevel) / 16) + 3;
            var effectiveDifference = Math.Max(Math.Abs(characterLevel - effectiveArenaLevel) - safeZone, 0);
            double xpMultiplier;

            xpMultiplier = Math.Pow((characterLevel + 5) / (characterLevel + 5 + Math.Pow(effectiveDifference, 2.5)), 1.5);

            if (characterLevel >= 95) //For player levels equal to or higher than 95:
                xpMultiplier *= 1d / (1 + 0.1 * (characterLevel - 94));

            xpMultiplier = Math.Max(xpMultiplier, 0.01);
            return xpMultiplier;
        }

        public class MapItem
        {
            public Color DrawColor = Color.Transparent;
            public RectangleF DrawRect;
            public string Name;
            public double Penalty;

            public MapItem(string Name, RectangleF DrawRect)
            {
                this.Name = Name;
                this.DrawRect = DrawRect;
            }
        }
    }
}
