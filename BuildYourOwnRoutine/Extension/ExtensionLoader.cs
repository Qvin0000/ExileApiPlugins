using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace TreeRoutine.Routine.BuildYourOwnRoutine.Extension
{
    internal class ExtensionLoader
    {
        public static void LoadAllExtensions(ExtensionCache cache, string directory)
        {
            string[] extensions = Directory.GetFiles(directory, "*.dll");
            foreach(var extension in extensions)
            {
                LoadExtension(cache, extension);
            }
        }

        private static void LoadExtension(ExtensionCache cache, string path)
        {
            var myAsm = Assembly.LoadFrom(path);
            if (myAsm == null) return;

            var asmTypes = myAsm.GetTypes();
            if (asmTypes.Length == 0) return;

            foreach (var type in asmTypes)
            {
                if (type.IsSubclassOf(typeof(Extension)) && !type.IsAbstract)
                {
                    var extension = (Extension)Activator.CreateInstance(type);

                    cache.LoadedExtensions.Add(extension);
                }
            }
        }
    }
}
