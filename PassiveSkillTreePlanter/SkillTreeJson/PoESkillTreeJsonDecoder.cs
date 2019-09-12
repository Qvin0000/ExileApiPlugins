using System.Collections.Generic;
using ExileCore;
using Newtonsoft.Json;
using SharpDX;

namespace PassiveSkillTreePlanter.SkillTreeJson
{
    //Many thanks to https://github.com/EmmittJ/PoESkillTree

    public class PoESkillTreeJsonDecoder
    {
        public List<SkillNodeGroup> NodeGroups;
        public List<SkillNode> Nodes;
        public Dictionary<ushort, SkillNode> Skillnodes;
        public PoESkillTree SkillTree;

        public void Decode(string jsonTree)
        {
            Nodes = new List<SkillNode>();

            var jss = new JsonSerializerSettings
            {
                Error = (sender, args) =>
                {
                    // This one is known: "509":{"x":_,"y":_,"oo":[],"n":[]}} has an Array in "oo".
                    // if (args.ErrorContext.Path != "groups.509.oo")
                    // PoeHUD.Plugins.BasePlugin.LogError("Exception while deserializing Json tree" + args.ErrorContext.Error, 5);
                    if (args.ErrorContext.Path == null || !args.ErrorContext.Path.EndsWith(".oo"))
                        Logger.Log.Error("Exception while deserializing Json tree" + args.ErrorContext.Error);

                    args.ErrorContext.Handled = true;
                }
            };

            SkillTree = JsonConvert.DeserializeObject<PoESkillTree>(jsonTree, jss);
            Skillnodes = new Dictionary<ushort, SkillNode>();
            NodeGroups = new List<SkillNodeGroup>();

            foreach (var nd in SkillTree.nodes)
            {
                var skillNode = new SkillNode
                {
                    Id = nd.Value.id, Name = nd.Value.dn, Orbit = nd.Value.o, OrbitIndex = nd.Value.oidx, bJevel = nd.Value.isJewelSocket,
                    bKeyStone = nd.Value.ks, bMastery = nd.Value.m, bMult = nd.Value.isMultipleChoice, bNotable = nd.Value.not,
                    linkedNodes = nd.Value._out
                };

                Nodes.Add(skillNode);
                Skillnodes.Add(nd.Value.id, skillNode);
            }

            NodeGroups = new List<SkillNodeGroup>();

            foreach (var gp in SkillTree.groups)
            {
                var ng = new SkillNodeGroup();
                ng.OcpOrb = gp.Value.oo;
                ng.Position = new Vector2((float) gp.Value.x, (float) gp.Value.y);

                foreach (var node in gp.Value.n)
                {
                    var nodeToAdd = Skillnodes[node];
                    ng.Nodes.Add(nodeToAdd);
                    nodeToAdd.SkillNodeGroup = ng;
                }

                NodeGroups.Add(ng);
            }
        }
    }
}
