using System;
using System.Collections.Generic;
using System.Linq;
using ExileCore;
using ExileCore.PoEMemory.Components;
using ExileCore.PoEMemory.MemoryObjects;
using ExileCore.Shared.Enums;
using SharpDX;

namespace MineDetonator
{
    public class Core : BaseSettingsPlugin<Settings>
    {
        private DateTime LastDetonTime;

        public override void Render()
        {
            if (!GameController.InGame)
                return;
            var actor = GameController.Player.GetComponent<Actor>();
            var deployedObjects = actor.DeployedObjects.Where(x => x.Entity != null && x.Entity.Path.Contains("Metadata/MiscellaneousObjects/RemoteMine")).ToList();

            var realRange = Settings.DetonateDist.Value;
            var mineSkill = actor.ActorSkills.Find(x => x.Name.ToLower().Contains("mine"));
            if (mineSkill != null)
            {
                if (mineSkill.Stats.TryGetValue(GameStat.TotalSkillAreaOfEffectPctIncludingFinal, out var areaPct))
                {
                    realRange += realRange * areaPct / 100f;
                    Settings.CurrentAreaPct.Value = realRange;
                }
                else
                {
                    Settings.CurrentAreaPct.Value = 100;
                }
            }
            else
            {
                Settings.CurrentAreaPct.Value = 0;
            }


            if (deployedObjects.Count == 0)
            {
                return;
            }

            var playerPos = GameController.Player.GridPos;

            _monsters = _monsters.Where(x => x.IsAlive).ToList();

            var nearMonsters = _monsters.Where(x => x != null &&
	            !x.GetComponent<Life>().HasBuff("hidden_monster") && 
	            !x.GetComponent<Life>().HasBuff("avarius_statue_buff") && 
				!x.GetComponent<Life>().HasBuff("hidden_monster_disable_minions") &&
                FilterNullAction(x.GetComponent<Actor>()) &&
				x.GetComponent<Actor>().CurrentAction?.Skill?.Name != "AtziriSummonDemons" &&
				x.GetComponent<Actor>().CurrentAction?.Skill?.Id != 728 &&//Lab?
	            Vector2.Distance(playerPos, x.GridPos) < realRange).ToList();

            if (nearMonsters.Count == 0)
                return;

	        Settings.TriggerReason = "Path: " + nearMonsters[0].Path;

            if(Settings.Debug.Value)
			    LogMessage($"Ents: {nearMonsters.Count}. Last: {nearMonsters[0].Path}", 2);

            if ((DateTime.Now - LastDetonTime).TotalMilliseconds > Settings.DetonateDelay.Value)
            {
                if (deployedObjects.Any(x => x.Entity != null && x.Entity.IsValid && x.Entity.GetComponent<Stats>().StatDictionary[GameStat.CannotDie] == 0))
                {
                    LastDetonTime = DateTime.Now;
                    Keyboard.KeyPress(Settings.DetonateKey.Value);
                }
            }
        }

        private List<Entity> _monsters = new List<Entity>();

        #region Overrides of BasePlugin

        public override void EntityAdded(Entity entityWrapper)
        {
            if (entityWrapper == null)
                return;

            if (!entityWrapper.HasComponent<Monster>())
                return;

            if (!entityWrapper.IsAlive)
                return;

            if (!entityWrapper.IsHostile)
                return;

	        if (entityWrapper.Path.StartsWith("Metadata/Monsters/LeagueBetrayal/BetrayalTaserNet") ||
	            entityWrapper.Path.StartsWith("Metadata/Monsters/LeagueBetrayal/BetrayalUpgrades/UnholyRelic") ||
	            entityWrapper.Path.StartsWith("Metadata/Monsters/LeagueBetrayal/BetrayalUpgrades/BetrayalDaemonSummonUnholyRelic"))
		        return;

            _monsters.Add(entityWrapper);
        }

        private bool FilterNullAction(Actor actor)
        {
            if (Settings.FilterNullAction.Value)
                return actor.CurrentAction != null;

            return true;
        }

        public override void EntityRemoved(Entity entityWrapper)
        {
            if (entityWrapper == null)
                return;

            if (!entityWrapper.HasComponent<Monster>())
                return;

            _monsters.Remove(entityWrapper);
        }

        #endregion
    }
}
