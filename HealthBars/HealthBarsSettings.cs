using Shared.Interfaces;
using Shared.Nodes;
using Shared.Attributes;

namespace HealthBars
{
    public class HealthBarsSettings : ISettings
    {
        public HealthBarsSettings() {
            Enable = new ToggleNode(false);
            ShowInTown = new ToggleNode(false);
            ShowES = new ToggleNode(true);
            ShowEnemies = new ToggleNode(true);
            Players = new UnitSettings(0x008000ff, 0);
            Minions = new UnitSettings(0x90ee90ff, 0);
            NormalEnemy = new UnitSettings(0xff0000ff, 0, 0x66ff66ff, false);
            MagicEnemy = new UnitSettings(0xff0000ff, 0x8888ffff, 0x66ff99ff, false);
            RareEnemy = new UnitSettings(0xff0000ff, 0xffff77ff, 0x66ff99ff, false);
            UniqueEnemy = new UnitSettings(0xff0000ff, 0xffa500ff, 0x66ff99ff, false);
            ShowDebuffPanel = new ToggleNode(false);
            DebuffPanelIconSize = new RangeNode<int>(20, 15, 40);
            GlobalZ = new RangeNode<int>(-100, -300, 300);
            PlayerZ = new RangeNode<int>(-100, -300, 300);
        }

        [Menu("Show In Town")]
        public ToggleNode ShowInTown { get; set; }

        [Menu("Show ES")]
        public ToggleNode ShowES { get; set; }

        [Menu("Show Enemies", 0, 3)]
        public ToggleNode ShowEnemies { get; set; }

        [Menu("Players", 1)]
        public UnitSettings Players { get; set; }

        [Menu("Minions", 2)]
        public UnitSettings Minions { get; set; }

        [Menu("Normal enemy", 3)]
        public UnitSettings NormalEnemy { get; set; }

        [Menu("Magic enemy", 4)]
        public UnitSettings MagicEnemy { get; set; }

        [Menu("Rare enemy", 5)]
        public UnitSettings RareEnemy { get; set; }

        [Menu("Unique enemy", 6)]
        public UnitSettings UniqueEnemy { get; set; }

        [Menu("Show Debuff Panel")]
        public ToggleNode ShowDebuffPanel { get; set; }

        [Menu("Size debuff icon")]
        public RangeNode<int> DebuffPanelIconSize { get; set; }

        [Menu("Z")]
        public RangeNode<int> GlobalZ { get; set; }

        [Menu("Player Z")]
        public RangeNode<int> PlayerZ { get; set; }

        [Menu("Hide Over UI")]
        public ToggleNode HideOverUi { get; set; } = new ToggleNode(true);

        [Menu("Using ImGui for render")]
        public ToggleNode ImGuiRender { get; set; } = new ToggleNode(false);

        public ToggleNode Enable { get; set; }
        public RangeNode<int> LimitDrawDistance { get; set; } = new RangeNode<int>(133, 0, 1000);

        [Menu("Rounding")]
        public RangeNode<float> Rounding { get; set; } = new RangeNode<float>(0, 0, 64);

        public ToggleNode MultiThreading { get; set; } = new ToggleNode(false);
        public RangeNode<int> MultiThreadingCountEntities { get; set; } = new RangeNode<int>(10,1,200);
        public RangeNode<int> ShowMinionOnlyBelowHp { get; set; } = new RangeNode<int>(50,1,100);
        public ToggleNode SelfHealthBarShow { get; set; } = new ToggleNode(true);
    }

    public class UnitSettings : ISettings
    {
        public UnitSettings(uint color, uint outline) {
            Enable = new ToggleNode(true);
            Width = new RangeNode<float>(100, 20, 250);
            Height = new RangeNode<float>(20, 5, 150);
            Color = color;
            Outline = outline;
            Under10Percent = 0xffffffff;
            PercentTextColor = 0xffffffff;
            HealthTextColor = 0xffffffff;
            HealthTextColorUnder10Percent = 0xffff00ff;
            ShowPercents = new ToggleNode(false);
            ShowHealthText = new ToggleNode(false);
            ShowFloatingCombatDamage = new ToggleNode(false);
            FloatingCombatTextSize = new RangeNode<int>(15, 10, 30);
            FloatingCombatDamageColor = SharpDX.Color.Yellow;
            FloatingCombatHealColor = SharpDX.Color.Lime;
            BackGround = SharpDX.Color.Black;
            TextSize = new RangeNode<int>(15, 10, 50);
            FloatingCombatStackSize = new RangeNode<int>(1, 1, 10);
        }

        public UnitSettings(uint color, uint outline, uint percentTextColor, bool showText) : this(color, outline) {
            PercentTextColor = percentTextColor;
            ShowPercents.Value = showText;
            ShowHealthText.Value = showText;
        }

        public RangeNode<float> Width { get; set; }
        public RangeNode<float> Height { get; set; }
        public ColorNode Color { get; set; }
        public ColorNode Outline { get; set; }
        public ColorNode BackGround { get; set; }
        public ColorNode Under10Percent { get; set; }
        public ColorNode PercentTextColor { get; set; }
        public ColorNode HealthTextColor { get; set; }
        public ColorNode HealthTextColorUnder10Percent { get; set; }
        public ToggleNode ShowPercents { get; set; }
        public ToggleNode ShowHealthText { get; set; }
        public RangeNode<int> TextSize { get; set; }

        [Menu("Floating Combat Text")]
        public ToggleNode ShowFloatingCombatDamage { get; set; }

        [Menu("Damage Color")]
        public ColorNode FloatingCombatDamageColor { get; set; }

        [Menu("Heal Color")]
        public ColorNode FloatingCombatHealColor { get; set; }

        [Menu("Text Size")]
        public RangeNode<int> FloatingCombatTextSize { get; set; }

        [Menu("Number of Lines")]
        public RangeNode<int> FloatingCombatStackSize { get; set; }

        [Menu("Enable")]
        public ToggleNode Enable { get; set; }
    }
}