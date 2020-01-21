using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using static PassiveSkillTreePlanter.PassiveSkillTreePlanter;

namespace PassiveSkillTreePlanter
{
    public class TreeConfig
    {
        public static List<string> GetBuilds()
        {
            var dirInfo = new DirectoryInfo(Core.SkillTreeUrlFilesDir);
            var files = dirInfo.GetFiles("*.json").Select(x => Path.GetFileNameWithoutExtension(x.Name)).ToList();
            return files;
        }

        public static SkillTreeData LoadBuild(string buildName)
        {
            var buildFile = $"{Core.SkillTreeUrlFilesDir}\\{buildName}.json";
            return LoadSettingFile<SkillTreeData>(buildFile);
        }

        public static TSettingType LoadSettingFile<TSettingType>(string fileName)
        {
            if (!File.Exists(fileName))
                return default(TSettingType);

            return JsonConvert.DeserializeObject<TSettingType>(File.ReadAllText(fileName));
        }

        public static void SaveSettingFile<TSettingType>(string fileName, TSettingType setting)
        {
            var buildFile = $"{fileName}.json";
            var serialized = JsonConvert.SerializeObject(setting, Formatting.Indented);

            File.WriteAllText(buildFile, serialized);
        }

        public class Tree
        {
            public int Level { get; set; } = 0;
            public string Tag { get; set; } = "";
            public string SkillTreeUrl { get; set; } = "";
        }

        public class SkillTreeData
        {
            public string Notes { get; set; } = "";
            public List<Tree> Trees { get; set; } = new List<Tree>();
            public string BuildLink { get; set; } = "";
            public int SelectedIndex { get; set; } = 0;
        }
    }
}