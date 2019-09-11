using Shared.Interfaces;
using Shared.Nodes;
using Shared.Attributes;
using Shared.Nodes;

namespace IconsBuilder
{
    public class IconsBuilderSettings : ISettings
    {
        public ToggleNode Enable { get; set; } = new ToggleNode(true);

        [Menu("Default size")]
        public float SizeDefaultIcon { get; set; } = new RangeNode<int>(16, 1, 50);

        [Menu("Size NPC icon")]
        public RangeNode<int> SizeNpcIcon { get; set; } = new RangeNode<int>(10, 1, 50);

        [Menu("Size monster icon")]
        public RangeNode<int> SizeEntityWhiteIcon { get; set; } = new RangeNode<int>(10, 1, 50);

        [Menu("Size magic monster icon")]
        public RangeNode<int> SizeEntityMagicIcon { get; set; } = new RangeNode<int>(10, 1, 50);

        [Menu("Size rare monster icon")]
        public RangeNode<int> SizeEntityRareIcon { get; set; } = new RangeNode<int>(10, 1, 50);

        [Menu("Size unique monster icon")]
        public RangeNode<int> SizeEntityUniqueIcon { get; set; } = new RangeNode<int>(10, 1, 50);

        [Menu("Size breach chest icon")]
        public RangeNode<int> SizeBreachChestIcon { get; set; } = new RangeNode<int>(10, 1, 50);

        [Menu("Size chests icon")]
        public RangeNode<int> SizeChestIcon { get; set; } = new RangeNode<int>(10, 1, 50);

        [Menu("Show small chests")]
        public ToggleNode ShowSmallChest { get; set; } = new ToggleNode(false);

        [Menu("Size small chests icon")]
        public RangeNode<int> SizeSmallChestIcon { get; set; } = new RangeNode<int>(10, 1, 50);

        [Menu("Size misc icon")]
        public RangeNode<int> SizeMiscIcon { get; set; } = new RangeNode<int>(10, 1, 50);

        [Menu("Size shrine icon")]
        public RangeNode<int> SizeShrineIcon { get; set; } = new RangeNode<int>(10, 1, 50);

        [Menu("Size secondory icon")]
        public RangeNode<int> SecondaryIconSize { get; set; } = new RangeNode<int>(10, 1, 50);

        [Menu("Hidden monster icon size")]
        public RangeNode<float> HideSize { get; set; } = new RangeNode<float>(1, 0, 1);

        [Menu("Debug information about entities")]
        public ToggleNode LogDebugInformation { get; set; } = new ToggleNode(true);

        [Menu("Reparse entities")]
        public ButtonNode Reparse { get; set; } = new ButtonNode();

        public ToggleNode MultiThreading { get; set; } = new ToggleNode(false);
        public RangeNode<int> MultiThreadingWhenEntityMoreThan { get; set; } = new RangeNode<int>(10,1,200);
        public ToggleNode HidePlayers { get; set; } = new ToggleNode(false);
        public ToggleNode HideMinions { get; set; } = new ToggleNode(false);
    }
}