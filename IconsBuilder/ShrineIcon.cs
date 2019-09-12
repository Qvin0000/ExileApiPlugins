using ExileCore;
using ExileCore.PoEMemory.Components;
using ExileCore.PoEMemory.MemoryObjects;
using ExileCore.Shared;
using ExileCore.Shared.Abstract;
using ExileCore.Shared.Enums;
using ExileCore.Shared.Helpers;

namespace IconsBuilder
{
    public class ShrineIcon : BaseIcon
    {
        public ShrineIcon(Entity entity, GameController gameController, IconsBuilderSettings settings) : base(entity, settings)
        {
            MainTexture = new HudTexture("Icons.png");
            MainTexture.UV = SpriteHelper.GetUV(MapIconsIndex.Shrine);
            Text = entity.GetComponent<Render>()?.Name;
            MainTexture.Size = settings.SizeShrineIcon;
            Show = () => entity.IsValid && entity.GetComponent<Shrine>().IsAvailable;
        }
    }
}
