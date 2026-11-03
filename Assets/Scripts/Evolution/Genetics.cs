using System;
using System.Collections.Generic;
using UnityEngine;

namespace EarthRogueOrigins.Evolution
{
    // Standalone, deterministic population genetics module.
    // Simulation randomness is injected; no UnityEngine.Random global state is touched.
    public enum Locus { Speed, Armor, Metabolism, Fecundity, Caution, ThermalTolerance }

    [Serializable]
    public sealed class DiploidGenome
    {
        public const int Loci = 6;
        public const int Alleles = Loci * 2;
        public float[] alleles = new float[Alleles];

        public float Phenotype(Locus locus)
        {
            int offset = (int)locus * 2;
            if (alleles == null || alleles.Length != Alleles)
                throw new InvalidOperationException("Genome must contain exactly 12 alleles.");
            return Mathf.Clamp01((alleles[offset] + alleles[offset + 1]) * 0.5f);
        }

        public static DiploidGenome Founder(System.Random random)
        {
            if (random == null) throw new ArgumentNullException(nameof(random));
            var genome = new DiploidGenome();
            for (int i = 0; i < Alleles; ++i)
                genome.alleles[i] = (float)(0.25 + random.NextDouble() * 0.5);
            return genome;
        }

        public static DiploidGenome Child(
            DiploidGenome mother, DiploidGenome father, System.Random random,
            float mutationProbability = 0.01f, float mutationAmplitude = 0.15f)
        {
            if (mother == null || father == null) throw new ArgumentNullException("parent");
            if (random == null) throw new ArgumentNullException(nameof(random));
            if (mutationProbability < 0 || mutationProbability > 1)
                throw new ArgumentOutOfRangeException(nameof(mutationProbability));
            if (mutationAmplitude < 0)
                throw new ArgumentOutOfRangeException(nameof(mutationAmplitude));

            var child = new DiploidGenome();
            for (int locus = 0; locus < Loci; ++locus)
            {
                int offset = locus * 2;
                float maternal = mother.alleles[offset + random.Next(2)];
                float paternal = father.alleles[offset + random.Next(2)];
                child.alleles[offset] = Mutate(maternal, random, mutationProbability, mutationAmplitude);
                child.alleles[offset + 1] = Mutate(paternal, random, mutationProbability, mutationAmplitude);
            }
            return child;
        }

        static float Mutate(float allele, System.Random random, float probability, float amplitude)
        {
            if (random.NextDouble() < probability)
                allele += (float)((random.NextDouble() * 2 - 1) * amplitude);
            return Mathf.Clamp01(allele);
        }
    }

    [Serializable]
    public sealed class LineageRecord
    {
        public int id;
        public int motherId = -1;
        public int fatherId = -1;
        public int generation;
        public int birthTick;
        public int deathTick = -1;
        public string name;
        public DiploidGenome genome;
    }

    [Serializable]
    public sealed class PopulationSnapshot
    {
        public int tick;
        public int births;
        public int deaths;
        public int population;
        public float[] meanTraits = new float[DiploidGenome.Loci];
    }

    public static class PopulationStatistics
    {
        public static PopulationSnapshot Snapshot(IReadOnlyList<LineageRecord> records, int tick)
        {
            if (records == null) throw new ArgumentNullException(nameof(records));
            var result = new PopulationSnapshot { tick = tick };
            foreach (var organism in records)
            {
                if (organism.birthTick > tick) continue;
                ++result.births;
                if (organism.deathTick >= 0 && organism.deathTick <= tick)
                {
                    ++result.deaths;
                    continue;
                }
                ++result.population;
                for (int locus = 0; locus < DiploidGenome.Loci; ++locus)
                    result.meanTraits[locus] += organism.genome.Phenotype((Locus)locus);
            }
            if (result.population > 0)
                for (int i = 0; i < result.meanTraits.Length; ++i)
                    result.meanTraits[i] /= result.population;
            return result;
        }
    }

    // A minimal, auditable selection test, not a full ecological simulation.
    // Higher armor increases survival under predators; no-predator survival is neutral.
    public static class SelectionExperiment
    {
        public static PopulationSnapshot Run(int seed, int generations, bool predators, int populationSize = 80)
        {
            if (generations < 0 || populationSize < 2)
                throw new ArgumentOutOfRangeException("generations/populationSize");
            var random = new System.Random(seed);
            var population = new List<DiploidGenome>(populationSize);
            for (int i = 0; i < populationSize; ++i) population.Add(DiploidGenome.Founder(random));

            for (int generation = 0; generation < generations; ++generation)
            {
                // Selection acts on parental contribution to the next generation.
                var weights = new double[population.Count];
                double total = 0;
                for (int i = 0; i < population.Count; ++i)
                {
                    double armor = population[i].Phenotype(Locus.Armor);
                    weights[i] = predators ? 0.1 + 2.5 * armor * armor : 1.0;
                    total += weights[i];
                }
                var children = new List<DiploidGenome>(populationSize);
                for (int i = 0; i < populationSize; ++i)
                {
                    var mother = population[WeightedIndex(weights, total, random)];
                    var father = population[WeightedIndex(weights, total, random)];
                    children.Add(DiploidGenome.Child(mother, father, random));
                }
                population = children;
            }
            var snapshot = new PopulationSnapshot { tick = generations, population = population.Count };
            for (int locus = 0; locus < DiploidGenome.Loci; ++locus)
            {
                foreach (var genome in population)
                    snapshot.meanTraits[locus] += genome.Phenotype((Locus)locus);
                snapshot.meanTraits[locus] /= population.Count;
            }
            return snapshot;
        }

        static int WeightedIndex(double[] weights, double total, System.Random random)
        {
            double selection = random.NextDouble() * total;
            for (int i = 0; i < weights.Length; ++i)
            {
                selection -= weights[i];
                if (selection <= 0) return i;
            }
            return weights.Length - 1;
        }
    }
}
