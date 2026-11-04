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
            var background=new Color32(31,38,36,255);
            for(int i=0;i<board.Length;i++)board[i]=background;
            var csv=new StringBuilder(
                "index,seed,genes,segments,steps,occupied,structural,contractile,sensory,transport,storage\n");

            for(int i=0;i<8;i++)
            {
                int seed=9100+i*71;
                var genome=OpenMorphGenome.Founder(seed);
                for(int m=0;m<5+i*3;m++)
                    genome=genome.Mutate(seed*53+m*1013);
                var field=OpenMorphTissue.Grow(genome,w,h,72);
                var pixels=OpenMorphTissue.Render(field);
                int col=i%4,row=1-i/4;
                for(int y=0;y<h;y++)
                for(int x=0;x<w;x++)
                {
                    var pixel=pixels[y*w+x];
                    if(pixel.a!=0)
                        board[(row*h+y)*(w*4)+col*w+x]=pixel;
                }
                csv.Append(i).Append(',').Append(seed).Append(',')
                    .Append(genome.genes.Count).Append(',')
                    .Append(OpenMorphDevelopment.Grow(genome).Length).Append(',')
                    .Append(field.steps).Append(',')
                    .Append(field.Occupied()).Append(',')
                    .Append(field.FateCount(OpenMorphTissue.Fate.Structural)).Append(',')
                    .Append(field.FateCount(OpenMorphTissue.Fate.Contractile)).Append(',')
                    .Append(field.FateCount(OpenMorphTissue.Fate.Sensory)).Append(',')
                    .Append(field.FateCount(OpenMorphTissue.Fate.Transport)).Append(',')
                    .Append(field.FateCount(OpenMorphTissue.Fate.Storage)).AppendLine();
            }
            var tex=new Texture2D(w*4,h*2,TextureFormat.RGBA32,false);
            tex.filterMode=FilterMode.Point;
            tex.SetPixels32(board);
            tex.Apply(false,false);
            try
            {
                File.WriteAllBytes(folder+"/actual_tissue_growth.png",
                    tex.EncodeToPNG());
                File.WriteAllText(folder+"/tissue_metrics.csv",csv.ToString());
                File.WriteAllText(folder+"/provenance.txt",
                    "Unity C# OpenMorphGenome -> OpenMorphTissue.Grow -> Render\n"+
                    "Diagnostic 2D tissue growth with inherited branching-guidance field.\n"+
                    "NOT biological tissues, organs, viability or an evolved ecosystem.\n");
            }
            finally {UnityEngine.Object.DestroyImmediate(tex);}
            Debug.Log("Open tissue growth audit exported: "+folder);
        }
    }
}
#endif
