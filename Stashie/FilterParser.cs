using System;
using System.Collections.Generic;
using System.Linq;
using Basic;
using Exile;

namespace Stashie
{
    public class FilterParser
    {
        private const char CYMBOL_COMMANDSDIVIDE = ',';
        private const char CYMBOL_COMMAND_FILTER_OR = '|';
        private const char CYMBOL_NAMEDIVIDE = ':';
        private const char CYMBOL_SUBMENUNAME = ':';
        private const char CYMBOL_NOT = '!';
        private const string COMMENTCYMBOL = "#";
        private const string COMMENTCYMBOLALT = "//";

        //String compare
        private const string PARAMETER_CLASSNAME = "classname";

        private const string PARAMETER_BASENAME = "basename";
        private const string PARAMETER_PATH = "path";

        //Number compare
        private const string PARAMETER_QUALITY = "itemquality";

        private const string PARAMETER_RARITY = "rarity";
        private const string PARAMETER_ILVL = "ilvl";
        private const string PARAMETER_MapTier = "tier";

        //Boolean
        private const string PARAMETER_IDENTIFIED = "identified";

        private const string PARAMETER_ISELDER = "Elder";
        private const string PARAMETER_ISSHAPER = "Shaper";

        private const string PARAMETER_ISFRACTURED = "Fractured";

        //Operations
        private const string OPERATION_NONEQUALITY = "!=";

        private const string OPERATION_LESSEQUAL = "<=";
        private const string OPERATION_BIGGERQUAL = ">=";

        private const string OPERATION_EQUALITY = "=";
        private const string OPERATION_BIGGER = ">";
        private const string OPERATION_LESS = "<";
        private const string OPERATION_CONTAINS = "^";
        private const string OPERATION_NOTCONTAINS = "!^";

        private static readonly string[] Operations =
        {
            OPERATION_NONEQUALITY, OPERATION_LESSEQUAL, OPERATION_BIGGERQUAL, OPERATION_NOTCONTAINS, OPERATION_EQUALITY,
            OPERATION_BIGGER, OPERATION_LESS, OPERATION_CONTAINS,
        };

        public static List<CustomFilter> Parse(string[] filtersLines) {
            var allFilters = new List<CustomFilter>();

            for (var i = 0; i < filtersLines.Length; ++i)
            {
                var filterLine = filtersLines[i];

                filterLine = filterLine.Replace("\t", "");

                if (filterLine.StartsWith(COMMENTCYMBOL)) continue;
                if (filterLine.StartsWith(COMMENTCYMBOLALT)) continue;

                if (filterLine.Replace(" ", "").Length == 0) continue;

                var nameIndex = filterLine.IndexOf(CYMBOL_NAMEDIVIDE);
                if (nameIndex == -1)
                {
                    DebugWindow.LogMsg("Filter parser: Can't find filter name in line: " + (i + 1), 5);
                    continue;
                }

                var newFilter = new CustomFilter {Name = filterLine.Substring(0, nameIndex).Trim(), Index = i + 1};

                var filterCommandsLine = filterLine.Substring(nameIndex + 1);

                var submenuIndex = filterCommandsLine.IndexOf(CYMBOL_SUBMENUNAME);
                if (submenuIndex != -1)
                {
                    newFilter.SubmenuName = filterCommandsLine.Substring(submenuIndex + 1);
                    filterCommandsLine = filterCommandsLine.Substring(0, submenuIndex);
                }

                var filterCommands = filterCommandsLine.Split(CYMBOL_COMMANDSDIVIDE);
                newFilter.Commands = filterCommandsLine;

                var filterErrorParse = false;

                foreach (var command in filterCommands)
                {
                    if (string.IsNullOrEmpty(command.Replace(" ", ""))) continue;

                    if (command.Contains(CYMBOL_COMMAND_FILTER_OR))
                    {
                        var orFilterCommands = command.Split(CYMBOL_COMMAND_FILTER_OR);
                        var newOrFilter = new BaseFilter {BAny = true};
                        newFilter.Filters.Add(newOrFilter);

                        foreach (var t in orFilterCommands)
                        {
                            if (ProcessCommand(newOrFilter, t)) continue;
                            filterErrorParse = true;
                            break;
                        }

                        if (filterErrorParse) break;
                    }
                    else
                    {
                        if (ProcessCommand(newFilter, command)) continue;

                        filterErrorParse = true;
                        break;
                    }
                }

                if (!filterErrorParse) allFilters.Add(newFilter);
            }

            return allFilters;
        }

        private static bool ProcessCommand(BaseFilter newFilter, string command) {
            command = command.Trim();

            if (command.Contains(PARAMETER_IDENTIFIED))
            {
                var identCommand = new IdentifiedItemFilter {BIdentified = command[0] != CYMBOL_NOT};
                newFilter.Filters.Add(identCommand);
                return true;
            }

            if (command.Contains(PARAMETER_ISELDER))
            {
                var elderCommand = new ElderItemFiler {isElder = command[0] != CYMBOL_NOT};
                newFilter.Filters.Add(elderCommand);
                return true;
            }

            if (command.Contains(PARAMETER_ISSHAPER))
            {
                var shaperCommand = new ShaperItemFiler {isShaper = command[0] != CYMBOL_NOT};
                newFilter.Filters.Add(shaperCommand);
                return true;
            }

            if (command.Contains(PARAMETER_ISFRACTURED))
            {
                var synthesisCommand = new FracturedItemFiler() {isFractured = command[0] != CYMBOL_NOT};
                newFilter.Filters.Add(synthesisCommand);
                return true;
            }

            string parameter;
            string operation;
            string value;

            if (!ParseCommand(command, out parameter, out operation, out value))
            {
                DebugWindow.LogMsg("Filter parser: Can't parse filter part: " + command, 5);
                return false;
            }


            var stringComp = new FilterParameterCompare {CompareString = value};

            switch (parameter.ToLower())
            {
                case PARAMETER_CLASSNAME:
                    stringComp.StringParameter = data => data.ClassName;
                    break;
                case PARAMETER_BASENAME:
                    stringComp.StringParameter = data => data.BaseName;
                    break;
                case PARAMETER_PATH:
                    stringComp.StringParameter = data => data.Path;
                    break;
                case PARAMETER_RARITY:
                    stringComp.StringParameter = data => data.Rarity.ToString();
                    break;
                case PARAMETER_QUALITY:
                    stringComp.IntParameter = data => data.ItemQuality;
                    stringComp.CompareInt = int.Parse(value);
                    stringComp.StringParameter = data => data.ItemQuality.ToString();
                    break;
                case PARAMETER_MapTier:
                    stringComp.IntParameter = data => data.MapTier;
                    stringComp.CompareInt = int.Parse(value);
                    stringComp.StringParameter = data => data.MapTier.ToString();
                    break;
                case PARAMETER_ILVL:
                    stringComp.IntParameter = data => data.ItemLevel;
                    stringComp.CompareInt = int.Parse(value);
                    stringComp.StringParameter = data => data.ItemLevel.ToString();
                    break;
                default:
                    DebugWindow.LogMsg($"Filter parser: Parameter is not defined in code: {parameter}", 10);
                    return false;
            }

            switch (operation.ToLower())
            {
                case OPERATION_EQUALITY:
                    stringComp.CompDeleg = data => stringComp.StringParameter(data).Equals(stringComp.CompareString);
                    break;
                case OPERATION_NONEQUALITY:
                    stringComp.CompDeleg = data => !stringComp.StringParameter(data).Equals(stringComp.CompareString);
                    break;
                case OPERATION_CONTAINS:
                    stringComp.CompDeleg = data => stringComp.StringParameter(data).Contains(stringComp.CompareString);
                    break;
                case OPERATION_NOTCONTAINS:
                    stringComp.CompDeleg = data => !stringComp.StringParameter(data).Contains(stringComp.CompareString);
                    break;


                case OPERATION_BIGGER:
                    if (stringComp.IntParameter == null)
                    {
                        DebugWindow.LogMsg(
                            $"Filter parser error: Can't compare string parameter with {OPERATION_BIGGER} (numerical) operation. Statement: {command}",
                            10);
                        return false;
                    }

                    stringComp.CompDeleg = data => stringComp.IntParameter(data) > stringComp.CompareInt;
                    break;
                case OPERATION_LESS:
                    if (stringComp.IntParameter == null)
                    {
                        DebugWindow.LogMsg(
                            $"Filter parser error: Can't compare string parameter with {OPERATION_LESS} (numerical) operation. Statement: {command}",
                            10);
                        return false;
                    }

                    stringComp.CompDeleg = data => stringComp.IntParameter(data) < stringComp.CompareInt;
                    break;
                case OPERATION_LESSEQUAL:
                    if (stringComp.IntParameter == null)
                    {
                        DebugWindow.LogMsg(
                            $"Filter parser error: Can't compare string parameter with {OPERATION_LESSEQUAL} (numerical) operation. Statement: {command}",
                            10);
                        return false;
                    }

                    stringComp.CompDeleg = data => stringComp.IntParameter(data) <= stringComp.CompareInt;
                    break;

                case OPERATION_BIGGERQUAL:
                    if (stringComp.IntParameter == null)
                    {
                        DebugWindow.LogMsg(
                            $"Filter parser error: Can't compare string parameter with {OPERATION_BIGGERQUAL} (numerical) operation. Statement: {command}",
                            10);
                        return false;
                    }

                    stringComp.CompDeleg = data => stringComp.IntParameter(data) >= stringComp.CompareInt;
                    break;

                default:
                    DebugWindow.LogMsg($"Filter parser: Operation is not defined in code: {operation}", 10);
                    return false;
            }

            newFilter.Filters.Add(stringComp);
            return true;
        }


        private static bool ParseCommand(string command, out string parameter, out string operation, out string value) {
            parameter = "";
            operation = "";
            value = "";

            var operationIndex = -1;
            foreach (var t in Operations)
            {
                operationIndex = command.IndexOf(t, StringComparison.Ordinal);

                if (operationIndex == -1) continue;

                operation = t;
                break;
            }

            if (operationIndex == -1) return false;

            parameter = command.Substring(0, operationIndex).Trim();

            value = command.Substring(operationIndex + operation.Length).Trim();
            return true;
        }
    }
}