using System.IO;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PassiveSkillTreePlanter
{
    //Many thanks to https://github.com/EmmittJ/PoESkillTree

    public class PassiveSkillTreeJson_Downloader
    {
        private const string TreeAddress = "https://www.pathofexile.com/passive-skill-tree/";

        public static async Task<string> DownloadSkillTreeToFileAsync(string filePath)
        {
            var code = await new HttpClient().GetStringAsync(TreeAddress);
            var regex = new Regex("var passiveSkillTreeData.*");
            var skillTreeObj = regex.Match(code).Value.Replace("\\/", "/");
            skillTreeObj = skillTreeObj.Substring(27, skillTreeObj.Length - 27 - 1) + "";
            File.WriteAllText(filePath, skillTreeObj);
            return skillTreeObj;
        }
    }
}