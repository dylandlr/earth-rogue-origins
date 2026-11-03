using System;
using NUnit.Framework;
using EarthRogueOrigins.Evolution;

namespace EarthRogueOrigins.Tests
{
    public sealed class EvolutionTests
    {
        [Test]
        public void FounderIsDeterministicForASeed()
        {
            var a = DiploidGenome.Founder(new Random(123));
            var b = DiploidGenome.Founder(new Random(123));
            CollectionAssert.AreEqual(a.alleles, b.alleles);
        }

        [Test]
        public void ChildWithoutMutationInheritsOneAllelePerParentAtEveryLocus()
        {
            var mother = new DiploidGenome();
            var father = new DiploidGenome();
            for (int i = 0; i < DiploidGenome.Alleles; ++i)
            {
                mother.alleles[i] = i % 2 == 0 ? 0.1f : 0.3f;
                father.alleles[i] = i % 2 == 0 ? 0.7f : 0.9f;
            }
            var child = DiploidGenome.Child(mother, father, new Random(42), 0f);
            for (int locus = 0; locus < DiploidGenome.Loci; ++locus)
            {
                Assert.That(child.alleles[locus * 2], Is.AnyOf(0.1f, 0.3f));
                Assert.That(child.alleles[locus * 2 + 1], Is.AnyOf(0.7f, 0.9f));
            }
        }

        [Test]
        public void AllelesStayWithinBoundsAfterMutation()
        {
            var random = new Random(7);
            var parent = DiploidGenome.Founder(random);
            for (int i = 0; i < 1000; ++i)
            {
                var child = DiploidGenome.Child(parent, parent, random, 1f, 2f);
                foreach (float value in child.alleles) Assert.That(value, Is.InRange(0f, 1f));
            }
        }

        [Test]
        public void SelectionExperimentIsReproducible()
        {
            var a = SelectionExperiment.Run(2046, 35, true);
            var b = SelectionExperiment.Run(2046, 35, true);
            CollectionAssert.AreEqual(a.meanTraits, b.meanTraits);
        }

        [Test]
        public void PredatorPressureIncreasesMeanArmorAcrossReplicates()
        {
            double withPredators = 0, withoutPredators = 0;
            const int replicates = 20;
            for (int seed = 0; seed < replicates; seed++)
            {
                withPredators += SelectionExperiment.Run(seed, 45, true).meanTraits[(int)Locus.Armor];
                withoutPredators += SelectionExperiment.Run(seed, 45, false).meanTraits[(int)Locus.Armor];
            }
            Assert.Greater(withPredators / replicates, withoutPredators / replicates);
        }
    }
}
