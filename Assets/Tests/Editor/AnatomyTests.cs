using System;
using NUnit.Framework;
using UnityEngine;
using EarthRogueOrigins.Anatomy;
using EarthRogueOrigins.Evolution;

namespace EarthRogueOrigins.Tests
{
    public sealed class AnatomyTests
    {
        [Test]
        public void FoundersAndChildrenAreReproducible()
        {
            var a=DevelopmentalGenome.Founder(new Random(612));
            var b=DevelopmentalGenome.Founder(new Random(612));
            CollectionAssert.AreEqual(a.regulatory,b.regulatory);
            var child1=DevelopmentalGenome.Child(a,b,new Random(9));
            var child2=DevelopmentalGenome.Child(a,b,new Random(9));
            CollectionAssert.AreEqual(child1.regulatory,child2.regulatory);
        }

        [Test]
        public void DevelopmentProducesValidAnatomyAcrossManySeeds()
        {
            for(int seed=0;seed<100;seed++)
            {
                var genome=DevelopmentalGenome.Founder(new Random(seed));
                var phenotype=Development.Express(genome);
                Assert.IsTrue(Development.IsValid(phenotype),"Seed "+seed);
                Assert.Greater(phenotype.head.x,phenotype.tailRoot.x);
                Assert.Greater(phenotype.frontLeg.x,phenotype.rearLeg.x);
            }
        }

        [Test]
        public void DorsalArmorFollowsBackCurve()
        {
            var g=DevelopmentalGenome.Founder(new Random(24));
            g.regulatory[(int)BodyGene.ArmorCoverage]=1f;
            var p=Development.Express(g);
            Assert.AreEqual(17,p.dorsalSurface.Length);
            for(int i=1;i<p.dorsalSurface.Length;i++)
                Assert.Greater(p.dorsalSurface[i].x,p.dorsalSurface[i-1].x);
            Assert.Greater(p.dorsalSurface[8].y,p.dorsalSurface[0].y);
        }

        [Test]
        public void LargeAquaticFinImprovesSwimmingButCostsMass()
        {
            var g=DevelopmentalGenome.Founder(new Random(9));
            g.regulatory[(int)BodyGene.TailFin]=0f;
            var baseForm=Development.Express(g);
            g.regulatory[(int)BodyGene.TailFin]=1f;
            var swimmer=Development.Express(g);
            Assert.Greater(swimmer.swimmingSpeed,baseForm.swimmingSpeed);
            Assert.Greater(swimmer.bodyMass,baseForm.bodyMass);
        }

        [Test]
        public void ArmorIncreasesDefenseAndUpkeep()
        {
            var g=DevelopmentalGenome.Founder(new Random(9));
            g.regulatory[(int)BodyGene.ArmorCoverage]=0f;
            var low=Development.Express(g);
            g.regulatory[(int)BodyGene.ArmorCoverage]=1f;
            var high=Development.Express(g);
            Assert.Greater(high.defense,low.defense);
            Assert.Greater(high.upkeep,low.upkeep);
        }

        [Test]
        public void RasterizedCreatureHasVisiblePixelsAndSafeDimensions()
        {
            var p=Development.Express(DevelopmentalGenome.Founder(new Random(12)));
            var texture=AnatomySpriteBaker.Bake(p,99);
            try
            {
                Assert.AreEqual(384,texture.width);
                Assert.AreEqual(224,texture.height);
                int visible=0;
                foreach(var pixel in texture.GetPixels32())if(pixel.a>0)visible++;
                Assert.Greater(visible,300);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(texture);
            }
        }

        [Test]
        public void InheritedAnatomyStaysWithinGenomeBounds()
        {
            var random=new Random(1);
            var a=DevelopmentalGenome.Founder(random);
            var b=DevelopmentalGenome.Founder(random);
            for(int i=0;i<200;i++)
            {
                var child=DevelopmentalGenome.Child(a,b,random,1f);
                foreach(float value in child.regulatory)
                    Assert.That(value,Is.InRange(0f,1f));
                Assert.IsTrue(Development.IsValid(Development.Express(child)));
                a=b;b=child;
            }
        }
    }
}
