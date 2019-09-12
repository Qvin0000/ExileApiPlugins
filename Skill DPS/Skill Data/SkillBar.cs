using System.Collections.Generic;
using ExileCore.PoEMemory;
using ExileCore.PoEMemory.Components;
using ExileCore.PoEMemory.MemoryObjects;

namespace Skill_DPS.Skill_Data
{
    public class SkillBar
    {
        public static List<Data> CurrentSkills()
        {
            var returnSkills = new List<Data>();
            var ids = RemoteMemoryObject.pTheGame.IngameState.ServerData.SkillBarIds;

            for (var index = 0; index < ids.Count; index++)
            {
                var skillId = ids[index];

                if (skillId == 0)
                    continue;

                if (GetSkill(skillId) == null) continue;

                returnSkills.Add(new Data
                {
                    Skill = GetSkill(skillId),
                    SkillElement = RemoteMemoryObject.pTheGame.IngameState.IngameUi.SkillBar.Children[index]
                });
            }

            return returnSkills;
        }

        public static ActorSkill GetSkill(ushort ID)
        {
            var actor = RemoteMemoryObject.pTheGame.IngameState.Data.LocalPlayer.GetComponent<Actor>();

            foreach (var actorSkill in actor.ActorSkills)
            {
                if (actorSkill.Id != ID) continue;

                return actorSkill;
            }

            return null;
        }

        public class Data
        {
            public ActorSkill Skill { get; set; }
            public Element SkillElement { get; set; }
        }
    }
}
