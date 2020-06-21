using System.Windows.Forms;
using ExileCore.Shared.Interfaces;
using ExileCore.Shared.Nodes;

namespace PickIt
{
    public class PickItSettings : ISettings
    {
        public PickItSettings()
        {
            Enable = new ToggleNode(false);
            PickUpKey = Keys.F1;
            PickupRange = new RangeNode<int>(600, 1, 1000);
            ChestRange = new RangeNode<int>(500, 1, 1000);
            ExtraDelay = new RangeNode<int>(0, 0, 200);
            Sockets = new ToggleNode(true);
            TotalSockets = new RangeNode<int>(6, 1, 6);
            Links = new ToggleNode(true);
            LargestLink = new RangeNode<int>(6, 1, 6);
            RGB = new ToggleNode(true);
            AllDivs = new ToggleNode(true);
            AllCurrency = new ToggleNode(true);
            AllUniques = new ToggleNode(true);
            Maps = new ToggleNode(true);
            UniqueMap = new ToggleNode(true);
            MapFragments = new ToggleNode(true);
            MapTier = new RangeNode<int>(1, 1, 16);
            QuestItems = new ToggleNode(true);
            Gems = new ToggleNode(true);
            GemQuality = new RangeNode<int>(1, 0, 20);
            GroundChests = new ToggleNode(false);
            ShaperItems = new ToggleNode(true);
            MetamorphBodyParts = new ToggleNode(true);
            ElderItems = new ToggleNode(true);
            FracturedItems = new ToggleNode(true);
            Rares = new ToggleNode(true);
            RareJewels = new ToggleNode(true);
            RareRings = new ToggleNode(true);
            RareRingsilvl = new RangeNode<int>(1, 0, 100);
            RareAmulets = new ToggleNode(true);
            RareAmuletsilvl = new RangeNode<int>(1, 0, 100);
            RareBelts = new ToggleNode(true);
            RareBeltsilvl = new RangeNode<int>(1, 0, 100);
            RareGloves = new ToggleNode(false);
            RareGlovesilvl = new RangeNode<int>(1, 0, 100);
            RareBoots = new ToggleNode(false);
            RareBootsilvl = new RangeNode<int>(1, 0, 100);
            RareHelmets = new ToggleNode(false);
            RareHelmetsilvl = new RangeNode<int>(1, 0, 100);
            RareWeapon = new ToggleNode(false);
            RareWeaponWidth = new RangeNode<int>(2, 1, 2);
            RareWeaponHeight = new RangeNode<int>(3, 1, 4);
            ItemCells = new RangeNode<int>(4, 1, 8);
            RareWeaponilvl = new RangeNode<int>(1, 0, 100);
            RareArmour = new ToggleNode(false);
            RareArmourilvl = new RangeNode<int>(1, 0, 100);
            RareShield = new ToggleNode(false);
            RareShieldilvl = new RangeNode<int>(1, 0, 100);
            PickUpEverything = new ToggleNode(false);
            NormalRuleFile = string.Empty;
            MagicRuleFile = string.Empty;
            RareRuleFile = string.Empty;
            UniqueRuleFile = string.Empty;
            LeftClickToggleNode = new ToggleNode(true);
            OverrideItemPickup = new ToggleNode(false);
            MouseSpeed = new RangeNode<float>(1, 0, 30);
        }

        public ToggleNode Enable { get; set; }
        public HotkeyNode PickUpKey { get; set; }
        public RangeNode<int> PickupRange { get; set; }
        public RangeNode<int> ChestRange { get; set; }
        public RangeNode<int> ExtraDelay { get; set; }
        public ToggleNode ShaperItems { get; set; }
        public ToggleNode MetamorphBodyParts { get; set; }
        public ToggleNode ElderItems { get; set; }
        public ToggleNode FracturedItems { get; set; }
        public ToggleNode Rares { get; set; }
        public ToggleNode RareJewels { get; set; }
        public ToggleNode RareRings { get; set; }
        public RangeNode<int> RareRingsilvl { get; set; }
        public ToggleNode RareAmulets { get; set; }
        public RangeNode<int> RareAmuletsilvl { get; set; }
        public ToggleNode RareBelts { get; set; }
        public RangeNode<int> RareBeltsilvl { get; set; }
        public ToggleNode RareGloves { get; set; }
        public RangeNode<int> RareGlovesilvl { get; set; }
        public ToggleNode RareBoots { get; set; }
        public RangeNode<int> RareBootsilvl { get; set; }
        public ToggleNode RareHelmets { get; set; }
        public RangeNode<int> RareHelmetsilvl { get; set; }
        public ToggleNode RareArmour { get; set; }
        public RangeNode<int> RareArmourilvl { get; set; }
        public ToggleNode RareShield { get; set; }
        public RangeNode<int> RareShieldilvl { get; set; }
        public ToggleNode RareWeapon { get; set; }
        public RangeNode<int> RareWeaponWidth { get; set; }
        public RangeNode<int> RareWeaponHeight { get; set; }
        public RangeNode<int> ItemCells { get; set; }
        public RangeNode<int> RareWeaponilvl { get; set; }
        public EmptyNode LinkSocketRgbEmptyNode { get; set; }
        public ToggleNode Sockets { get; set; }
        public RangeNode<int> TotalSockets { get; set; }
        public ToggleNode Links { get; set; }
        public RangeNode<int> LargestLink { get; set; }
        public ToggleNode RGB { get; set; }
        public EmptyNode AllOverridEmptyNode { get; set; }
        public ToggleNode PickUpEverything { get; set; }
        public ToggleNode AllDivs { get; set; }
        public ToggleNode AllCurrency { get; set; }
        public ToggleNode AllUniques { get; set; }
        public ToggleNode Maps { get; set; }
        public RangeNode<int> MapTier { get; set; }
        public ToggleNode UniqueMap { get; set; }
        public ToggleNode MapFragments { get; set; }
        public ToggleNode Gems { get; set; }
        public RangeNode<int> GemQuality { get; set; }
        public ToggleNode QuestItems { get; set; }
        public ToggleNode GroundChests { get; set; }
        public ToggleNode LeftClickToggleNode { get; set; }
        public ToggleNode OverrideItemPickup { get; set; }
        public string NormalRuleFile { get; set; }
        public string MagicRuleFile { get; set; }
        public string RareRuleFile { get; set; }
        public string UniqueRuleFile { get; set; }
        public RangeNode<float> MouseSpeed { get; set; }
        public ToggleNode ReturnMouseToBeforeClickPosition { get; set; } = new ToggleNode(true);
        public RangeNode<int> TimeBeforeNewClick { get; set; } = new RangeNode<int>(500, 0, 1500);
        public ToggleNode UseWeight { get; set; } = new ToggleNode(false);
    }
}
