using System;
using System.Collections.Generic;
using UnityEngine;

namespace EarthRogueOrigins.PixelCreatures
{
    /// <summary>
    /// First integrated genotype -> development -> environmental evaluation
    /// -> parent selection -> mutation loop. This is an explicit, small
    /// evolutionary *experiment*, not a claim of naturalistic evolution,
    /// ecological fitness, biomechanics or artificial general life.
    /// </summary>
    public static class OpenMorphEvolution
    {
        public sealed class Individual
        {
            public OpenMorphGenome genome;
            public int id,generation,parentId,genes,segments,occupied;
            public float fitness,scarceSurvival,moderateSurvival,diversity;
            public OpenMorphTissue.Field tissue;
        }

        public sealed class Run
        {
            public readonly List<Individual> evaluated=new List<Individual>();
            public readonly List<Individual> champions=new List<Individual>();
            public int seed,generations,populationSize;
        }

        const int W=48,H=36;
        const int DevelopmentSteps=36,MaintenanceSteps=50;
        const int MaxTissueCells=360;

        public static Run Evolve(int seed=3452,
            int generations=4,int populationSize=8)
        {
            if(generations<1||generations>16 ||
                populationSize<4||populationSize>24)
                throw new ArgumentOutOfRangeException(
                    "Use 1..16 generations and 4..24 individuals.");
            var run=new Run {
                seed=seed,generations=generations,
                populationSize=populationSize
            };
            var genomes=new List<OpenMorphGenome>();
            var parents=new List<int>();
            var random=new System.Random(seed);
            for(int i=0;i<populationSize;i++)
            {
                var genome=OpenMorphGenome.Founder(seed+i*101);
                // No curated anatomy or class-specific templates.
                for(int k=0;k<6+i%5;k++)
                    genome=genome.Mutate(random.Next(1,int.MaxValue));
                genomes.Add(genome);
                parents.Add(-1);
            }
            int nextId=0;
            for(int generation=0;generation<generations;generation++)
            {
                var cohort=new List<Individual>();
                for(int i=0;i<populationSize;i++)
                {
                    var genome=genomes[i];
                    var tissue=OpenMorphTissue.Grow(
                        genome,W,H,DevelopmentSteps,MaxTissueCells,
                        true,true);
                    var scarce=OpenMorphResourceTrial.Run(
                        tissue,OpenMorphResourceTrial.Climate.Scarce,
                        MaintenanceSteps);
                    var moderate=OpenMorphResourceTrial.Run(
                        tissue,OpenMorphResourceTrial.Climate.Moderate,
                        MaintenanceSteps);
                    int occupied=tissue.Occupied();
                    int segments=OpenMorphDevelopment.Grow(genome).Length;
                    var item=new Individual {
                        genome=genome,id=nextId++,generation=generation,
                        parentId=parents[i],genes=genome.genes.Count,
                        segments=segments,occupied=occupied,tissue=tissue,
                        scarceSurvival=scarce.livingCells/
                            (float)Math.Max(1,occupied),
                        moderateSurvival=moderate.livingCells/
                            (float)Math.Max(1,occupied)
                    };
                    // Not size-maximization: reject nearly empty bodies,
                    // reward maintained tissue under two environments,
                    // and retain an explicit diversity measurement.
                    float biomass=Mathf.Clamp01(occupied/55f);
                    float complexity=Mathf.Clamp01(
                        (segments-1)/6f);
                    item.fitness=
                        .42f*item.scarceSurvival+
                        .28f*item.moderateSurvival+
                        .20f*biomass+
                        .10f*complexity;
                    if(occupied<8)item.fitness*=.15f;
                    cohort.Add(item);
                    run.evaluated.Add(item);
                }
                foreach(var item in cohort)
                {
                    // Observable genome/phenotype diversity proxy.
                    // This is NOT an open-ended novelty metric.
                    float sum=0f;
                    foreach(var other in cohort)
                    {
                        if(other==item)continue;
                        sum+=Mathf.Abs(item.occupied-other.occupied)/
                            (float)MaxTissueCells;
                        sum+=Mathf.Abs(item.segments-other.segments)/
                            (float)OpenMorphGenome.MaxDevelopedNodes;
                    }
                    item.diversity=sum/Math.Max(1,cohort.Count-1);
                }
                cohort.Sort((a,b)=>{
                    int score=b.fitness.CompareTo(a.fitness);
                    if(score!=0)return score;
                    return a.id.CompareTo(b.id);
                });
                run.champions.Add(cohort[0]);
                if(generation==generations-1)break;

                // Elitism plus mutation from the best half. The entire
                // history is retained and immutable; offspring mutate
                // independent genome copies with deterministic seeds.
                int pool=Math.Max(2,populationSize/2);
                genomes=new List<OpenMorphGenome>();
                parents=new List<int>();
                genomes.Add(cohort[0].genome.Copy());
                parents.Add(cohort[0].id);
                for(int i=1;i<populationSize;i++)
                {
                    var parent=cohort[random.Next(pool)];
                    var offspring=parent.genome.Mutate(
                        random.Next(1,int.MaxValue));
                    if(i%3==0)
                        offspring=offspring.Mutate(random.Next(1,int.MaxValue));
                    offspring.Validate();
                    genomes.Add(offspring);
                    parents.Add(parent.id);
                }
            }
            return run;
        }
    }
}
