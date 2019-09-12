using System.Collections.Generic;
using System.Diagnostics;
using ExileCore.Shared.Enums;
using ExileCore.Shared.Static;
using SharpDX;

namespace ItemAlert
{
    [DebuggerDisplay("{Text}")]
    public sealed class AlertDrawStyle
    {
        public static readonly Color DefaultBackgroundColor = new ColorBGRA(0, 0, 0, 180);

        private static readonly Dictionary<ItemRarity, Color> colors = new Dictionary<ItemRarity, Color>
        {
            {ItemRarity.Normal, Color.White},
            {ItemRarity.Magic, HudSkin.MagicColor},
            {ItemRarity.Rare, HudSkin.RareColor},
            {ItemRarity.Unique, HudSkin.UniqueColor}
        };

        public AlertDrawStyle(object colorRef, int borderWidth, string text, int iconIndex)
        {
            BorderWidth = borderWidth;
            Text = text;
            IconIndex = iconIndex;

            if (colorRef is Color)
                TextColor = (Color) colorRef;
            else
                TextColor = GetTextColorByRarity((ItemRarity) colorRef);

            BorderColor = TextColor;
            BackgroundColor = DefaultBackgroundColor;
        }

        public AlertDrawStyle(string text, Color textColor, int borderWidth, Color borderColor, Color backgroundColor, int iconIndex)
        {
            TextColor = textColor;
            BorderWidth = borderWidth;
            BorderColor = borderColor;
            Text = text;
            IconIndex = iconIndex;
            BackgroundColor = backgroundColor;
        }

        public Color TextColor { get; }
        public int BorderWidth { get; }
        public Color BorderColor { get; }
        public Color BackgroundColor { get; }
        public string Text { get; }
        public int IconIndex { get; }

        public static Color GetTextColorByRarity(ItemRarity itemRarity)
        {
            Color tempColor;
            return colors.TryGetValue(itemRarity, out tempColor) ? tempColor : Color.White;
        }
    }
}
