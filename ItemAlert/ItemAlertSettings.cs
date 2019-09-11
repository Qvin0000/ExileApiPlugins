using Shared.Interfaces;
using Shared.Nodes;

namespace ItemAlert
{
    public class ItemAlertSettings : ISettings
    {
        public ToggleNode Enable { get; set; } = new ToggleNode(true);
        public ToggleNode Alternative { get; set; } = new ToggleNode(true);
        public ToggleNode ShowItemOnMap { get; set; } = new ToggleNode(true);
        public FileNode FilePath { get; set; } = "config/my.filter";
        public ToggleNode WithBorder { get; set; } = new ToggleNode(true);
        public ToggleNode WithSound { get; set; } = new ToggleNode(true);

        public RangeNode<int> LootIcon { get; set; } = new RangeNode<int>(7, 1, 50);
        public ToggleNode LootIconBorderColor { get; set; } = new ToggleNode(false);
        public RangeNode<int> ItemAlertUpdateTime { get; set; } = new RangeNode<int>(50, 10, 1000);
        public RangeNode<int> ItemAlertLimitWork { get; set; } = new RangeNode<int>(250, 10, 1000);
        public ToggleNode ShowText { get; set; } = new ToggleNode(true);
        public ToggleNode HideOthers { get; set; } = new ToggleNode(true);
        public RangeNode<int> TextSize { get; set; } = new RangeNode<int>(16, 10, 50);
        public ToggleNode DimOtherByPercentToggle { get; set; } = new ToggleNode(true);
        public ToggleNode BorderSettings { get; set; } = new ToggleNode(true);
        public ToggleNode UseDiamond { get; set; } = new ToggleNode(true);
        public ToggleNode MultiThreading { get; set; }= new ToggleNode(false);
        public RangeNode<int> MultiThreadingWhenItemMoreThan { get; set; } = new RangeNode<int>(30,5,500);

        public RangeNode<int> DimOtherByPercent = new RangeNode<int>(100, 1, 100);
    }
}