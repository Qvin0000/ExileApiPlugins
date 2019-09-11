using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Exile.PoEMemory.MemoryObjects;
using Shared.Enums;
using Shared.Static;
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

        public AlertDrawStyle(object colorRef, int borderWidth, string text, int iconIndex) {
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

        public static Color GetTextColorByRarity(ItemRarity itemRarity) {
            Color tempColor;
            return colors.TryGetValue(itemRarity, out tempColor) ? tempColor : Color.White;
        }

        public AlertDrawStyle(string text, Color textColor, int borderWidth, Color borderColor, Color backgroundColor, int iconIndex) {
            TextColor = textColor;
            BorderWidth = borderWidth;
            BorderColor = borderColor;
            Text = text;
            IconIndex = iconIndex;
            BackgroundColor = backgroundColor;
        }

        public Color TextColor { get; }
        public int BorderWidth { get; private set; }
        public Color BorderColor { get; private set; }
        public Color BackgroundColor { get; private set; }
        public string Text { get; private set; }
        public int IconIndex { get; private set; }
    }
}