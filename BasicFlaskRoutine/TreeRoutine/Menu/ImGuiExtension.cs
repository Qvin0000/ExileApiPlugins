using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ExileCore;
using ImGuiNET;
using ExileCore.Shared;
using SharpDX;
using ExileCore.Shared.Nodes;
using ImGuiVector2 = System.Numerics.Vector2;
using ImGuiVector4 = System.Numerics.Vector4;

namespace TreeRoutine.Menu
{
    public class ImGuiExtension
    {

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
        public static IEnumerable<Keys> KeyCodes() => Enum.GetValues(typeof(Keys)).Cast<Keys>();

        // Tooltip Hover
        public static void ToolTipWithText(string text, string desc)
        {
            ImGui.SameLine();
            ImGui.TextDisabled(text);
            if (ImGui.IsItemHovered(ImGuiHoveredFlags.AnyWindow))
            {
                ImGui.SetTooltip(desc);
            }
        }

        public static Keys HotkeySelector(string buttonName, Keys currentKey)
        {
            var open = true;
            if (ImGui.Button(buttonName))
            {
                ImGui.OpenPopup(buttonName);
                open = true;
            }

            if (ImGui.BeginPopupModal(buttonName, ref open, (ImGuiWindowFlags) 35))
            {
                if (Input.GetKeyState(Keys.Escape))
                {
                    ImGui.CloseCurrentPopup();
                    ImGui.EndPopup();
                }
                else
                {
                    foreach (var key in Enum.GetValues(typeof(Keys)))
                    {
                        var keyState = Input.GetKeyState((Keys) key);
                        if (keyState)
                        {
                            currentKey = (Keys) key;
                            ImGui.CloseCurrentPopup();
                            break;
                        }
                    }
                }

                ImGui.Text($" Press new key to change '{currentKey}' or Esc for exit.");

                ImGui.EndPopup();
            }

            return currentKey;
        }

        public static void SpacedTextHeader(string text)
        {
            ImGui.Spacing();
            ImGui.Separator();
            ImGui.Spacing();
            ImGui.Text(text);
            ImGui.Separator();
            ImGui.Spacing();
        }
    }
}