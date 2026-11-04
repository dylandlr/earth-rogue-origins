#if UNITY_EDITOR
using System;
using System.IO;
using System.Text;
using EarthRogueOrigins.PixelCreatures;
using UnityEngine;

namespace EarthRogueOrigins.Editor
{
    /// <summary>
    /// Actual Unity C# comparison of frozen local regulation with an
    /// inheritable recurrent 3-channel GRN. Never uses sprite templates.
    /// </summary>
    public static class OpenMorphGRNAudit
    {
        const int W=64,H=48;

        public static void Export(string folder,OpenMorphGenome[] genomes)
        {
            if(genomes==null||genomes.Length!=8)
                throw new ArgumentException("Expected eight developmental genomes.");
            var board=new Color32[W*4*H*2];
            var background=new Color32(31,38,36,255);
            for(int i=0;i<board.Length;i++)board[i]=background;
            var csv=new StringBuilder(
                "pair,seed,static_cells,recurrent_cells,occupancy_iou,changed_tissue_fates,recurrent_border,recurrent_components,recurrent_holes,static_pruned,recurrent_pruned,mean_growth_expression,mean_fate_expression,mean_brake_expression\n");

            for(int pair=0;pair<4;pair++)
            {
                var genome=genomes[pair*2];
                var staticTissue=OpenMorphTissue.Grow(
                    genome,W,H,72,800,true);
                var recurrentTissue=OpenMorphTissue.Grow(
                    genome,W,H,72,800,true,true);
                var delta=OpenMorphPairMetrics.Compare(staticTissue,recurrentTissue);
                var shape=OpenMorphTopologyMetrics.Analyze(recurrentTissue);
                int changed=0,count=0;
                float growth=0f,fate=0f,brake=0f;
                for(int i=0;i<staticTissue.cells.Length;i++)
                {
                    if(staticTissue.cells[i].fate!=recurrentTissue.cells[i].fate)
                        changed++;
                    var cell=recurrentTissue.cells[i];
                    if(cell.fate==OpenMorphTissue.Fate.Empty)continue;
                    count++;
                    growth+=cell.regulatoryGrowth;
                    fate+=cell.regulatoryFate;
                    brake+=cell.regulatoryBrake;
                }
                csv.Append(pair+1).Append(',').Append(genome.seed).Append(',')
                    .Append(staticTissue.Occupied()).Append(',')
                    .Append(recurrentTissue.Occupied()).Append(',')
                    .Append(Format(delta.jaccard)).Append(',')
                    .Append(changed).Append(',')
                    .Append(shape.borderCells).Append(',')
                    .Append(shape.components).Append(',')
                    .Append(shape.holes).Append(',')
                    .Append(staticTissue.prunedCells).Append(',')
                    .Append(recurrentTissue.prunedCells).Append(',')
                    .Append(Format(growth/Math.Max(1,count))).Append(',')
                    .Append(Format(fate/Math.Max(1,count))).Append(',')
                    .Append(Format(brake/Math.Max(1,count))).AppendLine();

                for(int arm=0;arm<2;arm++)
                {
                    var pixels=OpenMorphTissue.Render(
                        arm==0?staticTissue:recurrentTissue);
                    int index=pair*2+arm;
                    int col=index%4,row=1-index/4;
                    for(int y=0;y<H;y++)
                    for(int x=0;x<W;x++)
                    {
                        var pixel=pixels[y*W+x];
                        if(pixel.a!=0)
                            board[(row*H+y)*(W*4)+col*W+x]=pixel;
                    }
                }
            }
            var texture=new Texture2D(W*4,H*2,TextureFormat.RGBA32,false);
            texture.filterMode=FilterMode.Point;
            texture.SetPixels32(board);texture.Apply(false,false);
            try
            {
                File.WriteAllBytes(folder+"/actual_grn_counterfactual.png",
                    texture.EncodeToPNG());
                File.WriteAllText(folder+"/grn_counterfactual_metrics.csv",
                    csv.ToString());
            }
            finally {UnityEngine.Object.DestroyImmediate(texture);}
            File.AppendAllText(folder+"/provenance.txt",
                "GRN v1.1 audit: same genomes, seeds, 72 steps and 800-cell cap. "+
                "Static regulation vs signed recurrent expression. "+
                "No biological function inferred.\n");
        }

        static string Format(float number)
        {
            return number.ToString("F5",
                System.Globalization.CultureInfo.InvariantCulture);
        }
    }
}
#endif
