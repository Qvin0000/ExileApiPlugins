using System.Collections.Generic;
using System.Windows.Forms;
using ExileCore.Shared.Attributes;
using ExileCore.Shared.Interfaces;
using ExileCore.Shared.Nodes;

namespace Stashie
{
    public class StashieSettings : ISettings
    {
        public List<string> AllStashNames = new List<string>();
        public Dictionary<string, ListIndexNode> CustomFilterOptions;
        public Dictionary<string, RangeNode<int>> CustomRefillOptions;

        public StashieSettings()
        {
            Enable = new ToggleNode(false);
            RequireHotkey = new ToggleNode(true);
            DropHotkey = Keys.F3;
            ExtraDelay = new RangeNode<int>(0, 0, 2000);
            MouseSpeed = new RangeNode<int>(10, 0, 50);
            BlockInput = new ToggleNode(false);
            UseArrow = new ToggleNode(true);
            RefillCurrency = new ToggleNode(false);
            CurrencyStashTab = new ListIndexNode();
            AllowHaveMore = new ToggleNode(false);
            CustomFilterOptions = new Dictionary<string, ListIndexNode>();
            CustomRefillOptions = new Dictionary<string, RangeNode<int>>();
            VisitTabWhenDone = new ToggleNode(false);
            TabToVisitWhenDone = new RangeNode<int>(0, 0, 40);
        }

        [Menu("Settings", 500)]
        public EmptyNode Settings { get; set; }
        [Menu("Require Hotkey", "If you just want Stashie to drop items to stash, as soon as you open it.", 1000, 500)]
        public ToggleNode RequireHotkey { get; set; }
        [Menu("Hotkey", 1001, 1000)]
        public HotkeyNode DropHotkey { get; set; }
        [Menu("Extra Delay", "Is it going too fast? Then add a delay (in ms).", 2000, 500)]
        public RangeNode<int> ExtraDelay { get; set; }
        [Menu("Block Input", "Block user input (except: Ctrl+Alt+Delete) when dropping items to stash.", 3000, 500)]
        public ToggleNode BlockInput { get; set; }
        [Menu("When done, go to tab.", "After Stashie has dropped all items to their respective tabs, then go to the following tab.", 4000,
            500)]
        public ToggleNode VisitTabWhenDone { get; set; }
        [Menu("tab (index)", 4001, 4000)]
        public RangeNode<int> TabToVisitWhenDone { get; set; }
        public ToggleNode RefillCurrency { get; set; }
        public ListIndexNode CurrencyStashTab { get; set; }
        public ToggleNode AllowHaveMore { get; set; }
        [Menu("Use keyboard arrow", "For switch", 5000, 500)]
        public ToggleNode UseArrow { get; set; }
        [Menu("Mouse Steps", "", 5001, 500)]
        public RangeNode<int> MouseSpeed { get; set; }
        public ToggleNode Enable { get; set; }
    }
}
