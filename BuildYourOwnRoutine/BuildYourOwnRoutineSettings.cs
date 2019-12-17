using TreeRoutine.Routine.BuildYourOwnRoutine.Profile;
using System.Windows.Forms;
using ExileCore.Shared.Nodes;
using TreeRoutine.Routine.BuildYourOwnRoutine.Flask;
using ImGuiVector2 = System.Numerics.Vector2;
using ImGuiVector4 = System.Numerics.Vector4;

namespace TreeRoutine.Routine.BuildYourOwnRoutine
{
    public class BuildYourOwnRoutineSettings : BaseTreeSettings
    {
        public BuildYourOwnRoutineSettings() : base()
        {
            //var centerPos = BasePlugin.API.GameController.Window.GetWindowRectangle().Center;
            //LastSettingSize = new ImGuiVector2(620, 376);
            //LastSettingPos = new ImGuiVector2(centerPos.X - LastSettingSize.X / 2, centerPos.Y - LastSettingSize.Y / 2);
            ShowSettings = new ToggleNode(false);
            FlaskSettings = new FlaskSetting[5]
                                {
                                    new FlaskSetting(new HotkeyNode(Keys.D1)),
                                    new FlaskSetting(new HotkeyNode(Keys.D2)),
                                    new FlaskSetting(new HotkeyNode(Keys.D3)),
                                    new FlaskSetting(new HotkeyNode(Keys.D4)),
                                    new FlaskSetting(new HotkeyNode(Keys.D5))
                                };
            TicksPerSecond = new RangeNode<int>(10, 1, 30);
        }

        public FlaskSetting[] FlaskSettings { get; set; }

        public RangeNode<int> TicksPerSecond { get; set; } 

        public LoadedProfile LoadedProfile { get; set; }

        public ImGuiVector2 LastSettingPos { get; set; }
        public ImGuiVector2 LastSettingSize { get; set; }

        public ToggleNode ShowSettings { get; set; }
    }
}