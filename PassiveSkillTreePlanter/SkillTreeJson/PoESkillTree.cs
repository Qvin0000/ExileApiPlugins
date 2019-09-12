using System.Collections.Generic;
using Newtonsoft.Json;

namespace PassiveSkillTreePlanter
{
    //Many thanks to https://github.com/EmmittJ/PoESkillTree
    public class Character
    {
        public int base_str { get; set; }
        public int base_dex { get; set; }
        public int base_int { get; set; }
    }

    public class NodeGroup
    {
        public double x { get; set; }
        public double y { get; set; }
        public Dictionary<int, bool> oo { get; set; }
        public List<ushort> n { get; set; }
    }

    public class Main
    {
        public ushort id { get; set; }
        public string icon { get; set; }
        public bool ks { get; set; }
        public bool not { get; set; }
        public string dn { get; set; }
        public bool m { get; set; }
        public int[] spc { get; set; }
        public string[] sd { get; set; }
        public int g { get; set; }
        public int o { get; set; }
        public int oidx { get; set; }
        public int sa { get; set; }
        public int da { get; set; }
        public int ia { get; set; }
        [JsonProperty("out")]
        public List<int> ot { get; set; }
    }

    public class Node
    {
        public ushort id { get; set; }
        public string icon { get; set; }
        public bool ks { get; set; }
        public bool not { get; set; }
        public string dn { get; set; }
        public bool m { get; set; }
        public bool isJewelSocket { get; set; }
        public bool isMultipleChoice { get; set; }
        public bool isMultipleChoiceOption { get; set; }
        public int passivePointsGranted { get; set; }
        public string ascendancyName { get; set; }
        public bool isAscendancyStart { get; set; }
        public int[] spc { get; set; }
        public string[] sd { get; set; }
        public string[] reminderText { get; set; }
        public int g { get; set; }
        public int o { get; set; }
        public int oidx { get; set; }
        public int sa { get; set; }
        public int da { get; set; }
        public int ia { get; set; }
        [JsonProperty("out")]
        public List<ushort> _out { get; set; }
        [JsonProperty("in")]
        public List<ushort> _in { get; set; }
    }

    public class JsonConstants
    {
        public Dictionary<string, int> classes { get; set; }
        public Dictionary<string, int> characterAttributes { get; set; }
        public int PSSCentreInnerRadius { get; set; }
    }

    public class Art2D
    {
        public int x { get; set; }
        public int y { get; set; }
        public int w { get; set; }
        public int h { get; set; }
    }

    public class SkillSprite
    {
        public string filename { get; set; }
        public Dictionary<string, Art2D> coords { get; set; }
    }

    public class PoESkillTree
    {
        public Dictionary<int, Character> characterData { get; set; }
        public Dictionary<int, NodeGroup> groups { get; set; }
        public Main root { get; set; }
        public Main main { get; set; }
        public Dictionary<int, Node> nodes { get; set; }
        public Dictionary<string, Node> nodesDict { get; set; }
        public int min_x { get; set; }
        public int min_y { get; set; }
        public int max_x { get; set; }
        public int max_y { get; set; }
        public Dictionary<string, Dictionary<string, string>> assets { get; set; }
        public JsonConstants constants { get; set; }
        public string imageRoot { get; set; }
        public Dictionary<string, SkillSprite[]> skillSprites { get; set; }
        public double[] imageZoomLevels { get; set; }
    }

    public class Opts
    {
        public Dictionary<int, baseToAscClass> ascClasses { get; set; }
    }

    public class baseToAscClass
    {
        public string name { get; set; }
        public Dictionary<int, classes> classes { get; set; }
    }

    public class classes
    {
        [JsonProperty("name")]
        public string name { get; set; }
        [JsonProperty("displayName")]
        public string displayName { get; set; }
        [JsonProperty("flavourText")]
        public string flavourText { get; set; }
        [JsonProperty("flavourTextRect")]
        public string flavourTextRect { get; set; }
        [JsonProperty("flavourTextColour")]
        public string flavourTextColour { get; set; }
    }
}
