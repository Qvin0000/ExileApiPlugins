using System;
using System.Collections.Generic;
using System.Globalization;
using ExileCore;
using ExileCore.PoEMemory;
using ExileCore.PoEMemory.Components;
using ExileCore.PoEMemory.MemoryObjects;
using ExileCore.Shared.Enums;
using SharpDX;
using Vector2 = System.Numerics.Vector2;

namespace Skill_DPS.Core
{
    public class SkillDpsCore : BaseSettingsPlugin<Settings>
    {
        public override void Render()
        {
            base.Render();
            var hoverUi = GameController.Game.IngameState.UIHoverTooltip.Tooltip;

            var ids = GameController.IngameState.ServerData.SkillBarIds;

            if (ids == null || ids.Count == 0)
            {
                LogError("SkillBarIds is null or empty. Check offsets");
                return;
            }

            var actorSkills = RemoteMemoryObject.pTheGame.IngameState.Data.LocalPlayer.GetComponent<Actor>().ActorSkills;

            if (actorSkills == null || actorSkills.Count == 0)
            {
                LogError("ActorSkills is null or empty. Check offsets");
                return;
            }

            var skills = CurrentSkills(ids, actorSkills);

            if (skills == null || skills.Count == 0)
            {
                LogError("skills is null or empty");
                return;
            }

            foreach (var skill in skills)
            {
                var box = skill.SkillElement.GetClientRect();
                var newBox = new RectangleF(box.X, box.Y - 2, box.Width, -15);

                decimal value;
                if (hoverUi != null && hoverUi.GetClientRect().Intersects(newBox) && hoverUi.IsVisibleLocal) continue;

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

        public static List<Data> CurrentSkills(IList<ushort> ids, List<ActorSkill> actorSkills)
        {
            var returnSkills = new List<Data>();

            for (var index = 0; index < ids.Count; index++)
            {
                var skillId = ids[index];

                if (skillId == 0)
                    continue;

                foreach (var actorSkill in actorSkills)
                {
                    if (actorSkill.Id == skillId)
                    {
                        returnSkills.Add(new Data
                        {
                            Skill = actorSkill,
                            SkillElement = RemoteMemoryObject.pTheGame.IngameState.IngameUi.SkillBar.Children[index]
                        });

                        break;
                    }
                }
            }

            return returnSkills;
        }

        public class Data
        {
            public ActorSkill Skill { get; set; }
            public Element SkillElement { get; set; }
        }
    }
}
