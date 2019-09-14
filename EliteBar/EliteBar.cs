using System;
using System.Collections.Generic;
using System.Linq;
using ExileCore;
using ExileCore.PoEMemory.Components;
using ExileCore.PoEMemory.MemoryObjects;
using ExileCore.Shared.Enums;
using ExileCore.Shared.Helpers;
using ImGuiNET;
using SharpDX;
using Vector2 = System.Numerics.Vector2;

namespace EliteBar
{
    public class EliteBar : BaseSettingsPlugin<EliteBarSettings>
    {
        private readonly List<string> ignoredEntites = new List<string>
        {
            "Metadata/Monsters/LeagueIncursion/VaalSaucerTurret", "Metadata/Monsters/AvariusCasticus/AvariusCasticusStatue"
        };

        private readonly Queue<Entity> EntityAddedQueue = new Queue<Entity>();
        private bool showWindow = true;
        private Vector2 vect = Vector2.Zero;

        public override void OnLoad()
        {
            CanUseMultiThreading = true;
            Graphics.InitImage("directions.png");
            Graphics.InitImage("healthbar.png");
            Graphics.InitImage("healthbar_bg.png");
        }

        public override Job Tick()
        {
            if (Settings.MultiThreading)
                return GameController.MultiThreadManager.AddJob(TickLogic, nameof(EliteBar));

            TickLogic();
            return null;
        }

        private void TickLogic()
        {
            while (EntityAddedQueue.Count > 0)
            {
                var entity = EntityAddedQueue.Dequeue();

                if (ignoredEntites.Any(x => entity.Path.Contains(x))) continue;

                if (entity.IsValid && !entity.IsAlive) continue;

                if (!entity.IsHostile) continue;
                var rarity = entity.Rarity;

                if (!Settings.ShowWhite && rarity == MonsterRarity.White) continue;
                if (!Settings.ShowMagic && rarity == MonsterRarity.Magic) continue;
                if (!Settings.ShowRare && rarity == MonsterRarity.Rare) continue;
                if (!Settings.ShowUnique && rarity == MonsterRarity.Unique) continue;

                var color = Color.Red;

                switch (rarity)
                {
                    case MonsterRarity.White:
                        color = Settings.NormalMonster;
                        break;
                    case MonsterRarity.Magic:
                        color = Settings.MagicMonster;
                        break;
                    case MonsterRarity.Rare:
                        color = Settings.RareMonster;
                        break;
                    case MonsterRarity.Unique:
                        color = Settings.UniqueMonster;
                        break;
                    case MonsterRarity.Error:
                        color = Color.LightGray;
                        break;
                    default:
                        color = Color.Red;
                        break;
                }

                entity.SetHudComponent(new EliteDrawBar(entity, color));
            }

            foreach (var entity in GameController.EntityListWrapper.ValidEntitiesByType[EntityType.Monster])
            {
                var drawCmd = entity.GetHudComponent<EliteDrawBar>();
                drawCmd?.Update();
            }
        }

        public override void EntityAdded(Entity Entity)
        {
            if (!Settings.Enable.Value) return;
            if (Entity.Type == EntityType.Monster) EntityAddedQueue.Enqueue(Entity);
        }

        public override void AreaChange(AreaInstance area)
        {
            EntityAddedQueue.Clear();
        }

        public override bool Initialise()
        {
            Settings.RefreshEntities.OnPressed += () =>
            {
                foreach (var Entity in GameController.EntityListWrapper.ValidEntitiesByType[EntityType.Monster])
                {
                    EntityAdded(Entity);
                }
            };

            vect = new Vector2(-0.1f, -0.25f);
            return true;
        }

        public override void Render()
        {
            var index = 0;

            foreach (var entity in GameController.EntityListWrapper.ValidEntitiesByType[EntityType.Monster])
            {
                var structValue = entity.GetHudComponent<EliteDrawBar>();
                if (structValue == null) continue;
                if (!structValue.IsAlive) continue;
                var structValueCurLife = structValue.CurLife;
                var space = Settings.Space * index;
                var rectangleF = new RectangleF(Settings.X, Settings.Y + space, Settings.Width, Settings.Height);
                var monsterText = $"{structValue.Name} => {structValueCurLife:###' '###' '###} | {structValue.PercentLife * 100}%";
                var position = new SharpDX.Vector2(Settings.X + Settings.StartTextX, Settings.Y + space + Settings.StartTextY);

                if (Settings.UseImguiForDraw)
                {
                    ImGui.SetNextWindowPos(new Vector2(rectangleF.X, rectangleF.Y), ImGuiCond.Always, new Vector2(0.0f, 0.0f));
                    ImGui.SetNextWindowSize(new Vector2(rectangleF.Width, rectangleF.Height), ImGuiCond.Always);
                    ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(0f, 0f));

                    ImGui.Begin($"##Progress bar{index}", ref showWindow,
                        ImGuiWindowFlags.NoSavedSettings | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.AlwaysAutoResize |
                        ImGuiWindowFlags.NoInputs | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoTitleBar |
                        ImGuiWindowFlags.NoScrollbar);

                    ImGui.PushStyleColor(ImGuiCol.PlotHistogram, structValue.Color.ToImguiVec4());
                    ImGui.PushStyleColor(ImGuiCol.Text, Settings.TextColor.Value.ToImguiVec4());
                    ImGui.PushStyleColor(ImGuiCol.FrameBg, Settings.ImguiDrawBg.Value.ToImguiVec4());
                    ImGui.ProgressBar(structValue.PercentLife, vect, $"{monsterText}");
                    ImGui.PopStyleColor(3);

                    ImGui.End();
                    ImGui.PopStyleVar();
                }
                else
                {
                    Graphics.DrawImage("healthbar_bg.png", rectangleF, Color.Black);
                    rectangleF.Width *= structValue.PercentLife;
                    Graphics.DrawImage("healthbar.png", rectangleF, structValue.Color);
                    rectangleF.Inflate(1, 1);
                    Graphics.DrawFrame(rectangleF, Settings.BorderColor, 1);
                }

                var delta = structValue.Entity.GridPos -
                            GameController.Player.GridPos;

                var distance = delta.GetPolarCoordinates(out var phi);
                var rectUV = MathHepler.GetDirectionsUV(phi, distance);
                rectangleF.Left -= rectangleF.Height + 5;
                rectangleF.Width = rectangleF.Height;
                Graphics.DrawImage("directions.png", rectangleF, rectUV);

                Graphics.DrawText(monsterText, position, Settings.TextColor);

                Graphics.DrawText($"{Math.Floor(distance)}",
                    new SharpDX.Vector2(rectangleF.X - (rectangleF.Height + 7), rectangleF.Y + Settings.StartTextY),
                    Settings.TextColor);

                index++;
            }
        }

        public class EliteDrawBar
        {
            public EliteDrawBar(Entity Entity, Color color)
            {
                this.Entity = Entity;
                Color = color;
                Name = Entity.RenderName;
                IsAlive = true;
                CurLife = 0;
                PercentLife = 0;
            }

            public Entity Entity { get; }
            public string Name { get; }
            public Color Color { get; }
            public int CurLife { get; private set; }
            public float PercentLife { get; private set; }
            public bool IsAlive { get; private set; }

            public void Update()
            {
                IsAlive = Entity.IsValid && Entity.IsAlive;
                var lifeComponent = Entity.GetComponent<Life>();

                CurLife = lifeComponent == null ? 0 : lifeComponent.CurHP + lifeComponent.CurES;
                PercentLife = lifeComponent == null ? 0 : CurLife / (float) (lifeComponent.MaxHP + lifeComponent.MaxES);
            }
        }
    }
}
