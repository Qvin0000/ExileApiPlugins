using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ExileCore;
using TreeRoutine.Menu;
using TreeRoutine.Routine.BuildYourOwnRoutine.Extension;
using TreeRoutine.Routine.BuildYourOwnRoutine.Trigger;


namespace TreeRoutine.Routine.BuildYourOwnRoutine.UI
{

    internal class TriggerMenu
    {
        public TriggerMenu(ExtensionParameter extensionParameter, TriggerComposite parent)
        {
            ExtensionParameter = extensionParameter;
            Parent = parent;

            ActiveWorkingTriggerCondition = null;
            TriggerComposite = new TriggerComposite();
        }

        public TriggerMenu(ExtensionParameter extensionParameter, TriggerComposite parent, TriggerComposite trigger) : this(extensionParameter, parent)
        {
            EditedTrigger = trigger;
            TriggerComposite = new TriggerComposite(trigger);
        }

        public TriggerComposite Parent { get; set; }
        public TriggerComposite TriggerComposite { get; protected set; }
        public TriggerCondition ActiveWorkingTriggerCondition { get; set; }
        public TriggerCondition EditedTriggerCondition { get; set; }

        private ExtensionParameter ExtensionParameter { get; set; }

        private int FilterOption { get; set; } = 0;
        private int SelectedOption1 { get; set; } = -1;
        private int SelectedOption2 { get; set; } = -1;
        private int SelectedOption3 { get; set; } = -1;
        private TriggerComposite EditedTrigger { get; set; }

        private string OKMessage { get; set; }
        private bool OpenOKMenu { get; set; }

        private ExtensionAction ActiveWorkingExtensionAction { get; set; }
        private ExtensionCondition ActiveWorkingExtensionCondition { get; set; }

        public bool Render()
        {
            TriggerComposite.Name = ImGuiExtension.InputText("Trigger Name", TriggerComposite.Name, 50, ImGuiInputTextFlags.AlwaysInsertMode);

            bool shouldPerformClear = false;
            int radioTarget = (int)TriggerComposite.Type;
            shouldPerformClear |= ImGui.RadioButton("PrioritySelector", ref radioTarget, (int)TriggerType.PrioritySelector);
            ImGuiExtension.ToolTipWithText("(?)", "A PrioritySelector runs a list of composites in order from first to last until one of the composites returns true.");

            ImGui.SameLine();
            shouldPerformClear |= ImGui.RadioButton("Sequence", ref radioTarget, (int)TriggerType.Sequence);
            ImGuiExtension.ToolTipWithText("(?)", "A Sequence runs a list of composites in order from first to last until one of the composites returns false.");

            shouldPerformClear |= ImGui.RadioButton("Decorator", ref radioTarget, (int)TriggerType.Decorator);
            ImGuiExtension.ToolTipWithText("(?)", "A Decorator will run its child if all conditions return true.\nThink of this as an if condition.");

            ImGui.SameLine();
            var choseAction = ImGui.RadioButton("Action", ref radioTarget, (int)TriggerType.Action);
            ImGuiExtension.ToolTipWithText("(?)", "An action is a final point in a behavior tree.\nActions can return true or false.\nAll branches MUST end in an action.");

            if (choseAction && TriggerComposite.Children != null && TriggerComposite.Children.Any() // If we chose action, make sure we don't have any children
                || (TriggerType)radioTarget == TriggerType.Decorator && TriggerComposite.Children != null && TriggerComposite.Children.Count > 1) // if we chose decorator, make sure we don't have more than 1 child
            {
                OpenOKMenu = true;
                OKMessage = "Trigger has too many children to change to this type.\nRemove unnecessary children and try again.";

                // Reset everything so the below logic can remain the same
                shouldPerformClear = false;
                choseAction = false;
                radioTarget = (int)TriggerComposite.Type;
            }

            if (shouldPerformClear || choseAction)
            {
                ActiveWorkingTriggerCondition = null;
                TriggerComposite.Action = null;
                SelectedOption1 = -1;
                SelectedOption2 = -1;
            }

            // Save off the selected type into the composite
            TriggerComposite.Type = (TriggerType)radioTarget;

            // There is no configuration unless we are an action or decorator
            if (TriggerComposite.Type == TriggerType.Action || TriggerComposite.Type == TriggerType.Decorator)
            {
                if (ImGui.TreeNodeEx("Configuration", ImGuiTreeNodeFlags.Leaf))
                {
                    if (TriggerComposite.Type == TriggerType.Action)
                    {
                        // TODO: HashSet is not guaranteed ordered. This should be fixed later
                        var filterList = ExtensionParameter.Plugin.ExtensionCache.ActionFilterList.ToList();
                        FilterOption = ImGuiExtension.ComboBox("Filter Type", FilterOption, filterList);

                        var actionList = ExtensionParameter.Plugin.ExtensionCache.ActionList;
                        if (filterList[FilterOption] != ExtensionComponentFilterType.None)
                        {
                            actionList = actionList.Where(x => x.GetFilterTypes().Contains(filterList[FilterOption])).ToList();
                        }

                        int previouslySelectedAction = SelectedOption1;
                        SelectedOption1 = ImGuiExtension.ComboBox("Action List", SelectedOption1, actionList.Select(x => x.Owner + ": " + x.Name).ToList());
                        if (SelectedOption1 >= 0)
                        {
                            // If we do not have an extension action cached OR we swapped actions... initialize values
                            if (ActiveWorkingExtensionAction == null || previouslySelectedAction != SelectedOption1)
                            {
                                var action = actionList[SelectedOption1];
                                if (TriggerComposite.Action == null)
                                    TriggerComposite.Action = new TriggerAction(action.Owner, action.Name);

                                ActiveWorkingExtensionAction = action.GetAction();
                            }

                            // Render the menu and ensure the parameters get saved to the action
                            ActiveWorkingExtensionAction.CreateConfigurationMenu(ExtensionParameter, ref TriggerComposite.Action.Parameters);
                        }
                    }
                    else if (TriggerComposite.Type == TriggerType.Decorator)
                    {
                        TriggerComposite.AlwaysContinue  = ImGuiExtension.Checkbox("Always Continue", TriggerComposite.AlwaysContinue);
                        ImGuiExtension.ToolTipWithText("(?)", "When enabled, the decorator will not end a sequence, even if its child fails.");

                        SelectedOption1 = ImGuiExtension.ComboBox("Conditions", SelectedOption1, TriggerComposite.ConditionList.Select(x => x.Owner + ": " + x.Name).ToList());
                        if (ImGui.Button("AddCondition"))
                        {
                            //if (ActiveWorkingTriggerCondition == null)
                            //{
                                ActiveWorkingTriggerCondition = new TriggerCondition();
                            //}

                            ImGui.OpenPopup("Add Condition");
                        }

                        var selectedCondition = TriggerComposite.ConditionList.ElementAtOrDefault(SelectedOption1);
                        if (selectedCondition != null)
                        {
                            ImGui.SameLine();

                            if (ImGui.Button("Edit"))
                            {
                                //if (ActiveWorkingTriggerCondition == null)
                                //{
                                    ActiveWorkingTriggerCondition = new TriggerCondition(selectedCondition);
                                    EditedTriggerCondition = selectedCondition;
                                //}

                                ImGui.OpenPopup("Add Condition");
                            }

                            ImGui.SameLine();

                            if (ImGui.Button("Remove"))
                            {
                                TriggerComposite.ConditionList.Remove(TriggerComposite.ConditionList[SelectedOption1]);
                            }
                        }

                        RenderAddConditionMenu();
                    }
                    ImGui.TreePop();
                }
            }

            if (ImGui.Button("Save Trigger"))
            {
                // Validation
                if (TriggerComposite.Name == null || TriggerComposite.Name.Length == 0)
                {
                    OpenOKMenu = true;
                    OKMessage = "Must give a trigger name.";
                }
                else
                {
                    if (EditedTrigger != null)
                    {
                        // We are editing. Copy everything from the new trigger to the old instance
                        EditedTrigger.Name = TriggerComposite.Name;
                        EditedTrigger.Type = TriggerComposite.Type;
                        EditedTrigger.Children = TriggerComposite.Children;
                        EditedTrigger.ConditionList = TriggerComposite.ConditionList;
                        EditedTrigger.Action = TriggerComposite.Action;
                        // Finally, clear out our TriggerCompsite. Otherwise, the caller will think we are adding a new trigger
                        TriggerComposite = null;
                    }
                    return false;
                }
            }


            ImGui.SameLine();

            if (ImGui.Button("Cancel trigger"))
            {
                TriggerComposite = null;
                return false;
            }

            if (OpenOKMenu)
            {
                ImGui.OpenPopup("OK Menu");
                OpenOKMenu = false;
            }

            bool testOpen = true;
            if (ImGui.BeginPopupModal("OK Menu", ref testOpen, ImGuiWindowFlags.AlwaysAutoResize))
            {
                ImGui.TextDisabled(OKMessage);
                if (ImGui.SmallButton("OK"))
                {
                    ImGui.CloseCurrentPopup();
                }
                ImGui.EndPopup();
            }

            return true;
        }

        private void RenderAddConditionMenu()
        {
            bool testOpen = true;
            if (ImGui.BeginPopupModal("Add Condition", ref testOpen, ImGuiWindowFlags.AlwaysAutoResize))
            {
                ExtensionParameter.Plugin.LogMessage("Message Logged!", 5.0f);
                // TODO: HashSet is not guaranteed ordered. This should be fixed later
                var filterList = ExtensionParameter.Plugin.ExtensionCache.ConditionFilterList.ToList();
                FilterOption = ImGuiExtension.ComboBox("Filter Type", FilterOption, filterList);

                var conditionList = ExtensionParameter.Plugin.ExtensionCache.ConditionList;
                if (filterList[FilterOption] != ExtensionComponentFilterType.None)
                {
                    conditionList = conditionList.Where(x => x.GetFilterTypes().Contains(filterList[FilterOption])).ToList();
                }

                int previouslySelectedCondition = SelectedOption2;
                if (ActiveWorkingTriggerCondition.Name != null && SelectedOption2 < 0 || conditionList[SelectedOption2].Name != ActiveWorkingTriggerCondition.Name)
                {
                    // This means we are probably attempting to edit. Set the selected option to the one being edited
                    SelectedOption2 = conditionList.FindIndex(x => ActiveWorkingTriggerCondition.Owner == x.Owner && ActiveWorkingTriggerCondition.Name == x.Name);
                }
                SelectedOption2 = ImGuiExtension.ComboBox("Conditions", SelectedOption2, conditionList.Select(x => x.Owner + ": " + x.Name).ToList());

                var condition = conditionList.ElementAtOrDefault(SelectedOption2);

                if (condition != null)
                {
                    ActiveWorkingTriggerCondition.Owner = condition.Owner;
                    ActiveWorkingTriggerCondition.Name = condition.Name;

                    if (TriggerComposite.ConditionList.Any())
                    {
                        int conditionTarget = ActiveWorkingTriggerCondition.Linker == null ? 0 : (int)ActiveWorkingTriggerCondition.Linker;
                        // We need a linker condition
                        if (ImGui.RadioButton("And", ref conditionTarget, (int)TriggerConditionType.And))
                            ActiveWorkingTriggerCondition.Linker = TriggerConditionType.And;
                        ImGui.SameLine();

                        if (ImGui.RadioButton("Or", ref conditionTarget, (int)TriggerConditionType.Or))
                            ActiveWorkingTriggerCondition.Linker = TriggerConditionType.Or;
                    }

                    if (ActiveWorkingExtensionCondition == null || previouslySelectedCondition != SelectedOption2)
                    {
                        ActiveWorkingExtensionCondition = condition.GetCondition();
                        ActiveWorkingExtensionCondition.Initialise(ActiveWorkingTriggerCondition.Parameters);
                    }

                    ActiveWorkingExtensionCondition.CreateConfigurationMenu(ExtensionParameter, ref ActiveWorkingTriggerCondition.Parameters);

                    if (ImGui.Button("Save condition"))
                    {
                        // If we are saving an edit, copy all the new values over
                        if (EditedTriggerCondition != null)
                        {
                            EditedTriggerCondition.Owner = ActiveWorkingTriggerCondition.Owner;
                            EditedTriggerCondition.Name = ActiveWorkingTriggerCondition.Name;
                            EditedTriggerCondition.Parameters = ActiveWorkingTriggerCondition.Parameters;
                            EditedTriggerCondition.Linker = ActiveWorkingTriggerCondition.Linker;
                        }
                        else
                        {
                            // Saving a new condition
                            TriggerComposite.ConditionList.Add(ActiveWorkingTriggerCondition);
                            SelectedOption1 = TriggerComposite.ConditionList.Count - 1;
                        }

                        // Select the condition we just added
                        ActiveWorkingTriggerCondition = null;
                        EditedTriggerCondition = null;
                        ActiveWorkingExtensionCondition = null;
                        ImGui.CloseCurrentPopup();
                    }
                }

                if (ImGui.Button("Cancel condition"))
                {
                    ActiveWorkingTriggerCondition = null;
                    EditedTriggerCondition = null;
                    ActiveWorkingExtensionCondition = null;
                    ImGui.CloseCurrentPopup();
                }

                ImGui.EndPopup();
            }
        }
    }
}
