using System;
using NUnit.Framework;
using UnityEngine;
using EarthRogueOrigins.PixelCreatures;
using EarthRogueOrigins.Anatomy;

namespace EarthRogueOrigins.Tests
{
    public sealed class PixelCreaturePipelineTests
    {
        [Test]
        public void AllStarterPlansGenerateNonemptyRgbaSprites()
        {
            foreach(CreaturePlan plan in Enum.GetValues(typeof(CreaturePlan)))
            {
                var r=PixelCreatureRecipe.Founder(plan,2046);
                var pixels=PixelCreaturePainter.RenderPixels(r);
                Assert.AreEqual(PixelCreaturePainter.Width*PixelCreaturePainter.Height,pixels.Length);
                int visible=0;
                foreach(var pixel in pixels)if(pixel.a>0)visible++;
                Assert.Greater(visible,100,plan.ToString());
            }
        }

        [Test]
        public void RepeatedRecipeProducesExactSamePixels()
        {
            var recipe=PixelCreatureRecipe.Founder(CreaturePlan.Tetrapod,976);
            var a=PixelCreaturePainter.RenderPixels(recipe);
            var b=PixelCreaturePainter.RenderPixels(recipe.Copy());
            CollectionAssert.AreEqual(a,b);
        }

        [Test]
        public void AnatomyLayoutUsesOnlyValidSlots()
        {
            foreach(CreaturePlan plan in Enum.GetValues(typeof(CreaturePlan)))
            {
                var r=PixelCreatureRecipe.Founder(plan,3);
                var layout=PixelCreatureDevelopment.Layout(r);
                bool hasHead=false,hasTrunk=false,hasEye=false;
                foreach(var part in layout)
                {
                    Assert.Greater(part.extent.x,0);
                    Assert.Greater(part.extent.y,0);
                    Assert.IsTrue(part.anchor.x>=0 && part.anchor.x<128);
                    Assert.IsTrue(part.anchor.y>=0 && part.anchor.y<96);
                    if(part.slot==PartSlot.Head)hasHead=true;
                    if(part.slot==PartSlot.Trunk)hasTrunk=true;
                    if(part.slot==PartSlot.Eye)hasEye=true;
                }
                Assert.IsTrue(hasHead && hasTrunk && hasEye);
            }
        }

        [Test]
        public void AllOrderedLayersComposeToSameFinalSprite()
        {
            // Fully opaque top-layer pixel compositing is the defined rule.
            var recipe=PixelCreatureRecipe.Founder(CreaturePlan.Tetrapod,300);
            var target=PixelCreaturePainter.RenderPixels(recipe);
            var composed=new Color32[target.Length];
            var parts=PixelCreatureDevelopment.Layout(recipe);
            Array.Sort(parts,(a,b)=>a.drawOrder.CompareTo(b.drawOrder));
            for(int index=0;index<parts.Length;index++)
            {
                var layer=PixelCreaturePainter.RenderLayerPixels(recipe,index);
                for(int p=0;p<layer.Length;p++)
                    if(layer[p].a>0)composed[p]=layer[p];
            }
            CollectionAssert.AreEqual(target,composed);
        }

        [Test]
        public void OffspringPreservePlanAndMutationsStayBounded()
        {
            var rng=new System.Random(14);
            var mother=PixelCreatureRecipe.Founder(CreaturePlan.Arthropod,111);
            var father=PixelCreatureRecipe.Founder(CreaturePlan.Arthropod,112);
            for(int i=0;i<40;i++)
            {
                var child=PixelCreatureRecipe.Offspring(mother,father,rng,1f);
                child.Validate();
                Assert.AreEqual(CreaturePlan.Arthropod,child.plan);
                Assert.That(child.armor,Is.InRange(0,1));
                Assert.That(child.tailLength,Is.InRange(0,1));
                mother=father;father=child;
            }
        }

        [Test]
        public void CrossPlanReproductionIsRejected()
        {
            var a=PixelCreatureRecipe.Founder(CreaturePlan.Tetrapod,1);
            var b=PixelCreatureRecipe.Founder(CreaturePlan.Fish,2);
            Assert.Throws<InvalidOperationException>(()=>
                PixelCreatureRecipe.Offspring(a,b,new System.Random(5)));
        }

        [Test]
        public void InvalidControlsAndUnknownRecipeVersionsFail()
        {
            var r=PixelCreatureRecipe.Founder(CreaturePlan.Fish,9);
            r.finSize=float.NaN;
            Assert.Throws<ArgumentOutOfRangeException>(()=>r.Validate());
            r.finSize=.5f;r.version=3;
            Assert.Throws<NotSupportedException>(()=>r.Validate());
        }

        [Test]
        public void EcologicalGenomeControlsPixelArmorAndFins()
        {
            var g=DevelopmentalGenome.Founder(new System.Random(93));
            g.regulatory[(int)BodyGene.ArmorCoverage]=0f;
            g.regulatory[(int)BodyGene.TailFin]=0f;
            var low=PixelRecipeAdapter.FromDevelopment(g,Development.Express(g),14);
            g.regulatory[(int)BodyGene.ArmorCoverage]=1f;
            g.regulatory[(int)BodyGene.TailFin]=1f;
            var high=PixelRecipeAdapter.FromDevelopment(g,Development.Express(g),14);
            Assert.AreEqual(CreaturePlan.Tetrapod,high.plan);
            Assert.Greater(high.armor,low.armor);
            Assert.Greater(high.finSize,low.finSize);
        }

        [Test]
        public void SwappingHeadAndBodyModulesChangesRenderedPixels()
        {
            var r=PixelCreatureRecipe.Founder(CreaturePlan.Tetrapod,120);
            r.headStyle=PartStyle.Rounded;
            r.trunkStyle=PartStyle.Rounded;
            var before=PixelCreaturePainter.RenderPixels(r);
            r.headStyle=PartStyle.LongSnout;
            r.trunkStyle=PartStyle.Slender;
            var after=PixelCreaturePainter.RenderPixels(r);
            int differences=0;
            for(int i=0;i<before.Length;i++)
                if(!before[i].Equals(after[i]))differences++;
            Assert.Greater(differences,30);
        }

        [Test]
        public void HeroArchetypesHaveDistinctControlledOutputs()
        {
            var keys=new System.Collections.Generic.HashSet<string>();
            foreach(CreatureArchetype family in Enum.GetValues(typeof(CreatureArchetype)))
            {
                var hero=PixelArtDirection.Hero(family,462);
                hero.Validate();
                var pixels=PixelCreaturePainter.RenderPixels(hero);
                int visible=0;
                ulong hash=14695981039346656037UL;
                unchecked
                {
                    foreach(var color in pixels)
                    {
                        if(color.a!=0)visible++;
                        hash^=color.r;hash*=1099511628211UL;
                        hash^=color.g;hash*=1099511628211UL;
                        hash^=color.b;hash*=1099511628211UL;
                        hash^=color.a;hash*=1099511628211UL;
                    }
                }
                Assert.Greater(visible,100,family.ToString());
                Assert.IsTrue(keys.Add(hash.ToString()),"Duplicate hero artwork: "+family);
            }
            Assert.AreEqual(8,keys.Count);
        }

        [Test]
        public void PaletteRampsHaveOrderedLightness()
        {
            foreach(CreaturePalette palette in Enum.GetValues(typeof(CreaturePalette)))
            {
                var ramp=PixelArtDirection.Ramp(palette);
                Func<Color32,float> value=c=>c.r*.2126f+c.g*.7152f+c.b*.0722f;
                Assert.Less(value(ramp.ink),value(ramp.shadow));
                Assert.Less(value(ramp.shadow),value(ramp.mid));
                Assert.Less(value(ramp.mid),value(ramp.light));
                Assert.Less(value(ramp.light),value(ramp.highlight));
            }
        }

        [Test]
        public void MarkingDensityChangesPixelsWithoutChangingBodyPlan()
        {
            var recipe=PixelArtDirection.Hero(CreatureArchetype.MarshCrawler,1924);
            recipe.pattern=MarkingPattern.MottledBack;
            recipe.spots=0f;
            var before=PixelCreaturePainter.RenderPixels(recipe);
            recipe.spots=1f;
            var after=PixelCreaturePainter.RenderPixels(recipe);
            int changed=0;
            for(int i=0;i<before.Length;i++)
                if(!before[i].Equals(after[i]))changed++;
            Assert.Greater(changed,0);
            Assert.AreEqual(CreaturePlan.Tetrapod,recipe.plan);
        }

        [Test]
        public void AuthoredHeadProfilesHaveDistinctContours()
        {
            var recipe=PixelArtDirection.Hero(CreatureArchetype.MarshCrawler,2046);
            recipe.headStyle=PartStyle.Robust;
            var broad=PixelCreaturePainter.RenderPixels(recipe);
            recipe.headStyle=PartStyle.Wedge;
            var wedge=PixelCreaturePainter.RenderPixels(recipe);
            recipe.headStyle=PartStyle.LongSnout;
            var longSnout=PixelCreaturePainter.RenderPixels(recipe);
            int broadWedge=0,wedgeLong=0;
            for(int i=0;i<broad.Length;i++)
            {
                if(!broad[i].Equals(wedge[i]))broadWedge++;
                if(!wedge[i].Equals(longSnout[i]))wedgeLong++;
            }
            Assert.Greater(broadWedge,55);
            Assert.Greater(wedgeLong,55);
        }

        [Test]
        public void AuthoredProfilesRemainInsideCanvasAcrossMorphologyExtremes()
        {
            var r=PixelArtDirection.Hero(CreatureArchetype.MarshCrawler,491);
            foreach(PartStyle style in new[]{PartStyle.Rounded,PartStyle.Robust,
                PartStyle.Slender,PartStyle.Wedge,PartStyle.LongSnout})
            {
                r.headStyle=style;
                foreach(float value in new[]{0f,.5f,1f})
                {
                    r.trunkLength=value;r.trunkDepth=value;r.headSize=value;
                    var pixels=PixelCreaturePainter.RenderPixels(r);
                    int visible=0;
                    for(int y=1;y<PixelCreaturePainter.Height-1;y++)
                    for(int x=1;x<PixelCreaturePainter.Width-1;x++)
                        if(pixels[y*PixelCreaturePainter.Width+x].a>0)visible++;
                    Assert.Greater(visible,250);
                }
            }
        }

        [Test]
        public void ArchetypeLayoutsHaveOverlappingHeadAndBodyEnvelopes()
        {
            foreach(CreatureArchetype archetype in Enum.GetValues(typeof(CreatureArchetype)))
            {
                var r=PixelArtDirection.Hero(archetype,45);
                var parts=PixelCreatureDevelopment.Layout(r);
                PixelPartDescriptor head=null,trunk=null;
                foreach(var part in parts)
                {
                    if(part.slot==PartSlot.Head)head=part;
                    if(part.slot==PartSlot.Trunk)trunk=part;
                }
                Assert.NotNull(head);
                Assert.NotNull(trunk);
                Assert.IsTrue(PixelSilhouetteTemplates.AttachmentOverlap(head,trunk),
                    "Disconnected head envelope for "+archetype);
            }
        }

        [Test]
        public void RepaintingCustomPigmentChangesRenderedPixels()
        {
            var r=PixelArtDirection.Hero(CreatureArchetype.MarshCrawler,17);
            var before=PixelCreaturePainter.RenderPixels(r);
            r.baseColor=new Color32(191,114,105,255);
            var after=PixelCreaturePainter.RenderPixels(r);
            int changed=0;
            for(int i=0;i<before.Length;i++)if(!before[i].Equals(after[i]))changed++;
            Assert.Greater(changed,70);
        }

        [Test]
        public void LayerIndexMustBeInRange()
        {
            var r=PixelCreatureRecipe.Founder(CreaturePlan.Fish,4);
            Assert.Throws<ArgumentOutOfRangeException>(()=>
                PixelCreaturePainter.RenderLayerPixels(r,-1));
        }
    }
}
