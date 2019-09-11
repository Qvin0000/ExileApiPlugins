using Basic;
using Exile;
using Exile.PoEMemory.MemoryObjects;
using Shared.Abstract;
using Shared;
using Shared.Helpers;
using Shared.Interfaces;
using PoEMemory.Components;
using Shared;
using Shared.Enums;
using SharpDX;

namespace IconsBuilder
{
    public class ShrineIcon : BaseIcon
    {
        public ShrineIcon(Entity entity, GameController gameController, IconsBuilderSettings settings) : base(entity, settings) {
            MainTexture = new HudTexture("Icons.png");
            MainTexture.UV = SpriteHelper.GetUV(MapIconsIndex.Shrine);
            Text = entity.GetComponent<Render>()?.Name;
            MainTexture.Size = settings.SizeShrineIcon;
            Show = () => entity.IsValid && entity.GetComponent<Shrine>().IsAvailable;
        }
    }
}