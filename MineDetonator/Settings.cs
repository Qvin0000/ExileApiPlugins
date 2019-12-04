using System.Windows.Forms;
using ExileCore.Shared.Interfaces;
using ExileCore.Shared.Nodes;
using ExileCore.Shared.Attributes;
using Newtonsoft.Json;

namespace MineDetonator
{
    //All properties and public fields of this class will be saved to file
	public class Settings : ISettings
    {
		public Settings()
		{
            Enable = new ToggleNode(true);

        }

	    [Menu("Detonate Key")]
	    public HotkeyNode DetonateKey { get; set; } = new HotkeyNode(Keys.D);

	    [Menu("Detonate Delay")]
	    public RangeNode<int> DetonateDelay { get; set; } = new RangeNode<int>(300, 100, 2000);

		[Menu("Detonate Dist")]
		public RangeNode<float> DetonateDist { get; set; } = new RangeNode<float>(75, 10, 200);

	    [Menu("Current area %")]
	    public RangeNode<float> CurrentAreaPct { get; set; } = new RangeNode<float>(0, 0, 500);

	    [Menu("Debug: Last trigger reason")]
	    public TextNode TriggerReason { get; set; } = new TextNode("a2");

	    [Menu("Filter empty action (Shaper only!)")]
	    public ToggleNode FilterNullAction { get; set; } = new ToggleNode(false);

	    [Menu("Debug")]
	    public ToggleNode Debug { get; set; } = new ToggleNode(false);

        public ToggleNode Enable { get; set; }
    }
}
