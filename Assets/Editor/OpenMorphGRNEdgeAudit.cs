#if UNITY_EDITOR
using System;
using System.IO;
using System.Text;
using EarthRogueOrigins.PixelCreatures;
using UnityEngine;

namespace EarthRogueOrigins.Editor
{
    /// <summary>
    /// Tests the causal influence of signed gene-to-gene interactions
    /// while retaining the recurrent regulatory update itself.
    /// </summary>
    public static class OpenMorphGRNEdgeAudit
    {
        const int W=64,H=48;
        public static void Export(string folder,OpenMorphGenome[] genomes)
        {
            if(genomes==null || genomes.Length!=8)
                throw new ArgumentException("Expected eight genomes.");
            var board=new Color32[W*4*H*2];
            var bg=new Color32(31,38,36,255);
            for(int i=0;i<board.Length;i++)board[i]=bg;
            var csv=new StringBuilder(
                "pair,seed,zero_edges_cells,signed_edges_cells,occupancy_iou,changed_fates,zero_edges_components,signed_edges_components,zero_edges_border,signed_edges_border\n");

            for(int pair=0;pair<4;pair++)
            {
                var signedGenome=genomes[pair*2];
                var noEdges=signedGenome.Copy();
                foreach(var gene in noEdges.genes)
                {
                    gene.growthToFate=0f;
                    gene.fateToGrowth=0f;
                    gene.growthToBrake=0f;
                    gene.brakeToGrowth=0f;
                    gene.fateToBrake=0f;
                    gene.brakeToFate=0f;
                }
                noEdges.Validate();
                var a=OpenMorphTissue.Grow(noEdges,W,H,72,800,true,true);
                var b=OpenMorphTissue.Grow(signedGenome,W,H,72,800,true,true);
                var overlap=OpenMorphPairMetrics.Compare(a,b);
                var topologyA=OpenMorphTopologyMetrics.Analyze(a);
                var topologyB=OpenMorphTopologyMetrics.Analyze(b);
                int changed=0;
                for(int i=0;i<a.cells.Length;i++)
                    if(a.cells[i].fate!=b.cells[i].fate)changed++;
                csv.Append(pair+1).Append(',').Append(signedGenome.seed).Append(',')
                    .Append(a.Occupied()).Append(',')
                    .Append(b.Occupied()).Append(',')
                    .Append(Format(overlap.jaccard)).Append(',')
                    .Append(changed).Append(',')
                    .Append(topologyA.components).Append(',')
                    .Append(topologyB.components).Append(',')
                    .Append(topologyA.borderCells).Append(',')
                    .Append(topologyB.borderCells).AppendLine();
                for(int arm=0;arm<2;arm++)
                {
                    var image=OpenMorphTissue.Render(arm==0?a:b);
                    int index=pair*2+arm;
                    int col=index%4,row=1-index/4;
                    for(int y=0;y<H;y++)
                    for(int x=0;x<W;x++)
                    {
                        var pixel=image[y*W+x];
                        if(pixel.a!=0)
                            board[(row*H+y)*(W*4)+col*W+x]=pixel;
                    }
                }
            }
            var tex=new Texture2D(W*4,H*2,TextureFormat.RGBA32,false);
            tex.filterMode=FilterMode.Point;
            tex.SetPixels32(board);tex.Apply(false,false);
            try
            {
                File.WriteAllBytes(folder+"/actual_grn_edge_ablation.png",
                    tex.EncodeToPNG());
                File.WriteAllText(folder+"/grn_edge_ablation_metrics.csv",
                    csv.ToString());
            }
            finally {UnityEngine.Object.DestroyImmediate(tex);}
            File.AppendAllText(folder+"/provenance.txt",
                "Recurrent GRN signed-edge ablation: identical genomes "+
                "except six interaction weights set to zero; "+
                "same 72 ticks, seed and 800-cell cap.\n");
        }

        static string Format(float x)
        {
            return x.ToString("F5",
                System.Globalization.CultureInfo.InvariantCulture);
        }
    }
}
#endif
