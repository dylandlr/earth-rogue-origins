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
    /// One-click end-to-end emergent morphology experiment. All images are
    /// native Unity C# outputs of actual selected developmental genomes.
    /// No artist-drawn animal templates or image-generation substitutes.
    /// </summary>
    public static class OpenMorphEvolutionAudit
    {
        [Serializable]
        sealed class GenomeRecord
        {
            public int id,generation,parentId,seed;
            public OpenMorphGenome.Gene[] genes;
        }

        [MenuItem("Earth: Rogue Origins/Evolution/Run Emergent Creature Generations")]
        public static void Run()
        {
            string folder="ValidationResults/Emergent/run-"+
                DateTime.UtcNow.ToString("yyyyMMdd-HHmmss-fff");
            Directory.CreateDirectory(folder);
            var result=OpenMorphEvolution.Evolve();
            var csv=new StringBuilder(
                "generation,id,parent_id,seed,genes,segments,occupied,scarce_survival,moderate_survival,fitness,diversity,novelty,selection_score,guide_coverage,supported_tissue_cells,expressed_branches,unexpressed_branches,causal_branches,strongest_branch_effect\n");
            var branches=new StringBuilder(
                "generation,id,innovation,guided_cells,occupied_cells,expression_fraction,expressed\n");
            var causalCsv=new StringBuilder(
                "generation,id,innovation,removed_genes,original_cells,ablated_cells,changed_cells,occupancy_iou,changed_fraction,morphologically_active\n");
            var champions=new StringBuilder(
                "generation,id,parent_id,genes,segments,occupied,fitness,scarce_survival,moderate_survival\n");
            foreach(var animal in result.evaluated)
            {
                csv.Append(animal.generation).Append(',')
                    .Append(animal.id).Append(',')
                    .Append(animal.parentId).Append(',')
                    .Append(animal.genome.seed).Append(',')
                    .Append(animal.genes).Append(',')
                    .Append(animal.segments).Append(',')
                    .Append(animal.occupied).Append(',')
                    .Append(N(animal.scarceSurvival)).Append(',')
                    .Append(N(animal.moderateSurvival)).Append(',')
                    .Append(N(animal.fitness)).Append(',')
                    .Append(N(animal.diversity)).Append(',')
                    .Append(N(animal.novelty)).Append(',')
                    .Append(N(animal.selectionScore)).Append(',')
                    .Append(N(animal.bodyGuidanceCoverage)).Append(',')
                    .Append(animal.supportedTissueCells).Append(',')
                    .Append(animal.expressedBranches).Append(',')
                    .Append(animal.unexpressedBranches).Append(',')
                    .Append(animal.causalBranches).Append(',')
                    .Append(N(animal.strongestBranchEffect)).AppendLine();
            }
            foreach(var animal in result.champions)
                champions.Append(animal.generation).Append(',')
                    .Append(animal.id).Append(',')
                    .Append(animal.parentId).Append(',')
                    .Append(animal.genes).Append(',')
                    .Append(animal.segments).Append(',')
                    .Append(animal.occupied).Append(',')
                    .Append(N(animal.fitness)).Append(',')
                    .Append(N(animal.scarceSurvival)).Append(',')
                    .Append(N(animal.moderateSurvival)).AppendLine();

            // Show every specimen in the final generation, not merely
            // the visually appealing or highest-scoring individuals.
            const int tileW=128,tileH=96;
            var board=new Color32[4*tileW*2*tileH];
            var structures=new Color32[4*tileW*2*tileH];
            var alignment=new Color32[4*tileW*2*tileH];
            var background=new Color32(31,38,36,255);
            for(int i=0;i<board.Length;i++)
            {
                board[i]=background;
                structures[i]=background;
                alignment[i]=background;
            }
            int final=result.generations-1;
            int slot=0;
            foreach(var animal in result.evaluated)
            {
                if(animal.generation!=final)continue;
                var causalReport=OpenMorphCausalBranches.Analyze(
                    animal.genome,animal.tissue);
                foreach(var impact in causalReport.branches)
                    causalCsv.Append(animal.generation).Append(',')
                        .Append(animal.id).Append(',')
                        .Append(impact.innovation).Append(',')
                        .Append(impact.removedGenes).Append(',')
                        .Append(impact.originalCells).Append(',')
                        .Append(impact.ablatedCells).Append(',')
                        .Append(impact.changedCells).Append(',')
                        .Append(N(impact.occupancyJaccard)).Append(',')
                        .Append(N(impact.changedFraction)).Append(',')
                        .Append(impact.morphologicallyActive?"true":"false")
                        .AppendLine();
                var expressionReport=OpenMorphBranchExpression.Analyze(
                    animal.genome,animal.tissue);
                foreach(var gene in expressionReport.genes)
                    branches.Append(animal.generation).Append(',')
                        .Append(animal.id).Append(',')
                        .Append(gene.innovation).Append(',')
                        .Append(gene.guidedCells).Append(',')
                        .Append(gene.occupiedCells).Append(',')
                        .Append(N(gene.expressionFraction)).Append(',')
                        .Append(gene.expressed?"true":"false")
                        .AppendLine();
                var pixels=OpenMorphTissue.Render(animal.tissue);
                var geometry=OpenMorphRasterizer.Render(animal.genome);
                var guidance=OpenMorphOrganizers.Build(animal.genome,
                    animal.tissue.width,animal.tissue.height);
                int col=slot%4,row=1-slot/4;
                for(int y=0;y<tileH;y++)
                for(int x=0;x<tileW;x++)
                {
                    var geomPixel=geometry[y*tileW+x];
                    if(geomPixel.a!=0)
                        structures[(row*tileH+y)*tileW*4+col*tileW+x]=geomPixel;
                }
                for(int y=0;y<animal.tissue.height;y++)
                for(int x=0;x<animal.tissue.width;x++)
                {
                    var pixel=pixels[y*animal.tissue.width+x];
                    bool present=pixel.a!=0;
                    bool guided=guidance.activation[
                        y*animal.tissue.width+x]>=.25f;
                    if(!present&&!guided)continue;
                    // Nearest-neighbor doubling, centered on a native
                    // 128x96 tile (tissue field 48x36 => 96x72).
                    int startX=col*tileW+16+x*2;
                    int startY=row*tileH+12+y*2;
                    for(int yy=0;yy<2;yy++)
                    for(int xx=0;xx<2;xx++)
                    {
                        int index=(startY+yy)*tileW*4+startX+xx;
                        if(present)board[index]=pixel;
                        // White=matched tissue/guidance; magenta=tissue
                        // outside inherited organizer; cyan=unused cue.
                        alignment[index]=present&&guided?
                            new Color32(230,231,215,255):
                            present?new Color32(207,99,180,255):
                            new Color32(76,183,198,255);
                    }
                }
                var snapshot=new GenomeRecord {
                    id=animal.id,generation=animal.generation,
                    parentId=animal.parentId,seed=animal.genome.seed,
                    genes=animal.genome.genes.ToArray()
                };
                File.WriteAllText(folder+"/genome_"+animal.id+".json",
                    JsonUtility.ToJson(snapshot,true));
                slot++;
            }
            if(slot!=result.populationSize)
                throw new InvalidOperationException("Final generation incomplete.");
            var tex=new Texture2D(tileW*4,tileH*2,
                TextureFormat.RGBA32,false);
            tex.filterMode=FilterMode.Point;
            tex.SetPixels32(board);tex.Apply(false,false);
            var alignmentTex=new Texture2D(tileW*4,tileH*2,
                TextureFormat.RGBA32,false);
            alignmentTex.filterMode=FilterMode.Point;
            alignmentTex.SetPixels32(alignment);
            alignmentTex.Apply(false,false);
            var geometryTex=new Texture2D(tileW*4,tileH*2,
                TextureFormat.RGBA32,false);
            geometryTex.filterMode=FilterMode.Point;
            geometryTex.SetPixels32(structures);
            geometryTex.Apply(false,false);
            try
            {
                File.WriteAllBytes(folder+"/actual_emergent_generation.png",
                    tex.EncodeToPNG());
                File.WriteAllBytes(folder+"/actual_emergent_structures.png",
                    geometryTex.EncodeToPNG());
                File.WriteAllBytes(folder+"/actual_expression_alignment.png",
                    alignmentTex.EncodeToPNG());
                File.WriteAllText(folder+"/population.csv",csv.ToString());
                File.WriteAllText(folder+"/champions.csv",champions.ToString());
                File.WriteAllText(folder+"/branch_expression.csv",
                    branches.ToString());
                File.WriteAllText(folder+"/causal_branch_ablation.csv",
                    causalCsv.ToString());
                File.WriteAllText(folder+"/provenance.txt",
                    "Native Unity C# developmental tissue and recurrent GRN.\n"+
                    "Seed="+result.seed+" generations="+result.generations+
                    " population="+result.populationSize+"; no species templates.\n"+
                    "White=organizer and tissue overlap; magenta=unguided "+
                    "tissue; cyan=unexpressed growth guidance. "+
                    "The presentation-fitted structural PNG is NOT "+
                    "in the same coordinate frame as the tissue.\n"+
                    "Quality-diversity parent selection retains the "+
                    "highest raw fitness champion.\n"+
                    "Experimental survival-weighted selection; NOT emergent "+
                    "locomotion, reproduction in an ecosystem or a complete organism.\n");
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(tex);
                UnityEngine.Object.DestroyImmediate(geometryTex);
                UnityEngine.Object.DestroyImmediate(alignmentTex);
            }
            Debug.Log("Emergent generation audit exported: "+folder);
        }

        static string N(float value)
        {
            return value.ToString("F6",
                System.Globalization.CultureInfo.InvariantCulture);
        }
    }
}
#endif
