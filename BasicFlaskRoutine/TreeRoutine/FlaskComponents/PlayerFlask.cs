using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ExileCore.PoEMemory.Components;

namespace TreeRoutine.FlaskComponents
{
    public enum FlaskInstantType
    {
        None,
        LowLife,
        Partial,
        Full
    }

    public class PlayerFlask
    {
        public int Index { get; set; } = 0;
        public String Name { get; set; } = "None";
        public int TotalUses { get; set; } = 0;
        public String BuffString1 { get; set; } = "";
        //For Hybrid Flask as Hybrid flask have two buffs.
        public String BuffString2 { get; set; } = "";

        public FlaskActions Action1 { get; set; } = FlaskActions.None;
        public FlaskActions Action2 { get; set; } = FlaskActions.None;
        public FlaskInstantType InstantType { get; set; }
        public Boolean Instant => InstantType != FlaskInstantType.None;
        public Mods Mods { get; set; } = null;

        public override string ToString()
        {
            return $"PlayerFlask[{Index}, {Name}, {Action1}, {Action2} {InstantType}]";
        }
    }
}
