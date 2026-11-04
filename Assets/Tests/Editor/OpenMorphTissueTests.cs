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
        public void BranchGenesChangeTheMorphogenFieldAwayFromOrigin()
        {
            var founder=OpenMorphGenome.Founder(77);
            var branched=founder.Copy();
            branched.genes.Add(new OpenMorphGenome.Gene {
                innovation=1,parentInnovation=0,
                angle=85f,attachX=.7f,length=.85f,width=.20f,
                repeat=2,mirror=true,rigidity=.9f
            });
            var plain=OpenMorphGrowthGuide.Build(founder,64,48);
            var extended=OpenMorphGrowthGuide.Build(branched,64,48);
            int changed=0;
            float maxChange=0f;
            for(int i=0;i<plain.activation.Length;i++)
            {
                float difference=extended.activation[i]-plain.activation[i];
                if(difference>.05f)changed++;
                maxChange=Mathf.Max(maxChange,difference);
                Assert.That(extended.activation[i],Is.InRange(0f,1f));
            }
            Assert.Greater(changed,20,
                "Added branches must change growth guidance beyond the original body.");
            Assert.Greater(maxChange,.1f);
        }

        [Test]
        public void GrowthGuideIsDeterministicAndDependsOnStructure()
        {
            var genome=OpenMorphGenome.Founder(19);
            for(int i=0;i<12;i++)genome=genome.Mutate(900+i*311);
            var a=OpenMorphGrowthGuide.Build(genome,48,36);
            var b=OpenMorphGrowthGuide.Build(genome,48,36);
            Assert.AreEqual(a.activation.Length,b.activation.Length);
            for(int i=0;i<a.activation.Length;i++)
            {
                Assert.AreEqual(a.activation[i],b.activation[i]);
                Assert.AreEqual(a.innovation[i],b.innovation[i]);
            }
            Assert.Greater(a.At(24,18),0f);
        }

        [Test]
        public void BranchCounterfactualTracksActualTissueNotJustGeneCount()
        {
            var baseline=OpenMorphGenome.Founder(9100);
            var branched=baseline.Copy();
            branched.genes.Add(new OpenMorphGenome.Gene {
                innovation=1,parentInnovation=0,
                attachX=.6f,angle=75f,length=.85f,width=.15f,
                rigidity=.85f,pigment=.75f,repeat=2
            });
            var source=OpenMorphTissue.Grow(baseline,64,48,72);
            var changed=OpenMorphTissue.Grow(branched,64,48,72);
            Assert.AreEqual(72,source.steps);
            Assert.AreEqual(72,changed.steps);
            // Record two measurable phenotypes; the visual acceptance
            // gate is performed on the actual Unity exported pair.
            int differentCells=0,differentOccupancy=0;
            for(int i=0;i<source.cells.Length;i++)
            {
                if(source.cells[i].fate!=changed.cells[i].fate)
                    differentCells++;
                if((source.cells[i].fate==OpenMorphTissue.Fate.Empty)!=
                   (changed.cells[i].fate==OpenMorphTissue.Fate.Empty))
                    differentOccupancy++;
            }
            Assert.Greater(source.Occupied(),0);
            Assert.Greater(changed.Occupied(),0);
            Assert.Greater(differentCells,0,
                "Adding a branch did not change developed tissue fates.");
            Assert.Greater(differentOccupancy,0,
                "Adding a branch changed color but not occupancy geometry.");
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
