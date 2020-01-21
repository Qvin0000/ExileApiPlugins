using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using ExileCore;

namespace PassiveSkillTreePlanter.UrlDecoders
{
    public class PoePlannerUrlDecoder
    {
        //Many thanks to https://github.com/EmmittJ/PoESkillTree
        private static readonly Regex UrlRegex = new Regex(@"(http(|s):\/\/|)(\w*\.|)poeplanner\.com\/(?<build>[\w-=]+)");

        public static bool UrlMatch(string buildUrl)
        {
            return UrlRegex.IsMatch(buildUrl);
        }

        public static List<ushort> Decode(string url)
        {
            var nodesId = new List<ushort>();

            var buildSegment = url.Split('/').LastOrDefault();

            if (buildSegment == null)
            {
                Logger.Log.Error("Can't decode PoePlanner Url", 5);
                return new List<ushort>();
            }

            buildSegment = buildSegment
                .Replace("-", "+")
                .Replace("_", "/");

            var rawBytes = Convert.FromBase64String(buildSegment);

            var skillsBuffSize = (rawBytes[3] << 8) | rawBytes[4];

            //var aurasBuffSize = rawBytes[5 + skillsBuffSize] << 8 | rawBytes[6 + skillsBuffSize];
            //var equipBuffSize = rawBytes[7 + skillsBuffSize + aurasBuffSize] << 8 | rawBytes[8 + skillsBuffSize + aurasBuffSize];

            var NodesData = new byte[skillsBuffSize];
            Array.Copy(rawBytes, 5, NodesData, 0, skillsBuffSize);

            //var skilledNodesCount = NodesData[2] << 8 | NodesData[3];

            //int i = 4;
            try
            {
                for (var i = 4; i < skillsBuffSize - 1; i += 2)
                {
                    nodesId.Add((ushort)((NodesData[i] << 8) | NodesData[i + 1]));
                }

                /*
            while (i < skillsBuffSize + 3)
            {
                nodesId.Add((ushort)(NodesData[i++] << 8 | NodesData[i++]));
            }
            */
            }
            catch
            {
                Logger.Log.Error("Error while parsing some PoePlanner nodes from Url.", 5);
            }

            return nodesId;
        }
    }
}
