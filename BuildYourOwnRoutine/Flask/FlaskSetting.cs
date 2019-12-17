using ExileCore.Shared.Nodes;

namespace TreeRoutine.Routine.BuildYourOwnRoutine.Flask
{
    public class FlaskSetting
    {
        public FlaskSetting()
        {

        }

        public FlaskSetting (HotkeyNode hotkey)
        {
            Hotkey = hotkey;
        }

        public HotkeyNode Hotkey { get; set; }
    }
}
