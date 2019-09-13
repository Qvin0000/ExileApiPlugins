using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TreeRoutine.FlaskComponents
{
    public class MiscBuffInfo
    {
        public Dictionary<string, string> flaskNameToBuffConversion = new Dictionary<string, string>();
        // For Hybrid Flask as Hybrid Flask have two buffs.
        public Dictionary<string, string> flaskNameToBuffConversion2 = new Dictionary<string, string>();
    }
}
