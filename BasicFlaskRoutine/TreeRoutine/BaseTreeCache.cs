using TreeRoutine.FlaskComponents;
using System;

namespace TreeRoutine
{
    public class BaseTreeCache
    {
        public FlaskInformation FlaskInfo { get; set; } = null;

        public DebuffPanelConfig DebuffPanelConfig { get; set; } = null;

        public MiscBuffInfo MiscBuffInfo { get; set; } = null;

        public Boolean InTown { get; set; } = false;

        public Boolean InHideout { get; set; } = false;

    }
}
