using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Exile.PoEMemory.MemoryObjects;
using Shared.Interfaces;
using PoEMemory.Components;
using Shared.Enums;
using Shared.Helpers;
using SharpDX;

namespace HealthBars
{
    public class DebuffPanelConfig
    {
        public Dictionary<string, int> Bleeding { get; set; }
        public Dictionary<string, int> Corruption { get; set; }
        public Dictionary<string, int> Poisoned { get; set; }
        public Dictionary<string, int> Frozen { get; set; }
        public Dictionary<string, int> Chilled { get; set; }
        public Dictionary<string, int> Burning { get; set; }
        public Dictionary<string, int> Shocked { get; set; }
        public Dictionary<string, int> WeakenedSlowed { get; set; }
    }

    public class HealthBar
    {
        private Action OnHostileChange;
        private bool isHostile;
        public bool IsHostile
        {
            get
            {
                var entityIsHostile = Entity.IsHostile;
                if (isHostile != entityIsHostile)
                {
                    isHostile = entityIsHostile;
                    OnHostileChange?.Invoke();
                }

                return entityIsHostile;
            }
        }

        private int _lastHp;
        public bool Skip = false;
        public double DiedFrames = 0;
        public int MaxHp { get; private set; }
        private readonly Stopwatch dpsStopwatch = Stopwatch.StartNew();
        private const int DPS_CHECK_TIME = 1000;
        private const int DPS_FAST_CHECK_TIME = 200;
        private const int DPS_POP_TIME = 2000;

        public float HpPercent { get; set; }
        public RectangleF BackGround;
        public bool CanNotDie = false;
        private static List<string> IgnoreEntitiesList = new List<string> {"MonsterFireTrap2", "MonsterBlastRainTrap",};

        private TimeCache<float> _distance;
        public float Distance => _distance.Value;

        public Func<bool> IsHidden;

        public HealthBar(Entity entity, HealthBarsSettings settings) {
            Entity = entity;
            _distance = new TimeCache<float>(() => entity.DistancePlayer, 200);

            IsHidden = () => entity.IsHidden;
            // If ignored entity found, skip
            foreach (var _entity in IgnoreEntitiesList)
                if (entity.Path.Contains(_entity))
                    return;
            Update(entity, settings);
            //CanNotDie = entity.GetComponent<Stats>().StatDictionary.ContainsKey(GameStat.CannotDie);
            CanNotDie = entity.Path.StartsWith("Metadata/Monsters/Totems/Labyrinth");
            if (entity.HasComponent<ObjectMagicProperties>() && entity.GetComponent<ObjectMagicProperties>().Mods.Contains("MonsterConvertsOnDeath_"))
            {
                OnHostileChange = () =>
                {
                    if (_init) Update(Entity, settings);
                };
            }
        }

        public Life Life => Entity.GetComponent<Life>();
        public Entity Entity { get; private set; }
        public UnitSettings Settings { get; private set; }
        public CreatureType Type { get; private set; }


        private bool _init;
        public void Update(Entity entity, HealthBarsSettings settings) {
            if (entity.HasComponent<Player>())
            {
                Type = CreatureType.Player;
                Settings = settings.Players;
            }
            else if (entity.HasComponent<Monster>())
            {
                if (entity.IsHostile)
                {
                    switch (entity.GetComponent<ObjectMagicProperties>().Rarity)
                    {
                        case MonsterRarity.White:
                            Type = CreatureType.Normal;
                            Settings = settings.NormalEnemy;
                            break;

                        case MonsterRarity.Magic:
                            Type = CreatureType.Magic;
                            Settings = settings.MagicEnemy;
                            break;

                        case MonsterRarity.Rare:
                            Settings = settings.RareEnemy;
                            Type = CreatureType.Rare;
                            break;

                        case MonsterRarity.Unique:
                            Settings = settings.UniqueEnemy;
                            Type = CreatureType.Unique;
                            break;
                        default:
                            Settings = settings.Minions;
                            Type = CreatureType.Minion;
                            break;
                    }
                }
                else
                {
                    Type = CreatureType.Minion;
                    Settings = settings.Minions;
                }
            }

            _lastHp = GetFullHp();
            MaxHp = Life.MaxHP;
            _init = true;
        }

        public bool IsShow(bool showEnemy) {
            if (Settings == null)
                return false;
            return !IsHostile ? Settings.Enable.Value : Settings.Enable.Value && showEnemy && IsHostile;
        }

        public LinkedList<int> DpsQueue { get; } = new LinkedList<int>();
        public Color Color
        {
            get
            {
                if (IsHidden())
                    return Color.LightGray;
                if (HpPercent <= 0.1f)
                    return Settings.Under10Percent;
                else
                    return Settings.Color;
            }
        }

        public float HpWidth { get; set; }
        public float EsWidth { get; set; }

        public void DpsRefresh() {
            var chechTime = DpsQueue.Count > 0 ? DPS_CHECK_TIME : DPS_FAST_CHECK_TIME;
            if (dpsStopwatch.ElapsedMilliseconds >= chechTime)
            {
                var hp = GetFullHp();
                if (hp > -1000000 && hp < 10000000 && _lastHp != hp)
                {
                    DpsQueue.AddFirst(-(_lastHp - hp));
                    if (DpsQueue.Count > Settings.FloatingCombatStackSize)
                    {
                        DpsQueue.RemoveLast();
                        dpsStopwatch.Restart();
                    }

                    _lastHp = hp;
                }
            }
        }

        public void DpsDequeue() {
            if (dpsStopwatch.ElapsedMilliseconds >= DPS_POP_TIME)
            {
                if (DpsQueue.Count > 0) DpsQueue.RemoveLast();
                dpsStopwatch.Restart();
            }
        }

        private int GetFullHp() => Life.CurHP + Life.CurES;
    }
}