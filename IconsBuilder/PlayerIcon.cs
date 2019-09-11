using System.Collections.Generic;
using Exile;
using Exile.PoEMemory.MemoryObjects;
using PoEMemory.Components;
using Shared;
using Shared.Abstract;
using Shared.Enums;
using Shared.Helpers;
using SharpDX;

namespace IconsBuilder
{
    public class PlayerIcon : BaseIcon
    {
        public PlayerIcon(Entity entity, GameController gameController, IconsBuilderSettings settings, Dictionary<string, Size2> modIcons) :
            base(entity, settings) {
            Show = () => entity.IsValid && !settings.HidePlayers;
            if (_HasIngameIcon) return;
            MainTexture = new HudTexture("Icons.png") {UV = SpriteHelper.GetUV(MapIconsIndex.OtherPlayer)};
            Text = entity.GetComponent<Player>().PlayerName;
        }
    }
}