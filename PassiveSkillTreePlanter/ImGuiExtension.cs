using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using ExileCore.Shared.Nodes;
using ImGuiNET;
using SharpDX;
using ImGuiVector2 = System.Numerics.Vector2;
using ImGuiVector4 = System.Numerics.Vector4;

namespace PassiveSkillTreePlanter
{
    public class ImGuiExtension
    {
        public static bool BeginWindow(string title, ref bool isOpened, int x, int y, int width, int height, bool autoResize = false)
        {
            ImGui.SetNextWindowPos(new ImGuiVector2(width + x, height + y), ImGuiCond.Appearing, new ImGuiVector2(1, 1));
            ImGui.SetNextWindowSize(new ImGuiVector2(width, height), ImGuiCond.Appearing);
            return ImGui.Begin(title);
        }

        public static bool BeginWindow(string title, ref bool isOpened, int x, int y, int width, int height, ImGuiWindowFlags flags, bool autoResize = false)
        {
            ImGui.SetNextWindowPos(new ImGuiVector2(width + x, height + y), ImGuiCond.Appearing, new ImGuiVector2(1, 1));
            ImGui.SetNextWindowSize(new ImGuiVector2(width, height), ImGuiCond.Appearing);
            return ImGui.Begin(title, ref isOpened, autoResize ? ImGuiWindowFlags.AlwaysAutoResize | flags : ImGuiWindowFlags.None | flags);
        }

        public static bool BeginWindow(string title, ref bool isOpened, float x, float y, float width, float height, bool autoResize = false)
        {
            ImGui.SetNextWindowPos(new ImGuiVector2(width + x, height + y), ImGuiCond.Appearing, new ImGuiVector2(1, 1));
            ImGui.SetNextWindowSize(new ImGuiVector2(width, height), ImGuiCond.Appearing);
            return ImGui.Begin(title);
        }
        // Int Sliders
        public static int IntSlider(string labelString, int value, int minValue, int maxValue)
        {
            var refValue = value;
            ImGui.SliderInt(labelString, ref refValue, minValue, maxValue, "%.00f");
            return refValue;
        }

        public static int IntSlider(string labelString, string sliderString, int value, int minValue, int maxValue)
        {
            var refValue = value;
            ImGui.SliderInt(labelString, ref refValue, minValue, maxValue, $"{sliderString}: {value}");
            return refValue;
        }

        public static int IntSlider(string labelString, RangeNode<int> setting)
        {
            var refValue = setting.Value;
            ImGui.SliderInt(labelString, ref refValue, setting.Min, setting.Max);
            return refValue;
        }

        public static int IntSlider(string labelString, string sliderString, RangeNode<int> setting)
        {
            var refValue = setting.Value;
            ImGui.SliderInt(labelString, ref refValue, setting.Min, setting.Max, $"{sliderString}: {setting.Value}");
            return refValue;
        }

        // float Sliders
        public static float FloatSlider(string labelString, float value, float minValue, float maxValue)
        {
            var refValue = value;
            ImGui.SliderFloat(labelString, ref refValue, minValue, maxValue, "%.00f", 1f);
            return refValue;
        }

        public static float FloatSlider(string labelString, float value, float minValue, float maxValue, float power)
        {
            var refValue = value;
            ImGui.SliderFloat(labelString, ref refValue, minValue, maxValue, "%.00f", power);
            return refValue;
        }

        public static float FloatSlider(string labelString, string sliderString, float value, float minValue, float maxValue)
        {
            var refValue = value;
            ImGui.SliderFloat(labelString, ref refValue, minValue, maxValue, $"{sliderString}: {value}", 1f);
            return refValue;
        }

        public static float FloatSlider(string labelString, string sliderString, float value, float minValue, float maxValue, float power)
        {
            var refValue = value;
            ImGui.SliderFloat(labelString, ref refValue, minValue, maxValue, $"{sliderString}: {value}", power);
            return refValue;
        }

        public static float FloatSlider(string labelString, RangeNode<float> setting)
        {
            var refValue = setting.Value;
            ImGui.SliderFloat(labelString, ref refValue, setting.Min, setting.Max, "%.00f", 1f);
            return refValue;
        }

        public static float FloatSlider(string labelString, RangeNode<float> setting, float power)
        {
            var refValue = setting.Value;
            ImGui.SliderFloat(labelString, ref refValue, setting.Min, setting.Max, "%.00f", power);
            return refValue;
        }

        public static float FloatSlider(string labelString, string sliderString, RangeNode<float> setting)
        {
            var refValue = setting.Value;
            ImGui.SliderFloat(labelString, ref refValue, setting.Min, setting.Max, $"{sliderString}: {setting.Value}", 1f);
            return refValue;
        }

        public static float FloatSlider(string labelString, string sliderString, RangeNode<float> setting, float power)
        {
            var refValue = setting.Value;
            ImGui.SliderFloat(labelString, ref refValue, setting.Min, setting.Max, $"{sliderString}: {setting.Value}", power);
            return refValue;
        }

        // Checkboxes
        public static bool Checkbox(string labelString, bool boolValue)
        {
            ImGui.Checkbox(labelString, ref boolValue);
            return boolValue;
        }

        public static bool Checkbox(string labelString, bool boolValue, out bool outBool)
        {
            ImGui.Checkbox(labelString, ref boolValue);
            outBool = boolValue;
            return boolValue;
        }

        // Hotkey Selector
        public static IEnumerable<Keys> KeyCodes()
        {
            return Enum.GetValues(typeof(Keys)).Cast<Keys>();
        }

        // Color Pickers
        public static Color ColorPicker(string labelName, Color inputColor)
        {
            var color = inputColor.ToVector4();
            var colorToVect4 = new ImGuiVector4(color.X, color.Y, color.Z, color.W);

            if (ImGui.ColorEdit4(labelName, ref colorToVect4, ImGuiColorEditFlags.AlphaBar))
                return new Color(colorToVect4.X, colorToVect4.Y, colorToVect4.Z, colorToVect4.W);

            return inputColor;
        }

        // Combo Box

        public static string ComboBox(string sideLabel, string currentSelectedItem, List<string> objectList,
            ImGuiComboFlags comboFlags = ImGuiComboFlags.HeightRegular)
        {
            if (ImGui.BeginCombo(sideLabel, currentSelectedItem, comboFlags))
            {
                var refObject = currentSelectedItem;

                for (var n = 0; n < objectList.Count; n++)
                {
                    var isSelected = refObject == objectList[n];

                    if (ImGui.Selectable(objectList[n], isSelected))
                    {
                        ImGui.EndCombo();
                        return objectList[n];
                    }

                    if (isSelected) ImGui.SetItemDefaultFocus();
                }

                ImGui.EndCombo();
            }

            return currentSelectedItem;
        }

        public static string ComboBox(string sideLabel, string currentSelectedItem, List<string> objectList, out bool didChange,
            ImGuiComboFlags comboFlags = ImGuiComboFlags.HeightRegular)
        {
            if (ImGui.BeginCombo(sideLabel, currentSelectedItem, comboFlags))
            {
                var refObject = currentSelectedItem;

                for (var n = 0; n < objectList.Count; n++)
                {
                    var isSelected = refObject == objectList[n];

                    if (ImGui.Selectable(objectList[n], isSelected))
                    {
                        didChange = true;
                        ImGui.EndCombo();
                        return objectList[n];
                    }

                    if (isSelected) ImGui.SetItemDefaultFocus();
                }

                ImGui.EndCombo();
            }

            didChange = false;
            return currentSelectedItem;
        }

        public static unsafe string InputText(string label, string currentValue, uint maxLength, ImGuiInputTextFlags flags)
        {
            var currentStringBytes = Encoding.Default.GetBytes(currentValue);
            var buffer = new byte[maxLength];
            Array.Copy(currentStringBytes, buffer, Math.Min(currentStringBytes.Length, maxLength));
            int cursor_pos = -1;
            int Callback(ImGuiInputTextCallbackData* data)
            {
                int* p_cursor_pos = (int*)data->UserData;

                if (ImGuiNative.ImGuiInputTextCallbackData_HasSelection(data) == 0)
                    *p_cursor_pos = data->CursorPos;
                return 0;
            }

            ImGui.InputText(label, buffer, maxLength, flags, Callback, (IntPtr)(&cursor_pos));
            return Encoding.Default.GetString(buffer).TrimEnd('\0').TrimEnd('\u0000').Replace("\u0000", "");
        }

        public static unsafe string MultiLineTextBox(string label, string currentValue, uint maxLength, ImGuiVector2 vect2, ImGuiInputTextFlags flags)
        {
            var testString = currentValue;
            var currentStringBytes = Encoding.Default.GetBytes(currentValue);
            var buffer = new byte[maxLength];
            Array.Copy(currentStringBytes, buffer, Math.Min(currentStringBytes.Length, maxLength));
            int cursor_pos = -1;
            int Callback(ImGuiInputTextCallbackData* data)
            {
                int* p_cursor_pos = (int*)data->UserData;

                if (ImGuiNative.ImGuiInputTextCallbackData_HasSelection(data) == 0)
                    *p_cursor_pos = data->CursorPos;
                return 0;
            }

            ImGui.InputTextMultiline(label, ref currentValue, maxLength, vect2, flags, Callback);
            return currentValue.TrimEnd('\0').TrimEnd('\u0000').Replace("\u0000", "");
        }
    }
}
