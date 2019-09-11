using System.Collections.Generic;
using ExileCore;
using ExileCore.PoEMemory.Components;
using ExileCore.PoEMemory.MemoryObjects;
using ExileCore.Shared;
using ExileCore.Shared.Abstract;
using ExileCore.Shared.Enums;
using ExileCore.Shared.Helpers;
using SharpDX;

namespace IconsBuilder
{
    public class PlayerIcon : BaseIcon
    {
        public PlayerIcon(Entity entity, GameController gameController, IconsBuilderSettings settings, Dictionary<string, Size2> modIcons) :
            base(entity, settings)
        {
            Show = () => entity.IsValid && !settings.HidePlayers;
            if (_HasIngameIcon) return;
            MainTexture = new HudTexture("Icons.png") {UV = SpriteHelper.GetUV(MapIconsIndex.OtherPlayer)};
            Text = entity.GetComponent<Player>().PlayerName;
        }
    }
}
