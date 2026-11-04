using NUnit.Framework;
using UnityEngine;
using EarthRogueOrigins.PixelCreatures;

namespace EarthRogueOrigins.Tests
{
    public sealed class PixelVisualMetricsTests
    {
        [Test]
        public void ConnectedComponentsAndBoundsUseOpaquePixelsOnly()
        {
            var pixels=new Color32[8*6];
            var color=new Color32(100,120,90,255);
            pixels[1*8+1]=color;
            pixels[1*8+2]=color;
            pixels[2*8+2]=color;
            pixels[4*8+6]=color; // isolated pixel
            var result=PixelCreatureVisualMetrics.Analyze(pixels,8,6);
            Assert.AreEqual(4,result.nonTransparentPixels);
            Assert.AreEqual(3,result.dominantConnectedPixels);
            Assert.AreEqual(2,result.components);
            Assert.AreEqual(1,result.distinctOpaqueColors);
            Assert.That(result.dominantFraction,Is.EqualTo(.75f).Within(.001f));
            Assert.AreEqual(1,result.minX);
            Assert.AreEqual(6,result.maxX);
            Assert.AreEqual(1,result.minY);
            Assert.AreEqual(4,result.maxY);
        }

        [Test]
        public void EmptyAndInvalidImagesAreHandledExplicitly()
        {
            var empty=PixelCreatureVisualMetrics.Analyze(new Color32[16],4,4);
            Assert.AreEqual(0,empty.nonTransparentPixels);
            Assert.AreEqual(0,empty.components);
            Assert.AreEqual(-1,empty.maxX);
            Assert.Throws<System.ArgumentException>(()=>
                PixelCreatureVisualMetrics.Analyze(new Color32[5],4,4));
        }

        [Test]
        public void AllHeroImagesProduceDeterministicVisualMetrics()
        {
            foreach(CreatureArchetype archetype in
                System.Enum.GetValues(typeof(CreatureArchetype)))
            {
                var recipe=PixelArtDirection.Hero(archetype,2046);
                var first=PixelCreatureVisualMetrics.Analyze(
                    PixelCreaturePainter.RenderPixels(recipe));
                var repeat=PixelCreatureVisualMetrics.Analyze(
                    PixelCreaturePainter.RenderPixels(recipe));
                Assert.AreEqual(first.nonTransparentPixels,repeat.nonTransparentPixels);
                Assert.AreEqual(first.components,repeat.components);
                Assert.AreEqual(first.distinctOpaqueColors,repeat.distinctOpaqueColors);
                Assert.AreEqual(first.dominantConnectedPixels,repeat.dominantConnectedPixels);
                Assert.Greater(first.nonTransparentPixels,150);
                Assert.That(first.minX,Is.InRange(1,127));
                Assert.That(first.maxX,Is.InRange(0,126));
            }
        }
    }
}
