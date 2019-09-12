using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using ExileCore;
using ExileCore.PoEMemory;
using ExileCore.PoEMemory.Components;
using ExileCore.PoEMemory.Elements;
using ExileCore.PoEMemory.MemoryObjects;
using ExileCore.Shared;
using ExileCore.Shared.Cache;
using ExileCore.Shared.Enums;
using ExileCore.Shared.Helpers;
using ImGuiNET;
using SharpDX;
using ImGuiVector2 = System.Numerics.Vector2;
using ImGuiVector4 = System.Numerics.Vector4;

namespace DevTree
{
    public partial class DevPlugin : BaseSettingsPlugin<DevSetting>
    {
        private const BindingFlags Flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static |
                                           BindingFlags.FlattenHierarchy;

        private readonly Random _rnd = new Random(123);
        private int _version;
        private TimeCache<Color> ColorSwaper;
        private List<Entity> DebugEntities = new List<Entity>(128);
        private double Error = 0;
        private readonly Dictionary<string, MethodInfo> genericMethodCache = new Dictionary<string, MethodInfo>();
        private MethodInfo GetComponentMethod;
        private readonly HashSet<string> IgnoredPropertioes = new HashSet<string> {"M", "TheGame", "Address"};
        private string inputFilter = "";
        private readonly Dictionary<string, object> objects = new Dictionary<string, object>();
        private readonly Dictionary<string, long> OffsetFinder = new Dictionary<string, long>(24);
        private List<string> Rarities;
        private MonsterRarity selectedRarity;
        private string selectedRarityString = "All";
        private readonly Dictionary<string, int> Skips = new Dictionary<string, int>();
        private bool windowState;

        public override void OnLoad()
        {
            var values = Enum.GetNames(typeof(MonsterRarity));
            Rarities = new List<string>(values);
            Rarities.Add("All");
        }

        public override bool Initialise()
        {
            Force = true;
            GetComponentMethod = typeof(Entity).GetMethod("GetComponent");

            try
            {
                InitObjects();
            }
            catch (Exception e)
            {
                LogError($"{e}");
            }

            ColorSwaper = new TimeCache<Color>(() =>
            {
                return Color.Yellow;
                return new Color(_rnd.Next(255), _rnd.Next(255), _rnd.Next(255), 255);
                ;
            }, 25);

            Input.RegisterKey(Settings.ToggleWindowKey);
            Name = "Dev Tree";
            return true;
        }

        public override void AreaChange(AreaInstance area)
        {
            InitObjects();
            Skips.Clear();
            OffsetFinder.Clear();
        }

        public void InitObjects()
        {
            /*objects[nameof(GameController)] = GameController;
            objects[nameof(GameController.Game)] = GameController.Game;
            objects[nameof(GameController.Game.IngameState)] = GameController.Game.IngameState;
            objects[nameof(GameController.Cache)] = GameController.Cache;
            objects[nameof(GameController.Game.IngameState.IngameUi)] = GameController.Game.IngameState.IngameUi;
            objects[nameof(GameController.Game.IngameState.Data)] = GameController.Game.IngameState.Data;
            objects[nameof(GameController.Game.IngameState.ServerData)] = GameController.Game.IngameState.ServerData;*/
            objects.Clear();
            AddObjects(GameController.Cache);
            AddObjects(GameController);
            AddObjects(GameController.Game);
            AddObjects(GameController.Game.IngameState);
            AddObjects(GameController.Game.IngameState.IngameUi);
            AddObjects(GameController.Game.IngameState.Data);
            AddObjects(GameController.Game.IngameState.ServerData);
            AddObjects(GameController.Game.IngameState.ServerData.PlayerInventories[0].Inventory);
            AddObjects(GameController.Game.IngameState.ServerData.PlayerInventories[0].Inventory.Items, "Items");
            AddObjects(GameController.Game.IngameState.IngameUi.InventoryPanel[InventoryIndex.PlayerInventory], "Inventory panel");
            AddObjects(GameController.IngameState.IngameUi.ItemsOnGroundLabels, "Label on grounds");
            _version++;
        }

        public void AddObjects(object o, string name = null)
        {
            if (o == null)
            {
                DebugWindow.LogError($"{Name} cant add object to debug.");
                return;
            }

            var type = o.GetType();
            if (name == null) name = $"{type.Name}";

            if (o is RemoteMemoryObject)
            {
                var propertyInfo = type.GetProperty("Address");
                if (propertyInfo != null) name += $" ({(long) propertyInfo.GetValue(o, null):X})##InitObject";
            }

            objects[name] = o;
        }

        public override void Render()
        {
            /*if (RenderDebugInformation.Tick >= 500)
            {
                Error += RenderDebugInformation.Tick;
                if (Error >= 5000)
                {
                    LogError("Reloaded because works more than 5 sec", 5);
                    InitObjects();
                    Error = 0;
                }
            }*/

            if (Settings.ToggleWindow)
            {
                if (Settings.ToggleWindowKey.PressedOnce())
                {
                    Settings.ToggleWindowState = !Settings.ToggleWindowState;
                }

                if (!Settings.ToggleWindowState)
                    return;
            }

            windowState = Settings.Enable;
            ImGui.Begin($"{Name}", ref windowState);

            if (Settings.Enable != windowState)
            {
                if (!Settings.ToggleWindow)
                    Settings.Enable.Value = windowState;
                else
                    Settings.ToggleWindowState = windowState;
            }

            if (ImGui.Button("Reload")) InitObjects();

            ImGui.SameLine();
            ImGui.PushItemWidth(200);

            if (ImGui.InputText("Filter", ref inputFilter, 128))
            {
            }

            ImGui.PopItemWidth();
            ImGui.SameLine();
            ImGui.PushItemWidth(128);

            if (ImGui.BeginCombo("Rarity", selectedRarityString))
            {
                for (var index = 0; index < Rarities.Count; index++)
                {
                    var rarity = Rarities[index];
                    var isSelected = selectedRarityString == rarity;

                    if (ImGui.Selectable(rarity, isSelected))
                    {
                        selectedRarityString = rarity;
                        selectedRarity = (MonsterRarity) index;
                    }

                    if (isSelected) ImGui.SetItemDefaultFocus();
                }

                ImGui.EndCombo();
            }

            ImGui.PopItemWidth();
            ImGui.SameLine();

            if (ImGui.Button("Debug around entities"))
            {
                var entites = GameController.Entities;
                var player = GameController.Player;
                var playerGridPos = player.GridPos;

                if (!selectedRarityString.Equals("All", StringComparison.Ordinal))
                {
                    if (inputFilter.Length == 0)
                    {
                        DebugEntities = entites.Where(x => x.Rarity == selectedRarity &&
                                                           x.GridPos.Distance(playerGridPos) < Settings.NearestEntsRange)
                            .OrderBy(x => x.GridPos.Distance(playerGridPos)).ToList();
                    }
                    else
                    {
                        DebugEntities = entites
                            .Where(x => x.Path.Contains(inputFilter) &&
                                        x.GetComponent<ObjectMagicProperties>()?.Rarity == selectedRarity &&
                                        x.GridPos.Distance(playerGridPos) < Settings.NearestEntsRange)
                            .OrderBy(x => x.GridPos.Distance(playerGridPos)).ToList();
                    }
                }
                else
                {
                    if (inputFilter.Length == 0)
                    {
                        DebugEntities = entites.Where(x => x.GridPos.Distance(playerGridPos) < Settings.NearestEntsRange)
                            .OrderBy(x => x.GridPos.Distance(playerGridPos)).ToList();
                    }
                    else
                    {
                        DebugEntities = entites
                            .Where(x => x.Path.Contains(inputFilter) &&
                                        x.GridPos.Distance(playerGridPos) < Settings.NearestEntsRange)
                            .OrderBy(x => x.GridPos.Distance(playerGridPos)).ToList();
                    }
                }
            }

            foreach (var o in objects)
            {
                if (ImGui.TreeNode($"{o.Key}##{_version}"))
                {
                    ImGui.Indent();

                    try
                    {
                        Debug(o.Value);
                    }
                    catch (Exception e)
                    {
                        DebugWindow.LogError($"{Name} -> {e}");
                    }

                    ImGui.Unindent();
                    ImGui.TreePop();
                }
            }

            if (ImGui.TreeNode("UIHover"))
            {
                ImGui.Indent();

                try
                {
                    Debug(GameController.IngameState.UIHover);
                }
                catch (Exception e)
                {
                    DebugWindow.LogError($"UIHover -> {e}");
                }

                ImGui.Unindent();
                ImGui.TreePop();
            }

            if (ImGui.TreeNode("UIHover as Item"))
            {
                ImGui.Indent();

                try
                {
                    Debug(GameController.IngameState.UIHover.AsObject<HoverItemIcon>());
                }
                catch (Exception e)
                {
                    DebugWindow.LogError($"UIHover -> {e}");
                }

                ImGui.Unindent();
                ImGui.TreePop();
            }

            if (ImGui.TreeNode("Only visible InGameUi"))
            {
                ImGui.Indent();
                var os = GameController.IngameState.IngameUi.Children.Where(x => x.IsVisibleLocal);
                var index = 0;

                foreach (var el in os)
                {
                    try
                    {
                        if (ImGui.TreeNode($"{el.Address:X} - {el.X}:{el.Y},{el.Width}:{el.Height}##{el.GetHashCode()}"))
                        {
                            var keyForOffset = $"{el.Address}{el.GetHashCode()}";

                            if (OffsetFinder.TryGetValue(keyForOffset, out var offset))
                                ImGui.Text($"Offset: {offset:X}");
                            else
                            {
                                var IngameUi = GameController.IngameState.IngameUi;
                                var pointers = IngameUi.M.ReadPointersArray(IngameUi.Address, IngameUi.Address + 10000);

                                for (var i = 0; i < pointers.Count; i++)
                                {
                                    var p = pointers[i];
                                    if (p == el.Address) OffsetFinder[keyForOffset] = i * 0x8;
                                }
                            }

                            Debug(el);
                            ImGui.TreePop();
                        }

                        if (ImGui.IsItemHovered())
                        {
                            var clientRectCache = el.GetClientRectCache;
                            Graphics.DrawFrame(clientRectCache, ColorSwaper.Value, 1);

                            foreach (var element in el.Children)
                            {
                                clientRectCache = element.GetClientRectCache;
                                Graphics.DrawFrame(clientRectCache, ColorSwaper.Value, 1);
                            }
                        }

                        index++;
                    }
                    catch (Exception e)
                    {
                        DebugWindow.LogError($"UIHover -> {e}");
                    }
                }

                ImGui.Unindent();
                ImGui.TreePop();
            }

            if (DebugEntities.Count > 0 && ImGui.TreeNode($"Entities {DebugEntities.Count}"))
            {
                var camera = GameController.IngameState.Camera;

                for (var index = 0; index < DebugEntities.Count; index++)
                {
                    var debugEntity = DebugEntities[index];
                    var worldtoscreen = camera.WorldToScreen(debugEntity.Pos);
                    Graphics.DrawBox(worldtoscreen.TranslateToNum(-9, -9), worldtoscreen.TranslateToNum(18, 18), Color.Black);
                    Graphics.DrawText($"{index}", worldtoscreen);

                    if (ImGui.TreeNode($"[{index}] {debugEntity}"))
                    {
                        Debug(debugEntity);
                        ImGui.TreePop();
                    }
                }
            }

            ImGui.End();
        }

        public void Debug(object obj, Type type = null)
        {
            try
            {
                if (obj == null)
                {
                    ImGui.TextColored(Color.Red.ToImguiVec4(), "Null");
                    return;
                }

                if (type == null) type = obj.GetType();

                if (Convert.GetTypeCode(obj) == TypeCode.Object)
                {
                    var methodInfo = type.GetMethod("ToString", Type.EmptyTypes);

                    if (methodInfo != null && (methodInfo.Attributes & MethodAttributes.VtableLayoutMask) == 0)
                    {
                        var toString = methodInfo?.Invoke(obj, null);
                        if (toString != null) ImGui.TextColored(Color.Orange.ToImguiVec4(), toString.ToString());
                    }
                }

                if (type.BaseType == typeof(MulticastDelegate) && type.GenericTypeArguments.Length == 1)
                {
                    ImGui.TextColored(Color.Lime.ToImguiVec4(), type.GetMethod("Invoke")?.Invoke(obj, null).ToString());

                    return;
                }

                //IEnumerable from start
                var isEnumerable = IsEnumerable(type);

                if (isEnumerable)
                {
                    var collection = obj as ICollection;

                    if (collection != null)
                    {
                        var index = 0;

                        foreach (var col in collection)
                        {
                            var colType = col.GetType();
                            string colName;

                            switch (col)
                            {
                                case Entity e:
                                    colName = e.Path;
                                    break;
                                default:
                                    colName = colType.Name;

                                    break;
                            }

                            var methodInfo = colType.GetMethod("ToString", Type.EmptyTypes);

                            if (methodInfo != null && (methodInfo.Attributes & MethodAttributes.VtableLayoutMask) == 0)
                            {
                                var toString = methodInfo?.Invoke(col, null);
                                if (toString != null) colName = $"{toString}";
                            }

                            if (ImGui.TreeNode($"[{index}] {colName}"))
                            {
                                Debug(col, colType);

                                ImGui.TreePop();
                            }

                            index++;
                        }

                        ImGui.TreePop();

                        return;
                    }

                    var enumerable = obj as IEnumerable;

                    if (enumerable != null)
                    {
                        var index = 0;

                        foreach (var col in enumerable)
                        {
                            var colType = col.GetType();
                            string colName;

                            switch (col)
                            {
                                case Entity e:
                                    colName = e.Path;
                                    break;
                                default:
                                    colName = colType.Name;

                                    break;
                            }

                            var methodInfo = colType.GetMethod("ToString", Type.EmptyTypes);

                            if (methodInfo != null && (methodInfo.Attributes & MethodAttributes.VtableLayoutMask) == 0)
                            {
                                var toString = methodInfo?.Invoke(col, null);
                                if (toString != null) colName = $"{toString}";
                            }

                            if (ImGui.TreeNode($"[{index}] {colName}"))
                            {
                                Debug(col, colType);

                                ImGui.TreePop();
                            }

                            index++;
                        }

                        ImGui.TreePop();
                        return;
                    }
                }

                if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(KeyValuePair<,>))
                {
                    var key = type.GetProperty("Key").GetValue(obj, null);
                    var value = type.GetProperty("Value").GetValue(obj, null);
                    var valueType = value.GetType();

                    if (IsEnumerable(valueType))
                    {
                        var count = valueType.GetProperty("Count").GetValue(value, null);

                        if (ImGui.TreeNode($"{key} {count}"))
                        {
                            Debug(value);
                            ImGui.TreePop();
                        }
                    }
                }

                var isMemoryObject = obj as RemoteMemoryObject;

                if (isMemoryObject != null && isMemoryObject.Address == 0)
                {
                    ImGui.TextColored(Color.Red.ToImguiVec4(), "Address 0. Cant read this object.");
                    return;
                }

                ImGui.Indent();
                ImGui.BeginTabBar($"Tabs##{obj.GetHashCode()}");

                if (ImGui.BeginTabItem("Properties"))
                {
                    if (isMemoryObject != null)
                    {
                        ImGui.Text("Address: ");
                        ImGui.SameLine();
                        ImGui.PushStyleColor(ImGuiCol.Text, new ImGuiVector4(1, 0.647f, 0, 1));
                        ImGui.PushStyleColor(ImGuiCol.Button, new ImGuiVector4(0, 0, 0, 0));
                        ImGui.PushStyleColor(ImGuiCol.ButtonHovered, new ImGuiVector4(0.25f, 0.25f, 0.25f, 1));
                        ImGui.PushStyleColor(ImGuiCol.ButtonActive, new ImGuiVector4(1, 1, 1, 1));

                        if (ImGui.SmallButton($"{((RemoteMemoryObject) obj).Address:X}"))
                            ImGui.SetClipboardText($"{((RemoteMemoryObject) obj).Address:X}");

                        ImGui.PopStyleColor(4);

                        var asComponent = isMemoryObject as Component;

                        if (asComponent != null)
                        {
                            ImGui.Text("OwnerAddress: ");
                            ImGui.SameLine();
                            ImGui.PushStyleColor(ImGuiCol.Text, new ImGuiVector4(1, 0.647f, 0, 1));
                            ImGui.PushStyleColor(ImGuiCol.Button, new ImGuiVector4(0, 0, 0, 0));
                            ImGui.PushStyleColor(ImGuiCol.ButtonHovered, new ImGuiVector4(0.25f, 0.25f, 0.25f, 1));
                            ImGui.PushStyleColor(ImGuiCol.ButtonActive, new ImGuiVector4(1, 1, 1, 1));

                            if (ImGui.SmallButton($"{asComponent.OwnerAddress:X}")) ImGui.SetClipboardText($"{asComponent.OwnerAddress:X}");

                            ImGui.PopStyleColor(4);
                        }

                        switch (obj)
                        {
                            case Entity e:
                                if (ImGui.TreeNode($"Components: {e.CacheComp.Count}##e{e.Address}"))
                                {
                                    foreach (var component in e.CacheComp)
                                    {
                                        var compFullName = typeof(Positioned).AssemblyQualifiedName.Replace(nameof(Positioned), component.Key);
                                        var componentType = Type.GetType(compFullName);

                                        if (componentType == null)
                                        {
                                            ImGui.Text($"{component.Key}: Not implemented.");
                                            ImGui.SameLine();
                                            ImGui.PushStyleColor(ImGuiCol.Text, new ImGuiVector4(1, 0.647f, 0, 1));
                                            ImGui.PushStyleColor(ImGuiCol.Button, new ImGuiVector4(0, 0, 0, 0));
                                            ImGui.PushStyleColor(ImGuiCol.ButtonHovered, new ImGuiVector4(0.25f, 0.25f, 0.25f, 1));
                                            ImGui.PushStyleColor(ImGuiCol.ButtonActive, new ImGuiVector4(1, 1, 1, 1));

                                            if (ImGui.SmallButton($"{component.Value:X}")) ImGui.SetClipboardText($"{component.Value:X}");

                                            ImGui.PopStyleColor(4);

                                            continue;
                                        }

                                        if (!genericMethodCache.TryGetValue(component.Key, out var generic))
                                        {
                                            generic = GetComponentMethod.MakeGenericMethod(componentType);
                                            genericMethodCache[component.Key] = generic;
                                        }

                                        var g = generic.Invoke(e, null);

                                        if (ImGui.TreeNode($"{component.Key} ##{e.Address}"))
                                        {
                                            Debug(g);
                                            ImGui.TreePop();
                                        }
                                    }

                                    ImGui.TreePop();
                                }

                                if (e.HasComponent<Base>())
                                {
                                    if (ImGui.TreeNode($"Item info##{e.Address}"))
                                    {
                                        var BIT = GameController.Files.BaseItemTypes.Translate(e.Path);
                                        Debug(BIT);
                                        ImGui.TreePop();
                                    }
                                }

                                break;
                        }
                    }

                    var properties = type.GetProperties(Flags).Where(x => x.GetIndexParameters().Length == 0)
                        .OrderBy(x => x.PropertyType.GetInterface("IEnumerable") != null).ThenBy(x => x.Name);

                    foreach (var property in properties)
                    {
                        if (isMemoryObject != null && IgnoredPropertioes.Contains(property.Name)) continue;

                        var propertyValue = property.GetValue(obj);

                        if (propertyValue == null)
                        {
                            ImGui.Text($"{property.Name}: ");
                            ImGui.SameLine();
                            ImGui.TextColored(Color.Red.ToImguiVec4(), "Null");
                            continue;
                        }

                        //Draw primitives
                        if (IsSimpleType(property.PropertyType))
                        {
                            ImGui.Text($"{property.Name}: ");
                            ImGui.SameLine();
                            ImGui.PushStyleColor(ImGuiCol.Text, new ImGuiVector4(1, 0.647f, 0, 1));
                            ImGui.PushStyleColor(ImGuiCol.Button, new ImGuiVector4(0, 0, 0, 0));
                            ImGui.PushStyleColor(ImGuiCol.ButtonHovered, new ImGuiVector4(0.25f, 0.25f, 0.25f, 1));
                            ImGui.PushStyleColor(ImGuiCol.ButtonActive, new ImGuiVector4(1, 1, 1, 1));

                            if (ImGui.SmallButton(propertyValue.ToString())) ImGui.SetClipboardText(propertyValue.ToString());

                            ImGui.PopStyleColor(4);
                        }
                        else
                        {
                            //Draw enumrable 
                            isEnumerable = IsEnumerable(property.PropertyType);

                            if (isEnumerable)
                            {
                                var collection = propertyValue as ICollection;

                                if (collection == null)
                                    continue;

                                if (collection.Count > 0)
                                {
                                    ImGui.TextColored(Color.OrangeRed.ToImguiVec4(), $"[{collection.Count}]");

                                    var isElementEnumerable = property.PropertyType.GenericTypeArguments.Length == 1 &&
                                                              (property.PropertyType.GenericTypeArguments[0] == typeof(Element) ||
                                                               property.PropertyType.GenericTypeArguments[0].IsSubclassOf(typeof(Element)));

                                    if (isElementEnumerable)
                                    {
                                        if (ImGui.IsItemHovered())
                                        {
                                            var index = 0;

                                            foreach (var el in collection)
                                            {
                                                if (el is Element e)
                                                {
                                                    var clientRectCache = e.GetClientRectCache;
                                                    Graphics.DrawFrame(clientRectCache, ColorSwaper.Value, 1);
                                                    Graphics.DrawText(index.ToString(), clientRectCache.Center);
                                                    index++;
                                                }
                                            }
                                        }
                                    }

                                    ImGui.SameLine();
                                    var strId = $"{property.Name} ##{type.FullName}";

                                    if (ImGui.TreeNode(strId))
                                    {
                                        var skipId = $"{strId} {obj.GetHashCode()}";

                                        if (Skips.TryGetValue(skipId, out var skip))
                                        {
                                            ImGui.InputInt("Skip", ref skip, 1, 100);
                                            Skips[skipId] = skip;
                                        }
                                        else
                                            Skips[skipId] = 0;

                                        var index = -1;

                                        foreach (var col in collection)
                                        {
                                            index++;

                                            if (index < skip)
                                                continue;

                                            if (col == null)
                                            {
                                                ImGui.TextColored(Color.Red.ToImguiVec4(), "Null");
                                                continue;
                                            }

                                            var colType = col.GetType();
                                            string colName;

                                            switch (col)
                                            {
                                                case Entity e:
                                                    colName = e.Path;
                                                    break;
                                                case Inventory e:
                                                    colName = $"{e.InvType} Count: ({e.ItemCount}) Box:{e.TotalBoxesInInventoryRow}";
                                                    break;
                                                case Element e when e.Text?.Length > 0:
                                                    colName = $"{e.Text}##{index}";
                                                    break;
                                                case Element e:
                                                    colName = $"{colType.Name}";
                                                    break;
                                                default:
                                                    colName = $"{colType.Name}";
                                                    break;
                                            }

                                            if (IsSimpleType(colType))
                                                ImGui.TextUnformatted(col.ToString());
                                            else
                                            {
                                                Element element = null;

                                                if (isElementEnumerable)
                                                {
                                                    element = col as Element;

                                                    //  colName += $" ({element.ChildCount})";
                                                    ImGui.Text($" ({element.ChildCount})");
                                                    ImGui.SameLine();
                                                }
                                                else
                                                {
                                                    var methodInfo = colType.GetMethod("ToString", Type.EmptyTypes);

                                                    if (methodInfo != null &&
                                                        (methodInfo.Attributes & MethodAttributes.VtableLayoutMask) == 0)
                                                    {
                                                        var toString = methodInfo?.Invoke(col, null);
                                                        if (toString != null) colName = $"{toString}";
                                                    }
                                                }

                                                if (ImGui.TreeNode($"[{index}] {colName} ##{col.GetHashCode()}"))
                                                {
                                                    if (element != null && ImGui.IsItemHovered())
                                                    {
                                                        if (element.Width > 0 && element.Height > 0)
                                                            Graphics.DrawFrame(element.GetClientRectCache, ColorSwaper.Value, 2);
                                                    }

                                                    Debug(col, colType);

                                                    ImGui.TreePop();
                                                }

                                                if (isElementEnumerable && element != null)
                                                {
                                                    if (ImGui.IsItemHovered())
                                                    {
                                                        if (element.Width > 0 && element.Height > 0)
                                                            Graphics.DrawFrame(element.GetClientRectCache, ColorSwaper.Value, 2);
                                                    }
                                                }
                                            }

                                            if (index - skip > Settings.LimitForCollection)
                                            {
                                                ImGui.TreePop();
                                                break;
                                            }
                                        }

                                        ImGui.TreePop();
                                    }
                                }
                                else
                                {
                                    ImGui.Indent();
                                    ImGui.TextColored(Color.Red.ToImguiVec4(), $"{property.Name} [Empty]");
                                    ImGui.Unindent();
                                }
                            }

                            //Debug others objects
                            else
                            {
                                if (property.Name.Equals("Value"))
                                    Debug(propertyValue);
                                else
                                {
                                    string name;
                                    var isMemoryObj = propertyValue is RemoteMemoryObject;

                                    if (isMemoryObj)
                                        name = $"{property.Name} [{((RemoteMemoryObject) propertyValue).Address:X}]##{type.FullName}";
                                    else
                                        name = $"{property.Name} ##{type.FullName}";

                                    if (ImGui.TreeNode(name))
                                    {
                                        Debug(propertyValue);

                                        ImGui.TreePop();
                                    }

                                    if (isMemoryObj)
                                    {
                                        switch (propertyValue)
                                        {
                                            case Element e:

                                                if (ImGui.IsItemHovered())
                                                {
                                                    if (e.Width > 0 && e.Height > 0)
                                                        Graphics.DrawFrame(e.GetClientRectCache, ColorSwaper.Value, 2);
                                                }

                                                break;
                                        }
                                    }
                                }
                            }
                        }
                    }

                    ImGui.EndTabItem();
                }

                if (ImGui.BeginTabItem("Fields"))
                {
                    /*var strId = $"Fields##{obj.GetHashCode()}";
                    if (ImGui.TreeNode(strId))
                    {*/
                    var fields = type.GetFields(Flags);

                    foreach (var field in fields)
                    {
                        var fieldValue = field.GetValue(obj);

                        if (IsSimpleType(field.FieldType))

                            //if(Convert.GetTypeCode(fieldValue) != TypeCode.Object)
                        {
                            ImGui.Text($"{field.Name}: ");
                            ImGui.SameLine();
                            ImGui.PushStyleColor(ImGuiCol.Text, new ImGuiVector4(1, 0.647f, 0, 1));
                            ImGui.PushStyleColor(ImGuiCol.Button, new ImGuiVector4(0, 0, 0, 0));
                            ImGui.PushStyleColor(ImGuiCol.ButtonHovered, new ImGuiVector4(0.25f, 0.25f, 0.25f, 1));
                            ImGui.PushStyleColor(ImGuiCol.ButtonActive, new ImGuiVector4(1, 1, 1, 1));

                            if (ImGui.SmallButton($"{fieldValue}")) ImGui.SetClipboardText($"{fieldValue}");

                            ImGui.PopStyleColor(4);
                        }
                        else
                        {
                            if (ImGui.TreeNode($"{field.Name} {type.FullName}"))
                            {
                                Debug(fieldValue);
                                ImGui.TreePop();
                            }
                        }
                    }

                    /*ImGui.TreePop();
                }*/
                    ImGui.EndTabItem();
                }

                if (isMemoryObject != null && ImGui.BeginTabItem("Dynamic"))
                {
                    var remoteMemoryObject = (RemoteMemoryObject) obj;
                    ImGui.TextColored(Color.GreenYellow.ToImguiVec4(), "Address: ");
                    if (ImGui.IsItemClicked()) ImGui.SetClipboardText(remoteMemoryObject.Address.ToString());

                    ImGui.SameLine();

                    ImGui.PushStyleColor(ImGuiCol.Text, new ImGuiVector4(1, 0.647f, 0, 1));
                    ImGui.PushStyleColor(ImGuiCol.Button, new ImGuiVector4(0, 0, 0, 0));
                    ImGui.PushStyleColor(ImGuiCol.ButtonHovered, new ImGuiVector4(0.25f, 0.25f, 0.25f, 1));
                    ImGui.PushStyleColor(ImGuiCol.ButtonActive, new ImGuiVector4(1, 1, 1, 1));

                    if (ImGui.SmallButton($"{remoteMemoryObject.Address:X}")) ImGui.SetClipboardText(remoteMemoryObject.Address.ToString());

                    ImGui.PopStyleColor(4);
                    ImGui.EndTabItem();

                    switch (remoteMemoryObject)
                    {
                        case Entity e:
                            var key = $"{e.Address}{e.Id}{e.Path}";

                            if (e.GetComponent<Render>() != null)
                            {
                                if (ImGui.TreeNode($"World position##{remoteMemoryObject.Address}"))
                                {
                                    var pos = e.Pos;
                                    var renderComponentBounds = e.GetComponent<Render>().Bounds;
                                    var toScreen = GameController.IngameState.Camera.WorldToScreen(pos);

                                    ImGui.Text($"Position: {pos}");
                                    ImGui.Text($"To Screen: {toScreen}");

                                    Graphics.DrawFrame(toScreen.ToVector2Num(),
                                        toScreen.TranslateToNum(renderComponentBounds.X, -renderComponentBounds.Y),
                                        Color.Orange, 0, 1, 0);
                                }
                            }

                            break;
                        case Element e:
                            var keyForOffsetFinder = $"{e.Address}{e.GetHashCode()}";

                            if (OffsetFinder.TryGetValue(keyForOffsetFinder, out var offset))
                                ImGui.TextColored(Color.Aqua.ToImguiVec4(), $"Offset: {offset:X}");
                            else
                            {
                                var parent = e.Parent;

                                if (parent != null)
                                {
                                    var pointers = e.M.ReadPointersArray(parent.Address, parent.Address + 8000 * 0x8);
                                    var found = false;

                                    for (var index = 0; index < pointers.Count; index++)
                                    {
                                        var p = pointers[index];

                                        if (p == e.Address)
                                        {
                                            OffsetFinder[keyForOffsetFinder] = index * 0x8;
                                            found = true;
                                            break;
                                        }
                                    }

                                    if (!found) OffsetFinder[keyForOffsetFinder] = -1;
                                }
                                else
                                    OffsetFinder[keyForOffsetFinder] = -1;
                            }

                            //if (ImGui.Button($"Change Visible##{e.GetHashCode()}"))
                            //{
                            //    var currentState = e.IsVisibleLocal;
                            //    var offsetELementIsVivible = Extensions.GetOffset<ElementOffsets>(nameof(ElementOffsets.IsVisibleLocal));
                            //    var b = (byte) (currentState ? 19 : 23);

                            //    var writeProcessMemory = ProcessMemory.WriteProcessMemory(e.M.OpenProcessHandle,
                            //        new IntPtr(e.Address + offsetELementIsVivible),
                            //        b);
                            //}

                            break;
                    }
                }

                ImGui.EndTabBar();
                ImGui.Unindent();
            }
            catch (Exception e)
            {
                LogError($"{Name} -> {e}");
            }
        }
    }
}
