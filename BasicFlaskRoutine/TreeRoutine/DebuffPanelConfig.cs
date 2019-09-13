using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TreeRoutine
{
    public class DebuffPanelConfig
    {
        public Dictionary<string, int> Bleeding { get; set; }
        public Dictionary<string, int> Corruption { get; set; }
        public Dictionary<string, int> Poisoned { get; set; }
        public Dictionary<string, int> Frozen { get; set; }
        public Dictionary<string, int> Chilled { get; set; }
        public Dictionary<string, int> Burning { get; set; }
        public Dictionary<string, int> Shocked { get; set; }
        public Dictionary<string, int> WeakenedSlowed { get; set; }
    }
}
