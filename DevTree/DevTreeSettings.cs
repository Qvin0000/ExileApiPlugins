using System.Windows.Forms;
using ExileCore.Shared.Attributes;
using ExileCore.Shared.Interfaces;
using ExileCore.Shared.Nodes;

namespace DevTree
{
    public class DevSetting : ISettings
    {
        public bool Opened = false;

        public DevSetting()
        {
            DebugNearestEnts = Keys.NumPad6;
            DebugHoverItem = Keys.NumPad5;
            NearestEntsRange = new RangeNode<int>(300, 1, 2000);
            LimitEntriesDrawn = new ToggleNode(true);
            EntriesDrawLimit = new RangeNode<int>(500, 1, 5000);
            Enable = new ToggleNode(false);
            LimitForCollection = new RangeNode<int>(500, 2, 5000);
        }

        [Menu("Debug Nearest Ents")]
        public HotkeyNode DebugNearestEnts { get; set; }
        [Menu("Debug Hover Item")]
        public HotkeyNode DebugHoverItem { get; set; }
        [Menu("Nearest Ents Range")]
        public RangeNode<int> NearestEntsRange { get; set; }
        [Menu("Limit for collections")]
        public RangeNode<int> LimitForCollection { get; set; }
        [Menu("Limit Entries Drawn", 0)]
        public ToggleNode LimitEntriesDrawn { get; set; }
        [Menu("Entries Draw Limit", 1, 0)]
        public RangeNode<int> EntriesDrawLimit { get; set; }
        public ToggleNode Enable { get; set; }
    }
}
