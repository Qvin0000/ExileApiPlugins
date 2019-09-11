using Shared.Interfaces;
using Shared.Nodes;
using Shared.Nodes;
using SharpDX;

namespace AdvancedTooltip
{
    public class WeaponDpsSettings : ISettings
    {
        public WeaponDpsSettings() {
            Enable = new ToggleNode(true);
            TextColor = new ColorBGRA(254, 192, 118, 255);
            DpsTextSize = new RangeNode<int>(16, 10, 50);
            DpsNameTextSize = new RangeNode<int>(13, 10, 50);
            BackgroundColor = new ColorBGRA(255, 255, 0, 255);
            DmgColdColor = new ColorBGRA(54, 100, 146, 255);
            DmgFireColor = new ColorBGRA(150, 0, 0, 255);
            DmgLightningColor = new ColorBGRA(255, 215, 0, 255);
            DmgChaosColor = new ColorBGRA(208, 31, 144, 255);
            pDamageColor = new ColorBGRA(255, 255, 255, 255);
            eDamageColor = new ColorBGRA(0, 255, 255, 255);
        }

        public ToggleNode Enable { get; set; }
        public ColorNode TextColor { get; set; }
        public RangeNode<int> DpsTextSize { get; set; }
        public RangeNode<int> DpsNameTextSize { get; set; }
        public ColorNode BackgroundColor { get; set; }
        public ColorNode DmgFireColor { get; set; }
        public ColorNode DmgColdColor { get; set; }
        public ColorNode DmgLightningColor { get; set; }
        public ColorNode DmgChaosColor { get; set; }
        public ColorNode pDamageColor { get; set; }
        public ColorNode eDamageColor { get; set; }
    }
}