using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace PassiveSkillTreePlanter.UrlDecoders
{
    public class PathOfExileUrlDecoder
    {
        //Many thanks to https://github.com/EmmittJ/PoESkillTree
        private static readonly Regex UrlRegex =
            new Regex(@"(http(|s):\/\/|)(\w*\.|)pathofexile\.com\/(fullscreen-|)passive-skill-tree\/(?<build>[\w-=]+)");

        public static bool UrlMatch(string buildUrl)
        {
            return UrlRegex.IsMatch(buildUrl);
        }

        public static List<ushort> Decode(string url)
        {
            var nodesId = new List<ushort>();

            var textToDecode = url.Substring(url.IndexOf("tree/") + 5).Replace("-", "+").Replace("_", "/");

            var data = Convert.FromBase64String(textToDecode);

            var Version = (data[0] << 24) | (data[1] << 16) | (data[2] << 8) | data[3];
            var Class = data[4];
            var aClass = data[5];

            for (var k = Version > 3 ? 7 : 6; k < data.Length; k += 2)
            {
                var nodeId = (ushort) ((data[k] << 8) | data[k + 1]);
                nodesId.Add(nodeId);
            }

            return nodesId;
        }
    }
}
