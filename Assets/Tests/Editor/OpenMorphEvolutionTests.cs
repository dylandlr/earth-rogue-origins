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
