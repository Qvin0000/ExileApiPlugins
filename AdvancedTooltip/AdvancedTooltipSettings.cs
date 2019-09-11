using Shared.Interfaces;
using Shared.Nodes;
using Shared.Attributes;
using Shared.Nodes;

namespace AdvancedTooltip
{
    public class AdvancedTooltipSettings : ISettings
    {
        public AdvancedTooltipSettings() {
            Enable = new ToggleNode(false);
            ItemLevel = new ItemLevelSettings();
            ItemMods = new ItemModsSettings();
            WeaponDps = new WeaponDpsSettings();
        }

        public ToggleNode Enable { get; set; }

        [Menu("Item Level", 10)]
        public ItemLevelSettings ItemLevel { get; set; }

        [Menu("Item Mods", 20)]
        public ItemModsSettings ItemMods { get; set; }

        [Menu("Weapon Dps", 30)]
        public WeaponDpsSettings WeaponDps { get; set; }
    }
}