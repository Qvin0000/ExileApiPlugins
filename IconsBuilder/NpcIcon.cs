using Basic;
using Exile;
using Exile.PoEMemory.MemoryObjects;
using Shared.Abstract;
using Shared;
using Shared.Helpers;
using Shared.Interfaces;
using PoEMemory.Components;
using Shared.Enums;
using SharpDX;

namespace IconsBuilder
{
    public class NpcIcon : BaseIcon
    {
        public NpcIcon(Entity entity, GameController gameController, IconsBuilderSettings settings) : base(entity, settings) {
            if (!_HasIngameIcon) MainTexture = new HudTexture("Icons.png");

            MainTexture.Size = settings.SizeNpcIcon;
            var component = entity.GetComponent<Render>();
            Text = component?.Name.Split(',')[0];
            Show = () => entity.IsValid;
            if (_HasIngameIcon) return;
            if (entity.Path.StartsWith("Metadata/NPC/League/Cadiro"))
                MainTexture.UV = SpriteHelper.GetUV(MapIconsIndex.QuestObject);
            else if (entity.Path.StartsWith("Metadata/Monsters/LeagueBetrayal/MasterNinjaCop"))
                MainTexture.UV = SpriteHelper.GetUV(MapIconsIndex.BetrayalSymbolDjinn);
            else
                MainTexture.UV = SpriteHelper.GetUV(MapIconsIndex.NPC);
        }
    }
}