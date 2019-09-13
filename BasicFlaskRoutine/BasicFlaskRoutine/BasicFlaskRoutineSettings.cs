using System.Windows.Forms;
using ExileCore.Shared.Nodes;
using TreeRoutine.Routine.BasicFlaskRoutine.Flask;

namespace TreeRoutine.Routine.BasicFlaskRoutine
{
    public class BasicFlaskRoutineSettings : BaseTreeSettings
    {
        public BasicFlaskRoutineSettings()
        {
            TicksPerSecond = new RangeNode<int>(10, 1, 30);
            EnableInHideout  = new ToggleNode(false);
            AutoFlask = new ToggleNode(false);
            ForceBubblingAsInstantOnly = new ToggleNode(false);
            ForcePanickedAsInstantOnly = new ToggleNode(false);
            HPPotion = new RangeNode<int>(65, 0, 100);
            InstantHPPotion = new RangeNode<int>(35, 0, 100);
            DisableLifeSecUse = new ToggleNode(false);
            ManaPotion = new RangeNode<int>(65, 0, 100);
            InstantManaPotion = new RangeNode<int>(35, 0, 100);
            MinManaFlask = new RangeNode<int>(50, 0, 100);
            RemAilment = new ToggleNode(false);
            RemFrozen = new ToggleNode(false);
            RemBurning = new ToggleNode(false);
            RemShocked = new ToggleNode(false);
            RemCurse = new ToggleNode(false);
            RemPoison = new ToggleNode(false);
            RemBleed = new ToggleNode(false);
            CorruptCount = new RangeNode<int>(10, 0, 20);
            SpeedFlaskEnable = new ToggleNode(false);
            QuicksilverFlaskEnable = new ToggleNode(false);
            SilverFlaskEnable = new ToggleNode(false);
            MinMsPlayerMoving = new RangeNode<int>(1500, 1, 10000);
            DefensiveFlaskEnable = new ToggleNode(false);
            HPPercentDefensive = new RangeNode<int>(50, 0, 100);
            ESPercentDefensive = new RangeNode<int>(50, 0, 100);
            OffensiveAsDefensiveEnable = new ToggleNode(false);
            DefensiveMonsterCount = new RangeNode<int>(0, 0, 30);
            DefensiveMonsterDistance = new RangeNode<int>(400, 0, 1500);
            DefensiveCountNormalMonsters = new ToggleNode(false);
            DefensiveCountRareMonsters = new ToggleNode(false);
            DefensiveCountMagicMonsters = new ToggleNode(false);
            DefensiveCountUniqueMonsters = new ToggleNode(false);
            OffensiveFlaskEnable = new ToggleNode(false);
            HPPercentOffensive = new RangeNode<int>(50, 0, 100);
            ESPercentOffensive = new RangeNode<int>(50, 0, 100);
            OffensiveMonsterCount = new RangeNode<int>(0, 0, 30);
            OffensiveMonsterDistance = new RangeNode<int>(400, 0, 1500);
            OffensiveCountNormalMonsters = new ToggleNode(false);
            OffensiveCountRareMonsters = new ToggleNode(false);
            OffensiveCountMagicMonsters = new ToggleNode(false);
            OffensiveCountUniqueMonsters = new ToggleNode(false);
            FlaskSettings = new FlaskSetting[5]
            {
                new FlaskSetting(new ToggleNode(true), new HotkeyNode(Keys.D1), new RangeNode<int>(0, 0, 5)),
                new FlaskSetting(new ToggleNode(true), new HotkeyNode(Keys.D2), new RangeNode<int>(0, 0, 5)),
                new FlaskSetting(new ToggleNode(true), new HotkeyNode(Keys.D3), new RangeNode<int>(0, 0, 5)),
                new FlaskSetting(new ToggleNode(true), new HotkeyNode(Keys.D4), new RangeNode<int>(0, 0, 5)),
                new FlaskSetting(new ToggleNode(true), new HotkeyNode(Keys.D5), new RangeNode<int>(0, 0, 5))
            };
        }

        public RangeNode<int> TicksPerSecond { get; set; }

        public ToggleNode EnableInHideout { get; set; }

        public ToggleNode AutoFlask { get; set; }

        public ToggleNode ForceBubblingAsInstantOnly { get; set; }
        public ToggleNode ForcePanickedAsInstantOnly { get; set; }

        public RangeNode<int> HPPotion { get; set; }
        public RangeNode<int> InstantHPPotion { get; set; }
        public ToggleNode DisableLifeSecUse { get; set; }

        public RangeNode<int> ManaPotion { get; set; }
        public RangeNode<int> InstantManaPotion { get; set; }
        public RangeNode<int> MinManaFlask { get; set; }


        public ToggleNode RemAilment { get; set; }
        public ToggleNode RemFrozen { get; set; }
        public ToggleNode RemBurning { get; set; }
        public ToggleNode RemShocked { get; set; }
        public ToggleNode RemCurse { get; set; }
        public ToggleNode RemPoison { get; set; }
        public ToggleNode RemBleed { get; set; }
        public RangeNode<int> CorruptCount { get; set; }


        public ToggleNode SpeedFlaskEnable { get; set; }
        public ToggleNode QuicksilverFlaskEnable { get; set; }
        public ToggleNode SilverFlaskEnable { get; set; }
        public RangeNode<int> MinMsPlayerMoving { get; set; }

        public ToggleNode DefensiveFlaskEnable { get; set; }
        public RangeNode<int> HPPercentDefensive { get; set; }
        public RangeNode<int> ESPercentDefensive { get; set; }
        public ToggleNode OffensiveAsDefensiveEnable { get; set; }

        public RangeNode<int> DefensiveMonsterCount { get; set; }
        public RangeNode<int> DefensiveMonsterDistance { get; set; }
        public ToggleNode DefensiveCountNormalMonsters { get; set; }
        public ToggleNode DefensiveCountRareMonsters { get; set; }
        public ToggleNode DefensiveCountMagicMonsters { get; set; }
        public ToggleNode DefensiveCountUniqueMonsters { get; set; }


        public ToggleNode OffensiveFlaskEnable { get; set; }
        public RangeNode<int> HPPercentOffensive { get; set; }
        public RangeNode<int> ESPercentOffensive { get; set; }

        public RangeNode<int> OffensiveMonsterCount { get; set; }
        public RangeNode<int> OffensiveMonsterDistance { get; set; }
        public ToggleNode OffensiveCountNormalMonsters { get; set; }
        public ToggleNode OffensiveCountRareMonsters { get; set; }
        public ToggleNode OffensiveCountMagicMonsters { get; set; }
        public ToggleNode OffensiveCountUniqueMonsters { get; set; }

        public FlaskSetting[] FlaskSettings { get; set; }
    }
}