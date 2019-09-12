using System;
using ExileCore;
using ExileCore.PoEMemory.MemoryObjects;
using ExileCore.Shared;
using ExileCore.Shared.Abstract;
using ExileCore.Shared.Enums;

namespace ItemAlert
{
    internal class ItemIcon : BaseIcon
    {
        public ItemIcon(Entity entity, HudTexture hudTexture, Func<bool> func, GameController gameController, ItemAlertSettings settings) :
            base(entity, settings)
        {
            MainTexture = hudTexture;
            Show = () => func() && entity.IsValid;
            MainTexture.Size = settings.LootIcon;
            Priority = IconPriority.Medium;
        }
    }
}
