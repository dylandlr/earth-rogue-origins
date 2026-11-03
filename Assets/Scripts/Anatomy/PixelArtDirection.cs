using System;
using UnityEngine;

namespace EarthRogueOrigins.PixelCreatures
{
    public enum CreaturePalette { SwampOlive, RustClay, Slate, PaleSand, Moss, MarshTeal }
    public enum MarkingPattern { MottledBack, SideFreckles, DorsalStripe, TailBands, Sparse, None }
    public enum CreatureArchetype
    {
        MarshCrawler, ClayRunner, SlateNewt, PaleJuvenile,
        ArmoredCrawler, PaddleFish, MarshFish, SegmentedBurrower
    }

    public struct PixelColorRamp
    {
        public readonly Color32 ink, shadow, mid, light, highlight, pattern;
        public PixelColorRamp(Color32 ink,Color32 shadow,Color32 mid,Color32 light,
            Color32 highlight,Color32 pattern)
        {
            this.ink=ink;this.shadow=shadow;this.mid=mid;
            this.light=light;this.highlight=highlight;this.pattern=pattern;
        }
    }

    /// <summary>
    /// Palette-locked and family-specific design bible. Names describe visual
    /// archetypes, not guaranteed biological species or ecological classes.
    /// </summary>
    public static class PixelArtDirection
    {
        static Color32 Hex(uint rgb)
        {
            return new Color32((byte)(rgb>>16),(byte)(rgb>>8),(byte)rgb,255);
        }

        public static PixelColorRamp Ramp(CreaturePalette palette)
        {
            switch(palette)
            {
                case CreaturePalette.RustClay:
                    return new PixelColorRamp(Hex(0x211C1A),Hex(0x68453D),Hex(0xAD715D),
                        Hex(0xD19270),Hex(0xE8B394),Hex(0x794B43));
                case CreaturePalette.Slate:
                    return new PixelColorRamp(Hex(0x222326),Hex(0x55575A),Hex(0x85878B),
                        Hex(0xB5B3B0),Hex(0xD6D3C9),Hex(0x65676B));
                case CreaturePalette.PaleSand:
                    return new PixelColorRamp(Hex(0x282620),Hex(0x83776A),Hex(0xC6B8A2),
                        Hex(0xDFD1B8),Hex(0xEFE5D1),Hex(0x988778));
                case CreaturePalette.Moss:
                    return new PixelColorRamp(Hex(0x222A22),Hex(0x53684B),Hex(0x859868),
                        Hex(0xA5AD7C),Hex(0xD2D0A0),Hex(0x596B4C));
                case CreaturePalette.MarshTeal:
                    return new PixelColorRamp(Hex(0x1C2929),Hex(0x345A60),Hex(0x628F8E),
                        Hex(0x91B2A8),Hex(0xC7D4BA),Hex(0x39666B));
                default:
                    return new PixelColorRamp(Hex(0x1A1D19),Hex(0x4D5B3A),Hex(0x7E8A59),
                        Hex(0xA7B277),Hex(0xD8D7A9),Hex(0x586745));
            }
        }

        public static PixelCreatureRecipe Hero(CreatureArchetype archetype,int seed)
        {
            CreaturePlan plan=archetype==CreatureArchetype.PaddleFish ||
                archetype==CreatureArchetype.MarshFish?CreaturePlan.Fish:
                archetype==CreatureArchetype.SegmentedBurrower?CreaturePlan.Arthropod:
                CreaturePlan.Tetrapod;
            var recipe=PixelCreatureRecipe.Founder(plan,seed);
            recipe.id=archetype.ToString().ToLowerInvariant()+"_"+seed;
            recipe.archetype=archetype;
            recipe.pattern=MarkingPattern.MottledBack;
            recipe.trunkLength=.58f;
            recipe.trunkDepth=.65f;
            recipe.headSize=.62f;
            recipe.legLength=.35f;
            recipe.tailLength=.57f;
            recipe.armor=.08f;
            recipe.spots=.55f;
            switch(archetype)
            {
                case CreatureArchetype.MarshCrawler:
                    recipe.palette=CreaturePalette.SwampOlive;
                    recipe.headStyle=PartStyle.Robust;recipe.trunkStyle=PartStyle.Robust;
                    break;
                case CreatureArchetype.ClayRunner:
                    recipe.palette=CreaturePalette.RustClay;
                    recipe.headStyle=PartStyle.LongSnout;recipe.trunkStyle=PartStyle.Slender;
                    recipe.limbStyle=PartStyle.Slender;recipe.legLength=.85f;
                    recipe.pattern=MarkingPattern.SideFreckles;recipe.spots=.33f;
                    break;
                case CreatureArchetype.SlateNewt:
                    recipe.palette=CreaturePalette.Slate;
                    recipe.headStyle=PartStyle.Wedge;recipe.trunkStyle=PartStyle.Slender;
                    recipe.tailLength=.86f;recipe.pattern=MarkingPattern.DorsalStripe;
                    break;
                case CreatureArchetype.PaleJuvenile:
                    recipe.palette=CreaturePalette.PaleSand;
                    recipe.headSize=.88f;recipe.trunkLength=.28f;
                    recipe.legLength=.21f;recipe.spots=.23f;recipe.pattern=MarkingPattern.Sparse;
                    break;
                case CreatureArchetype.ArmoredCrawler:
                    recipe.palette=CreaturePalette.Moss;
                    recipe.trunkStyle=PartStyle.Robust;recipe.armor=.87f;
                    recipe.legLength=.55f;recipe.pattern=MarkingPattern.TailBands;
                    break;
                case CreatureArchetype.PaddleFish:
                    recipe.palette=CreaturePalette.MarshTeal;
                    recipe.headStyle=PartStyle.Wedge;recipe.tailStyle=PartStyle.Fan;
                    recipe.finSize=.92f;recipe.tailLength=.83f;
                    recipe.pattern=MarkingPattern.DorsalStripe;
                    break;
                case CreatureArchetype.MarshFish:
                    recipe.palette=CreaturePalette.Slate;
                    recipe.headStyle=PartStyle.LongSnout;recipe.tailStyle=PartStyle.Fan;
                    recipe.finSize=.65f;recipe.spots=.38f;
                    break;
                case CreatureArchetype.SegmentedBurrower:
                    recipe.palette=CreaturePalette.RustClay;
                    recipe.trunkStyle=PartStyle.Segmented;recipe.headStyle=PartStyle.Wedge;
                    recipe.armor=.78f;recipe.pattern=MarkingPattern.TailBands;
                    recipe.trunkLength=.83f;
                    break;
            }
            ApplyPalette(recipe,recipe.palette);
            recipe.Validate();
            return recipe;
        }

        public static void ApplyPalette(PixelCreatureRecipe recipe,CreaturePalette palette)
        {
            var ramp=Ramp(palette);
            recipe.palette=palette;
            recipe.baseColor=ramp.mid;
            recipe.secondaryColor=ramp.pattern;
            recipe.accentColor=ramp.highlight;
        }
    }
}
