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
                "generation,id,parent_id,seed,genes,segments,occupied,scarce_survival,moderate_survival,fitness,diversity\n");
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
                    .Append(N(animal.diversity)).AppendLine();
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
            var background=new Color32(31,38,36,255);
            for(int i=0;i<board.Length;i++)
            {
                board[i]=background;
                structures[i]=background;
            }
            int final=result.generations-1;
            int slot=0;
            foreach(var animal in result.evaluated)
            {
                if(animal.generation!=final)continue;
                var pixels=OpenMorphTissue.Render(animal.tissue);
                var geometry=OpenMorphRasterizer.Render(animal.genome);
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
                    if(pixel.a==0)continue;
                    // Nearest-neighbor doubling, centered on a native
                    // 128x96 tile (tissue field 48x36 => 96x72).
                    int startX=col*tileW+16+x*2;
                    int startY=row*tileH+12+y*2;
                    for(int yy=0;yy<2;yy++)
                    for(int xx=0;xx<2;xx++)
                        board[(startY+yy)*tileW*4+startX+xx]=pixel;
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
                File.WriteAllText(folder+"/population.csv",csv.ToString());
                File.WriteAllText(folder+"/champions.csv",champions.ToString());
                File.WriteAllText(folder+"/provenance.txt",
                    "Native Unity C# developmental tissue and recurrent GRN.\n"+
                    "Seed="+result.seed+" generations="+result.generations+
                    " population="+result.populationSize+"; no species templates.\n"+
                    "Experimental survival-weighted selection; NOT emergent "+
                    "locomotion, reproduction in an ecosystem or a complete organism.\n");
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(tex);
                UnityEngine.Object.DestroyImmediate(geometryTex);
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
