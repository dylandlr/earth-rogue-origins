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
        public void AxialAndTipOrganizerFieldsAreBoundedAndDeterministic()
        {
            var genome=OpenMorphGenome.Founder(601);
            genome.genes.Add(new OpenMorphGenome.Gene {
                innovation=1,parentInnovation=0,length=.9f,width=.12f,
                angle=65f,repeat=2,mirror=true
            });
            var a=OpenMorphOrganizers.Build(genome,64,48);
            var b=OpenMorphOrganizers.Build(genome,64,48);
            Assert.Less(a.axial[24*64],a.axial[24*64+63]);
            int tipCells=0,activeCells=0;
            for(int i=0;i<a.activation.Length;i++)
            {
                Assert.That(a.activation[i],Is.InRange(0f,1f));
                Assert.That(a.distal[i],Is.InRange(0f,1f));
                Assert.That(a.inhibition[i],Is.InRange(0f,1f));
                Assert.AreEqual(a.activation[i],b.activation[i]);
                Assert.AreEqual(a.distal[i],b.distal[i]);
                Assert.AreEqual(a.innovation[i],b.innovation[i]);
                if(a.distal[i]>.15f)tipCells++;
                if(a.activation[i]>.15f)activeCells++;
            }
            Assert.Greater(tipCells,0);
            Assert.Greater(activeCells,tipCells);
        }

        [Test]
        public void StructuredGrowthMaintainsBudgetConnectivityAndReplay()
        {
            var genome=OpenMorphGenome.Founder(9100);
            genome.genes.Add(new OpenMorphGenome.Gene {
                innovation=1,parentInnovation=0,attachX=.6f,
                angle=75f,length=.85f,width=.15f,
                rigidity=.85f,repeat=2
            });
            var first=OpenMorphTissue.Grow(genome,64,48,72,800,true);
            var again=OpenMorphTissue.Grow(genome,64,48,72,800,true);
            Assert.That(first.Occupied(),Is.InRange(1,800));
            Assert.AreEqual(1,OpenMorphTopologyMetrics.Analyze(first).components);
            Assert.AreEqual(first.prunedCells,again.prunedCells);
            Assert.That(first.prunedCells,Is.InRange(0,72));
            foreach(var cell in first.cells)
            {
                if(cell.fate==OpenMorphTissue.Fate.Empty)continue;
                Assert.GreaterOrEqual(cell.organizerInnovation,0);
            }
            Assert.AreEqual(1f,
                OpenMorphPairMetrics.Compare(first,again).jaccard,.00001f);
        }

        [Test]
        public void RegulatoryAllelesRemainHeritableAndBounded()
        {
            var genome=OpenMorphGenome.Founder(717);
            for(int k=0;k<100;k++)
            {
                genome=genome.Mutate(107+k*71);
                genome.Validate();
                foreach(var gene in genome.genes)
                {
                    Assert.That(gene.growthResponse,Is.InRange(0f,1f));
                    Assert.That(gene.tipResponse,Is.InRange(0f,1f));
                    Assert.That(gene.inhibitionResponse,Is.InRange(0f,1f));
                }
            }
            var copy=genome.Copy();
            Assert.AreEqual(genome.genes.Count,copy.genes.Count);
            for(int i=0;i<genome.genes.Count;i++)
            {
                Assert.AreEqual(genome.genes[i].growthResponse,
                    copy.genes[i].growthResponse);
                Assert.AreEqual(genome.genes[i].tipResponse,
                    copy.genes[i].tipResponse);
                Assert.AreEqual(genome.genes[i].inhibitionResponse,
                    copy.genes[i].inhibitionResponse);
            }
        }

        [Test]
        public void RegulatoryResponseChangesWithInheritedAlleles()
        {
            var weak=OpenMorphGenome.Founder(99).genes[0];
            var strong=weak.Copy();
            weak.growthResponse=.1f;
            weak.tipResponse=.1f;
            weak.inhibitionResponse=.9f;
            strong.growthResponse=.9f;
            strong.tipResponse=.9f;
            strong.inhibitionResponse=.1f;
            var slow=OpenMorphRegulation.Evaluate(weak,
                .7f,.2f,.8f,.6f,.25f,.1f);
            var fast=OpenMorphRegulation.Evaluate(strong,
                .7f,.2f,.8f,.6f,.25f,.1f);
            Assert.Greater(fast.growthMultiplier,slow.growthMultiplier);
            Assert.Greater(fast.tipBoost,slow.tipBoost);
            Assert.Less(fast.inhibition,slow.inhibition);
        }

        [Test]
        public void RegulatoryPerturbationIsNotAStructuralMutation()
        {
            var genome=OpenMorphGenome.Founder(121);
            genome.genes.Add(new OpenMorphGenome.Gene {
                innovation=1,parentInnovation=0,
                angle=70f,repeat=2,length=.8f
            });
            var variant=genome.Copy();
            foreach(var gene in variant.genes)
            {
                gene.growthResponse=.9f;
                gene.tipResponse=.9f;
                gene.inhibitionResponse=.1f;
            }
            Assert.AreEqual(OpenMorphDevelopment.Grow(genome).Length,
                OpenMorphDevelopment.Grow(variant).Length);
            var organizerA=OpenMorphOrganizers.Build(genome,48,36);
            var organizerB=OpenMorphOrganizers.Build(variant,48,36);
            for(int i=0;i<organizerA.activation.Length;i++)
            {
                Assert.AreEqual(organizerA.activation[i],
                    organizerB.activation[i]);
                Assert.AreEqual(organizerA.innovation[i],
                    organizerB.innovation[i]);
            }
            var original=OpenMorphTissue.Grow(genome,48,36,72,800,true);
            var changed=OpenMorphTissue.Grow(variant,48,36,72,800,true);
            Assert.AreEqual(1,OpenMorphTopologyMetrics.Analyze(
                original).components);
            Assert.AreEqual(1,OpenMorphTopologyMetrics.Analyze(
                changed).components);
            Assert.That(original.Occupied(),Is.InRange(1,800));
            Assert.That(changed.Occupied(),Is.InRange(1,800));
            Assert.AreEqual(1f,OpenMorphPairMetrics.Compare(
                changed,OpenMorphTissue.Grow(variant,48,36,72,800,true)
                ).jaccard,.00001f);
        }

        [Test]
        public void FateOnlyMutationChangesDifferentiationWithoutChangingGrowth()
        {
            var original=OpenMorphGenome.Founder(9100);
            original.genes[0].fateResponse=.12f;
            var altered=original.Copy();
            altered.genes[0].fateResponse=.98f;
            original.Validate();altered.Validate();
            var a=OpenMorphTissue.Grow(original,48,36,48,400,true);
            var b=OpenMorphTissue.Grow(altered,48,36,48,400,true);
            var overlap=OpenMorphPairMetrics.Compare(a,b);
            Assert.AreEqual(a.Occupied(),b.Occupied());
            Assert.AreEqual(1f,overlap.jaccard,.00001f);
            Assert.AreEqual(a.prunedCells,b.prunedCells);
            int changedFates=0;
            for(int i=0;i<a.cells.Length;i++)
            {
                if(a.cells[i].fate!=b.cells[i].fate)changedFates++;
                Assert.AreEqual(a.cells[i].energy,b.cells[i].energy);
                Assert.AreEqual(a.cells[i].signalA,b.cells[i].signalA);
                Assert.AreEqual(a.cells[i].signalB,b.cells[i].signalB);
            }
            Assert.Greater(changedFates,0,
                "Fate-only allele must actually change local differentiation.");
        }

        [Test]
        public void TissueFateRegulationIsGeneticallyIndependentOfTipResponse()
        {
            var g=OpenMorphGenome.Founder(81).genes[0];
            var low=g.Copy();
            var high=g.Copy();
            low.fateResponse=0f;
            high.fateResponse=1f;
            Assert.AreNotEqual(
                OpenMorphRegulation.Differentiate(low,.65f,.25f,.4f),
                OpenMorphRegulation.Differentiate(high,.65f,.25f,.4f));
            var original=OpenMorphRegulation.Evaluate(
                low,.65f,.25f,.4f,.2f,.5f,.1f);
            var variant=OpenMorphRegulation.Evaluate(
                high,.65f,.25f,.4f,.2f,.5f,.1f);
            Assert.AreEqual(original.growthMultiplier,variant.growthMultiplier);
            Assert.AreEqual(original.tipBoost,variant.tipBoost);
            Assert.AreEqual(original.inhibition,variant.inhibition);
        }

        [Test]
        public void SignedGeneRegulatoryCouplingsAreHeritableAndBounded()
        {
            var genome=OpenMorphGenome.Founder(1203);
            for(int k=0;k<75;k++)genome=genome.Mutate(321+k*19);
            genome.Validate();
            var copy=genome.Copy();
            Assert.AreEqual(genome.genes.Count,copy.genes.Count);
            for(int i=0;i<genome.genes.Count;i++)
            {
                var a=genome.genes[i];var b=copy.genes[i];
                Assert.That(a.growthToFate,Is.InRange(-1f,1f));
                Assert.That(a.fateToGrowth,Is.InRange(-1f,1f));
                Assert.That(a.brakeToGrowth,Is.InRange(-1f,1f));
                Assert.That(a.growthToBrake,Is.InRange(-1f,1f));
                Assert.That(a.fateToBrake,Is.InRange(-1f,1f));
                Assert.That(a.brakeToFate,Is.InRange(-1f,1f));
                Assert.AreEqual(a.growthToFate,b.growthToFate);
                Assert.AreEqual(a.brakeToGrowth,b.brakeToGrowth);
            }
        }

        [Test]
        public void RecurrentRegulationDependsOnPreviousExpressionState()
        {
            var gene=OpenMorphGenome.Founder(7).genes[0];
            gene.growthToFate=.9f;
            gene.brakeToGrowth=-.9f;
            var low=new OpenMorphGRN.State {
                growth=.1f,fate=.3f,brake=.2f
            };
            var high=new OpenMorphGRN.State {
                growth=.9f,fate=.3f,brake=.8f
            };
            var a=OpenMorphGRN.Step(gene,low,.6f,.3f,.7f,.25f);
            var b=OpenMorphGRN.Step(gene,high,.6f,.3f,.7f,.25f);
            Assert.AreNotEqual(a.growth,b.growth);
            Assert.AreNotEqual(a.fate,b.fate);
            foreach(float v in new[]{a.growth,a.fate,a.brake,
                b.growth,b.fate,b.brake})
                Assert.That(v,Is.InRange(0f,1f));
        }

        [Test]
        public void RecurrentGenomeDevelopmentIsBoundedConnectedAndRepeatable()
        {
            var genome=OpenMorphGenome.Founder(9100);
            genome.genes.Add(new OpenMorphGenome.Gene {
                innovation=1,parentInnovation=0,repeat=2,
                angle=75f,length=.85f,width=.15f
            });
            var a=OpenMorphTissue.Grow(genome,48,36,36,400,true,true);
            var b=OpenMorphTissue.Grow(genome,48,36,36,400,true,true);
            Assert.That(a.Occupied(),Is.InRange(1,400));
            Assert.AreEqual(1,OpenMorphTopologyMetrics.Analyze(a).components);
            Assert.AreEqual(1f,OpenMorphPairMetrics.Compare(a,b).jaccard,.00001f);
            for(int i=0;i<a.cells.Length;i++)
            {
                Assert.AreEqual(a.cells[i].fate,b.cells[i].fate);
                Assert.AreEqual(a.cells[i].regulatoryGrowth,
                    b.cells[i].regulatoryGrowth);
                Assert.AreEqual(a.cells[i].regulatoryFate,
                    b.cells[i].regulatoryFate);
                Assert.AreEqual(a.cells[i].regulatoryBrake,
                    b.cells[i].regulatoryBrake);
            }
            Assert.Throws<System.ArgumentException>(()=>
                OpenMorphTissue.Grow(genome,48,36,8,400,false,true));
        }

        [Test]
        public void NeutralGRNLeavesStructuredBirthProbabilityUnchanged()
        {
            var neutral=new OpenMorphGRN.State {
                growth=.5f,brake=.5f,fate=.2f
            };
            foreach(float chance in new[]{0f,.01f,.1f,.3f,.7f,1f})
                Assert.AreEqual(chance,
                    OpenMorphGRN.ModulateGrowth(chance,neutral),.000001f);
        }

        [Test]
        public void GRNModulationIsBoundedAndNotADoubleSuppression()
        {
            var low=new OpenMorphGRN.State {growth=0f,brake=1f};
            var high=new OpenMorphGRN.State {growth=1f,brake=0f};
            foreach(float chance in new[]{0f,.03f,.1f,.2f,.5f,1f})
            {
                var down=OpenMorphGRN.ModulateGrowth(chance,low);
                var up=OpenMorphGRN.ModulateGrowth(chance,high);
                Assert.That(down,Is.InRange(0f,1f));
                Assert.That(up,Is.InRange(0f,1f));
                Assert.LessOrEqual(down,chance);
                Assert.GreaterOrEqual(up,chance);
                Assert.LessOrEqual(chance-down,.14f+.00001f);
                Assert.LessOrEqual(up-chance,.14f+.00001f);
            }
        }

        [Test]
        public void GrowthTraceReconcilesEveryAcceptedBirthAndPruning()
        {
            var genome=OpenMorphGenome.Founder(1902);
            genome.genes.Add(new OpenMorphGenome.Gene {
                innovation=1,parentInnovation=0,
                length=.8f,angle=70f,repeat=2
            });
            foreach(bool recurrent in new[]{false,true})
            {
                var field=OpenMorphTissue.Grow(
                    genome,48,36,36,400,true,recurrent);
                Assert.AreEqual(37,field.occupiedByTick.Length);
                Assert.AreEqual(36,field.acceptedBirthsByTick.Length);
                Assert.AreEqual(1,field.occupiedByTick[0]);
                int totalPruned=0;
                for(int t=0;t<36;t++)
                {
                    Assert.GreaterOrEqual(field.proposedBirthsByTick[t],
                        field.acceptedBirthsByTick[t]);
                    Assert.AreEqual(field.occupiedByTick[t]+
                        field.acceptedBirthsByTick[t]-
                        field.prunedByTick[t],
                        field.occupiedByTick[t+1],
                        "Mass conservation of recorded cellular births "+
                        "and pruning failed at tick "+t);
                    totalPruned+=field.prunedByTick[t];
                }
                Assert.AreEqual(field.Occupied(),
                    field.occupiedByTick[36]);
                Assert.AreEqual(totalPruned,field.prunedCells);
            }
        }

        [Test]
        public void GrowthOpportunityLedgerSeparatesRegulatoryEffectsFromEligibility()
        {
            var genome=OpenMorphGenome.Founder(502995);
            foreach(bool recurrent in new[]{false,true})
            {
                var field=OpenMorphTissue.Grow(
                    genome,48,36,30,400,true,recurrent);
                Assert.AreEqual(30,field.eligibleSitesByTick.Length);
                int totalEligible=0;
                for(int t=0;t<30;t++)
                {
                    int eligible=field.eligibleSitesByTick[t];
                    totalEligible+=eligible;
                    Assert.GreaterOrEqual(eligible,field.proposedBirthsByTick[t]);
                    Assert.GreaterOrEqual(field.baseChanceSumByTick[t],0f);
                    Assert.GreaterOrEqual(field.finalChanceSumByTick[t],0f);
                    Assert.LessOrEqual(field.baseChanceSumByTick[t],
                        eligible+.0001f);
                    Assert.LessOrEqual(field.finalChanceSumByTick[t],
                        eligible+.0001f);
                    Assert.LessOrEqual(field.inhibitedSitesByTick[t]+
                        field.boostedSitesByTick[t],eligible);
                    if(!recurrent)
                    {
                        Assert.AreEqual(field.baseChanceSumByTick[t],
                            field.finalChanceSumByTick[t]);
                        Assert.AreEqual(0,field.inhibitedSitesByTick[t]);
                        Assert.AreEqual(0,field.boostedSitesByTick[t]);
                    }
                }
                Assert.Greater(totalEligible,0);
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
