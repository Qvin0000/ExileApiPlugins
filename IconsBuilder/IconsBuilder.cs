using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ExileCore;
using ExileCore.PoEMemory.Components;
using ExileCore.PoEMemory.MemoryObjects;
using ExileCore.Shared;
using ExileCore.Shared.Abstract;
using ExileCore.Shared.Enums;
using JM.LinqFaster;
using SharpDX;

namespace IconsBuilder
{
    public class IconsBuilder : BaseSettingsPlugin<IconsBuilderSettings>
    {
        private const string ALERT_CONFIG = "config\\new_mod_alerts.txt";

        private readonly EntityType[] Chests =
        {
            EntityType.Chest, EntityType.SmallChest
        };

        private readonly List<string> ignoreEntites = new List<string>
        {
            "Metadata/Monsters/Frog/FrogGod/SilverPool",
            "Metadata/MiscellaneousObjects/WorldItem",
            "Metadata/Pet/Weta/Basic",
            "Metadata/Monsters/Daemon/SilverPoolChillDaemon",
            "Metadata/Monsters/Daemon",
            "Metadata/Monsters/Frog/FrogGod/SilverOrbFromMonsters",
            "Metadata/Terrain/Labyrinth/Objects/Puzzle_Parts/TimerGears",
            "Metadata/Chests/DelveChests/DelveAzuriteVeinEncounter",
            "Metadata/Chests/DelveChests/DelveAzuriteVeinEncounterNoDrops"
        };

        private readonly Dictionary<string, Size2> modIcons = new Dictionary<string, Size2>();

        private readonly EntityType[] SkippedEntity =
        {
            EntityType.WorldItem, EntityType.HideoutDecoration, EntityType.Effect, EntityType.Light, EntityType.ServerObject
        };

        private Queue<Entity> _addedIcon = new Queue<Entity>(128);
        private int rnd = 1000;

        private void LoadConfig()
        {
            var readAllLines = File.ReadAllLines(ALERT_CONFIG);

            foreach (var readAllLine in readAllLines)
            {
                if (readAllLine.StartsWith("#")) continue;
                var s = readAllLine.Split(';');
                var sz = s[2].Trim().Split(',');
                modIcons[s[0]] = new Size2(int.Parse(sz[0]), int.Parse(sz[1]));
            }
        }

        public override void EntityIgnored(Entity Entity)
        {
            if (!Settings.Enable.Value) return;
        }

        public override void EntityRemoved(Entity Entity)
        {
            if (!Settings.Enable.Value) return;
            if (Entity.Type == EntityType.Effect) return;
        }

        public override void EntityAdded(Entity Entity)
        {
            if (!Settings.Enable.Value) return;
            if (SkippedEntity.Any(x => x == Entity.Type)) return;
            _addedIcon.Enqueue(Entity);
        }

        //Probably now outdated, need more tests 
        private IEnumerator FixIcons()
        {
            yield return new WaitTime(1000);
            _addedIcon = new Queue<Entity>(GameController.Entities.Where(x => x.IsValid));
        }

        public override void AreaChange(AreaInstance area)
        {
            Core.MainRunner.Run(new Coroutine(FixIcons(), this, "Fix map icons"));
        }

        public override bool Initialise()
        {
            LoadConfig();
            Graphics.InitImage("sprites.png");

            Settings.Reparse.OnPressed += () =>
            {
                foreach (var entity in GameController.EntityListWrapper.Entities)
                {
                    if (!entity.IsValid) continue;
                    EntityAdded(entity);
                }
            };

            return true;
        }

        public override Job Tick()
        {
            if (!Settings.Enable.Value) return null;

            if (Settings.MultiThreading && _addedIcon.Count >= Settings.MultiThreadingWhenEntityMoreThan)
                return GameController.MultiThreadManager.AddJob(TickLogic, nameof(IconsBuilder));

            TickLogic();
            return null;
        }

        private void TickLogic()
        {
            while (_addedIcon.Count > 0)
            {
                try
                {
                    var dequeue = _addedIcon.Dequeue();
                    var entityAddedLogic = EntityAddedLogic(dequeue);

                    if (entityAddedLogic != null)
                        dequeue.SetHudComponent(entityAddedLogic);
                }
                catch (Exception ex)
                {
                    DebugWindow.LogError($"{nameof(IconsBuilder)} -> {ex}", 3);
                }
            }
        }

        private bool SkipEntity(Entity entity)
        {
            if (entity.Type == EntityType.Daemon) return true;
            if (ignoreEntites.AnyF(x => entity.Path.Contains(x))) return true;
            return false;
        }

        private BaseIcon EntityAddedLogic(Entity entity)
        {
            if (SkipEntity(entity)) return null;

            //Monsters
            if (entity.Type == EntityType.Monster)
            {
                if (!entity.IsAlive) return null;

                if (entity.League == LeagueType.Legion)
                    return new LegionIcon(entity, GameController, Settings, modIcons);

                return new MonsterIcon(entity, GameController, Settings, modIcons);
            }

            //NPC
            if (entity.Type == EntityType.Npc)
                return new NpcIcon(entity, GameController, Settings);

            //Player
            if (entity.Type == EntityType.Player)
            {
                if (GameController.IngameState.Data.LocalPlayer.Address == entity.Address ||
                    GameController.IngameState.Data.LocalPlayer.GetComponent<Render>().Name == entity.RenderName) return null;

                if (!entity.IsValid) return null;
                return new PlayerIcon(entity, GameController, Settings, modIcons);
            }

            //Chests
            if (Chests.AnyF(x => x == entity.Type) && !entity.IsOpened)
                return new ChestIcon(entity, GameController, Settings);

            //Area transition
            if (entity.Type == EntityType.AreaTransition)
                return new MiscIcon(entity, GameController, Settings);

            //Shrine
            if (entity.HasComponent<Shrine>())
                return new ShrineIcon(entity, GameController, Settings);

            if (entity.HasComponent<Transitionable>() && entity.HasComponent<MinimapIcon>())
            {
                //Mission marker
                if (entity.Path.Equals("Metadata/MiscellaneousObjects/MissionMarker", StringComparison.Ordinal) ||
                    entity.GetComponent<MinimapIcon>().Name.Equals("MissionTarget", StringComparison.Ordinal))
                    return new MissionMarkerIcon(entity, GameController, Settings);

                return new MiscIcon(entity, GameController, Settings);
            }

            if (entity.HasComponent<MinimapIcon>() && entity.HasComponent<Targetable>())
                return new MiscIcon(entity, GameController, Settings);

            if (entity.Path.Contains("Metadata/Terrain/Leagues/Delve/Objects/EncounterControlObjects/AzuriteEncounterController"))
                return new MiscIcon(entity, GameController, Settings);

            if (entity.Type == EntityType.LegionMonolith) return new MiscIcon(entity, GameController, Settings);

            return null;
        }
    }
}
