using System;
using System.Collections.Generic;
using System.Linq;
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using ExileCore;
using ExileCore.PoEMemory.Components;
using ExileCore.PoEMemory.MemoryObjects;
using ExileCore.Shared.Enums;
using ExileCore.Shared.Static;
using JM.LinqFaster;
using PoeFilterParser.Model;
using SharpDX;

namespace ItemAlert
{
    public class PoeFilterVisitor : PoeFilterBaseVisitor<AlertDrawStyle>
    {
        private readonly GameController gameController;
        private readonly ItemAlertSettings settings;
        private readonly IParseTree tree;
        private Entity itemEntity;

        public PoeFilterVisitor(IParseTree tree, GameController gameController, ItemAlertSettings settings)
        {
            this.tree = tree;
            this.gameController = gameController;
            this.settings = settings;
        }

        public AlertDrawStyle Visit(Entity itemEntity)
        {
            this.itemEntity = itemEntity;
            return base.Visit(tree);
        }

        public override string ToString()
        {
            return itemEntity.Path;
        }

        public override AlertDrawStyle VisitMain(PoeFilterParser.Model.PoeFilterParser.MainContext context)
        {
            if (itemEntity == null || gameController == null || gameController.Files == null ||
                gameController.Files.BaseItemTypes == null) return null;

            var baseItemType = gameController.Files.BaseItemTypes.Translate(itemEntity.Path);

            if (baseItemType == null)
                return null;

            var basename = baseItemType.BaseName;
            if (basename.Contains("Scroll")) return null;

            var dropLevel = baseItemType.DropLevel;
            ItemClass tmp;
            string className;

            if (gameController.Files.ItemClasses.contents.TryGetValue(baseItemType.ClassName, out tmp))
                className = tmp.ClassName;
            else
                className = baseItemType.ClassName;

            var itemBase = itemEntity.GetComponent<Base>();
            var width = baseItemType.Width;
            var height = baseItemType.Height;
            var blocks = context.block();
            var mods = itemEntity.GetComponent<Mods>();
            var modsIsNull = mods == null;
            var skillGemLevel = itemEntity.GetComponent<SkillGem>()?.Level ?? 0;
            var isMap = itemEntity.HasComponent<Map>();
            var componentStack = itemEntity.GetComponent<Stack>();
            var stackSize = 0;

            if (componentStack != null)
                stackSize = componentStack.Size;

            var isShapedMap = false;
            var isElderMap = false;

            if (isMap && !modsIsNull)
            {
                foreach (var mod in mods.ItemMods)
                {
                    if (mod.Name == "MapElder")
                        isElderMap = mod.Value1 == 1;
                    else if (mod.Name == "MapShaped")
                        isShapedMap = mod.Value1 == 1;
                }
            }

            var explicitMods = new List<string>();
            var modsSynthesised = false;
            var modsIdentified = false;
            var modsItemLevel = 0;
            var modsHaveFractured = false;
            var modsCountFractured = 0;
            var anyEnchantment = false;

            if (!modsIsNull)
            {
                foreach (var mod in mods.ItemMods)
                {
                    explicitMods.Add(mod.Name);
                }

                modsItemLevel = mods.ItemLevel;
                modsIdentified = mods.Identified;
                modsHaveFractured = mods.HaveFractured;
                modsSynthesised = mods.Synthesised;
                modsCountFractured = mods.CountFractured;
            }

            var itemRarity = mods?.ItemRarity ?? ItemRarity.Normal;
            var quality = 0;
            if (itemEntity.HasComponent<Quality>()) quality = itemEntity.GetComponent<Quality>().ItemQuality;

            var text = basename;
            if (quality > 0) text = $"Superior {text}";
            var sockets = itemEntity.GetComponent<Sockets>();
            var numberOfSockets = sockets?.NumberOfSockets ?? 0;
            var largestLinkSize = sockets?.LargestLinkSize ?? 0;
            var socketGroup = sockets?.SocketGroup ?? new List<string>();
            var path = itemEntity.Path;

            var defaultTextColor = HudSkin.CurrencyColor;

            //if (basename.Contains("Portal") || basename.Contains("Wisdom")) { return null; }

            /*if (path.Contains("Currency"))
                defaultTextColor = HudSkin.CurrencyColor;
            else if (path.Contains("DivinationCards"))
                defaultTextColor = HudSkin.DivinationCardColor;
            else if (path.Contains("Talismans"))
                defaultTextColor = HudSkin.TalismanColor;
            else if (isSkillHGem)
                defaultTextColor = HudSkin.SkillGemColor;
            else defaultTextColor = AlertDrawStyle.GetTextColorByRarity(itemRarity);

            var defaultBorderWidth = isMap || path.Contains("VaalFragment") ? 1 : 0;*/
            bool skip;

            bool Valid(bool s)
            {
                skip = !s;
                return skip;
            }

            foreach (var block in blocks)
            {
                var showBlock = block.visibility().SHOW();
                var showThisItem = showBlock != null && showBlock.GetText().Equals("Show", StringComparison.Ordinal);
                var backgroundColor = AlertDrawStyle.DefaultBackgroundColor;
                var borderColor = Color.White;
                var textColor = defaultTextColor;
                var borderWidth = 1;
                var sound = -1;
                var statements = block.statement();

                skip = false;

                foreach (var statement in statements)
                {
                    if (Valid(statement.poeBaseType()?.@params().strValue()?.AnyF(x => basename.Contains(GetRawText(x))) ?? true))
                        break;

                    if (Valid(statement.poeClass()?.@params().strValue().AnyF(x => className.Contains(GetRawText(x))) ?? true))
                        break;

                    var rarityContext = statement.poeRarity();

                    if (Valid(rarityContext?.rariryParams().rarityValue().AnyF(x =>
                    {
                        Enum.TryParse<ItemRarity>(GetRawText(x), true, out var filterRarity);
                        return OpConvertor(rarityContext.compareOpNullable()).Invoke((int) itemRarity, (int) filterRarity);
                    }) ?? true))
                        break;

                    var itemLevelContext = statement.poeItemLevel();

                    if (Valid(itemLevelContext == null || !modsIsNull &&
                              CalculateDigitsCondition(itemLevelContext.compareOpNullable(), itemLevelContext.digitsParams(),
                                  modsItemLevel)))
                        break;

                    var dropLevelContext = statement.poeDropLevel();

                    if (Valid(dropLevelContext == null ||
                              CalculateDigitsCondition(dropLevelContext.compareOpNullable(), dropLevelContext.digitsParams(), dropLevel)))
                        break;

                    var socketsContext = statement.poeSockets();

                    if (Valid(socketsContext == null || CalculateDigitsCondition(
                                  socketsContext.compareOpNullable(), socketsContext.digitsParams(), numberOfSockets)))
                        break;

                    var linkedSocketsContext = statement.poeLinkedSockets();

                    if (Valid(linkedSocketsContext == null || CalculateDigitsCondition(
                                  linkedSocketsContext.compareOpNullable(), linkedSocketsContext.digitsParams(), largestLinkSize)))
                        break;

                    var socketGroupContext = statement.poeSocketGroup();

                    if (Valid(socketGroupContext == null || socketGroupContext
                                  .socketParams().socketValue()
                                  .AnyF(x => IsContainSocketGroup(socketGroup, GetRawText(x)))))
                        break;

                    var identifiedContext = statement.poeIdentified();

                    if (Valid(identifiedContext == null || !modsIsNull && itemRarity != ItemRarity.Normal &&
                              Convert.ToBoolean(identifiedContext.Boolean().GetText()) == modsIdentified))
                        break;

                    var corruptedContext = statement.poeCorrupted();

                    if (Valid(corruptedContext == null || itemBase?.isCorrupted == Convert.ToBoolean(corruptedContext.Boolean().GetText())))
                        break;

                    var poeFracturedContext = statement.poeFracturedItem();

                    if (Valid(poeFracturedContext == null ||
                              !modsIsNull && modsHaveFractured == Convert.ToBoolean(poeFracturedContext.Boolean().GetText())))
                        break;

                    /*var poeFracturedItemCountContext = statement.poeFracturedItemCount();
                    if (Valid(poeFracturedItemCountContext == null || CalculateDigitsCondition(
                                  poeFracturedItemCountContext.compareOpNullable(), poeFracturedItemCountContext.digitsParams(),
                                  modsCountFractured))) break;*/
                    var poeSynthesisedContext = statement.poeSynthesisedItem();

                    if (Valid(poeSynthesisedContext == null || modsSynthesised))
                        break;

                    var poeElderContext = statement.poeElderItem();

                    if (Valid(poeElderContext == null || itemBase.isElder == Convert.ToBoolean(poeElderContext.Boolean().GetText())))
                        break;

                    var poeShaperContext = statement.poeShaperItem();

                    if (Valid(poeShaperContext == null || itemBase == null ||
                              itemBase.isShaper == Convert.ToBoolean(poeShaperContext.Boolean().GetText())))
                        break;

                    var poeQualityContext = statement.poeQuality();

                    if (Valid(poeQualityContext == null ||
                              CalculateDigitsCondition(poeQualityContext.compareOpNullable(), poeQualityContext.digitsParams(), quality)))
                        break;

                    var poeWidthContext = statement.poeWidth();

                    if (Valid(poeWidthContext == null ||
                              CalculateDigitsCondition(poeWidthContext.compareOpNullable(), poeWidthContext.digitsParams(), width)))
                        break;

                    var poeHeightContext = statement.poeHeight();

                    if (Valid(poeHeightContext == null ||
                              CalculateDigitsCondition(poeHeightContext.compareOpNullable(), poeHeightContext.digitsParams(), height)))
                        break;

                    if (isMap)
                    {
                        var poeShapedMapContext = statement.poeShapedMap();

                        if (Valid(poeShapedMapContext == null || isShapedMap == Convert.ToBoolean(poeShapedMapContext.Boolean().GetText())))
                            break;

                        var poeElderMapContext = statement.poeElderMap();

                        if (Valid(poeElderMapContext == null || isElderMap == Convert.ToBoolean(poeElderMapContext.Boolean().GetText())))
                            break;
                    }

                    var poeStackSizeContext = statement.poeStackSize();

                    if (Valid(poeStackSizeContext == null ||
                              CalculateDigitsCondition(poeStackSizeContext.compareOpNullable(), poeStackSizeContext.digitsParams(),
                                  stackSize)))
                        break;

                    var poeExplicitModContext = statement.poeHasExplicitMod();

                    if (Valid(poeExplicitModContext == null ||
                              poeExplicitModContext.@params().strValue().AnyF(x => explicitMods.ContainsF(GetRawText(x)))))
                        break;

                    var poeGemSkillContext = statement.poeGemLevel();

                    if (Valid(poeGemSkillContext == null || CalculateDigitsCondition(poeGemSkillContext.compareOpNullable(),
                                  poeGemSkillContext.digitsParams(), skillGemLevel)))
                        break;

                    var poeAnyEnchantmentContext = statement.poeAnyEnchantment();

                    //not implemented always false
                    if (Valid(poeAnyEnchantmentContext == null || false))
                        break;

                    /*//not implemented always false
                    var poeHasEnchantmentContext = statement.poeHasEnchantment();
                    if (Valid(poeHasEnchantmentContext == null || false))
                    {
                        break;
                    }*/

                    var poeBackgroundColorContext = statement.poeBackgroundColor();
                    if (poeBackgroundColorContext != null) backgroundColor = ToColor(poeBackgroundColorContext.color());

                    var poeBorderColorContext = statement.poeBorderColor();

                    if (poeBorderColorContext != null)
                    {
                        borderColor = ToColor(poeBorderColorContext.color());
                        borderWidth = borderColor.A == 0 ? 0 : 1;
                    }

                    var poeTextColorContext = statement.poeTextColor();
                    if (poeTextColorContext != null) textColor = ToColor(poeTextColorContext.color());

                    /*var poeAlertSoundContext = statement.poeAlertSound();
                    if (poeAlertSoundContext != null)
                    {
                        try
                        {
                            sound = Convert.ToInt32(
                                poeAlertSoundContext.soundId().GetText());
                        }
                        catch (Exception)
                        {
                            sound = 1;
                        }
                    }*/
                }

                if (skip)
                    continue;

                if (!skip)
                {
                    if (!showThisItem)
                        return null;

                    int iconIndex;

                    if (largestLinkSize == 6)
                        iconIndex = 3;
                    else if (numberOfSockets == 6)
                        iconIndex = 0;
                    else if (IsContainSocketGroup(socketGroup, "RGB"))
                        iconIndex = 1;
                    else iconIndex = -1;

                    var debugline = block.start.Line;
                    return new AlertDrawStyle(text, textColor, borderWidth, borderColor, backgroundColor, iconIndex);
                }
            }

            return null;
        }

        private string GetRawText(ParserRuleContext context)
        {
            return context.GetText().Trim('"');
        }

        private bool CalculateDigitsCondition(PoeFilterParser.Model.PoeFilterParser.CompareOpNullableContext compareOpNullable,
            PoeFilterParser.Model.PoeFilterParser.DigitsParamsContext digitsParams, int value)
        {
            var compareFunc = OpConvertor(compareOpNullable);

            return digitsParams.DIGITS().AnyF(y =>
            {
                var poeValue = Convert.ToInt32(y.GetText());
                return compareFunc(value, poeValue);
            });
        }

        private bool IsContainSocketGroup(List<string> socketGroup, string str)
        {
            if (socketGroup == null || socketGroup.Count < 0) return false;
            str = string.Concat(str.OrderBy(y => y));
            return socketGroup.Select(group => string.Concat(group.OrderBy(y => y))).Any(sortedGroup => sortedGroup.Contains(str));
        }

        private Color ToColor(PoeFilterParser.Model.PoeFilterParser.ColorContext colorContext)
        {
            var red = Convert.ToByte(colorContext.red().GetText());
            var green = Convert.ToByte(colorContext.green().GetText());
            var blue = Convert.ToByte(colorContext.blue().GetText());
            var alphaContext = colorContext.alpha();
            var alpha = alphaContext.DIGITS() != null ? Convert.ToByte(alphaContext.GetText()) : 255;
            return new Color(red, green, blue, alpha);
        }

        private Func<int, int, bool> OpConvertor(PoeFilterParser.Model.PoeFilterParser.CompareOpNullableContext terminalnode)
        {
            var text = terminalnode.COMPAREOP()?.GetText() ?? "=";

            switch (text)
            {
                case "=": return (x, y) => x == y;
                case "<": return (x, y) => x < y;
                case ">": return (x, y) => x > y;
                case "<=": return (x, y) => x <= y;
                case ">=": return (x, y) => x >= y;
            }

            throw new ArrayTypeMismatchException();
        }
    }
}
