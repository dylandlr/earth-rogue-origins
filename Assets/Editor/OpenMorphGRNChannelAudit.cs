#if UNITY_EDITOR
using System;
using System.IO;
using System.Text;
using EarthRogueOrigins.PixelCreatures;
using UnityEngine;

namespace EarthRogueOrigins.Editor
{
    /// <summary>
    /// Compares the consequences of independent growth- and brake-channel
    /// contributions to an otherwise unchanged recurrent GRN.
    /// All arms share each genome, developmental rules and random field.
    /// </summary>
    public static class OpenMorphGRNChannelAudit
    {
        const int W=64,H=48;

        public static void Export(string folder,OpenMorphGenome[] genomes)
        {
            if(genomes==null||genomes.Length!=8)
                throw new ArgumentException("Four genome pairs are required.");
            var csv=new StringBuilder(
                "pair,seed,channel,occupied,eligible_sites,proposed_births,accepted_births,inhibited_sites,boosted_sites,mean_base_chance,mean_final_chance,components,border_cells,pruned_cells\n");
            var modes=new[]{
                OpenMorphGRN.BirthChannel.Full,
                OpenMorphGRN.BirthChannel.GrowthOnly,
                OpenMorphGRN.BirthChannel.BrakeOnly,
                OpenMorphGRN.BirthChannel.Neutral
            };
            for(int pair=0;pair<4;pair++)
            {
                var genome=genomes[pair*2];
                var board=new Color32[W*4*H];
                var background=new Color32(31,38,36,255);
                for(int i=0;i<board.Length;i++)board[i]=background;
                for(int arm=0;arm<modes.Length;arm++)
                {
                    var field=OpenMorphTissue.Grow(
                        genome,W,H,72,800,true,true,modes[arm]);
                    var shape=OpenMorphTopologyMetrics.Analyze(field);
                    int eligible=Sum(field.eligibleSitesByTick);
                    csv.Append(pair+1).Append(',').Append(genome.seed).Append(',')
                        .Append(modes[arm]).Append(',')
                        .Append(field.Occupied()).Append(',')
                        .Append(eligible).Append(',')
                        .Append(Sum(field.proposedBirthsByTick)).Append(',')
                        .Append(Sum(field.acceptedBirthsByTick)).Append(',')
                        .Append(Sum(field.inhibitedSitesByTick)).Append(',')
                        .Append(Sum(field.boostedSitesByTick)).Append(',')
                        .Append(Format(Mean(field.baseChanceSumByTick,eligible))).Append(',')
                        .Append(Format(Mean(field.finalChanceSumByTick,eligible))).Append(',')
                        .Append(shape.components).Append(',')
                        .Append(shape.borderCells).Append(',')
                        .Append(field.prunedCells).AppendLine();

                    var image=OpenMorphTissue.Render(field);
                    for(int y=0;y<H;y++)
                    for(int x=0;x<W;x++)
                    {
                        var pixel=image[y*W+x];
                        if(pixel.a!=0)board[y*(W*4)+arm*W+x]=pixel;
                    }
                }
                var texture=new Texture2D(W*4,H,TextureFormat.RGBA32,false);
                texture.filterMode=FilterMode.Point;
                texture.SetPixels32(board);texture.Apply(false,false);
                try
                {
                    File.WriteAllBytes(folder+"/actual_grn_channels_pair_"+
                        (pair+1)+".png",texture.EncodeToPNG());
                }
                finally {UnityEngine.Object.DestroyImmediate(texture);}
            }
            File.WriteAllText(folder+"/grn_channel_ablation_metrics.csv",
                csv.ToString());
            File.AppendAllText(folder+"/provenance.txt",
                "v1.4 recurrent GRN channel interventions: image columns "+
                "Full, GrowthOnly, BrakeOnly, Neutral. "+
                "All arms use identical genome, seed, 72 steps and "+
                "800-cell cap; only the growth decision contribution "+
                "of the two expression channels differs.\n");
        }

        static int Sum(int[] values)
        {
            int sum=0;foreach(var v in values)sum+=v;return sum;
        }

        static float Mean(float[] values,int eligible)
        {
            if(eligible==0)return 0f;
            float sum=0f;foreach(var v in values)sum+=v;
            return sum/eligible;
        }

        static string Format(float x)
        {
            return x.ToString("F6",
                System.Globalization.CultureInfo.InvariantCulture);
        }
    }
}
#endif
