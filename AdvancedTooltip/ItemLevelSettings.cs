using ExileCore.Shared.Interfaces;
using ExileCore.Shared.Nodes;
using SharpDX;

namespace AdvancedTooltip
{
    public class ItemLevelSettings : ISettings
    {
        public ItemLevelSettings()
        {
            Enable = new ToggleNode(true);
            TextSize = new RangeNode<int>(16, 10, 50);
            TextColor = new ColorBGRA(255, 255, 0, 255);
            BackgroundColor = new ColorBGRA(0, 0, 0, 230);
        }

        public RangeNode<int> TextSize { get; set; }
        public ColorNode TextColor { get; set; }
        public ColorNode BackgroundColor { get; set; }
        public ToggleNode Enable { get; set; }
    }
}
