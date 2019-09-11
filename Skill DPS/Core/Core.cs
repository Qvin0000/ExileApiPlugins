using System;
using System.Globalization;
using Exile;
using Shared.Enums;
using SharpDX;
using Skill_DPS.Skill_Data;
using Vector2 = System.Numerics.Vector2;

namespace Skill_DPS.Core
{
    public class Core : BaseSettingsPlugin<Settings>
    {
        public override void Render()
        {
            base.Render();
            var hoverUi = GameController.Game.IngameState.UIHoverTooltip.Tooltip;

            var skills = SkillBar.CurrentSkills();
            foreach (var skill in skills)
            {
                var box = skill.SkillElement.GetClientRect();
                var newBox = new RectangleF(box.X, box.Y - 2, box.Width, -15);

                decimal value;

                if (hoverUi.GetClientRect().Intersects(newBox) && hoverUi.IsVisibleLocal) continue;

                if (skill.Skill.Stats.TryGetValue(GameStat.HundredTimesDamagePerSecond, out var val0))
                    value = val0 / (decimal) 100d;
                else if (skill.Skill.Stats.TryGetValue(GameStat.HundredTimesAttacksPerSecond, out var val1))
                    value = val1 / (decimal) 100d;
                else if (skill.Skill.Stats.TryGetValue(GameStat.HundredTimesAverageDamagePerSkillUse, out var val2))
                    value = val2 / (decimal) 100d;
                else if (skill.Skill.Stats.TryGetValue(GameStat.IntermediaryFireSkillDotDamageToDealPerMinute, out var val3))
                    value = val3 / (decimal) 60;
                else if (skill.Skill.Stats.TryGetValue(GameStat.BaseSkillShowAverageDamageInsteadOfDps, out var val4))
                    value = val4 / (decimal) 100d;
                else
                    continue;

                //
                // if (value <= 0) continue;

                var pos = new Vector2(newBox.Center.X, newBox.Center.Y - Settings.FontSize / 2f);
                Graphics.DrawText(ToKmb(Convert.ToDecimal(value)), pos, Settings.FontColor, Settings.FontSize, FontAlign.Center);

                Graphics.DrawBox(newBox, Settings.BackgroundColor);
                Graphics.DrawFrame(newBox, Settings.BorderColor, 1);
            }
        }

        public static string ToKmb(decimal num)
        {
            if (num > 999999999) return num.ToString("0,,,.###B", CultureInfo.InvariantCulture);
            if (num > 999999) return num.ToString("0,,.##M", CultureInfo.InvariantCulture);
            if (num > 999) return num.ToString("0,.##K", CultureInfo.InvariantCulture);

            return num.ToString("0.#", CultureInfo.InvariantCulture);
        }
    }
}
