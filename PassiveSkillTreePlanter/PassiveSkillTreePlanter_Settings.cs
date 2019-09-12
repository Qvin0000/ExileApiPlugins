using ExileCore.Shared.Attributes;
using ExileCore.Shared.Interfaces;
using ExileCore.Shared.Nodes;
using SharpDX;
using ImGuiVector2 = System.Numerics.Vector2;

namespace PassiveSkillTreePlanter
{
    public class PassiveSkillTreePlanterSettings : ISettings
    {
        public PassiveSkillTreePlanterSettings()
        {
            PickedBorderColor = new ColorNode(Color.Gray);
            UnpickedBorderColor = new ColorNode(Color.Green);
            WrongPickedBorderColor = new ColorNode(Color.Red);
            LineWidth = new RangeNode<int>(3, 0, 5);
            LineColor = new ColorNode(new Color(0, 255, 0, 50));
            SelectedURLFile = string.Empty;
        }

        public RangeNode<int> PickedBorderWidth { get; set; } = new RangeNode<int>(1, 1, 5);
        public RangeNode<int> UnpickedBorderWidth { get; set; } = new RangeNode<int>(3, 1, 5);
        public RangeNode<int> WrongPickedBorderWidth { get; set; } = new RangeNode<int>(3, 1, 5);
        public ColorNode PickedBorderColor { get; set; }
        public ColorNode UnpickedBorderColor { get; set; }
        public ColorNode WrongPickedBorderColor { get; set; }
        public RangeNode<int> LineWidth { get; set; }
        public ColorNode LineColor { get; set; }
        public string SelectedURLFile { get; set; }
        public ToggleNode Enable { get; set; } = new ToggleNode(true);
    }
}
