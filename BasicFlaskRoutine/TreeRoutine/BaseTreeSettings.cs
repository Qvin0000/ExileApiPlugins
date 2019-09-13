using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ExileCore.Shared.Attributes;
using ExileCore.Shared.Interfaces;
using ExileCore.Shared.Nodes;


namespace TreeRoutine
{
    public class BaseTreeSettings : ISettings
    {

        public BaseTreeSettings()
        {
            Enable = new ToggleNode(false);
            Debug = new ToggleNode(false);
        }

        [Menu("Enable")]
        public ToggleNode Enable { get; set; } 

        [Menu("Debug")]
        public ToggleNode Debug { get; set; }
    }
}
