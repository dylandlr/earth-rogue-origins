using NUnit.Framework;
using UnityEngine;
using EarthRogueOrigins.PixelCreatures;

namespace EarthRogueOrigins.Tests
{
    public sealed class OpenMorphTissueTests
    {
        [Test]
        public void FounderStartsAsSingleViableSeedCell()
        {
            var field=OpenMorphTissue.Grow(OpenMorphGenome.Founder(41),
                32,24,0);
            Assert.AreEqual(0,field.steps);
            Assert.AreEqual(1,field.Occupied());
            Assert.AreEqual(OpenMorphTissue.Fate.Structural,
                field.At(16,12).fate);
        }

        [Test]
        public void TissueGrowthIsExactlyReproducible()
        {
            var genome=OpenMorphGenome.Founder(31);
            for(int i=0;i<10;i++)genome=genome.Mutate(1200+i*7);
            var a=OpenMorphTissue.Grow(genome,32,24,24);
            var b=OpenMorphTissue.Grow(genome,32,24,24);
            Assert.AreEqual(a.steps,b.steps);
            for(int i=0;i<a.cells.Length;i++)
            {
                Assert.AreEqual(a.cells[i].fate,b.cells[i].fate);
                Assert.AreEqual(a.cells[i].signalA,b.cells[i].signalA);
                Assert.AreEqual(a.cells[i].signalB,b.cells[i].signalB);
                Assert.AreEqual(a.cells[i].lineage,b.cells[i].lineage);
                Assert.AreEqual(a.cells[i].energy,b.cells[i].energy);
                Assert.IsFalse(float.IsNaN(a.cells[i].signalA));
                Assert.That(a.cells[i].signalA,Is.InRange(0f,1f));
                Assert.That(a.cells[i].signalB,Is.InRange(0f,1f));
            }
        }

        [Test]
        public void TissueOnlyGrowsFromExistingAdjacentTissue()
        {
            var g=OpenMorphGenome.Founder(11);
            var current=OpenMorphTissue.Grow(g,32,24,0);
            for(int step=1;step<=18;step++)
            {
                var next=OpenMorphTissue.Grow(g,32,24,step);
                for(int y=0;y<24;y++)
                for(int x=0;x<32;x++)
                {
                    if(current.At(x,y).fate!=OpenMorphTissue.Fate.Empty ||
                       next.At(x,y).fate==OpenMorphTissue.Fate.Empty)
                        continue;
                    bool neighbor=false;
                    if(x>0 && current.At(x-1,y).fate!=OpenMorphTissue.Fate.Empty)
                        neighbor=true;
                    if(x<31 && current.At(x+1,y).fate!=OpenMorphTissue.Fate.Empty)
                        neighbor=true;
                    if(y>0 && current.At(x,y-1).fate!=OpenMorphTissue.Fate.Empty)
                        neighbor=true;
                    if(y<23 && current.At(x,y+1).fate!=OpenMorphTissue.Fate.Empty)
                        neighbor=true;
                    Assert.IsTrue(neighbor,
                        "New cell appeared without a parent at "+x+","+y);
                }
                current=next;
            }
            Assert.GreaterOrEqual(current.Occupied(),1);
        }

        [Test]
        public void FateCountsPartitionAllOccupiedCells()
        {
            var genome=OpenMorphGenome.Founder(99);
            var field=OpenMorphTissue.Grow(genome,40,30,30);
            int total=0;
            foreach(OpenMorphTissue.Fate fate in new[]{
                OpenMorphTissue.Fate.Structural,
                OpenMorphTissue.Fate.Contractile,
                OpenMorphTissue.Fate.Sensory,
                OpenMorphTissue.Fate.Transport,
                OpenMorphTissue.Fate.Storage})
                total+=field.FateCount(fate);
            Assert.AreEqual(field.Occupied(),total);
            var image=OpenMorphTissue.Render(field);
            Assert.AreEqual(40*30,image.Length);
            int painted=0;
            foreach(var pixel in image)if(pixel.a>0)painted++;
            Assert.AreEqual(field.Occupied(),painted);
        }

        [Test]
        public void InvalidSimulationBoundsAreRejected()
        {
            var genome=OpenMorphGenome.Founder(3);
            Assert.Throws<System.ArgumentOutOfRangeException>(()=>
                OpenMorphTissue.Grow(genome,4,40,10));
            Assert.Throws<System.ArgumentOutOfRangeException>(()=>
                OpenMorphTissue.Grow(genome,64,48,257));
        }
    }
}
