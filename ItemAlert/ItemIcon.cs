using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Exile;
using Exile.PoEMemory.MemoryObjects;
using Shared.Abstract;
using Shared;
using Shared.Enums;

namespace ItemAlert
{
    internal class ItemIcon : BaseIcon
    {
        public ItemIcon(Entity entity, HudTexture hudTexture, Func<bool> func, GameController gameController, ItemAlertSettings settings) :
            base(entity, settings) {
            MainTexture = hudTexture;
            Show = () => func() && entity.IsValid;
            MainTexture.Size = settings.LootIcon;
            Priority = IconPriority.Medium;
        }
    }
}