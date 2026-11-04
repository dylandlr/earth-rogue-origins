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
        public void TopologyMetricsDistinguishConnectedMassAndTrueCavity()
        {
            var f=new OpenMorphTissue.Field(12,12);
            // Build a five-by-five ring around an internal 3x3 hole.
            for(int y=3;y<=7;y++)
            for(int x=3;x<=7;x++)
            {
                if(x!=3&&x!=7&&y!=3&&y!=7)continue;
                f.cells[y*f.width+x].fate=OpenMorphTissue.Fate.Structural;
            }
            var ring=OpenMorphTopologyMetrics.Analyze(f);
            Assert.AreEqual(16,ring.occupied);
            Assert.AreEqual(1,ring.components);
            Assert.AreEqual(1,ring.holes);
            Assert.AreEqual(0,ring.borderCells);
            Assert.AreEqual(5,ring.bboxWidth);
            Assert.AreEqual(5,ring.bboxHeight);
            Assert.AreEqual(32,ring.perimeter);
            // Closing the cavity must alter topology, not just mass.
            for(int y=4;y<=6;y++)
            for(int x=4;x<=6;x++)
                f.cells[y*f.width+x].fate=OpenMorphTissue.Fate.Storage;
            var filled=OpenMorphTopologyMetrics.Analyze(f);
            Assert.AreEqual(25,filled.occupied);
            Assert.AreEqual(1,filled.components);
            Assert.AreEqual(0,filled.holes);
            Assert.AreEqual(20,filled.perimeter);
            Assert.Greater(filled.compactness,ring.compactness);
        }

        [Test]
        public void BoundaryTouchAndMultipleColoniesAreDetected()
        {
            var f=new OpenMorphTissue.Field(10,10);
            f.cells[0].fate=OpenMorphTissue.Fate.Structural;
            f.cells[99].fate=OpenMorphTissue.Fate.Contractile;
            var metrics=OpenMorphTopologyMetrics.Analyze(f);
            Assert.AreEqual(2,metrics.occupied);
            Assert.AreEqual(2,metrics.components);
            Assert.AreEqual(2,metrics.borderCells);
            Assert.AreEqual(0,metrics.holes);
            Assert.AreEqual(8,metrics.perimeter);
        }

        [Test]
        public void LimitedTissueBudgetNeverExceedsCellAllowance()
        {
            var baseline=OpenMorphGenome.Founder(9100);
            var branch=baseline.Copy();
            branch.genes.Add(new OpenMorphGenome.Gene {
                innovation=1,parentInnovation=0,attachX=.6f,
                angle=75f,length=.85f,width=.15f,
                rigidity=.85f,pigment=.75f,repeat=2
            });
            foreach(var genome in new[]{baseline,branch})
            {
                foreach(int budget in new[]{1,25,80,180})
                {
                    var field=OpenMorphTissue.Grow(genome,64,48,72,budget);
                    Assert.That(field.Occupied(),Is.InRange(1,budget));
                    Assert.AreEqual(72,field.steps);
                    Assert.AreEqual(1,
                        OpenMorphTopologyMetrics.Analyze(field).components,
                        "A resource limit cannot create disconnected tissue");
                }
            }
        }

        [Test]
        public void UnlimitedModeMatchesDefaultAndBudgetedModeReplays()
        {
            var genome=OpenMorphGenome.Founder(95);
            genome.genes.Add(new OpenMorphGenome.Gene {
                innovation=1,parentInnovation=0,
                angle=-85f,repeat=2,mirror=true,
                length=.75f,width=.19f
            });
            var oldMode=OpenMorphTissue.Grow(genome,48,36,50);
            var explicitUnlimited=OpenMorphTissue.Grow(
                genome,48,36,50,int.MaxValue);
            var limitedA=OpenMorphTissue.Grow(genome,48,36,50,120);
            var limitedB=OpenMorphTissue.Grow(genome,48,36,50,120);
            for(int i=0;i<oldMode.cells.Length;i++)
            {
                Assert.AreEqual(oldMode.cells[i].fate,
                    explicitUnlimited.cells[i].fate);
                Assert.AreEqual(limitedA.cells[i].fate,
                    limitedB.cells[i].fate);
                Assert.AreEqual(limitedA.cells[i].signalA,
                    limitedB.cells[i].signalA);
            }
        }

        [Test]
        public void OccupancyIntersectionUnionAndJaccardAreExact()
        {
            var a=new OpenMorphTissue.Field(8,8);
            var b=new OpenMorphTissue.Field(8,8);
            a.cells[2*8+2].fate=OpenMorphTissue.Fate.Structural;
            a.cells[2*8+3].fate=OpenMorphTissue.Fate.Structural;
            a.cells[3*8+2].fate=OpenMorphTissue.Fate.Storage;
            b.cells[2*8+2].fate=OpenMorphTissue.Fate.Contractile;
            b.cells[2*8+3].fate=OpenMorphTissue.Fate.Storage;
            b.cells[3*8+3].fate=OpenMorphTissue.Fate.Structural;
            var delta=OpenMorphPairMetrics.Compare(a,b);
            Assert.AreEqual(3,delta.aOccupied);
            Assert.AreEqual(3,delta.bOccupied);
            Assert.AreEqual(2,delta.common);
            Assert.AreEqual(1,delta.onlyA);
            Assert.AreEqual(1,delta.onlyB);
            Assert.AreEqual(4,delta.union);
            Assert.AreEqual(.5f,delta.jaccard,.00001f);
            Assert.AreEqual(.5f,delta.symmetricDifferenceFraction,.00001f);
            Assert.AreEqual(0,
                OpenMorphPairMetrics.Compare(a,a).onlyB);
            Assert.AreEqual(1f,
                OpenMorphPairMetrics.Compare(a,a).jaccard,.00001f);
        }

        [Test]
        public void EqualBudgetMorphologiesAreDeterministicAcrossBudgetSweep()
        {
            var a=OpenMorphGenome.Founder(820);
            var b=a.Copy();
            b.genes.Add(new OpenMorphGenome.Gene {
                innovation=1,parentInnovation=0,repeat=2,
                attachX=.6f,angle=75f,length=.85f,width=.15f
            });
            foreach(int limit in new[]{180,400,800})
            {
                var baseline=OpenMorphTissue.Grow(a,64,48,72,limit);
                var branched=OpenMorphTissue.Grow(b,64,48,72,limit);
                var repeated=OpenMorphTissue.Grow(b,64,48,72,limit);
                var metrics=OpenMorphPairMetrics.Compare(baseline,branched);
                var replay=OpenMorphPairMetrics.Compare(branched,repeated);
                Assert.That(metrics.aOccupied,Is.InRange(1,limit));
                Assert.That(metrics.bOccupied,Is.InRange(1,limit));
                Assert.AreEqual(1f,replay.jaccard,.00001f);
                Assert.AreEqual(0,replay.onlyA);
                Assert.AreEqual(0,replay.onlyB);
                Assert.AreEqual(1,OpenMorphTopologyMetrics.Analyze(
                    baseline).components);
                Assert.AreEqual(1,OpenMorphTopologyMetrics.Analyze(
                    branched).components);
            }
        }

        [Test]
        public void InvalidSimulationBoundsAreRejected()
        {
            var genome=OpenMorphGenome.Founder(3);
            Assert.Throws<System.ArgumentOutOfRangeException>(()=>
                OpenMorphTissue.Grow(genome,4,40,10));
            Assert.Throws<System.ArgumentOutOfRangeException>(()=>
                OpenMorphTissue.Grow(genome,64,48,257));
            Assert.Throws<System.ArgumentOutOfRangeException>(()=>
                OpenMorphTissue.Grow(genome,64,48,20,0));
        }
    }
}
