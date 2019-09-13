using System;
using System.IO;
using System.Threading;
using ExileCore;
using TreeRoutine.DefaultBehaviors.Helpers;
using TreeRoutine.FlaskComponents;
using Newtonsoft.Json;
using TreeRoutine.TreeSharp;

namespace TreeRoutine
{
    public abstract class BaseTreeRoutinePlugin<TSettings, TCache> : BaseSettingsPlugin<TSettings>
            where TSettings : BaseTreeSettings, new()
            where TCache : BaseTreeCache, new()
    {
        public int LogmsgTime { get; set; } = 3;
        public int ErrmsgTime { get; set; } = 10;

        protected Timer Timer { get; private set; } = null;

        public TCache Cache { get; protected set; }

        // These are useful for the helper methods
        public Action<string, float> Log => LogMessage;
        public Action<string, float> LogErr => LogError;

        public FlaskHelper<TSettings, TCache> FlaskHelper { get; set; } = new FlaskHelper<TSettings, TCache>();
        public PlayerHelper<TSettings, TCache> PlayerHelper { get; set; } = new PlayerHelper<TSettings, TCache>();
        public TreeHelper<TSettings, TCache> TreeHelper { get; set; } = new TreeHelper<TSettings, TCache>();

        public TSettingType LoadSettingFile<TSettingType>(String fileName)
        {
            if (!File.Exists(fileName))
            {
                LogError("BaseTreeRoutinePlugin: Cannot find " + fileName + " file. This plugin will exit.", 10);
                return default;
            }

            return JsonConvert.DeserializeObject<TSettingType>(File.ReadAllText(fileName));
        }

        public static void SaveSettingFile<TSettingType>(String fileName, TSettingType setting)
        {
            string serialized = JsonConvert.SerializeObject(setting);

            File.WriteAllText(fileName, serialized);
        }

        public override bool Initialise()
        {
            Name = "Base Tree Routine Plugin";
            
            Cache = new TCache();

            LoadSettingsFiles();
            InitializeHelpers();

            return true;
        }

        protected virtual void LoadSettingsFiles()
        {
            var flaskFilename = $"{DirectoryFullName}/config/flaskinfo.json";
            var debufFilename = $"{DirectoryFullName}/config/debuffPanel.json";
            var flaskBuffDetailsFileName = $"{DirectoryFullName}/config/FlaskBuffDetails.json";

            Cache.FlaskInfo = LoadSettingFile<FlaskInformation>(flaskFilename);
            Cache.DebuffPanelConfig = LoadSettingFile<DebuffPanelConfig>(debufFilename);
            Cache.MiscBuffInfo = LoadSettingFile<MiscBuffInfo>(flaskBuffDetailsFileName);
        }

        protected virtual void InitializeHelpers()
        {
            FlaskHelper.Core = this;
            PlayerHelper.Core = this;
            TreeHelper.Core = this;
        }

        public void TickTree(Composite treeRoot)
        {
            try
            {
                if (!Settings.Enable)
                    return;

                if (Settings.Debug)
                    LogMessage(Name + ": Tick", LogmsgTime);

                if (treeRoot == null)
                {
                    if (Settings.Debug)
                        LogError("Plugin " + Name + " tree root function returned null. Plugin is either still initialising, or has an error.", ErrmsgTime);
                    return;
                }

                if (treeRoot.LastStatus != null)
                {
                    treeRoot.Tick(null);

                    // If the last status wasn't running, stop the tree, and restart it.
                    if (treeRoot.LastStatus != RunStatus.Running)
                    {
                        treeRoot.Stop(null);

                        UpdateCache();
                        treeRoot.Start(null);
                    }
                }
                else
                {
                    UpdateCache();
                    treeRoot.Start(null);
                    RunStatus status = treeRoot.Tick(null);
                }
            }
            catch (Exception e)
            {
                LogError(Name + ": Exception! Printscreen this and post it.\n" + e.Message + "\n" + e.StackTrace, 30);
                throw e;
            }
        }

        protected virtual void UpdateCache()
        {

        }

        

        public override void AreaChange(AreaInstance area)
        {
            if (Settings.Debug)
                LogMessage(Name + ": Area has been changed.", LogmsgTime);

            if (Settings.Enable.Value && area != null)
            {
                Cache.InHideout = area.IsHideout;
                Cache.InTown = area.IsTown;
            }
        }
    }
}

