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
            public float novelty,selectionScore,bodyGuidanceCoverage;
            public int supportedTissueCells;
            public int expressedBranches,unexpressedBranches;
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
                    // Never reward unexpressed decorative structure.
                    var expression=OpenMorphBranchExpression.Analyze(
                        genome,tissue);
                    float functionalStructure=Mathf.Clamp01(
                        expression.expressedNonRootGenes/3f);
                    item.fitness=
                        .42f*item.scarceSurvival+
                        .28f*item.moderateSurvival+
                        .20f*biomass+
                        .10f*functionalStructure;
                    var expressed=OpenMorphPhenotypeMetrics.Analyze(
                        genome,tissue);
                    item.bodyGuidanceCoverage=expressed.coverageJaccard;
                    item.supportedTissueCells=expressed.supportedTissueCells;
                    item.expressedBranches=expression.expressedNonRootGenes;
                    item.unexpressedBranches=expression.nonRootGenes-
                        expression.expressedNonRootGenes;
                    if(occupied<8)item.fitness*=.15f;
                    cohort.Add(item);
                    run.evaluated.Add(item);
                }
                // Phenotype-distance based novelty, not genome counts.
                // Include historical tissue to discourage convergence on
                // the same high-fitness shape over multiple generations.
                var references=new List<OpenMorphTissue.Field>();
                foreach(var previous in run.evaluated)
                    references.Add(previous.tissue);
                foreach(var item in cohort)
                {
                    // The current individual is excluded by reference.
                    item.novelty=OpenMorphPhenotypeMetrics.Novelty(
                        item.tissue,references,3);
                    item.diversity=item.novelty;
                    // Keep raw survival-proxy fitness separate from the
                    // explicit quality-diversity parent selection score.
                    item.selectionScore=.78f*item.fitness+
                        .22f*item.novelty;
                }
                cohort.Sort((a,b)=>{
                    int score=b.fitness.CompareTo(a.fitness);
                    if(score!=0)return score;
                    return a.id.CompareTo(b.id);
                });
                run.champions.Add(cohort[0]);
                if(generation==generations-1)break;

                // Preserve the best raw-fitness champion. Build a
                // second pool ranked by quality+phenotypic novelty,
                // enforcing a minimum shape distance when possible.
                var bySelection=new List<Individual>(cohort);
                bySelection.Sort((a,b)=>{
                    int cmp=b.selectionScore.CompareTo(a.selectionScore);
                    return cmp!=0?cmp:a.id.CompareTo(b.id);
                });
                int poolSize=Math.Max(2,populationSize/2);
                var selectedParents=new List<Individual> {cohort[0]};
                const float minimumNovelDistance=.22f;
                // Reserve one place for the most novel sufficiently
                // viable organism. This protects an exploratory lineage
                // even when it ranks lower under survival-heavy scoring.
                Individual noveltyParent=null;
                foreach(var candidate in cohort)
                {
                    if(candidate.fitness<cohort[0].fitness*.55f)continue;
                    if(noveltyParent==null ||
                        candidate.novelty>noveltyParent.novelty ||
                        (candidate.novelty==noveltyParent.novelty &&
                         candidate.id<noveltyParent.id))
                        noveltyParent=candidate;
                }
                if(noveltyParent!=null &&
                    noveltyParent!=cohort[0] &&
                    selectedParents.Count<poolSize)
                    selectedParents.Add(noveltyParent);
                foreach(var candidate in bySelection)
                {
                    if(selectedParents.Count>=poolSize)break;
                    bool distinct=true;
                    foreach(var selected in selectedParents)
                    {
                        float distance=1f-OpenMorphPairMetrics.Compare(
                            candidate.tissue,selected.tissue).jaccard;
                        if(distance<minimumNovelDistance)
                        {
                            distinct=false;break;
                        }
                    }
                    if(distinct)selectedParents.Add(candidate);
                }
                // Keep a complete parent pool when the population is
                // genuinely homogeneous; do not invent diversity.
                foreach(var candidate in bySelection)
                {
                    if(selectedParents.Count>=poolSize)break;
                    if(!selectedParents.Contains(candidate))
                        selectedParents.Add(candidate);
                }
                genomes=new List<OpenMorphGenome>();
                parents=new List<int>();
                genomes.Add(cohort[0].genome.Copy());
                parents.Add(cohort[0].id);
                for(int i=1;i<populationSize;i++)
                {
                    // Round-robin ensures distinct selected parents are
                    // actually represented, instead of being starved out
                    // by repeated sampling of a single champion.
                    var parent=selectedParents[(i-1)%selectedParents.Count];
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
