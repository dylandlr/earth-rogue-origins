using NUnit.Framework;
using EarthRogueOrigins.PixelCreatures;

namespace EarthRogueOrigins.Tests
{
    public sealed class OpenMorphEvolutionTests
    {
        [Test]
        public void GenerationRunPreservesAncestryAndValidGenomes()
        {
            var run=OpenMorphEvolution.Evolve(3452,3,4);
            Assert.AreEqual(3,run.generations);
            Assert.AreEqual(4,run.populationSize);
            Assert.AreEqual(12,run.evaluated.Count);
            Assert.AreEqual(3,run.champions.Count);
            for(int i=0;i<run.evaluated.Count;i++)
            {
                var animal=run.evaluated[i];
                animal.genome.Validate();
                Assert.AreEqual(i,animal.id);
                Assert.AreEqual(i/4,animal.generation);
                Assert.Greater(animal.occupied,0);
                Assert.That(animal.fitness,Is.InRange(0f,1f));
                Assert.That(animal.scarceSurvival,Is.InRange(0f,1f));
                Assert.That(animal.moderateSurvival,Is.InRange(0f,1f));
                Assert.AreEqual(1,OpenMorphTopologyMetrics.Analyze(
                    animal.tissue).components);
                if(animal.generation==0)
                    Assert.AreEqual(-1,animal.parentId);
                else
                {
                    Assert.GreaterOrEqual(animal.parentId,0);
                    Assert.Less(animal.parentId,animal.id);
                    Assert.AreEqual(animal.generation-1,
                        run.evaluated[animal.parentId].generation);
                }
            }
        }

        [Test]
        public void EvolutionReplaysExactScoresAndGenomes()
        {
            var first=OpenMorphEvolution.Evolve(91,2,4);
            var second=OpenMorphEvolution.Evolve(91,2,4);
            for(int i=0;i<first.evaluated.Count;i++)
            {
                var a=first.evaluated[i];
                var b=second.evaluated[i];
                Assert.AreEqual(a.parentId,b.parentId);
                Assert.AreEqual(a.fitness,b.fitness);
                Assert.AreEqual(a.occupied,b.occupied);
                Assert.AreEqual(a.genome.genes.Count,b.genome.genes.Count);
                for(int k=0;k<a.genome.genes.Count;k++)
                {
                    var ga=a.genome.genes[k];
                    var gb=b.genome.genes[k];
                    Assert.AreEqual(ga.parentInnovation,gb.parentInnovation);
                    Assert.AreEqual(ga.innovation,gb.innovation);
                    Assert.AreEqual(ga.length,gb.length);
                    Assert.AreEqual(ga.growthResponse,gb.growthResponse);
                    Assert.AreEqual(ga.fateResponse,gb.fateResponse);
                }
            }
        }

        [Test]
        public void ExpressedGuidanceUsesActualDevelopmentCoordinates()
        {
            var genome=OpenMorphGenome.Founder(402);
            genome.genes.Add(new OpenMorphGenome.Gene {
                innovation=1,parentInnovation=0,
                length=.85f,angle=70f,repeat=2,mirror=true
            });
            var field=OpenMorphTissue.Grow(
                genome,48,36,36,360,true,true);
            var metrics=OpenMorphPhenotypeMetrics.Analyze(genome,field);
            Assert.AreEqual(field.Occupied(),metrics.occupied);
            Assert.Greater(metrics.expressedGuidanceCells,0);
            Assert.Greater(metrics.supportedTissueCells,0);
            Assert.AreEqual(metrics.occupied,
                metrics.supportedTissueCells+
                metrics.tissueOutsideGuidance);
            Assert.That(metrics.coverageJaccard,Is.InRange(0f,1f));
            Assert.That(metrics.supportedTissueFraction,Is.InRange(0f,1f));
        }

        [Test]
        public void PhenotypeNoveltyDetectsIdenticalAndDifferentTissue()
        {
            var genome=OpenMorphGenome.Founder(407);
            var a=OpenMorphTissue.Grow(genome,32,24,22,200,true,true);
            var b=OpenMorphTissue.Grow(genome,32,24,22,200,true,true);
            var c=new OpenMorphTissue.Field(32,24);
            c.cells[0].fate=OpenMorphTissue.Fate.Structural;
            float identical=OpenMorphPhenotypeMetrics.Novelty(
                a,new[]{b});
            float different=OpenMorphPhenotypeMetrics.Novelty(
                a,new[]{c});
            Assert.AreEqual(0f,identical,.000001f);
            Assert.Greater(different,identical);
        }

        [Test]
        public void QualityDiversityScoresAndAncestryReplay()
        {
            var first=OpenMorphEvolution.Evolve(112,2,4);
            var again=OpenMorphEvolution.Evolve(112,2,4);
            for(int i=0;i<first.evaluated.Count;i++)
            {
                var a=first.evaluated[i];
                var b=again.evaluated[i];
                Assert.AreEqual(a.novelty,b.novelty);
                Assert.AreEqual(a.selectionScore,b.selectionScore);
                Assert.AreEqual(a.bodyGuidanceCoverage,b.bodyGuidanceCoverage);
                Assert.AreEqual(a.parentId,b.parentId);
                Assert.That(a.novelty,Is.InRange(0f,1f));
                Assert.That(a.selectionScore,Is.InRange(0f,1f));
            }
        }

        [Test]
        public void BranchEvidenceReconcilesWithActualOccupiedTissue()
        {
            var genome=OpenMorphGenome.Founder(1123);
            genome.genes.Add(new OpenMorphGenome.Gene {
                innovation=1,parentInnovation=0,
                length=.85f,width=.15f,angle=80f,repeat=2
            });
            var tissue=OpenMorphTissue.Grow(
                genome,48,36,36,360,true,true);
            var report=OpenMorphBranchExpression.Analyze(genome,tissue);
            Assert.AreEqual(2,report.genes.Length);
            Assert.AreEqual(1,report.nonRootGenes);
            Assert.That(report.nonRootExpressionFraction,Is.InRange(0f,1f));
            foreach(var evidence in report.genes)
            {
                Assert.GreaterOrEqual(evidence.guidedCells,
                    evidence.occupiedCells);
                Assert.That(evidence.expressionFraction,Is.InRange(0f,1f));
                if(evidence.expressed)
                    Assert.GreaterOrEqual(evidence.occupiedCells,4);
            }
            Assert.GreaterOrEqual(report.unusedGuidanceCells,0);
        }

        [Test]
        public void UnexpressedGenesDoNotReceiveFunctionalStructureCredit()
        {
            var genome=OpenMorphGenome.Founder(81);
            genome.genes.Add(new OpenMorphGenome.Gene {
                innovation=1,parentInnovation=0,
                length=.85f,width=.1f,angle=75f
            });
            var seedOnly=new OpenMorphTissue.Field(48,36);
            seedOnly.cells[18*48+24].fate=OpenMorphTissue.Fate.Structural;
            var report=OpenMorphBranchExpression.Analyze(
                genome,seedOnly);
            Assert.AreEqual(1,report.nonRootGenes);
            Assert.AreEqual(0,report.expressedNonRootGenes);
            Assert.AreEqual(0f,report.nonRootExpressionFraction);
        }

        [Test]
        public void IntegratedEvolutionReportsBranchExpressionConsistently()
        {
            var run=OpenMorphEvolution.Evolve(902,2,4);
            foreach(var animal in run.evaluated)
            {
                var report=OpenMorphBranchExpression.Analyze(
                    animal.genome,animal.tissue);
                Assert.AreEqual(report.expressedNonRootGenes,
                    animal.expressedBranches);
                Assert.AreEqual(report.nonRootGenes,
                    animal.expressedBranches+animal.unexpressedBranches);
            }
        }

        [Test]
        public void AblationRemovesDescendantsWithoutChangingAncestorSeed()
        {
            var g=OpenMorphGenome.Founder(724);
            g.genes.Add(new OpenMorphGenome.Gene {
                innovation=1,parentInnovation=0,
                angle=60f,length=.9f,width=.2f
            });
            g.genes.Add(new OpenMorphGenome.Gene {
                innovation=2,parentInnovation=1,
                angle=-35f,length=.7f,width=.15f
            });
            g.genes.Add(new OpenMorphGenome.Gene {
                innovation=3,parentInnovation=0,
                angle=-60f,length=.6f,width=.12f
            });
            var knockout=OpenMorphCausalBranches.Ablate(g,1);
            knockout.Validate();
            Assert.AreEqual(g.seed,knockout.seed);
            Assert.AreEqual(2,knockout.genes.Count);
            Assert.AreEqual(0,knockout.genes[0].innovation);
            Assert.AreEqual(3,knockout.genes[1].innovation);
            Assert.AreEqual(4,g.genes.Count);
            Assert.Throws<System.ArgumentException>(()=>
                OpenMorphCausalBranches.Ablate(g,0));
        }

        [Test]
        public void CausalBranchAccountingUsesCounterfactualOccupancy()
        {
            var g=OpenMorphGenome.Founder(903);
            g.genes.Add(new OpenMorphGenome.Gene {
                innovation=1,parentInnovation=0,angle=75f,
                length=.9f,width=.2f,repeat=2,mirror=true
            });
            var tissue=OpenMorphTissue.Grow(
                g,48,36,36,360,true,true);
            var report=OpenMorphCausalBranches.Analyze(
                g,tissue,36,360);
            Assert.AreEqual(1,report.totalBranches);
            var knockout=OpenMorphCausalBranches.Ablate(g,1);
            var ablated=OpenMorphTissue.Grow(
                knockout,48,36,36,360,true,true);
            var delta=OpenMorphPairMetrics.Compare(tissue,ablated);
            Assert.AreEqual(delta.onlyA+delta.onlyB,
                report.branches[0].changedCells);
            Assert.AreEqual(delta.jaccard,
                report.branches[0].occupancyJaccard,.000001f);
            Assert.AreEqual(delta.symmetricDifferenceFraction,
                report.branches[0].changedFraction,.000001f);
            Assert.AreEqual(report.branches[0].changedFraction>=.035f,
                report.branches[0].morphologicallyActive);
        }

        [Test]
        public void NoNonRootGenesMeansNoCausalBranchCredit()
        {
            var g=OpenMorphGenome.Founder(104);
            var tissue=OpenMorphTissue.Grow(
                g,32,24,20,160,true,true);
            var report=OpenMorphCausalBranches.Analyze(
                g,tissue,20,160);
            Assert.AreEqual(0,report.totalBranches);
            Assert.AreEqual(0,report.activeBranches);
            Assert.AreEqual(0f,report.greatestChangedFraction);
        }

        [Test]
        public void PopulationConstraintsAreEnforced()
        {
            Assert.Throws<System.ArgumentOutOfRangeException>(()=>
                OpenMorphEvolution.Evolve(1,0,8));
            Assert.Throws<System.ArgumentOutOfRangeException>(()=>
                OpenMorphEvolution.Evolve(1,2,3));
            Assert.Throws<System.ArgumentOutOfRangeException>(()=>
                OpenMorphEvolution.Evolve(1,2,25));
        }
    }
}
