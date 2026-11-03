using System;
using NUnit.Framework;
using UnityEngine;
using EarthRogueOrigins.Anatomy;

namespace EarthRogueOrigins.Tests
{
    public sealed class AnimatedAnatomyTests
    {
        static AnatomicalPhenotype Form(int seed)
        {
            return Development.Express(DevelopmentalGenome.Founder(new System.Random(seed)));
        }

        [Test]
        public void SamePhenotypeHasSameVisualCacheIdentity()
        {
            var a=Form(87);
            var b=Form(87);
            Assert.AreEqual(PhenotypeSpriteCache.VisualKey(a),PhenotypeSpriteCache.VisualKey(b));
            Assert.AreEqual(PhenotypeSpriteCache.StableHash(PhenotypeSpriteCache.VisualKey(a)),
                PhenotypeSpriteCache.StableHash(PhenotypeSpriteCache.VisualKey(b)));
        }

        [Test]
        public void DifferentArmorTierCreatesDifferentVisualIdentity()
        {
            var g=DevelopmentalGenome.Founder(new System.Random(13));
            g.regulatory[(int)BodyGene.ArmorCoverage]=0f;
            string low=PhenotypeSpriteCache.VisualKey(Development.Express(g));
            g.regulatory[(int)BodyGene.ArmorCoverage]=1f;
            string high=PhenotypeSpriteCache.VisualKey(Development.Express(g));
            Assert.AreNotEqual(low,high);
        }

        [Test]
        public void SharedPhenotypesReturnTheSameSpriteReference()
        {
            using(var cache=new PhenotypeSpriteCache(2))
            {
                var p=Form(3);
                string a=cache.Acquire(p);
                string b=cache.Acquire(p);
                Assert.AreEqual(a,b);
                Assert.AreEqual(1,cache.Count);
                Assert.AreEqual(1,cache.MissCount);
                Assert.AreEqual(1,cache.HitCount);
                Assert.AreSame(cache.Frame(a,false,0),cache.Frame(b,false,0));
                cache.Release(a);
                cache.Release(b);
            }
        }

        [Test]
        public void SwimmingAndWalkingUseDistinctFrames()
        {
            using(var cache=new PhenotypeSpriteCache(2))
            {
                string key=cache.Acquire(Form(18));
                Assert.AreNotSame(cache.Frame(key,false,1),cache.Frame(key,true,1));
                Assert.AreSame(cache.Frame(key,false,0),cache.Frame(key,false,4));
                cache.Release(key);
            }
        }

        [Test]
        public void EvictionNeverRemovesAnInUseMorph()
        {
            using(var cache=new PhenotypeSpriteCache(1))
            {
                var a=Form(2);
                var b=Form(55);
                string first=cache.Acquire(a);
                string second=cache.Acquire(b);
                Assert.AreEqual(2,cache.Count,"Live entries must not be evicted.");
                Assert.NotNull(cache.Frame(first,false,0));
                cache.Release(first);
                cache.Release(second);
                string third=cache.Acquire(Form(92));
                Assert.GreaterOrEqual(cache.Count,1);
                Assert.NotNull(cache.Frame(third,false,0));
                cache.Release(third);
            }
        }

        [Test]
        public void PosedFramesRetainValidRasterBounds()
        {
            var form=Form(39);
            foreach(bool swim in new[]{false,true})
            for(int i=0;i<4;i++)
            {
                Texture2D texture=AnatomySpriteBaker.Bake(form,113,i*Mathf.PI*.5f,swim);
                try
                {
                    Assert.AreEqual(384,texture.width);
                    Assert.AreEqual(224,texture.height);
                    int visible=0;
                    foreach(Color32 pixel in texture.GetPixels32())
                        if(pixel.a>128)visible++;
                    Assert.Greater(visible,400);
                }
                finally{UnityEngine.Object.DestroyImmediate(texture);}
            }
        }
    }
}
