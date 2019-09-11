using ExileCore.Shared.Attributes;
using ExileCore.Shared.Interfaces;
using ExileCore.Shared.Nodes;
using SharpDX;

namespace DPSMeter
{
    public class DPSMeterSettings : ISettings
    {
        public DPSMeterSettings()
        {
            ShowInTown = new ToggleNode(false);
            DpsFontColor = new ColorBGRA(220, 190, 130, 255);
            PeakFontColor = new ColorBGRA(220, 190, 130, 255);
            BackgroundColor = new ColorBGRA(0, 0, 0, 255);
            ClearNode = new ButtonNode();
        }

        public ToggleNode Enable { get; set; } = new ToggleNode(false);
        public ToggleNode ShowInTown { get; set; }

        // public RangeNode<int> TextSize { get; set; } = new RangeNode<int>(16, 10, 20);
        public ColorNode DpsFontColor { get; set; }
        public ColorNode PeakFontColor { get; set; }
        public ColorNode BackgroundColor { get; set; }
        public ButtonNode ClearNode { get; set; }
        [Menu("Show AOE")]
        public ToggleNode ShowAOE { get; set; } = new ToggleNode(true);
        public ToggleNode ShowCurrentHitDamage { get; set; } = new ToggleNode(true);
        public ToggleNode HasCullingStrike { get; set; } = new ToggleNode(false);
        public RangeNode<int> UpdateTime { get; set; } = new RangeNode<int>(100, 50, 2000);
        public ToggleNode MultiThreading { get; set; } = new ToggleNode(false);
    }
}
