#if UNITY_EDITOR
using System;
using System.IO;
using System.Text;
using EarthRogueOrigins.PixelCreatures;
using UnityEngine;

namespace EarthRogueOrigins.Editor
{
    /// <summary>
    /// Tests whether the same inherited structural difference produces
    /// anatomical divergence at several fixed developmental cell budgets.
    /// True C# tissue pixels, no rendering substitute or inferred results.
    /// </summary>
    public static class OpenMorphBudgetSweep
    {
        const int Width=64,Height=48;
        static readonly int[] Budgets={180,400,800};

        public static void Export(string folder,OpenMorphGenome[] genomes)
        {
            if(genomes==null || genomes.Length!=8)
                throw new ArgumentException("Expected four genotype-matched pairs.");
            var csv=new StringBuilder(
                "budget,pair,baseline_cells,branch_cells,common_cells,only_baseline,only_branch,union_cells,jaccard,symmetric_difference_fraction,baseline_border,branch_border,baseline_perimeter,branch_perimeter,baseline_compactness,branch_compactness,baseline_components,branch_components,baseline_holes,branch_holes\n");
            var bg=new Color32(31,38,36,255);
            foreach(int budget in Budgets)
            {
                var board=new Color32[Width*4*Height*2];
                for(int i=0;i<board.Length;i++)board[i]=bg;
                var fields=new OpenMorphTissue.Field[8];
                var shape=new OpenMorphTopologyMetrics.Result[8];

                for(int i=0;i<8;i++)
                {
                    fields[i]=OpenMorphTissue.Grow(
                        genomes[i],Width,Height,72,budget);
                    shape[i]=OpenMorphTopologyMetrics.Analyze(fields[i]);
                    var pixels=OpenMorphTissue.Render(fields[i]);
                    int col=i%4,row=1-i/4;
                    for(int y=0;y<Height;y++)
                    for(int x=0;x<Width;x++)
                    {
                        var pixel=pixels[y*Width+x];
                        if(pixel.a!=0)
                            board[(row*Height+y)*(Width*4)+col*Width+x]=pixel;
                    }
                }
                for(int pair=0;pair<4;pair++)
                {
                    int a=pair*2,b=a+1;
                    var delta=OpenMorphPairMetrics.Compare(fields[a],fields[b]);
                    csv.Append(budget).Append(',').Append(pair+1).Append(',')
                        .Append(delta.aOccupied).Append(',')
                        .Append(delta.bOccupied).Append(',')
                        .Append(delta.common).Append(',')
                        .Append(delta.onlyA).Append(',')
                        .Append(delta.onlyB).Append(',')
                        .Append(delta.union).Append(',')
                        .Append(Format(delta.jaccard)).Append(',')
                        .Append(Format(delta.symmetricDifferenceFraction)).Append(',')
                        .Append(shape[a].borderCells).Append(',')
                        .Append(shape[b].borderCells).Append(',')
                        .Append(shape[a].perimeter).Append(',')
                        .Append(shape[b].perimeter).Append(',')
                        .Append(Format(shape[a].compactness)).Append(',')
                        .Append(Format(shape[b].compactness)).Append(',')
                        .Append(shape[a].components).Append(',')
                        .Append(shape[b].components).Append(',')
                        .Append(shape[a].holes).Append(',')
                        .Append(shape[b].holes).AppendLine();
                }
                var tex=new Texture2D(Width*4,Height*2,
                    TextureFormat.RGBA32,false);
                tex.filterMode=FilterMode.Point;
                tex.SetPixels32(board);tex.Apply(false,false);
                try
                {
                    File.WriteAllBytes(folder+"/actual_budget_"+budget+
                        "_tissue.png",tex.EncodeToPNG());
                }
                finally {UnityEngine.Object.DestroyImmediate(tex);}
            }
            File.WriteAllText(folder+"/budget_sweep_metrics.csv",csv.ToString());

            // Direct counterfactual of the growth mechanism itself.
            // Keep seed, genotype, 800-cell maximum and tick count fixed.
            // The only changed variable is developmental organization.
            var structuredBoard=new Color32[Width*4*Height*2];
            var organizedCsv=new StringBuilder(
                "index,seed,arm,mode,occupied,border_cells,components,holes,perimeter,compactness,legacy_vs_organized_iou,pruned_cells,active_organizers\n");
            for(int i=0;i<structuredBoard.Length;i++)
                structuredBoard[i]=bg;
            for(int i=0;i<8;i++)
            {
                var baseline=OpenMorphTissue.Grow(
                    genomes[i],Width,Height,72,800);
                var structured=OpenMorphTissue.Grow(
                    genomes[i],Width,Height,72,800,true);
                var metrics=OpenMorphTopologyMetrics.Analyze(structured);
                var delta=OpenMorphPairMetrics.Compare(baseline,structured);
                var pixels=OpenMorphTissue.Render(structured);
                int col=i%4,row=1-i/4;
                for(int y=0;y<Height;y++)
                for(int x=0;x<Width;x++)
                {
                    var pixel=pixels[y*Width+x];
                    if(pixel.a!=0)
                        structuredBoard[(row*Height+y)*(Width*4)+col*Width+x]=pixel;
                }
                var activeOrganizers=new System.Collections.Generic.HashSet<int>();
                foreach(var cell in structured.cells)
                    if(cell.fate!=OpenMorphTissue.Fate.Empty)
                        activeOrganizers.Add(cell.organizerInnovation);
                organizedCsv.Append(i).Append(',').Append(genomes[i].seed).Append(',')
                    .Append((i%2)==0?"baseline":"added_branch").Append(',')
                    .Append("organized").Append(',')
                    .Append(metrics.occupied).Append(',')
                    .Append(metrics.borderCells).Append(',')
                    .Append(metrics.components).Append(',')
                    .Append(metrics.holes).Append(',')
                    .Append(metrics.perimeter).Append(',')
                    .Append(Format(metrics.compactness)).Append(',')
                    .Append(Format(delta.jaccard)).Append(',')
                    .Append(structured.prunedCells).Append(',')
                    .Append(activeOrganizers.Count).AppendLine();
            }
            var organizedTexture=new Texture2D(Width*4,Height*2,
                TextureFormat.RGBA32,false);
            organizedTexture.filterMode=FilterMode.Point;
            organizedTexture.SetPixels32(structuredBoard);
            organizedTexture.Apply(false,false);
            try
            {
                File.WriteAllBytes(folder+"/actual_organized_800_tissue.png",
                    organizedTexture.EncodeToPNG());
                File.WriteAllText(folder+"/organized_800_metrics.csv",
                    organizedCsv.ToString());
            }
            finally { UnityEngine.Object.DestroyImmediate(organizedTexture); }

            // Regulation-only counterfactual: each genome is copied with
            // topology, dimensions, chemistry and random seed unchanged.
            // Only heritable growth/tip/inhibitory response alleles vary.
            var regBoard=new Color32[Width*4*Height*2];
            var regCsv=new StringBuilder(
                "pair,seed,original_cells,regulatory_cells,common_cells,jaccard,original_border,regulatory_border,original_components,regulatory_components,original_pruned,regulatory_pruned\n");
            for(int i=0;i<regBoard.Length;i++)regBoard[i]=bg;
            for(int pair=0;pair<4;pair++)
            {
                var originalGenome=genomes[pair*2];
                var alteredGenome=originalGenome.Copy();
                foreach(var gene in alteredGenome.genes)
                {
                    gene.growthResponse=Mathf.Clamp01(
                        gene.growthResponse+.26f);
                    gene.tipResponse=Mathf.Clamp01(
                        gene.tipResponse+.30f);
                    gene.inhibitionResponse=Mathf.Clamp01(
                        gene.inhibitionResponse-.25f);
                }
                alteredGenome.Validate();
                var source=OpenMorphTissue.Grow(
                    originalGenome,Width,Height,72,800,true);
                var variant=OpenMorphTissue.Grow(
                    alteredGenome,Width,Height,72,800,true);
                var overlap=OpenMorphPairMetrics.Compare(source,variant);
                var sourceShape=OpenMorphTopologyMetrics.Analyze(source);
                var variantShape=OpenMorphTopologyMetrics.Analyze(variant);
                regCsv.Append(pair+1).Append(',').Append(originalGenome.seed).Append(',')
                    .Append(sourceShape.occupied).Append(',')
                    .Append(variantShape.occupied).Append(',')
                    .Append(overlap.common).Append(',')
                    .Append(Format(overlap.jaccard)).Append(',')
                    .Append(sourceShape.borderCells).Append(',')
                    .Append(variantShape.borderCells).Append(',')
                    .Append(sourceShape.components).Append(',')
                    .Append(variantShape.components).Append(',')
                    .Append(source.prunedCells).Append(',')
                    .Append(variant.prunedCells).AppendLine();
                foreach(var arm in new[]{0,1})
                {
                    var pixels=OpenMorphTissue.Render(arm==0?source:variant);
                    int index=pair*2+arm;
                    int col=index%4,row=1-index/4;
                    for(int y=0;y<Height;y++)
                    for(int x=0;x<Width;x++)
                    {
                        var pixel=pixels[y*Width+x];
                        if(pixel.a!=0)
                            regBoard[(row*Height+y)*(Width*4)+col*Width+x]=pixel;
                    }
                }
            }
            var regTex=new Texture2D(Width*4,Height*2,
                TextureFormat.RGBA32,false);
            regTex.filterMode=FilterMode.Point;
            regTex.SetPixels32(regBoard);
            regTex.Apply(false,false);
            try
            {
                File.WriteAllBytes(folder+"/actual_regulatory_counterfactual.png",
                    regTex.EncodeToPNG());
                File.WriteAllText(folder+"/regulatory_counterfactual_metrics.csv",
                    regCsv.ToString());
            }
            finally { UnityEngine.Object.DestroyImmediate(regTex); }

            OpenMorphFateAudit.Export(folder,genomes);
            OpenMorphGRNAudit.Export(folder,genomes);
            OpenMorphGRNEdgeAudit.Export(folder,genomes);

            File.AppendAllText(folder+"/provenance.txt",
                "Budgets 180/400/800, same 72 ticks and seed within "+
                "each pair. No scale normalization; pairwise "+
                "intersection and union use raw cell coordinates.\n");
            Debug.Log("Open tissue budget sweep exported: "+folder);
        }

        static string Format(float value)
        {
            return value.ToString("F5",
                System.Globalization.CultureInfo.InvariantCulture);
        }
    }
}
#endif
