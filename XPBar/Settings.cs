using ExileCore.Shared.Interfaces;
using ExileCore.Shared.Nodes;

namespace XPBar
{
    public class Settings : ISettings
    {
        public Settings()
        {
            ExampleRangeNode = new RangeNode<int>(1, 0, 100);
        }

        public RangeNode<int> ExampleRangeNode { get; set; }
        public ToggleNode Enable { get; set; } = new ToggleNode(true);
    }
}
