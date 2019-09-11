using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Basic;
using Basic.Models;
using Exile;
using Exile.PoEMemory.MemoryObjects;
using PoEMemory;
using PoEMemory.Components;
using PoEMemory.Elements;
using Shared.Enums;
using Map = PoEMemory.Components.Map;

namespace PickIt
{
    public class CustomItem
    {
        public string BaseName { get; private set; } = "";
        public string ClassName { get; private set; } = "";
        public LabelOnGround LabelOnGround { get; private set; }
        public float Distance { get; }
        public Entity GroundItem { get; private set; }
        public int Height { get; private set; }
        public bool IsElder { get; private set; }
        public bool IsIdentified { get; private set; }
        public bool IsRGB { get; private set; }
        public bool IsShaper { get; private set; }
        public bool IsWeapon { get; private set; }
        public int ItemLevel { get; private set; }
        public int LargestLink { get; private set; }
        public int MapTier { get; private set; }
        public string Path { get; private set; }
        public int Quality { get; private set; }
        public ItemRarity Rarity { get; private set; }
        public int Sockets { get; private set; }
        public int Width { get; private set; }
        public bool IsFractured { get; private set; }

        public Func<bool> IsTargeted;

        public bool IsValid = false;

        public int Weight { get; set; }

        public CustomItem(LabelOnGround item, FilesContainer fs, float distance, Dictionary<string, int> weightsRules) {
            LabelOnGround = item;
            Distance = distance;
            var itemItemOnGround = item.ItemOnGround;
            var worldItem = itemItemOnGround?.GetComponent<WorldItem>();
            if (worldItem == null) return;
            var groundItem = worldItem.ItemEntity;
            GroundItem = groundItem;
            Path = groundItem?.Path;
            if (GroundItem == null) return;
            if (Path != null && Path.Length < 1)
            {
                DebugWindow.LogMsg($"World: {worldItem.Address:X} P: {Path}", 2);
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

        public override string ToString() => $"{BaseName} ({ClassName}) W: {Weight} Dist: {Distance}";
    }
}