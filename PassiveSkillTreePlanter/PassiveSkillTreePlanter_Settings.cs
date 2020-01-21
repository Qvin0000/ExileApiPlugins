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
            BorderWidth = new RangeNode<int>(1, 1, 5);
            LineWidth = new RangeNode<int>(3, 0, 5);
            LineColor = new ColorNode(Color.Gray);
            SelectedURLFile = string.Empty;
        }
        public RangeNode<int> offsetX { get; set; } = new RangeNode<int>(12450, 11000, 14000);
        public RangeNode<int> offsetY { get; set; } = new RangeNode<int>(11850, 11000, 13000);

        public RangeNode<int> BorderWidth { get; set; }

        public RangeNode<int> LineWidth { get; set; }

        public ColorNode LineColor { get; set; }

        public string SelectedURLFile { get; set; }
        public string SelectedURL { get; set; } = "";
        public string SelectedTreeName { get; set; } = "";


        public RangeNode<int> PickedBorderWidth { get; set; } = new RangeNode<int>(1, 1, 5);
        public RangeNode<int> UnpickedBorderWidth { get; set; } = new RangeNode<int>(3, 1, 5);
        public RangeNode<int> WrongPickedBorderWidth { get; set; } = new RangeNode<int>(3, 1, 5);

        public ColorNode PickedBorderColor { get; set; } = new ColorNode();
        public ColorNode UnpickedBorderColor { get; set; } = new ColorNode(Color.Green);
        public ColorNode WrongPickedBorderColor { get; set; } = new ColorNode(Color.Red);

        public TreeConfig.SkillTreeData SelectedBuild { get; set; } = new TreeConfig.SkillTreeData();
        public TreeConfig.SkillTreeData SelectedBuildCreating { get; set; } = new TreeConfig.SkillTreeData();

        public ToggleNode EnableEzTreeChanger { get; set; } = new ToggleNode(true);
        public ToggleNode Enable { get; set; } = new ToggleNode(true);
    }
}