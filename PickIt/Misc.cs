using Basic;
using Exile.PoEMemory.MemoryObjects;
using Shared.Interfaces;
using PoEMemory;
using PoEMemory.Components;
using SharpDX;

namespace PickIt
{
    public class Misc
    {
        public static float EntityDistance(Entity entity, Entity player) {
            var component = entity?.GetComponent<Render>();
            if (component == null)
                return 9999999f;
            var objectPosition = component.Pos;

            return Vector3.Distance(objectPosition, player.GetComponent<Render>().Pos);
        }
    }
}