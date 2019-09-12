using ExileCore.Shared.Attributes;
using ExileCore.Shared.Interfaces;
using ExileCore.Shared.Nodes;

namespace MinimapIcons
{
    public class MapIconsSettings : ISettings
    {
        [Menu("Draw Monster")]
        public ToggleNode DrawMonsters { get; set; } = new ToggleNode(false);
        [Menu("Icons on minimap")]
        public ToggleNode IconsOnMinimap { get; set; } = new ToggleNode(true);
        [Menu("Icons on large map")]
        public ToggleNode IconsOnLargeMap { get; set; } = new ToggleNode(true);
        [Menu("Size secondory icon")]
        public RangeNode<int> SecondaryIconSize { get; set; } = new RangeNode<int>(10, 1, 25);
        [Menu("Hidden monster icon size")]
        public RangeNode<float> HideSize { get; set; } = new RangeNode<float>(1, 0, 1);
        [Menu("Z for text")]
        public RangeNode<float> ZForText { get; set; } = new RangeNode<float>(-10, -50, 50);
        public ToggleNode DrawOnlyOnLargeMap { get; set; } = new ToggleNode(true);
        public ToggleNode MultiThreading { get; set; } = new ToggleNode(false);
        public ToggleNode DrawNotValid { get; set; } = new ToggleNode(true);
        public ToggleNode Enable { get; set; } = new ToggleNode(false);
    }
}
