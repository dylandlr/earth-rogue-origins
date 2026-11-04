#if UNITY_EDITOR
using System;
using System.IO;
using System.Text;
using EarthRogueOrigins.PixelCreatures;
using UnityEditor;
using UnityEngine;

namespace EarthRogueOrigins.Editor
{
    /// <summary>
    /// Runs the real C# tissue development rule set, exporting raw 64x48
    /// multicellular fates. These are experimental tissues, not claimed
    /// viable animals, and do not borrow the original creature sprites.
    /// </summary>
    public static class OpenMorphTissueAudit
    {
        [MenuItem("Earth: Rogue Origins/Evolution/Export Open Tissue Growth Audit")]
        public static void Run()
        {
            const int w=64,h=48;
            string folder="ValidationResults/OpenTissue/run-"+
                DateTime.UtcNow.ToString("yyyyMMdd-HHmmss-fff");
            Directory.CreateDirectory(folder);
            var board=new Color32[w*4*h*2];
            var equalBudgetBoard=new Color32[w*4*h*2];
            var budgetCsv=new StringBuilder(
                "index,seed,arm,cell_budget,occupied,perimeter,compactness,components,holes,border_cells,bbox_width,bbox_height\n");
            var background=new Color32(31,38,36,255);
            for(int i=0;i<board.Length;i++)
            {
                board[i]=background;
                equalBudgetBoard[i]=background;
            }
            var csv=new StringBuilder(
                "index,seed,genes,segments,steps,occupied,structural,contractile,sensory,transport,storage,arm,bbox_width,bbox_height,perimeter,components,holes,border_cells,compactness,fill_fraction\n");

            var genomes=new OpenMorphGenome[8];
            // Four genotype-matched counterfactual pairs. Each pair has
            // identical genome state and seed except for one inserted
            // developmental branch. This directly tests whether new
            // topology changes the grown cellular phenotype.
            for(int i=0;i<8;i++)
            {
                int pair=i/2;
                int seed=9100+pair*71;
                var genome=OpenMorphGenome.Founder(seed);
                for(int m=0;m<8+pair*3;m++)
                    genome=genome.Mutate(seed*53+m*1013);
                if((i%2)==1)
                {
                    int maxInnovation=0;
                    foreach(var existing in genome.genes)
                        maxInnovation=Math.Max(maxInnovation,existing.innovation);
                    genome.genes.Add(new OpenMorphGenome.Gene {
                        innovation=maxInnovation+1,
                        parentInnovation=0,attachX=.6f,attachY=0f,
                        angle=pair%2==0?75f:-75f,
                        length=.85f,width=.15f,
                        repeat=2,mirror=pair>=2,rigidity=.85f,pigment=.75f
                    });
                }
                genome.Validate();
                genomes[i]=genome;
                var field=OpenMorphTissue.Grow(genome,w,h,72);
                var pixels=OpenMorphTissue.Render(field);
                // Same genomes, seed and number of ticks; constrain each
                // organism to equal maximum tissue mass for shape analysis.
                const int sharedBudget=180;
                var budgetField=OpenMorphTissue.Grow(genome,w,h,72,sharedBudget);
                var budgetPixels=OpenMorphTissue.Render(budgetField);
                var budgetTopology=OpenMorphTopologyMetrics.Analyze(budgetField);
                int col=i%4,row=1-i/4;
                int minX=w,minY=h,maxX=-1,maxY=-1;
                for(int y=0;y<h;y++)
                for(int x=0;x<w;x++)
                {
                    var pixel=pixels[y*w+x];
                    if(pixel.a==0)continue;
                    board[(row*h+y)*(w*4)+col*w+x]=pixel;
                    minX=Math.Min(minX,x);maxX=Math.Max(maxX,x);
                    minY=Math.Min(minY,y);maxY=Math.Max(maxY,y);
                }
                var topology=OpenMorphTopologyMetrics.Analyze(field);
                for(int y=0;y<h;y++)
                for(int x=0;x<w;x++)
                {
                    var pixel=budgetPixels[y*w+x];
                    if(pixel.a!=0)
                        equalBudgetBoard[(row*h+y)*(w*4)+col*w+x]=pixel;
                }
                budgetCsv.Append(i).Append(',').Append(seed).Append(',')
                    .Append((i%2)==0?"baseline":"added_branch").Append(',')
                    .Append(sharedBudget).Append(',')
                    .Append(budgetTopology.occupied).Append(',')
                    .Append(budgetTopology.perimeter).Append(',')
                    .Append(budgetTopology.compactness.ToString("F5",
                        System.Globalization.CultureInfo.InvariantCulture)).Append(',')
                    .Append(budgetTopology.components).Append(',')
                    .Append(budgetTopology.holes).Append(',')
                    .Append(budgetTopology.borderCells).Append(',')
                    .Append(budgetTopology.bboxWidth).Append(',')
                    .Append(budgetTopology.bboxHeight).AppendLine();
                csv.Append(i).Append(',').Append(seed).Append(',')
                    .Append(genome.genes.Count).Append(',')
                    .Append(OpenMorphDevelopment.Grow(genome).Length).Append(',')
                    .Append(field.steps).Append(',')
                    .Append(field.Occupied()).Append(',')
                    .Append(field.FateCount(OpenMorphTissue.Fate.Structural)).Append(',')
                    .Append(field.FateCount(OpenMorphTissue.Fate.Contractile)).Append(',')
                    .Append(field.FateCount(OpenMorphTissue.Fate.Sensory)).Append(',')
                    .Append(field.FateCount(OpenMorphTissue.Fate.Transport)).Append(',')
                    .Append(field.FateCount(OpenMorphTissue.Fate.Storage)).Append(',')
                    .Append((i%2)==0?"baseline":"added_branch").Append(',')
                    .Append(maxX<minX?0:maxX-minX+1).Append(',')
                    .Append(maxY<minY?0:maxY-minY+1).Append(',')
                    .Append(topology.perimeter).Append(',')
                    .Append(topology.components).Append(',')
                    .Append(topology.holes).Append(',')
                    .Append(topology.borderCells).Append(',')
                    .Append(topology.compactness.ToString("F5",
                        System.Globalization.CultureInfo.InvariantCulture)).Append(',')
                    .Append(topology.fillFraction.ToString("F5",
                        System.Globalization.CultureInfo.InvariantCulture))
                    .AppendLine();
            }
            var budgetTex=new Texture2D(w*4,h*2,TextureFormat.RGBA32,false);
            budgetTex.filterMode=FilterMode.Point;
            budgetTex.SetPixels32(equalBudgetBoard);
            budgetTex.Apply(false,false);
            var tex=new Texture2D(w*4,h*2,TextureFormat.RGBA32,false);
            tex.filterMode=FilterMode.Point;
            tex.SetPixels32(board);
            tex.Apply(false,false);
            try
            {
                File.WriteAllBytes(folder+"/actual_tissue_growth.png",
                    tex.EncodeToPNG());
                File.WriteAllText(folder+"/tissue_metrics.csv",csv.ToString());
                File.WriteAllBytes(folder+"/actual_equal_budget_tissue.png",
                    budgetTex.EncodeToPNG());
                File.WriteAllText(folder+"/equal_budget_metrics.csv",budgetCsv.ToString());
                File.WriteAllText(folder+"/provenance.txt",
                    "Unity C# OpenMorphGenome -> OpenMorphTissue.Grow -> Render\n"+
                    "Diagnostic 2D tissue growth; four matched baseline/added-branch pairs.\n"+
                    "Common random numbers keyed by seed, tick, x, y; "+
                    "morphology metrics include clipping and cavities.\n"+
                    "NOT biological tissues, organs, viability or an evolved ecosystem.\n");
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(tex);
                UnityEngine.Object.DestroyImmediate(budgetTex);
            }
            // Run matched developmental budget sweep with the exact
            // same genomes from the original contact sheets.
            OpenMorphBudgetSweep.Export(folder,genomes);
            Debug.Log("Open tissue growth audit exported: "+folder);
        }
    }
}
#endif
