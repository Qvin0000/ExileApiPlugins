using System.Windows.Forms;
using ExileCore.Shared.Attributes;
using ExileCore.Shared.Interfaces;
using ExileCore.Shared.Nodes;

public class Settings : ISettings
{
    public int CurrentImageDateDay = -1;
    public bool ShowImage = true;

    public Settings()
    {
        LabType = new ListNode {Value = "uber"};
        AutoLabDetection = new ToggleNode(true);
        X = new RangeNode<float>(500, 0, 2000);
        Y = new RangeNode<float>(0, 0, 2000);

        Transparency = new RangeNode<float>(255, 0, 255);
        Size = new RangeNode<float>(100, 10, 300);

        ToggleDraw = new HotkeyNode(Keys.Delete);
        Reload = new HotkeyNode(Keys.Insert);
    }

    [Menu("Lab Type")]
    public ListNode LabType { get; set; }
    [Menu("Auto Lab Detection")]
    public ToggleNode AutoLabDetection { get; set; }
    [Menu("Position X")]
    public RangeNode<float> X { get; set; }
    [Menu("Position Y")]
    public RangeNode<float> Y { get; set; }
    [Menu("Opacity")]
    public RangeNode<float> Transparency { get; set; }
    [Menu("Size")]
    public RangeNode<float> Size { get; set; }
    [Menu("Toggle Display")]
    public HotkeyNode ToggleDraw { get; set; }
    [Menu("Force Reload")]
    public HotkeyNode Reload { get; set; }
    public ToggleNode Enable { get; set; } = new ToggleNode(true);
}
