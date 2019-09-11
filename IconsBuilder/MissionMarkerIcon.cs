using ExileCore;
using ExileCore.PoEMemory.Components;
using ExileCore.PoEMemory.MemoryObjects;
using ExileCore.Shared;
using ExileCore.Shared.Abstract;
using ExileCore.Shared.Helpers;
using SharpDX;

namespace IconsBuilder
{
    public class MissionMarkerIcon : BaseIcon
    {
        public MissionMarkerIcon(Entity entity, GameController gameController, IconsBuilderSettings settings) : base(entity, settings)
        {
            MainTexture = new HudTexture();
            MainTexture.FileName = "Icons.png";
            MainTexture.UV = SpriteHelper.GetUV(16, new Size2F(14, 14));

            Show = () =>
            {
                var switchState = entity.GetComponent<Transitionable>() != null
                    ? entity.GetComponent<Transitionable>().Flag1
                    : (byte?) null;

                var isTargetable = entity.IsTargetable;
                return switchState == 1 || isTargetable;
            };

            MainTexture.Size = settings.SizeMiscIcon;
        }
    }
}
