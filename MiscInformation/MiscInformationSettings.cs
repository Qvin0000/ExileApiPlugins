using ExileCore.Shared.Interfaces;
using ExileCore.Shared.Nodes;
using SharpDX;

namespace MiscInformation
{
    public class MiscInformationSettings : ISettings
    {
        public MiscInformationSettings()
        {
            BackgroundColor = new ColorBGRA(0, 0, 0, 255);
            AreaTextColor = new ColorBGRA(140, 200, 255, 255);
            XphTextColor = new ColorBGRA(220, 190, 130, 255);
            XphGetLeft = new ColorBGRA(220, 190, 130, 255);
            TimeLeftColor = new ColorBGRA(220, 190, 130, 255);
            FpsTextColor = new ColorBGRA(220, 190, 130, 255);
            TimerTextColor = new ColorBGRA(220, 190, 130, 255);
            LatencyTextColor = new ColorBGRA(220, 190, 130, 255);
        }

        public ToggleNode Enable { get; set; } = new ToggleNode(true);
        public ColorNode BackgroundColor { get; set; }
        public ColorNode AreaTextColor { get; set; }
        public ColorNode XphTextColor { get; set; }
        public ColorNode XphGetLeft { get; set; }
        public ColorNode TimeLeftColor { get; set; }
        public ColorNode FpsTextColor { get; set; }
        public ColorNode TimerTextColor { get; set; }
        public ColorNode LatencyTextColor { get; set; }
    }
}
