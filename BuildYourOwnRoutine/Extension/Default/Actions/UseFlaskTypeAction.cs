using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TreeRoutine.DefaultBehaviors.Actions;
using TreeRoutine.FlaskComponents;
using TreeRoutine.Menu;
using TreeRoutine.TreeSharp;

namespace TreeRoutine.Routine.BuildYourOwnRoutine.Extension.Default.Actions
{
    internal class UseFlaskTypeAction : ExtensionAction
    {
        private bool useLife { get; set; } = false;
        private const String useLifeString = "useLife";

        private bool useMana { get; set; } = false;
        private const String useManaString = "useMana";

        private bool useHybrid { get; set; } = false;
        private const String useHybridString = "useHybrid";

        private bool useInstant { get; set; } = false;
        private const String useInstantString = "useInstant";

        private bool useDefense { get; set; } = false;
        private const String useDefenseString = "useDefense";

        private bool useUtility { get; set; } = false;
        private const String useUtilityString = "useUtility";

        private bool useSpeedrun { get; set; } = false;
        private const String useSpeedrunString = "useSpeedrun";

        private bool useOffense { get; set; } = false;
        private const String useOffenseString = "useOffense";

        private bool usePoison { get; set; } = false;
        private const String usePoisonString = "usePoison";

        private bool useFreeze { get; set; } = false;
        private const String useFreezeString = "useFreeze";

        private bool useIgnite { get; set; } = false;
        private const String useIgniteString = "useIgnite";

        private bool useShock { get; set; } = false;
        private const String useShockString = "useShock";

        private bool useBleed { get; set; } = false;
        private const String useBleedString = "useBleed";

        private bool useCurse { get; set; } = false;
        private const String useCurseString = "useCurse";

        private bool useUnique { get; set; } = false;
        private const String useUniqueString = "useUnique";

        private bool useOffenseAndSpeedrun { get; set; } = false;
        private const String useOffenseAndSpeedrunString = "useOffenseAndSpeedrun";

        private int reserveFlaskCharges { get; set; } = 0;
        private const String reserveFlaskChargesString = "reserveFlaskCharges";


        public UseFlaskTypeAction(string owner, string name) : base(owner, name)
        {

        }

        public override void Initialise(Dictionary<String, Object> Parameters)
        {
            useLife = ExtensionComponent.InitialiseParameterBoolean(useLifeString, useLife, ref Parameters);
            useMana = ExtensionComponent.InitialiseParameterBoolean(useManaString, useMana, ref Parameters);
            useHybrid = ExtensionComponent.InitialiseParameterBoolean(useHybridString, useHybrid, ref Parameters);
            useInstant = ExtensionComponent.InitialiseParameterBoolean(useInstantString, useInstant, ref Parameters);
            useDefense = ExtensionComponent.InitialiseParameterBoolean(useDefenseString, useDefense, ref Parameters);
            useUtility = ExtensionComponent.InitialiseParameterBoolean(useUtilityString, useUtility, ref Parameters);
            useSpeedrun = ExtensionComponent.InitialiseParameterBoolean(useSpeedrunString, useSpeedrun, ref Parameters);
            useOffense = ExtensionComponent.InitialiseParameterBoolean(useOffenseString, useOffense, ref Parameters);
            usePoison = ExtensionComponent.InitialiseParameterBoolean(usePoisonString, usePoison, ref Parameters);
            useFreeze = ExtensionComponent.InitialiseParameterBoolean(useFreezeString, useFreeze, ref Parameters);
            useIgnite = ExtensionComponent.InitialiseParameterBoolean(useIgniteString, useIgnite, ref Parameters);
            useShock = ExtensionComponent.InitialiseParameterBoolean(useShockString, useShock, ref Parameters);
            useBleed = ExtensionComponent.InitialiseParameterBoolean(useBleedString, useBleed, ref Parameters);
            useCurse = ExtensionComponent.InitialiseParameterBoolean(useCurseString, useCurse, ref Parameters);
            useUnique = ExtensionComponent.InitialiseParameterBoolean(useUniqueString, useUnique, ref Parameters);
            useOffenseAndSpeedrun = ExtensionComponent.InitialiseParameterBoolean(useOffenseAndSpeedrunString, useOffenseAndSpeedrun, ref Parameters);
            reserveFlaskCharges = ExtensionComponent.InitialiseParameterInt32(reserveFlaskChargesString, reserveFlaskCharges, ref Parameters);
        }

        public override bool CreateConfigurationMenu(ExtensionParameter extensionParameter, ref Dictionary<String, Object> Parameters)
        {
            ImGui.TextDisabled("Action Info");
            ImGuiExtension.ToolTipWithText("(?)", "This action is used to use a specific type(s) of flask.\nFlask Hotkey will be pulled from plugin settings.\nFlask types will be pulled from the file /config/flaskinfo.json");

            useLife = ImGuiExtension.Checkbox("Life", useLife);
            Parameters[useLifeString] = useLife.ToString();

            ImGui.SameLine();
            useMana = ImGuiExtension.Checkbox("Mana", useMana);
            Parameters[useManaString] = useMana.ToString();

            ImGui.SameLine();
            useHybrid = ImGuiExtension.Checkbox("Hybrid", useHybrid);
            Parameters[useHybridString] = useHybrid.ToString();

            useInstant = ImGuiExtension.Checkbox("Use Instant", useInstant);
            ImGuiExtension.ToolTipWithText("(?)", "This only makes sense to use with life/mana/hybrid flasks");

            Parameters[useInstantString] = useInstant.ToString();

            ImGui.Spacing();
            ImGui.Separator();
            ImGui.Spacing();

            useDefense = ImGuiExtension.Checkbox("Defense", useDefense);
            Parameters[useDefenseString] = useDefense.ToString();


            ImGui.SameLine();
            useOffense = ImGuiExtension.Checkbox("Offense", useOffense);
            Parameters[useOffenseString] = useOffense.ToString();

            useUtility = ImGuiExtension.Checkbox("Utility", useUtility);
            Parameters[useUtilityString] = useUtility.ToString();

            ImGui.SameLine();
            useSpeedrun = ImGuiExtension.Checkbox("Speedrun", useSpeedrun);
            Parameters[useSpeedrunString] = useSpeedrun.ToString();

            useUnique = ImGuiExtension.Checkbox("Unique", useUnique);
            Parameters[useUniqueString] = useUnique.ToString();

            useOffenseAndSpeedrun = ImGuiExtension.Checkbox("Offense and Speedrun", useOffenseAndSpeedrun);
            Parameters[useOffenseAndSpeedrunString] = useOffenseAndSpeedrun.ToString();

            ImGui.Spacing();
            ImGui.Separator();
            ImGui.Spacing();

            usePoison = ImGuiExtension.Checkbox("Poison", usePoison);
            Parameters[usePoisonString] = usePoison.ToString();

            ImGui.SameLine();
            useFreeze = ImGuiExtension.Checkbox("Freeze", useFreeze);
            Parameters[useFreezeString] = useFreeze.ToString();

            useIgnite = ImGuiExtension.Checkbox("Ignite", useIgnite);
            Parameters[useIgniteString] = useIgnite.ToString();

            ImGui.SameLine();
            useShock = ImGuiExtension.Checkbox("Shock", useShock);
            Parameters[useShockString] = useShock.ToString();

            useBleed = ImGuiExtension.Checkbox("Bleed", useBleed);
            Parameters[useBleedString] = useBleed.ToString();

            ImGui.SameLine();
            useCurse = ImGuiExtension.Checkbox("Curse", useCurse);
            Parameters[useCurseString] = useCurse.ToString();

            ImGui.Spacing();
            ImGui.Separator();
            ImGui.Spacing();

            reserveFlaskCharges = ImGuiExtension.IntSlider("Reserved Charges", reserveFlaskCharges, 0, 5);
            Parameters[reserveFlaskChargesString] = reserveFlaskCharges.ToString();

            return true;
        }

        public override Composite GetComposite(ExtensionParameter profileParameter)
        {
            List<FlaskActions> actions = new List<FlaskActions>();
            if (useLife) actions.Add(FlaskActions.Life);
            if (useMana) actions.Add(FlaskActions.Mana);
            if (useHybrid) actions.Add(FlaskActions.Hybrid);
            if (useDefense) actions.Add(FlaskActions.Defense);
            if (useUtility) actions.Add(FlaskActions.Utility);
            if (useSpeedrun) actions.Add(FlaskActions.Speedrun);
            if (useOffense) actions.Add(FlaskActions.Offense);
            if (usePoison) actions.Add(FlaskActions.PoisonImmune);
            if (useFreeze) actions.Add(FlaskActions.FreezeImmune);
            if (useIgnite) actions.Add(FlaskActions.IgniteImmune);
            if (useShock) actions.Add(FlaskActions.ShockImmune);
            if (useBleed) actions.Add(FlaskActions.BleedImmune);
            if (useCurse) actions.Add(FlaskActions.CurseImmune);
            if (useUnique) actions.Add(FlaskActions.UniqueFlask);
            if (useOffenseAndSpeedrun) actions.Add(FlaskActions.OFFENSE_AND_SPEEDRUN);

            if(actions.Count == 0)
            {
                profileParameter.Plugin.Log("No actions selected.", 5);
                return new TreeSharp.Action();
            }

            bool cleansing = usePoison || useFreeze || useIgnite || useShock || useBleed || useCurse;

            return createUseFlaskAction(profileParameter, actions, cleansing ? null : (bool?)useInstant, useInstant, null);
        }

        private Composite createUseFlaskAction(ExtensionParameter extensionParameter, List<FlaskActions> flaskActions, Boolean? instant, Boolean ignoreBuff, Func<List<FlaskActions>> ignoreFlasksWithAction = null)
        {
            return new UseHotkeyAction(extensionParameter.Plugin.KeyboardHelper, x =>
            {
                //extensionParameter.Plugin.Log("Searching for flask.", 5);

                var foundFlask = findFlaskMatchingAnyAction(extensionParameter, flaskActions, instant, ignoreBuff, ignoreFlasksWithAction);

                if (foundFlask == null)
                {
                    //extensionParameter.Plugin.Log("No flask found.", 5);
                    return null;
                }

                //extensionParameter.Plugin.Log("Found a flask! Index: " + foundFlask.Index, 5);


                return extensionParameter.Plugin.Settings.FlaskSettings[foundFlask.Index].Hotkey;
            });
        }

        private PlayerFlask findFlaskMatchingAnyAction(ExtensionParameter extensionParameter, List<FlaskActions> flaskActions, Boolean? instant, Boolean ignoreBuffs, Func<List<FlaskActions>> ignoreFlasksWithAction)
        {
            var allFlasks = extensionParameter.Plugin.FlaskHelper.GetAllFlaskInfo();

            // We have no flasks or settings for flasks?
            if (allFlasks == null || extensionParameter.Plugin.Settings.FlaskSettings == null)
            {
                extensionParameter.Plugin.Log("No flasks or no settings.", 5);
                return null;
            }

            if (extensionParameter.Plugin.Settings.Debug)
            {
                foreach (var flask in allFlasks)
                {
                    extensionParameter.Plugin.Log("Flask: " + flask.Name + " Instant: " + flask.Instant + " A1: " + flask.Action1 + " A2: " + flask.Action2, 5);
                }
            }

            List<FlaskActions> ignoreFlaskActions = ignoreFlasksWithAction == null ? null : ignoreFlasksWithAction();

            var flaskList = allFlasks
                     .Where(x =>
                     //extensionParameter.Plugin.Settings.FlaskSettings[x.Index].Enabled && 
                     flaskHasAvailableAction(flaskActions, ignoreFlaskActions, x)
                     && extensionParameter.Plugin.FlaskHelper.CanUsePotion(x, reserveFlaskCharges, instant == null)
                     && FlaskMatchesInstant(extensionParameter, x, instant)
                     && (ignoreBuffs || MissingFlaskBuff(extensionParameter, x))
                     ).OrderByDescending(x => flaskActions.Contains(x.Action1)).ThenByDescending(x => x.TotalUses).ToList();


            if (flaskList == null || !flaskList.Any())
            {
                if (extensionParameter.Plugin.Settings.Debug)
                {
                    extensionParameter.Plugin.Log("No flasks found for requested actions: " + String.Concat(flaskActions?.Select(x => x.ToString() + ",")), 5);
                }
                return null;
            }
            else if (extensionParameter.Plugin.Settings.Debug) extensionParameter.Plugin.Log("Using flask " + flaskList.FirstOrDefault()?.Name + " for actions: " + String.Concat(flaskActions?.Select(x => x.ToString() + ",")), 5);


            return flaskList.FirstOrDefault();
        }

        private bool flaskHasAvailableAction(List<FlaskActions> flaskActions, List<FlaskActions> ignoreFlaskActions, PlayerFlask flask)
        {
            return flaskActions.Any(x => x == flask.Action1 || x == flask.Action2)
                    && (ignoreFlaskActions == null || !ignoreFlaskActions.Any(x => x == flask.Action1 || x == flask.Action2));
        }

        private bool FlaskMatchesInstant(ExtensionParameter extensionParameter, PlayerFlask playerFlask, Boolean? instant)
        {
            return instant == null
                    || instant == false && CanUseFlaskAsRegen(playerFlask)
                    || instant == true && CanUseFlaskAsInstant(extensionParameter, playerFlask);
        }

        private bool CanUseFlaskAsInstant(ExtensionParameter extensionParameter, PlayerFlask playerFlask)
        {
            // If the flask is instant, no special logic needed
            return playerFlask.InstantType == FlaskInstantType.Partial
                    || playerFlask.InstantType == FlaskInstantType.Full
                    || playerFlask.InstantType == FlaskInstantType.LowLife && extensionParameter.Plugin.PlayerHelper.isHealthBelowPercentage(35);
        }

        private bool CanUseFlaskAsRegen(PlayerFlask playerFlask)
        {
            return playerFlask.InstantType == FlaskInstantType.None
                    || playerFlask.InstantType == FlaskInstantType.Partial
                    || playerFlask.InstantType == FlaskInstantType.LowLife;
        }

        private bool MissingFlaskBuff(ExtensionParameter extensionParameter, PlayerFlask playerFlask)
        {
            return !extensionParameter.Plugin.PlayerHelper.playerHasBuffs(new List<string> { playerFlask.BuffString1 }) || !extensionParameter.Plugin.PlayerHelper.playerHasBuffs(new List<string> { playerFlask.BuffString2 });
        }

        public override string GetDisplayName(bool isAddingNew)
        {
            string displayName = "Use Flask Type";

            if (!isAddingNew)
            {
                displayName += " [";
                if (useLife) displayName += ("Life,");
                if (useMana) displayName += ("Mana,");
                if (useHybrid) displayName += ("Hybrid,");
                if (useDefense) displayName += ("Defense,");
                if (useUtility) displayName += ("Utility,");
                if (useSpeedrun) displayName += ("Speedrun,");
                if (useOffense) displayName += ("Offense,");
                if (usePoison) displayName += ("Poison,");
                if (useFreeze) displayName += ("Freeze,");
                if (useIgnite) displayName += ("Ignite,");
                if (useShock) displayName += ("Shock,");
                if (useBleed) displayName += ("Bleed,");
                if (useCurse) displayName += ("Curse,");
                if (useUnique) displayName += ("Unique,");
                if (useOffenseAndSpeedrun) displayName += ("Off&Def,");
                if (reserveFlaskCharges > 0) displayName += ("Reserved=" + reserveFlaskCharges);
                displayName += "]";

            }

            return displayName;
        }
    }
}
