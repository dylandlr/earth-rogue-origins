#if UNITY_EDITOR
using System;
using System.IO;
using System.Text;
using EarthRogueOrigins.PixelCreatures;
using UnityEngine;

namespace EarthRogueOrigins.Editor
{
    /// <summary>
    /// Controls only the fate-response allele, not growth, geometry, or seed.
    /// A successful test changes cellular differentiation while retaining
    /// identical occupancy and counts under matched simulation conditions.
    /// </summary>
    public static class OpenMorphFateAudit
    {
        const int W=64,H=48;
        public static void Export(string folder,OpenMorphGenome[] genomes)
        {
            if(genomes==null||genomes.Length!=8)
                throw new ArgumentException("Four original/branch pairs required.");
            var board=new Color32[W*4*H*2];
            var background=new Color32(31,38,36,255);
            for(int i=0;i<board.Length;i++)board[i]=background;
            var csv=new StringBuilder(
                "pair,seed,original_cells,fate_changed_cells,occupied_iou,cell_fate_changes,original_components,changed_components,original_pruned,changed_pruned\n");
            for(int pair=0;pair<4;pair++)
            {
                var original=genomes[pair*2];
                var altered=original.Copy();
                foreach(var gene in altered.genes)
                    gene.fateResponse=.98f;
                altered.Validate();
                var a=OpenMorphTissue.Grow(original,W,H,72,800,true);
                var b=OpenMorphTissue.Grow(altered,W,H,72,800,true);
                var overlap=OpenMorphPairMetrics.Compare(a,b);
                int differentFates=0;
                for(int cell=0;cell<a.cells.Length;cell++)
                    if(a.cells[cell].fate!=b.cells[cell].fate)
                        differentFates++;
                var topA=OpenMorphTopologyMetrics.Analyze(a);
                var topB=OpenMorphTopologyMetrics.Analyze(b);
                csv.Append(pair+1).Append(',').Append(original.seed).Append(',')
                    .Append(a.Occupied()).Append(',').Append(b.Occupied()).Append(',')
                    .Append(Format(overlap.jaccard)).Append(',')
                    .Append(differentFates).Append(',')
                    .Append(topA.components).Append(',').Append(topB.components)
                    .Append(',').Append(a.prunedCells).Append(',')
                    .Append(b.prunedCells).AppendLine();
                for(int arm=0;arm<2;arm++)
                {
                    var pixels=OpenMorphTissue.Render(arm==0?a:b);
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
            var tex=new Texture2D(W*4,H*2,TextureFormat.RGBA32,false);
            tex.filterMode=FilterMode.Point;
            tex.SetPixels32(board);tex.Apply(false,false);
            try
            {
                File.WriteAllBytes(folder+"/actual_fate_only_counterfactual.png",
                    tex.EncodeToPNG());
                File.WriteAllText(folder+"/fate_only_metrics.csv",csv.ToString());
            }
            finally { UnityEngine.Object.DestroyImmediate(tex); }
            File.AppendAllText(folder+"/provenance.txt",
                "Fate-only v1 experiment: identical topology, growth regulation, "+
                "random seed and 800-cell cap; only fateResponse changed.\n");
        }

        static string Format(float x)
        {
            return x.ToString("F5",
                System.Globalization.CultureInfo.InvariantCulture);
        }
    }
}
#endif
