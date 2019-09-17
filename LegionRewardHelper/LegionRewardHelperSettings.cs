using ExileCore.Shared.Interfaces;
using ExileCore.Shared.Nodes;

namespace LegionRewardHelper
{
    public class LegionRewardHelperSettings : ISettings
    {
        public ToggleNode Enable { get; set; } = new ToggleNode(true);
        public RangeNode<int> Z { get; set; } = new RangeNode<int>(0, -300, 300);
        public RangeNode<int> Size { get; set; } = new RangeNode<int>(10, 3, 300);
    }
}