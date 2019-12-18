using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Forms;
using ExileCore;
using ExileCore.PoEMemory;
using ExileCore.PoEMemory.Components;
using ExileCore.PoEMemory.MemoryObjects;
using ExileCore.Shared.Cache;
using ExileCore.Shared.Enums;
using SharpDX;
using Vector2 = System.Numerics.Vector2;

namespace Skill_DPS.Core
{
    public class DrawSkills
    {
        
    }
    public class SkillDpsCore : BaseSettingsPlugin<Settings>
    {
        private TimeCache<bool> _cachedValue;
        List<Data> skills =new List<Data>();

        public override void OnLoad()
        {
            Input.RegisterKey(Keys.LControlKey);
            Settings.UpdateInterval.OnValueChanged += (sender, i) =>
            {
                _cachedValue.NewTime(i);
            };
            _cachedValue = new TimeCache<bool>(SkillDPSLogic,Settings.UpdateInterval);
        }

        private bool SkillDPSLogic()
        {
            var hoverUi = GameController.Game.IngameState.UIHoverTooltip.Tooltip;

            var ids = GameController.IngameState.ServerData.SkillBarIds;

            if (ids == null || ids.Count == 0)
            {
                LogError("SkillBarIds is null or empty. Check offsets");
                return false;
            }

            var actorSkills = RemoteMemoryObject.pTheGame.IngameState.Data.LocalPlayer.GetComponent<Actor>().ActorSkills;

            if (actorSkills == null || actorSkills.Count == 0)
            {
                LogError("ActorSkills is null or empty. Check offsets");
                return false;
            }

            var skillbarChildren = RemoteMemoryObject.pTheGame.IngameState.IngameUi.SkillBar.Children;

            if (skillbarChildren == null || skillbarChildren.Count == 0)
            {
                LogError("SkillBar.Children is null or empty. Check offsets");
                return false;
            }

            if (skillbarChildren.Count < 8)
            {
                LogError("Expecting at least 8 childs in SkillBar.Children");
                return false;
            }

            skills = CurrentSkills(ids, actorSkills, skillbarChildren);

            if (skills == null || skills.Count == 0)
            {
                LogError("skills is null or empty");
                return false;
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

                skill.Value = value;
                skill.Box = newBox;
                skill.Pos = new Vector2(newBox.Center.X, newBox.Center.Y - Settings.FontSize / 2f);
            }

            return true;
        }


        public override void Render()
        {
            if (!_cachedValue.Value)
            {
                return;
            }

            foreach (var skill in skills)
            {
                if (skill.Box.Location == SharpDX.Vector2.Zero || !skill.SkillElement.IsVisible)
                    continue;

                Graphics.DrawText(ToKmb(Convert.ToDecimal(skill.Value)), skill.Pos, Settings.FontColor, Settings.FontSize, FontAlign.Center);
                Graphics.DrawBox(skill.Box, Settings.BackgroundColor);
                Graphics.DrawFrame(skill.Box, Settings.BorderColor, 1);
            }
            
          
        }

        public static string ToKmb(decimal num)
        {
            if (num > 999999999) return num.ToString("0,,,.###B", CultureInfo.InvariantCulture);
            if (num > 999999) return num.ToString("0,,.##M", CultureInfo.InvariantCulture);
            if (num > 999) return num.ToString("0,.##K", CultureInfo.InvariantCulture);

            return num.ToString("0.#", CultureInfo.InvariantCulture);
        }

        public static List<Data> CurrentSkills(IList<ushort> ids, List<ActorSkill> actorSkills, IList<Element> skillbarChildren)
        {
            var returnSkills = new List<Data>();

            for (var index = 0; index < ids.Count; index++)
            {
                var skillId = ids[index];

                if (skillId == 0)
                    continue;

                for (var i = actorSkills.Count - 1; i >= 0; i--)
                {
                    var actorSkill = actorSkills[i];

                    if (actorSkill.Id == skillId)
                    {
                        returnSkills.Add(new Data
                        {
                            Skill = actorSkill,
                            SkillElement = skillbarChildren[index]
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

            public RectangleF Box { get; set; }
            public decimal Value { get; set; }
            public Vector2 Pos { get; set; }
        }
    }
}
