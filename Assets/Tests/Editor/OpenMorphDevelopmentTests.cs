using NUnit.Framework;
using UnityEngine;
using EarthRogueOrigins.PixelCreatures;

namespace EarthRogueOrigins.Tests
{
    public sealed class OpenMorphDevelopmentTests
    {
        [Test]
        public void StructuralMutationIsDeterministicAndAlwaysValid()
        {
            var a=OpenMorphGenome.Founder(18);
            var b=OpenMorphGenome.Founder(18);
            bool changedTopology=false;
            for(int i=0;i<180;i++)
            {
                int seed=i*17+219;
                int before=a.genes.Count;
                a=a.Mutate(seed);
                b=b.Mutate(seed);
                a.Validate();b.Validate();
                Assert.AreEqual(a.genes.Count,b.genes.Count);
                Assert.AreEqual(OpenMorphDevelopment.Grow(a).Length,
                    OpenMorphDevelopment.Grow(b).Length);
                if(a.genes.Count!=before)changedTopology=true;
                Assert.LessOrEqual(a.genes.Count,OpenMorphGenome.MaxGenes);
            }
            Assert.IsTrue(changedTopology,
                "Mutations never created or deleted structural genes.");
        }

        [Test]
        public void BranchingAndMirroringDevelopWithoutNamedAnimalSlots()
        {
            var g=OpenMorphGenome.Founder(99);
            g.genes.Add(new OpenMorphGenome.Gene {
                innovation=1,parentInnovation=0,
                attachX=-.2f,angle=45f,length=.28f,
                width=.12f,repeat=2,mirror=true
            });
            g.genes.Add(new OpenMorphGenome.Gene {
                innovation=2,parentInnovation=1,
                attachX=.8f,angle=-60f,length=.15f,
                width=.1f,repeat=2
            });
            g.Validate();
            var segments=OpenMorphDevelopment.Grow(g);
            // 1 root + 4 repeated mirrored branches + 8 children.
            Assert.AreEqual(13,segments.Length);
            Assert.AreEqual(-1,segments[0].parentIndex);
            for(int i=1;i<segments.Length;i++)
            {
                Assert.GreaterOrEqual(segments[i].parentIndex,0);
                Assert.Less(segments[i].parentIndex,i);
                Assert.Greater(segments[i].radius,0f);
            }
        }

        [Test]
        public void DifferentTopologyProducesDistinctActualRasterOutput()
        {
            var baseGenome=OpenMorphGenome.Founder(3);
            var branching=baseGenome.Copy();
            branching.genes.Add(new OpenMorphGenome.Gene {
                innovation=1,parentInnovation=0,angle=83f,
                attachX=.3f,length=.3f,repeat=3,mirror=true
            });
            var a=OpenMorphRasterizer.Render(baseGenome);
            var b=OpenMorphRasterizer.Render(branching);
            Assert.AreEqual(128*96,a.Length);
            Assert.AreEqual(128*96,b.Length);
            int changed=0,visible=0;
            for(int i=0;i<a.Length;i++)
            {
                if(!a[i].Equals(b[i]))changed++;
                if(b[i].a>0)visible++;
            }
            Assert.Greater(changed,100);
            Assert.Greater(visible,100);
            Assert.Greater(OpenMorphDevelopment.Grow(branching).Length,
                OpenMorphDevelopment.Grow(baseGenome).Length);
        }

        [Test]
        public void LargeRepeatedGenomesRemainBoundedAndFitCanvas()
        {
            var g=OpenMorphGenome.Founder(42);
            for(int i=1;i<12;i++)
                g.genes.Add(new OpenMorphGenome.Gene {
                    innovation=i,parentInnovation=i-1,
                    repeat=5,mirror=true,attachX=.5f,
                    angle=15f,length=.2f,width=.12f
                });
            g.Validate();
            var grown=OpenMorphDevelopment.Grow(g);
            Assert.LessOrEqual(grown.Length,OpenMorphGenome.MaxDevelopedNodes);
            var fitted=OpenMorphDevelopment.FitToCanvas(grown);
            foreach(var s in fitted)
            {
                Assert.That(s.start.x,Is.InRange(0f,128f));
                Assert.That(s.end.x,Is.InRange(0f,128f));
                Assert.That(s.start.y,Is.InRange(0f,96f));
                Assert.That(s.end.y,Is.InRange(0f,96f));
            }
        }

        [Test]
        public void CyclesAndUnknownParentsAreRejected()
        {
            var g=OpenMorphGenome.Founder(1);
            g.genes.Add(new OpenMorphGenome.Gene {
                innovation=7,parentInnovation=7
            });
            Assert.Throws<System.InvalidOperationException>(()=>g.Validate());
        }
    }
}
