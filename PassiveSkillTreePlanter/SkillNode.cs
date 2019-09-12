using System;
using System.Collections.Generic;
using SharpDX;

namespace PassiveSkillTreePlanter
{
    public class SkillNode
    {
        public static float[] OrbitRadii = {0, 81.5f, 163, 326, 489};
        public static float[] SkillsPerOrbit = {1, 6, 12, 12, 40};
        public bool bJevel;
        public bool bKeyStone;
        public bool bMastery;
        public bool bMult;
        public bool bNotable;
        public List<Vector2> DrawNodeLinks = new List<Vector2>();
        public Vector2 DrawPosition;

        //Cached for drawing
        public float DrawSize = 100;
        public ushort Id; // "id": -28194677,
        public List<ushort> linkedNodes = new List<ushort>();
        public string Name; //"dn": "Block Recovery",
        public int Orbit; //  "o": 1,
        public int OrbitIndex; // "oidx": 3,
        public SkillNodeGroup SkillNodeGroup;

        public Vector2 Position
        {
            get
            {
                if (SkillNodeGroup == null) return new Vector2();
                double d = OrbitRadii[Orbit];
                return SkillNodeGroup.Position - new Vector2((float) (d * Math.Sin(-Arc)), (float) (d * Math.Cos(-Arc)));
            }
        }

        public double Arc => GetOrbitAngle(OrbitIndex, (int) SkillsPerOrbit[Orbit]);

        public void Init()
        {
            DrawPosition = Position;

            if (bJevel)
                DrawSize = 160;

            if (bNotable)
                DrawSize = 170;

            if (bKeyStone)
                DrawSize = 250;
        }

        private static double GetOrbitAngle(int orbitIndex, int maxNodePositions)
        {
            return 2 * Math.PI * orbitIndex / maxNodePositions;
        }
    }

    public class SkillNodeGroup
    {
        public List<SkillNode> Nodes = new List<SkillNode>();
        public Dictionary<int, bool> OcpOrb = new Dictionary<int, bool>();
        public Vector2 Position;
    }
}
