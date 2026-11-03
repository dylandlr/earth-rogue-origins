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
        public void LayerIndexMustBeInRange()
        {
            var r=PixelCreatureRecipe.Founder(CreaturePlan.Fish,4);
            Assert.Throws<ArgumentOutOfRangeException>(()=>
                PixelCreaturePainter.RenderLayerPixels(r,-1));
        }
    }
}
