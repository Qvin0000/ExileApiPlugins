using Shared.Attributes;
using Shared.Interfaces;
using Shared.Nodes;
using SharpDX;

namespace EliteBar
{
    public class EliteBarSettings : ISettings
    {
        public EliteBarSettings() => Enable = new ToggleNode(false);
        public ToggleNode Enable { get; set; }

        [Menu("Limit for work coroutine")]
        public RangeNode<int> LimitCoroutine { get; set; } = new RangeNode<int>(50, 10, 200);

        [Menu("Space between bar")]

        public RangeNode<int> Space { get; set; } = new RangeNode<int>(20, -100, 100);

        [Menu("X")]
        public RangeNode<int> X { get; set; } = new RangeNode<int>(0, 0, 3000);

        [Menu("Y")]
        public RangeNode<int> Y { get; set; } = new RangeNode<int>(0, 0, 3000);

        [Menu("Width")]
        public RangeNode<int> Width { get; set; } = new RangeNode<int>(100, 1, 1000);

        [Menu("Height")]
        public RangeNode<int> Height { get; set; } = new RangeNode<int>(20, 1, 200);

        [Menu("Border color")]
        public ColorNode BorderColor { get; set; } = new ColorNode(Color.Black);

        [Menu("Draw text from ImGui")]
        public ToggleNode ImguiRenderText { get; set; } = new ToggleNode(true);

        [Menu("Refresh")]
        public ButtonNode RefreshEntities { get; set; } = new ButtonNode();

        [Menu("Offset X for text")]
        public RangeNode<int> StartTextX { get; set; } = new RangeNode<int>(0, -50, 350);

        [Menu("Offset Y for text")]
        public RangeNode<int> StartTextY { get; set; } = new RangeNode<int>(0, -50, 350);


        [Menu("Normal monster color")]
        public ColorNode NormalMonster { get; set; } = new ColorNode(Color.Red);

        [Menu("Magic monster color")]
        public ColorNode MagicMonster { get; set; } = new ColorNode(Color.MediumBlue);

        [Menu("Rare monster color")]
        public ColorNode RareMonster { get; set; } = new ColorNode(Color.Yellow);

        [Menu("Unique monster color")]
        public ColorNode UniqueMonster { get; set; } = new ColorNode(Color.Orange);


        [Menu("Show normal monster")]
        public ToggleNode ShowWhite { get; set; } = new ToggleNode(false);

        [Menu("Show magic monster")]
        public ToggleNode ShowMagic { get; set; } = new ToggleNode(false);

        [Menu("Show rare monster")]
        public ToggleNode ShowRare { get; set; } = new ToggleNode(true);

        [Menu("Show unique monster")]
        public ToggleNode ShowUnique { get; set; } = new ToggleNode(true);

        [Menu("Text Color")]
        public ColorNode TextColor { get; set; } = new ColorNode(Color.White);

        [Menu("Use imgui for draw")]
        public ToggleNode UseImguiForDraw { get; set; } = new ToggleNode(false);

        [Menu("BG for imgui")]
        public ColorNode ImguiDrawBg { get; set; } = new ColorNode(Color.Black);

        [Menu("Test")]
        public RangeNode<float> Test { get; set; } = new RangeNode<float>(1, 0, 1);

        public ToggleNode MultiThreading { get; set; } = new ToggleNode(false);
    }
}