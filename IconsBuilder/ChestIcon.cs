using System;
using System.Linq;
using ExileCore;
using ExileCore.PoEMemory.Components;
using ExileCore.PoEMemory.MemoryObjects;
using ExileCore.Shared;
using ExileCore.Shared.Abstract;
using ExileCore.Shared.Enums;
using ExileCore.Shared.Helpers;
using ExileCore.Shared.Static;
using SharpDX;

namespace IconsBuilder
{
    public class ChestIcon : BaseIcon
    {
        public ChestIcon(Entity entity, GameController gameController, IconsBuilderSettings settings) : base(entity, settings)
        {
            Update(entity, settings);
        }

        public ChestType CType { get; private set; }

        public void Update(Entity entity, IconsBuilderSettings settings)
        {
            if (Entity.Path.Contains("BreachChest"))
                CType = ChestType.Breach;
            else if (Entity.Path.Contains("Metadata/Chests/AbyssChest") ||
                     Entity.Path.Contains("Metadata/MiscellaneousObjects/Abyss/AbyssFinal") ||
                     Entity.Path.Contains("Metadata/Chests/Abyss/AbyssFinal"))
                CType = ChestType.Abyss;
            else if (Entity.Path.Contains("Metadata/Chests/Incursion"))
                CType = ChestType.Incursion;
            else if (Entity.Path.Contains("Fossil"))
                CType = ChestType.Fossil;
            else if (Entity.Path.Contains("Metadata/Chests/Delve"))
                CType = ChestType.Delve;
            else if (Entity.Path.Contains("Perandus"))
                CType = ChestType.Perandus;
            else if (Entity.Path.Contains("Metadata/Chests/StrongBoxes"))
                CType = ChestType.Strongbox;
            else if (Entity.Path.Contains("Metadata/Chests/Labyrinth/Labyrinth"))
                CType = ChestType.Labyrinth;
            else if (Entity.Path.Contains("Metadata/Chests/SynthesisChests/Synthesis"))
                CType = ChestType.Synthesis;
            else if (Entity.League == LeagueType.Legion)
                CType = ChestType.Legion;
            else
                CType = ChestType.SmallChest;

            Show = () => !Entity.IsOpened;

            if (!_HasIngameIcon)
                MainTexture = new HudTexture {FileName = "sprites.png"};
            else
            {
                MainTexture.Size = settings.SizeChestIcon;
                Text = Entity.GetComponent<Render>()?.Name;
                return;
            }

            switch (Rarity)
            {
                case MonsterRarity.White:
                    MainTexture.Color = Color.White;
                    break;
                case MonsterRarity.Magic:
                    MainTexture.Color = HudSkin.MagicColor;
                    break;

                case MonsterRarity.Rare:
                    MainTexture.Color = HudSkin.RareColor;
                    break;

                case MonsterRarity.Unique:
                    MainTexture.Color = HudSkin.UniqueColor;
                    break;

                default:
                    MainTexture.Color = Color.Purple;
                    break;
            }

            switch (CType)
            {
                case ChestType.Breach:
                    MainTexture.Size = settings.SizeBreachChestIcon;

                    if (Entity.Path.Contains("Large"))
                    {
                        MainTexture.UV = SpriteHelper.GetUV(strongboxesUV["Metadata/Chests/StrongBoxes/StrongboxDivination"],
                            new Size2F(7, 8));

                        MainTexture.Color = new ColorBGRA(240, 100, 255, 255);
                        Text = "Big Breach";
                    }
                    else
                    {
                        MainTexture.UV = SpriteHelper.GetUV(strongboxesUV["Metadata/Chests/StrongBoxes/Large"], new Size2F(7, 8));
                        MainTexture.Color = new ColorBGRA(240, 100, 255, 255);
                        Text = "Breach chest";
                    }

                    break;
                case ChestType.Abyss:
                    MainTexture.Size = settings.SizeChestIcon;

                    if (Entity.Path.Contains("AbyssFinalChest"))
                    {
                        MainTexture.UV = SpriteHelper.GetUV(strongboxesUV["Metadata/Chests/AbyssChest"], new Size2F(7, 8));
                        MainTexture.Color = Color.GreenYellow;
                        Text = Entity.GetComponent<Render>()?.Name;
                    }
                    else if (Entity.Path.Contains("AbyssChest"))
                    {
                        MainTexture.UV = SpriteHelper.GetUV(strongboxesUV["Metadata/Chests/StrongBoxes/StrongboxDivination"],
                            new Size2F(7, 8));

                        MainTexture.Color = Color.GreenYellow;
                    }

                    break;
                case ChestType.Incursion:
                    MainTexture.Size = settings.SizeChestIcon;
                    MainTexture.UV = SpriteHelper.GetUV(strongboxesUV["Metadata/Chests/StrongBoxes/StrongboxDivination"], new Size2F(7, 8));
                    MainTexture.Color = Color.OrangeRed;
                    Text = Entity.Path.Replace("Metadata/Chests/IncursionChest", "").Replace("s/IncursionChest", "");
                    break;
                case ChestType.Delve:
                    Priority = IconPriority.High;
                    MainTexture.Size = settings.SizeChestIcon;
                    MainTexture.Color = Color.GreenYellow;
                    Text = Entity.Path.Replace("Metadata/Chests/DelveChests/", "");

                    //Text = Text.Replace("ChestCurrency", "Cur");
                    //  Text = Text.Replace("Delve", "");
                    if (Text.EndsWith("NoDrops")) Text = "";

                    if (Entity.Path.Equals("Metadata/Chests/DelveChests/OffPathCurrency", StringComparison.Ordinal) ||
                        Entity.Path.Equals("Metadata/Chests/DelveChests/PathCurrency", StringComparison.Ordinal))
                    {
                        MainTexture.UV = SpriteHelper.GetUV(MyMapIconsIndex.Arcanist);
                        MainTexture.Color = Color.Orange;
                        Text = "Currency";
                    }
                    else if (Entity.Path.Equals("Metadata/Chests/DelveChests/OffPathTrinkets", StringComparison.Ordinal) ||
                             Entity.Path.Equals("Metadata/Chests/DelveChests/PathTrinkets", StringComparison.Ordinal))
                    {
                        MainTexture.UV = SpriteHelper.GetUV(MyMapIconsIndex.Jeweller);
                        Text = "Jew";
                    }
                    else if (PathCheck(entity, "Metadata/Chests/DelveChests/OffPathArmour", "Metadata/Chests/DelveChests/PathArmour"))
                    {
                        MainTexture.UV = SpriteHelper.GetUV(MyMapIconsIndex.Armoury);

                        Text =
                            $"{Entity.Path.Substring(Entity.Path.IndexOf("PathArmour", StringComparison.Ordinal) + "PathArmour".Length)}";

                        Text = Text.Trim();
                    }
                    else if (PathCheck(entity, "Metadata/Chests/DelveChests/OffPathWeapon", "Metadata/Chests/DelveChests/PathWeapon"))
                    {
                        MainTexture.UV = SpriteHelper.GetUV(MyMapIconsIndex.Artisan);

                        Text =
                            $"{Entity.Path.Substring(Entity.Path.IndexOf("PathWeapon", StringComparison.Ordinal) + "PathWeapon".Length)}";

                        Text = Text.Trim();
                    }
                    else if (PathCheck(entity, "Metadata/Chests/DelveChests/PathGeneric", "Metadata/Chests/DelveChests/OffPathGeneric"))
                    {
                        MainTexture.UV = SpriteHelper.GetUV(MyMapIconsIndex.Strongbox);

                        Text =
                            $"{Entity.Path.Substring(Entity.Path.IndexOf("PathGeneric", StringComparison.Ordinal) + "PathGeneric".Length)}";

                        Text = Text.Trim();
                    }
                    else if (Entity.Path.Contains("Metadata/Chests/DelveChests/DelveAzuriteVein"))
                    {
                        MainTexture.UV = SpriteHelper.GetUV(MyMapIconsIndex.Divination);
                        Text = Entity.GetComponent<Render>().Name.Replace("<rgb(175,238,238)>{", "").Replace("}", "");
                    }
                    else if (Entity.Path.Contains("Metadata/Chests/DelveChests/DelveMiningSuppliesDynamite"))
                    {
                        MainTexture.UV = SpriteHelper.GetUV(MyMapIconsIndex.Artisan);
                        Text = Entity.GetComponent<Render>().Name.Replace("<rgb(218,165,32)>{", "").Replace("}", "");
                    }
                    else if (Entity.Path.Contains("Metadata/Chests/DelveChests/DelveMiningSuppliesFlares"))
                    {
                        MainTexture.UV = SpriteHelper.GetUV(MyMapIconsIndex.WhiteTexturedEllipse);
                        MainTexture.Color = Color.Yellow;
                        Text = Entity.RenderName.Replace("<rgb(218,165,32)>{", "").Replace("}", "");
                    }
                    else if (Entity.Path.Contains("Metadata/Chests/DelveChests/Dynamite"))
                    {
                        if (PathCheck(entity, "Metadata/Chests/DelveChests/DynamiteWeapon"))
                        {
                            MainTexture.Color = Color.LightGray;
                            MainTexture.UV = SpriteHelper.GetUV(MyMapIconsIndex.Arsenal);
                            Text = $"{Entity.Path.Replace("Metadata/Chests/DelveChests/DynamiteWeapon", "W ")}";
                        }
                        else if (PathCheck(entity, "Metadata/Chests/DelveChests/DynamiteTrinkets"))
                        {
                            MainTexture.Color = Color.GreenYellow;
                            MainTexture.UV = SpriteHelper.GetUV(MyMapIconsIndex.Jeweller);
                            Text = $"{Entity.Path.Replace("Metadata/Chests/DelveChests/DynamiteTrinkets", "J ")}";
                        }
                        else if (PathCheck(entity, "Metadata/Chests/DelveChests/DynamiteArmour"))
                        {
                            MainTexture.Color = Color.LightGray;
                            MainTexture.UV = SpriteHelper.GetUV(MyMapIconsIndex.Armoury);
                            Text = $"{Entity.Path.Replace("Metadata/Chests/DelveChests/DynamiteArmour", "A ")}";
                        }
                        else if (PathCheck(entity, "Metadata/Chests/DelveChests/DynamiteGeneric"))
                        {
                            MainTexture.Color = Color.WhiteSmoke;
                            MainTexture.UV = SpriteHelper.GetUV(MyMapIconsIndex.Strongbox);
                            Text = $"{Entity.Path.Replace("Metadata/Chests/DelveChests/DynamiteGeneric", "G ")}";
                        }
                        else if (PathCheck(entity, "Metadata/Chests/DelveChests/DynamiteCurrency"))
                        {
                            MainTexture.Color = Color.Orange;
                            MainTexture.UV = SpriteHelper.GetUV(MyMapIconsIndex.Arcanist);
                            Text = $"{Entity.Path.Replace("Metadata/Chests/DelveChests/DynamiteCurrency", "$")}";
                        }
                        else
                        {
                            MainTexture.UV = SpriteHelper.GetUV(MyMapIconsIndex.Cartographer);
                            Text = Entity.GetComponent<Render>().Name.Replace("Hidden", "");
                            MainTexture.Color = Color.Aqua;
                        }
                    }
                    else if (Entity.Path.Contains("DelveChestGem"))
                        MainTexture.UV = SpriteHelper.GetUV(MyMapIconsIndex.Gemcutter);
                    else if (Entity.Path.Contains("Resonator"))
                    {
                        if (Entity.Path.EndsWith("3")) Priority = IconPriority.Critical;

                        MainTexture.UV = SpriteHelper.GetUV(MyMapIconsIndex.Divination);
                        MainTexture.Color = Color.Orange;
                    }
                    else if (Entity.Path.Contains("Metadata/Chests/DelveChests/"))
                    {
                        if (Entity.Path.Contains("Metadata/Chests/DelveChests/DelveChestTrinkets"))
                        {
                            MainTexture.UV = SpriteHelper.GetUV(MyMapIconsIndex.Jeweller);
                            MainTexture.Color = Color.Yellow;
                            Text = $"Jew {Entity.Path.Replace("Metadata/Chests/DelveChests/DelveChestTrinkets", "")}";
                        }
                        else if (Entity.Path.Contains("Metadata/Chests/DelveChests/DelveChestMap"))
                        {
                            MainTexture.UV = SpriteHelper.GetUV(MyMapIconsIndex.Cartographer);
                            MainTexture.Color = Color.Aqua;
                            Text = $"Maps {Entity.Path.Replace("Metadata/Chests/DelveChests/DelveChestMap", "")}";
                        }
                        else if (Entity.Path.Contains("Metadata/Chests/DelveChests/DelveChestGenericDivination"))
                        {
                            MainTexture.UV = SpriteHelper.GetUV(MyMapIconsIndex.Divination);
                            MainTexture.Color = Color.OrangeRed;
                            Text = $"Div {Entity.Path.Replace("Metadata/Chests/DelveChests/DelveChestGenericDivination", "")}";
                        }
                        else if (Entity.Path.Contains("Metadata/Chests/DelveChests/DelveChestWeapon"))
                        {
                            MainTexture.UV = SpriteHelper.GetUV(MyMapIconsIndex.Artisan);
                            Text = $"W {Entity.Path.Replace("Metadata/Chests/DelveChests/DelveChestWeapon", "")}";
                        }
                        else if (Entity.Path.Contains("Metadata/Chests/DelveChests/DelveChestGeneric"))
                        {
                            MainTexture.UV = SpriteHelper.GetUV(MyMapIconsIndex.Strongbox);
                            Text = $"G {Entity.Path.Replace("Metadata/Chests/DelveChests/DelveChestGeneric", "")}";
                            MainTexture.Color = Color.WhiteSmoke;
                        }
                        else if (Entity.Path.Contains("Metadata/Chests/DelveChests/DelveChestArmour"))
                        {
                            MainTexture.UV = SpriteHelper.GetUV(MyMapIconsIndex.Armoury);
                            Text = $"A {Entity.Path.Replace("Metadata/Chests/DelveChests/DelveChestArmour", "")}";
                            MainTexture.Color = Color.GreenYellow;
                        }
                        else if (Entity.Path.Contains("Metadata/Chests/DelveChests/DelveChestCurrency"))
                        {
                            MainTexture.UV = SpriteHelper.GetUV(MyMapIconsIndex.Arcanist);
                            Text = $"Cur {Entity.Path.Replace("Metadata/Chests/DelveChests/DelveChestCurrency", "")}";
                            MainTexture.Color = Color.Yellow;
                        }
                    }

                    else
                        MainTexture.UV = SpriteHelper.GetUV(strongboxesUV["Metadata/Chests/StrongBoxes/Large"], new Size2F(7, 8));

                    if (Entity.Path.EndsWith("3") || Entity.Path.EndsWith("2_1")) MainTexture.Color = Color.Magenta;

                    break;
                case ChestType.Strongbox:
                    MainTexture.Size = settings.SizeChestIcon;

                    if (strongboxesUV.TryGetValue(Entity.Path, out var result))
                    {
                        MainTexture.UV = SpriteHelper.GetUV(result, new Size2F(7, 8));
                        Text = Entity.GetComponent<Render>()?.Name;
                    }
                    else
                    {
                        MainTexture.UV = SpriteHelper.GetUV(MyMapIconsIndex.Strongbox);
                        Text = Entity.GetComponent<Render>()?.Name;
                    }

                    break;
                case ChestType.SmallChest:

                    MainTexture.Size = settings.SizeSmallChestIcon;

                    if (Entity.Path.Contains("VaultTreasurePile"))
                    {
                        MainTexture.UV = SpriteHelper.GetUV(strongboxesUV["Metadata/Chests/StrongBoxes/Arcanist"], new Size2F(7, 8));
                        MainTexture.Color = Color.Yellow;
                    }
                    else if (Entity.Path.Contains("SideArea/SideAreaChest"))
                    {
                        MainTexture.FileName = "Icons.png";
                        MainTexture.UV = SpriteHelper.GetUV(new Size2(4, 6), Constants.MapIconsSize);
                    }
                    else
                    {
                        MainTexture.FileName = "Icons.png";
                        MainTexture.UV = SpriteHelper.GetUV(MapIconsIndex.LootFilterSmallCyanSquare);
                        Show = () => Entity.IsValid && settings.ShowSmallChest && !Entity.IsOpened;
                    }

                    break;
                case ChestType.Fossil:
                    Priority = IconPriority.Critical;
                    MainTexture.Size = settings.SizeChestIcon;
                    MainTexture.UV = SpriteHelper.GetUV(MyMapIconsIndex.Divination);
                    var name = Entity.GetComponent<Render>().Name;
                    Text = name.Substring(0, name.IndexOf(" ", StringComparison.Ordinal));
                    MainTexture.Color = Color.Pink;
                    if (FossilRarity.TryGetValue(Text, out var clr)) MainTexture.Color = clr;

                    MainTexture.Color = MainTexture.Color;
                    break;
                case ChestType.Perandus:
                    MainTexture.Size = settings.SizeChestIcon;
                    MainTexture.UV = SpriteHelper.GetUV(strongboxesUV["Metadata/Chests/StrongBoxes/Large"], new Size2F(7, 8));
                    MainTexture.Color = Color.LightGreen;
                    Text = Entity.GetComponent<Render>()?.Name;
                    break;
                case ChestType.Labyrinth:
                    MainTexture.Size = settings.SizeChestIcon;
                    MainTexture.UV = SpriteHelper.GetUV(MyMapIconsIndex.Divination);
                    Text = Entity.Path.Replace("Metadata/Chests/Labyrinth/Labyrinth", "");
                    MainTexture.Color = Color.ForestGreen;
                    break;
                case ChestType.Synthesis:
                    Priority = IconPriority.Critical;
                    MainTexture.Size = settings.SizeChestIcon;
                    MainTexture.UV = SpriteHelper.GetUV(MyMapIconsIndex.Divination);
                    Text = Entity.Path.Split('/').Last();
                    MainTexture.Color = Color.Aquamarine;
                    break;
                case ChestType.Legion:
                    MainTexture.FileName = "Icons.png";
                    Priority = IconPriority.Critical;
                    MainTexture.Size = settings.SizeChestIcon;

                    if (Entity.GetComponent<Stats>().StatDictionary.TryGetValue(GameStat.MonsterMinimapIcon, out var minimapIconIndex))
                        MainTexture.UV = SpriteHelper.GetUV((MapIconsIndex) minimapIconIndex);
                    else
                        MainTexture.UV = SpriteHelper.GetUV(MyMapIconsIndex.Divination);

                    Text = ((MapIconsIndex) minimapIconIndex).ToString().Replace("Legion", "");
                    MainTexture.Color = Color.White;
                    break;
                default:
                    throw new ArgumentOutOfRangeException("Chest type not found.");
            }

            //Debug, useful for delve chests          
            if (settings.LogDebugInformation && Show())
            {
                Logger.Log.Information(
                    $"Chest debug -> CType:{CType} Path: {Entity.Path} #\t\tText: {Text} #\t\tRender Name: {Entity.GetComponent<Render>().Name}");

                if (Entity.GetComponent<Stats>()?.StatDictionary != null)
                {
                    foreach (var i in Entity.GetComponent<Stats>().StatDictionary)
                    {
                        Logger.Log.Information($"Stat: {i.Key} = {i.Value}");
                    }
                }

                if (Entity.GetComponent<ObjectMagicProperties>() != null)
                {
                    foreach (var mod in Entity.GetComponent<ObjectMagicProperties>().Mods)
                    {
                        Logger.Log.Information($"Mods: {mod}");
                    }
                }
            }
        }
    }
}
