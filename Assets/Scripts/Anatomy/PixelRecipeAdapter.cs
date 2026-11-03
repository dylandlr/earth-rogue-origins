using System;
using UnityEngine;
using EarthRogueOrigins.Anatomy;

namespace EarthRogueOrigins.PixelCreatures
{
    /// <summary>
    /// Visual bridge between the existing tetrapod developmental model and the
    /// independently composable pixel recipe. Physics remains driven by Development.
    /// Fish and arthropods require their own authoritative physiology before gameplay use.
    /// </summary>
    public static class PixelRecipeAdapter
    {
        public static PixelCreatureRecipe FromDevelopment(DevelopmentalGenome genome,
            AnatomicalPhenotype phenotype,int stableSeed)
        {
            if(genome==null || phenotype==null)
                throw new ArgumentNullException("Developmental genome and phenotype required.");
            if(phenotype.bodyPlan!=0)
                throw new NotSupportedException("Only tetrapod physiology is wired to gameplay.");
            var recipe=PixelCreatureRecipe.Founder(CreaturePlan.Tetrapod,stableSeed);
            recipe.id="organism_"+stableSeed;
            recipe.trunkLength=Normalize(phenotype.trunkLength,.65f,1.35f);
            recipe.trunkDepth=Normalize(phenotype.trunkDepth,.29f,.62f);
            recipe.headSize=Normalize(phenotype.headRadius,.14f,.30f);
            recipe.legLength=Normalize(phenotype.limbLength,.19f,.70f);
            recipe.tailLength=Normalize(phenotype.tailLength,.28f,1.15f);
            recipe.finSize=Normalize(phenotype.tailFinSize,0f,.33f);
            recipe.armor=phenotype.armorCoverage;
            recipe.spots=phenotype.patternDensity;
            recipe.eyeSize=Normalize(phenotype.eyeRadius,.018f,.055f);
            float pigment=genome.Get(BodyGene.Hue);
            recipe.palette=pigment<.20f?CreaturePalette.SwampOlive:
                pigment<.40f?CreaturePalette.RustClay:
                pigment<.65f?CreaturePalette.Slate:
                pigment<.85f?CreaturePalette.PaleSand:CreaturePalette.Moss;
            recipe.baseColor=Palette(pigment);
            recipe.secondaryColor=Scale(recipe.baseColor,.65f);
            recipe.accentColor=new Color32(199,180,133,255);
            recipe.limbStyle=genome.Get(BodyGene.LimbMuscle)>.6f?
                PartStyle.Robust:PartStyle.Slender;
            recipe.headStyle=phenotype.headRadius>.24f?
                PartStyle.Robust:genome.Get(BodyGene.NeckLength)>.65f?
                PartStyle.LongSnout:PartStyle.Rounded;
            recipe.trunkStyle=phenotype.trunkDepth>.51f?
                PartStyle.Robust:PartStyle.Rounded;
            recipe.tailStyle=phenotype.tailFinSize>.16f?
                PartStyle.Fan:PartStyle.Slender;
            recipe.Validate();
            return recipe;
        }

        static float Normalize(float x,float min,float max)
        {
            return Mathf.Clamp01((x-min)/Mathf.Max(.00001f,max-min));
        }
        static Color32 Palette(float hue)
        {
            var a=new Color32(149,157,110,255);
            var b=new Color32(194,122,98,255);
            var c=new Color32(145,145,153,255);
            var d=new Color32(203,184,160,255);
            if(hue<.333f)return Color32.Lerp(a,b,hue*3f);
            if(hue<.666f)return Color32.Lerp(b,c,(hue-.333f)*3f);
            return Color32.Lerp(c,d,(hue-.666f)*3f);
        }
        static Color32 Scale(Color32 c,float scale)
        {
            return new Color32((byte)(c.r*scale),(byte)(c.g*scale),
                (byte)(c.b*scale),255);
        }
    }
}
