using System.Windows.Forms;
using ExileCore.Shared.Interfaces;
using ExileCore.Shared.Nodes;
using ExileCore.Shared.Attributes;

namespace BuffUtil
{
    public class BuffUtilSettings : ISettings
    {
        public BuffUtilSettings()
        {
            BloodRage = new ToggleNode(false);
            BloodRageKey = new HotkeyNode(Keys.E);
            BloodRageConnectedSkill = new RangeNode<int>(1, 1, 8);
            BloodRageMaxHP = new RangeNode<int>(100, 0, 100);
            BloodRageMaxMP = new RangeNode<int>(100, 0, 100);

            SteelSkin = new ToggleNode(false);
            SteelSkinKey = new HotkeyNode(Keys.W);
            SteelSkinConnectedSkill = new RangeNode<int>(1, 1, 8);
            SteelSkinMaxHP = new RangeNode<int>(90, 0, 100);

            ImmortalCall = new ToggleNode(false);
            ImmortalCallKey = new HotkeyNode(Keys.T);
            ImmortalCallConnectedSkill = new RangeNode<int>(1, 1, 8);
            ImmortalCallMaxHP = new RangeNode<int>(50, 0, 100);

            MoltenShell = new ToggleNode(false);
            MoltenShellKey = new HotkeyNode(Keys.Q);
            MoltenShellConnectedSkill = new RangeNode<int>(1, 1, 8);
            MoltenShellMaxHP = new RangeNode<int>(50, 0, 100);

            PhaseRun = new ToggleNode(false);
            PhaseRunKey = new HotkeyNode(Keys.R);
            PhaseRunConnectedSkill = new RangeNode<int>(1, 1, 8);
            PhaseRunMaxHP = new RangeNode<int>(90, 0, 100);

            WitheringStep = new ToggleNode(false);
            WitheringStepKey = new HotkeyNode(Keys.R);
            WitheringStepConnectedSkill = new RangeNode<int>(1, 1, 8);
            WitheringStepMaxHP = new RangeNode<int>(90, 0, 100);

            BladeFlurry = new ToggleNode(false);
            BladeFlurryMinCharges = new RangeNode<int>(6, 1, 6);
            BladeFlurryUseLeftClick = new ToggleNode(false);
            BladeFlurryWaitForInfused = new ToggleNode(true);

            ScourgeArrow = new ToggleNode(false);
            ScourgeArrowMinCharges = new RangeNode<int>(5, 1, 6);
            ScourgeArrowUseLeftClick = new ToggleNode(false);
            ScourgeArrowWaitForInfused = new ToggleNode(true);

            RequireMinMonsterCount = new ToggleNode(false);
            NearbyMonsterCount = new RangeNode<int>(1, 1, 30);
            NearbyMonsterMaxDistance = new RangeNode<int>(500, 1, 2000);
            DisableInHideout = new ToggleNode(true);
            Debug = new ToggleNode(false);
            SilenceErrors = new ToggleNode(false);
        }

        public ToggleNode Enable { get; set; }

        #region Blood Rage

        [Menu("Blood Rage", 1)] public ToggleNode BloodRage { get; set; }

        [Menu("Blood Rage Key", "Which key to press to activate Blood Rage?", 11, 1)]
        public HotkeyNode BloodRageKey { get; set; }


        [Menu("Connected Skill", "Set the skill slot (1 = top left, 8 = bottom right)", 12, 1)]
        public RangeNode<int> BloodRageConnectedSkill { get; set; }

        [Menu("Max HP", "HP percent above which skill is not cast", 13, 1)]
        public RangeNode<int> BloodRageMaxHP { get; set; }

        [Menu("Max Mana", "Mana percent above which skill is not cast", 14, 1)]
        public RangeNode<int> BloodRageMaxMP { get; set; }

        #endregion

        #region Steel Skin

        [Menu("Steel Skin", 2)] public ToggleNode SteelSkin { get; set; }

        [Menu("Steel Skin Key", 21, 2)] public HotkeyNode SteelSkinKey { get; set; }

        [Menu("Connected Skill", "Set the skill slot (1 = top left, 8 = bottom right)", 22, 2)]
        public RangeNode<int> SteelSkinConnectedSkill { get; set; }

        [Menu("Max HP", "HP percent above which skill is not cast", 23, 2)]
        public RangeNode<int> SteelSkinMaxHP { get; set; }

        #endregion

        #region Immortal Call

        [Menu("Immortal Call", 3)] public ToggleNode ImmortalCall { get; set; }

        [Menu("Immortal Call Key", 31, 3)] public HotkeyNode ImmortalCallKey { get; set; }

        [Menu("Connected Skill", "Set the skill slot (1 = top left, 8 = bottom right)", 32, 3)]
        public RangeNode<int> ImmortalCallConnectedSkill { get; set; }

        [Menu("Max HP", "HP percent above which skill is not cast", 33, 3)]
        public RangeNode<int> ImmortalCallMaxHP { get; set; }

        #endregion

        #region Molten Shell

        [Menu("Molten Shell", 4)] public ToggleNode MoltenShell { get; set; }

        [Menu("Molten Shell Key", 41, 4)] public HotkeyNode MoltenShellKey { get; set; }

        [Menu("Connected Skill", "Set the skill slot (1 = top left, 8 = bottom right)", 42, 4)]
        public RangeNode<int> MoltenShellConnectedSkill { get; set; }

        [Menu("Max HP", "HP percent above which skill is not cast", 43, 4)]
        public RangeNode<int> MoltenShellMaxHP { get; set; }

        #endregion

        #region Phase Run

        [Menu("Phase Run", 5)] public ToggleNode PhaseRun { get; set; }

        [Menu("Phase Run Key", 51, 5)] public HotkeyNode PhaseRunKey { get; set; }

        [Menu("Connected Skill", "Set the skill slot (1 = top left, 8 = bottom right)", 52, 5)]
        public RangeNode<int> PhaseRunConnectedSkill { get; set; }

        [Menu("Max HP", "HP percent above which skill is not cast", 53, 5)]
        public RangeNode<int> PhaseRunMaxHP { get; set; }

        #endregion

        #region Withering Step

        [Menu("Withering Step", 6)] public ToggleNode WitheringStep { get; set; }

        [Menu("Withering Step Key", 61, 6)] public HotkeyNode WitheringStepKey { get; set; }

        [Menu("Connected Skill", "Set the skill slot (1 = top left, 8 = bottom right)", 62, 6)]
        public RangeNode<int> WitheringStepConnectedSkill { get; set; }

        [Menu("Max HP", "HP percent above which skill is not cast", 63, 6)]
        public RangeNode<int> WitheringStepMaxHP { get; set; }

        #endregion

        #region Blade Flurry

        [Menu("Blade Flurry", "Use mouse click to release Blade Flurry charges", 7)] public ToggleNode BladeFlurry { get; set; }

        [Menu("Min charges", "Minimal amount of BF charges to release", 71, 7)]
        public RangeNode<int> BladeFlurryMinCharges { get; set; }

        [Menu("Use left click", "Use left click instead of right click to release charges", 72, 7)] 
        public ToggleNode BladeFlurryUseLeftClick { get; set; }
        
        [Menu("Wait for Infused Channeling buff", "Wait for Infused Channeling buff before release", 73, 7)] 
        public ToggleNode BladeFlurryWaitForInfused { get; set; }

        #endregion

        #region Scourge Arrow

        [Menu("Scourge Arrow", "Use mouse click to release Scourge Arrow charges", 8)] public ToggleNode ScourgeArrow { get; set; }

        [Menu("Min charges", "Minimal amount of BF charges to release", 81, 8)]
        public RangeNode<int> ScourgeArrowMinCharges { get; set; }

        [Menu("Use left click", "Use left click instead of right click to release charges", 82, 8)] 
        public ToggleNode ScourgeArrowUseLeftClick { get; set; }
        
        [Menu("Wait for Infused Channeling buff", "Wait for Infused Channeling buff before release", 83, 8)] 
        public ToggleNode ScourgeArrowWaitForInfused { get; set; }
        #endregion

        #region Misc

        [Menu("Misc", 10)] public EmptyNode MiscSettings { get; set; }

        [Menu("Nearby monsters", "Require a minimum count of nearby monsters to cast buffs?", 101, 10)]
        public ToggleNode RequireMinMonsterCount { get; set; }

        [Menu("Range", "Minimum count of nearby monsters to cast", 102, 10)]
        public RangeNode<int> NearbyMonsterCount { get; set; }

        [Menu("Range", "Max distance of monsters to player to count as nearby", 103, 10)]
        public RangeNode<int> NearbyMonsterMaxDistance { get; set; }

        [Menu("Disable in hideout", "Disable the plugin in hideout?", 104, 10)]
        public ToggleNode DisableInHideout { get; set; }
        
        [Menu("Debug", "Print debug messages?", 105, 10)]
        public ToggleNode Debug { get; set; }
        
        [Menu("Silence errors", "Hide error messages?", 106, 10)]
        public ToggleNode SilenceErrors { get; set; }

        #endregion
    }
}