#if UNITY_EDITOR
using System;
using System.IO;
using System.Text;
using EarthRogueOrigins.PixelCreatures;
using UnityEngine;

namespace EarthRogueOrigins.Editor
{
    /// <summary>
    /// Diagnoses development collapse instead of inferring it from sprites.
    /// The two arms use identical organisms, seeds, cell caps and duration;
    /// only the opt-in GRN regulator differs from static organized growth.
    /// </summary>
    public static class OpenMorphGRNTraceAudit
    {
        public static void Export(string folder,OpenMorphGenome[] genomes)
        {
            if(genomes==null || genomes.Length!=8)
                throw new ArgumentException("Expected four matched genome pairs.");
            var rows=new StringBuilder(
                "pair,seed,mode,tick,occupied,proposed_births,accepted_births,pruned_cells,eligible_sites,mean_static_chance,mean_final_chance,inhibited_sites,boosted_sites\n");
            var outcomes=new StringBuilder(
                "pair,seed,static_final,recurrent_final,static_peak,recurrent_peak,static_last_birth,recurrent_last_birth,static_pruned,recurrent_pruned,static_total_eligible,recurrent_total_eligible,recurrent_inhibited_sites,recurrent_boosted_sites,static_mean_chance,recurrent_mean_static_chance,recurrent_mean_final_chance\n");
            for(int pair=0;pair<4;pair++)
            {
                var genome=genomes[pair*2];
                var staticField=OpenMorphTissue.Grow(
                    genome,64,48,72,800,true);
                var recurrent=OpenMorphTissue.Grow(
                    genome,64,48,72,800,true,true);
                WriteTrace(rows,pair+1,genome.seed,"static",staticField);
                WriteTrace(rows,pair+1,genome.seed,"recurrent",recurrent);
                outcomes.Append(pair+1).Append(',').Append(genome.seed).Append(',')
                    .Append(staticField.Occupied()).Append(',')
                    .Append(recurrent.Occupied()).Append(',')
                    .Append(Peak(staticField)).Append(',')
                    .Append(Peak(recurrent)).Append(',')
                    .Append(LastBirth(staticField)).Append(',')
                    .Append(LastBirth(recurrent)).Append(',')
                    .Append(staticField.prunedCells).Append(',')
                    .Append(recurrent.prunedCells).Append(',')
                    .Append(Sum(staticField.eligibleSitesByTick)).Append(',')
                    .Append(Sum(recurrent.eligibleSitesByTick)).Append(',')
                    .Append(Sum(recurrent.inhibitedSitesByTick)).Append(',')
                    .Append(Sum(recurrent.boostedSitesByTick)).Append(',')
                    .Append(Format(MeanChance(staticField.baseChanceSumByTick,
                        staticField.eligibleSitesByTick))).Append(',')
                    .Append(Format(MeanChance(recurrent.baseChanceSumByTick,
                        recurrent.eligibleSitesByTick))).Append(',')
                    .Append(Format(MeanChance(recurrent.finalChanceSumByTick,
                        recurrent.eligibleSitesByTick))).AppendLine();
            }
            File.WriteAllText(folder+"/grn_growth_trace.csv",rows.ToString());
            File.WriteAllText(folder+"/grn_growth_outcomes.csv",outcomes.ToString());
            File.AppendAllText(folder+"/provenance.txt",
                "GRN v1.2 diagnostic growth trace: occupied cells and "+
                "division/pruning events per tick. Same seeds, genomes, "+
                "72 steps and 800-cell cap; structured static vs recurrent.\n");
        }

        static void WriteTrace(StringBuilder result,int pair,int seed,
            string mode,OpenMorphTissue.Field field)
        {
            for(int t=0;t<=field.steps;t++)
            {
                result.Append(pair).Append(',').Append(seed).Append(',')
                    .Append(mode).Append(',').Append(t).Append(',')
                    .Append(field.occupiedByTick[t]).Append(',')
                    .Append(t==0?0:field.proposedBirthsByTick[t-1]).Append(',')
                    .Append(t==0?0:field.acceptedBirthsByTick[t-1]).Append(',')
                    .Append(t==0?0:field.prunedByTick[t-1]).Append(',')
                    .Append(t==0?0:field.eligibleSitesByTick[t-1]).Append(',')
                    .Append(t==0?"0":Format(
                        MeanChanceAt(field.baseChanceSumByTick,
                            field.eligibleSitesByTick,t-1))).Append(',')
                    .Append(t==0?"0":Format(
                        MeanChanceAt(field.finalChanceSumByTick,
                            field.eligibleSitesByTick,t-1))).Append(',')
                    .Append(t==0?0:field.inhibitedSitesByTick[t-1]).Append(',')
                    .Append(t==0?0:field.boostedSitesByTick[t-1]).AppendLine();
            }
        }

        static int Sum(int[] values)
        {
            int total=0;
            foreach(int v in values)total+=v;
            return total;
        }

        static float MeanChance(float[] totals,int[] counts)
        {
            float sum=0f;int n=0;
            for(int i=0;i<totals.Length;i++)
            {
                sum+=totals[i];n+=counts[i];
            }
            return n==0?0f:sum/n;
        }

        static float MeanChanceAt(float[] totals,int[] counts,int index)
        {
            return counts[index]==0?0f:totals[index]/counts[index];
        }

        static string Format(float value)
        {
            return value.ToString("F6",
                System.Globalization.CultureInfo.InvariantCulture);
        }

        static int Peak(OpenMorphTissue.Field field)
        {
            int max=0;
            foreach(int count in field.occupiedByTick)
                max=Math.Max(max,count);
            return max;
        }

        static int LastBirth(OpenMorphTissue.Field field)
        {
            int last=0;
            for(int t=0;t<field.acceptedBirthsByTick.Length;t++)
                if(field.acceptedBirthsByTick[t]>0)last=t+1;
            return last;
        }
    }
}
#endif
