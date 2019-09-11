using System;
using System.Linq.Expressions;
using Basic;
using Exile;
using Exile.PoEMemory.MemoryObjects;
using Shared.Abstract;
using Shared;
using Shared.Helpers;
using Shared.Interfaces;
using PoEMemory.Components;
using Shared;
using Shared.Enums;
using SharpDX;

namespace IconsBuilder
{
    public class MiscIcon : BaseIcon
    {
        public override string ToString() => $"{Entity.Path} : ({Entity.Type}) :{Text}";

        public MiscIcon(Entity entity, GameController gameController, IconsBuilderSettings settings) : base(entity, settings) =>
            Update(entity, settings);


        public void Update(Entity entity, IconsBuilderSettings settings) {
            if (!_HasIngameIcon)
            {
                MainTexture = new HudTexture();
                MainTexture.FileName = "Icons.png";
                MainTexture.Size = settings.SizeMiscIcon;
            }
            else
            {
                MainTexture.Size = settings.SizeDefaultIcon;
                Text = RenderName;
                Priority = IconPriority.VeryHigh;
                if (entity.GetComponent<MinimapIcon>().Name.Equals("DelveRobot", StringComparison.Ordinal)) Text = "Follow Me";

                return;
            }

            if (entity.HasComponent<Targetable>())
            {
                if (entity.Path.Equals("Metadata/Terrain/Leagues/Synthesis/Objects/SynthesisPortal", StringComparison.Ordinal))
                    Show = () => entity.IsValid;
                else
                {
                    Show = () =>
                    {
                        var isVisible = false;
                        if (entity.HasComponent<MinimapIcon>())
                        {
                            var minimapIcon = entity.GetComponent<MinimapIcon>();
                            isVisible = minimapIcon.IsVisible && !minimapIcon.IsHide;
                        }

                        return entity.IsValid && isVisible && entity.IsTargetable;
                    };
                }
            }
            else
                Show = () => entity.IsValid && entity.GetComponent<MinimapIcon>().IsVisible;

            if (entity.HasComponent<Transitionable>() && entity.HasComponent<MinimapIcon>())
            {
                if (entity.Path.Equals("Metadata/MiscellaneousObjects/Abyss/AbyssCrackSpawners/AbyssCrackSkeletonSpawner"))
                {
                    MainTexture.UV = SpriteHelper.GetUV(MapIconsIndex.AbyssCrack);
                    Show = () => entity.IsValid && (entity.GetComponent<MinimapIcon>().IsHide == false ||
                                                    entity.GetComponent<Transitionable>().Flag1 == 1);
                }

                else if (entity.Path.Equals("Metadata/MiscellaneousObjects/Abyss/AbyssStartNode"))
                {
                    MainTexture.UV = SpriteHelper.GetUV(MapIconsIndex.Abyss);
                    Show = () => entity.IsValid && (entity.GetComponent<MinimapIcon>().IsHide == false ||
                                                    entity.GetComponent<Transitionable>().Flag1 == 1);
                }
                else if (entity.Path.Equals("Metadata/MiscellaneousObjects/Abyss/AbyssNodeSmall", StringComparison.Ordinal) ||
                         entity.Path.Equals("Metadata/MiscellaneousObjects/Abyss/AbyssNodeLarge", StringComparison.Ordinal) ||
                         entity.Path.StartsWith("Metadata/MiscellaneousObjects/Abyss/AbyssFinalNodeChest"))
                {
                    Show = () => entity.IsValid && (entity.GetComponent<MinimapIcon>().IsHide == false ||
                                                    entity.GetComponent<Transitionable>().Flag1 == 1);
                }
                else if (entity.Path.StartsWith("Metadata/Terrain/Leagues/Incursion/Objects/IncursionPortal", StringComparison.Ordinal))
                    Show = () => entity.IsValid && entity.GetComponent<Transitionable>().Flag1 < 3;
                else
                {
                    Priority = IconPriority.Critical;
                    Show = () => false;
                }
            }
            else if (entity.HasComponent<Targetable>())
            {
                if (entity.Path.Contains("Metadata/Terrain/Leagues/Delve/Objects/DelveMineral"))
                {
                    Priority = IconPriority.High;
                    MainTexture.UV = SpriteHelper.GetUV(MapIconsIndex.DelveMineralVein);
                    Text = "Sulphite";
                    Show = () => entity.IsValid && entity.IsTargetable;
                }
                else if (entity.Path.Contains("Metadata/Terrain/Leagues/Delve/Objects/EncounterControlObjects/AzuriteEncounterController"))
                {
                    Priority = IconPriority.High;
                    Text = "Start";
                    Show = () => entity.IsValid && entity.GetComponent<Transitionable>().Flag1 < 3;
                    MainTexture.UV = SpriteHelper.GetUV(MapIconsIndex.PartyLeader);
                }
            }
        }
    }
}