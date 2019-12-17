using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ExileCore.PoEMemory.Components;
using TreeRoutine.Routine.BuildYourOwnRoutine.Extension.Default.Actions;
using TreeRoutine.Routine.BuildYourOwnRoutine.Extension.Default.Conditions;

namespace TreeRoutine.Routine.BuildYourOwnRoutine.Extension.Default
{
    internal class DefaultExtension : Extension
    {
        public const string CacheStartedMoving = "StartedMoving";
        private Stopwatch MovingStopwatch { get; set; } = new Stopwatch();

        public const String CustomerTimerPrefix = "CustomTimerPrefix";

        public DefaultExtension()
        {
            this.Name = "Default";
        }

        public override List<ExtensionActionFactory> GetActions()
        {
            List<ExtensionActionFactory> list = new List<ExtensionActionFactory>
            {
                new UseFlaskActionFactory(Name),
                new SendKeyActionFactory(Name),
                new UseFlaskTypeActionFactory(Name),
                //new UseVaalSkillActionFactory(Name),
                new TimerActionFactory(Name)
            };

            return list;
        }

        public override List<ExtensionConditionFactory> GetConditions()
        {
            List<ExtensionConditionFactory> list = new List<ExtensionConditionFactory>
            {
                new ManaPercentConditionFactory(Name),
                new HealthPercentConditionFactory(Name),
                new EnergyShieldPercentConditionFactory(Name),
                new CanUseFlaskConditionFactory(Name),
                new InTownConditionFactory(Name),
                new InHideoutConditionFactory(Name),
                new HasCurableAilmentConditionFactory(Name),
                new PlayerMovingConditionFactory(Name),
                new LoweredResistanceConditionFactory(Name),
                new NearbyMonstersConditionFactory(Name),
                new TimerConditionFactory(Name),
                new HasFlaskBuffConditionFactory(Name),
                new PlayerHasBuffConditionFactory(Name),
                new IsKeyPressedConditionFactory(Name)
            };

            return list;
        }

        public override void UpdateCache(ExtensionParameter extensionParameter, Dictionary<string, Dictionary<string, object>> cache)
        {
            // First, get the dictionary out
            if (!cache.TryGetValue(Name, out Dictionary<string, object> myCache))
            {
                myCache = new Dictionary<string, object>();
                cache[Name] = myCache;
            }

            // Add cache values
            long elapsedMovingTime = 0;
            var player = extensionParameter.Plugin.GameController.Player.GetComponent<Actor>();
            if (player != null && player.Address != 0 && player.isMoving)
            {
                if (!MovingStopwatch.IsRunning)
                    MovingStopwatch.Start();
                elapsedMovingTime = MovingStopwatch.ElapsedMilliseconds;
            }
            else
            {
                MovingStopwatch.Reset();
            }

            myCache[CacheStartedMoving] = elapsedMovingTime;


        }

    }
}
