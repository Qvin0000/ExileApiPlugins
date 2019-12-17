using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ExileCore.Shared.Nodes;

namespace TreeRoutine.Routine.BasicFlaskRoutine.Flask
{
    public class FlaskSetting
    {
        public FlaskSetting()
        {
            // Initializing this here seems to fix the deserialization issue with min/max values
            ReservedUses = new RangeNode<int>(0, 0, 5);
        }

        public FlaskSetting (ToggleNode enabled, HotkeyNode hotkey, RangeNode<int> reservedUses)
        {
            Enabled = enabled;
            Hotkey = hotkey;
            ReservedUses = reservedUses;
        }

        public HotkeyNode Hotkey { get; set; }
        public ToggleNode Enabled { get; set; }
        public RangeNode<int> ReservedUses { get; set; }

    }
}
