using System;
using System.Collections.Generic;
using System.Linq;
using ExileCore;
using ExileCore.PoEMemory;
using ExileCore.PoEMemory.Components;
using ExileCore.PoEMemory.Elements;
using ExileCore.PoEMemory.MemoryObjects;
using ExileCore.Shared.Enums;
using Map = ExileCore.PoEMemory.Components.Map;

namespace PickIt
{
    public class CustomItem
    {
        public Func<bool> IsTargeted;
        public bool IsValid;

        public CustomItem(LabelOnGround item, FilesContainer fs, float distance, Dictionary<string, int> weightsRules)
        {
            LabelOnGround = item;
            Distance = distance;
            var itemItemOnGround = item.ItemOnGround;
            var worldItem = itemItemOnGround?.GetComponent<WorldItem>()?.ItemEntity ?? itemItemOnGround;
            if (worldItem == null) return;
            GroundItem = worldItem;
            Path = GroundItem?.Path;
            if (GroundItem == null) return;

            if (Path != null && Path.Length < 1)
            {
                DebugWindow.LogMsg($"World: {worldItem?.Address:X} P: {Path}", 2);
                DebugWindow.LogMsg($"Ground: {GroundItem.Address:X} P {Path}", 2);
                return;
            }

            IsTargeted = () =>
            {
                var isTargeted = itemItemOnGround.GetComponent<Targetable>()?.isTargeted;
                return isTargeted != null && (bool) isTargeted;
            };

            var baseItemType = fs.BaseItemTypes.Translate(Path);

            if (baseItemType != null)
            {
                ClassName = baseItemType.ClassName;
                BaseName = baseItemType.BaseName;
                Width = baseItemType.Width;
                Height = baseItemType.Height;
                if (weightsRules.TryGetValue(BaseName, out var w)) Weight = w;
            }

            var WeaponClass = new List<string>
            {
                "One Hand Mace",
                "Two Hand Mace",
                "One Hand Axe",
                "Two Hand Axe",
                "One Hand Sword",
                "Two Hand Sword",
                "Thrusting One Hand Sword",
                "Bow",
                "Claw",
                "Dagger",
                "Sceptre",
                "Staff",
                "Wand"
            };

            if (GroundItem.HasComponent<Quality>())
            {
                var quality = GroundItem.GetComponent<Quality>();
                Quality = quality.ItemQuality;
            }

            if (GroundItem.HasComponent<Base>())
            {
                var @base = GroundItem.GetComponent<Base>();
                IsElder = @base.isElder;
                IsShaper = @base.isShaper;
            }

            if (GroundItem.HasComponent<Mods>())
            {
                var mods = GroundItem.GetComponent<Mods>();
                Rarity = mods.ItemRarity;
                IsIdentified = mods.Identified;
                ItemLevel = mods.ItemLevel;
                IsFractured = mods.HaveFractured;
            }

            if (GroundItem.HasComponent<Sockets>())
            {
                var sockets = GroundItem.GetComponent<Sockets>();
                IsRGB = sockets.IsRGB;
                Sockets = sockets.NumberOfSockets;
                LargestLink = sockets.LargestLinkSize;
            }

            if (WeaponClass.Any(ClassName.Equals)) IsWeapon = true;

            MapTier = GroundItem.HasComponent<Map>() ? GroundItem.GetComponent<Map>().Tier : 0;
            IsValid = true;
        }

        public string BaseName { get; } = "";
        public string ClassName { get; } = "";
        public LabelOnGround LabelOnGround { get; }
        public float Distance { get; }
        public Entity GroundItem { get; }
        public int Height { get; }
        public bool IsElder { get; }
        public bool IsIdentified { get; }
        public bool IsRGB { get; }
        public bool IsShaper { get; }
        public bool IsWeapon { get; }
        public int ItemLevel { get; }
        public int LargestLink { get; }
        public int MapTier { get; }
        public string Path { get; }
        public int Quality { get; }
        public ItemRarity Rarity { get; }
        public int Sockets { get; }
        public int Width { get; }
        public bool IsFractured { get; }
        public int Weight { get; set; }
        public bool IsMetamorphBodyPart { get; set; }//MetamorphBodyPart don't have a WorldItem component so using this way

        public override string ToString()
        {
            return $"{BaseName} ({ClassName}) W: {Weight} Dist: {Distance}";
        }
    }
}
