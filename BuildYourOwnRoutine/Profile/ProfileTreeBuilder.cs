using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TreeRoutine.Routine.BuildYourOwnRoutine.Extension;
using TreeRoutine.Routine.BuildYourOwnRoutine.Trigger;
using TreeRoutine.TreeSharp;

namespace TreeRoutine.Routine.BuildYourOwnRoutine.Profile
{
    internal class ProfileTreeBuilder
    {
        private ExtensionCache ExtensionCache { get; set; }
        private ExtensionParameter ExtensionParameter { get; set; }

        public ProfileTreeBuilder (ExtensionCache extensionCache, ExtensionParameter extensionParameter)
        {
            this.ExtensionCache = extensionCache;
            this.ExtensionParameter = extensionParameter;

        }

        public Composite BuildTreeFromTriggerComposite(TriggerComposite profile)
        {
            switch (profile.Type)
            {
                case TriggerType.Action:
                    return CreateCompositeForAction(profile);
                case TriggerType.Decorator:
                    return CreateCompositeForDecorator(profile);
                case TriggerType.PrioritySelector:
                    return CreateCompositeForPrioritySelector(profile);
                case TriggerType.Sequence:
                    return CreateCompositeForSequence(profile);
            }

            return null;
        }

        public Composite CreateCompositeForAction(TriggerComposite composite)
        {
            // Look it up
            var actionFactory = ExtensionCache.ActionList.FirstOrDefault(x => x.Owner == composite.Action.Owner && x.Name == composite.Action.Name);

            // Do we meet the requirements for a decorator?
            if (actionFactory == null)
            {
                ExtensionParameter.Plugin.LogErr("Action not found!", 10);
                return null;
            }

            var actionInstance = actionFactory.GetAction();

            actionInstance.Initialise(composite.Action.Parameters);

            if (actionInstance == null)
            {
                ExtensionParameter.Plugin.LogErr("Action instance was null!", 10);
                return null;
            }

            // These are simple as they are self contained
            return actionInstance.GetComposite(ExtensionParameter);
        }

        public Composite CreateCompositeForDecorator(TriggerComposite composite)
        {
            // Do we meet the requirements for a decorator?
            if (composite.Children == null || composite.Children.Count != 1)
            {
                ExtensionParameter.Plugin.LogErr("Decorator was malformed!", 10);
                return null;
            }

            // If we should always continue, use a decorator continue instead
            if (composite.AlwaysContinue)
            {
                return new DecoratorContinue(x => EvaluateConditionList(composite.ConditionList), BuildTreeFromTriggerComposite(composite.Children.FirstOrDefault()));
            }
            else
            {
                return new Decorator(x => EvaluateConditionList(composite.ConditionList), BuildTreeFromTriggerComposite(composite.Children.FirstOrDefault()));
            }
        }

        public Composite CreateCompositeForPrioritySelector(TriggerComposite composite)
        {
            if (composite.Children == null || !composite.Children.Any())
            {
                ExtensionParameter.Plugin.LogErr("Priority Selector was malformed!", 10);
                return null;
            }

            return new PrioritySelector(composite.Children.Select(x => BuildTreeFromTriggerComposite(x)).ToArray());
        }

        public Composite CreateCompositeForSequence(TriggerComposite composite)
        {
            if (composite.Children == null || !composite.Children.Any())
            {
                ExtensionParameter.Plugin.LogErr("Sequence was malformed!", 10);
                return null;
            }

            return new Sequence(composite.Children.Select(x => BuildTreeFromTriggerComposite(x)).ToArray());
        }

        public bool EvaluateConditionList(List<TriggerCondition> conditionList)
        {
            bool currentCondition = true;
            if (conditionList != null)
            {
                foreach (var condition in conditionList)
                {
                    if (condition.Linker != TriggerConditionType.Or && !currentCondition)
                    {
                        if (ExtensionParameter.Plugin.Settings.Debug)
                        {
                            ExtensionParameter.Plugin.Log("Condition: " + condition.Name + " Linker: " + condition.Linker + " Hit first continue.", 5);
                        }
                        continue;

                    }

                    // If we hit an OR, and we're sitting on a true condition... no reason to go any further
                    if (condition.Linker == TriggerConditionType.Or && currentCondition)
                    {
                        if (ExtensionParameter.Plugin.Settings.Debug)
                        {
                            ExtensionParameter.Plugin.Log("Condition: " + condition.Name + " Linker: " + condition.Linker + " Hit first continue.", 5);
                        }
                        return true;
                    }

                    // TODO: It would be nice to not have to look this up every time.
                    var foundConditionFactory = ExtensionCache.ConditionList.FirstOrDefault(x => x.Owner == condition.Owner && x.Name == condition.Name);

                    if (foundConditionFactory == null)
                    {
                        ExtensionParameter.Plugin.LogErr("Condition not found!", 10);
                        continue;
                    }

                    var conditionInstance = foundConditionFactory.GetCondition();

                    if (conditionInstance == null)
                    {
                        ExtensionParameter.Plugin.LogErr("Condition Instance was null!", 10);
                        continue;
                    }

                    conditionInstance.Initialise(condition.Parameters);
                    Func<bool> conditionFunction = (() => conditionInstance.Invert != conditionInstance.GetCondition(ExtensionParameter)());
                    if (conditionFunction != null)
                        currentCondition = conditionFunction();

                    if (ExtensionParameter.Plugin.Settings.Debug)
                    {
                        ExtensionParameter.Plugin.Log("Condition: " + condition.Name + " Linker: " + condition.Linker + " Evaluated: " + currentCondition, 5);
                    }
                }
            }
            return currentCondition;
        }


    }
}
