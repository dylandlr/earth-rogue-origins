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
    /// Exports actual Unity-generated imagery from variable-topology growth
    /// genomes. Independent of the eight legacy animal hero templates.
    /// </summary>
    public static class OpenMorphContactSheet
    {
        [MenuItem("Earth: Rogue Origins/Art/Pixel Creatures/Export Open Morphology Experiments")]
        public static void Run()
        {
            string folder="ValidationResults/OpenMorph/run-"+
                DateTime.UtcNow.ToString("yyyyMMdd-HHmmss-fff");
            Directory.CreateDirectory(folder);
            int w=OpenMorphRasterizer.Width,h=OpenMorphRasterizer.Height;
            var board=new Color32[w*4*h*2];
            var background=new Color32(37,48,39,255);
            for(int i=0;i<board.Length;i++)board[i]=background;
            var csv=new StringBuilder("index,seed,genes,developed_segments,opaque_pixels\n");

            for(int index=0;index<8;index++)
            {
                int seed=2400+index*113;
                var genome=OpenMorphGenome.Founder(seed);
                // A reproducible open-ended random walk through genome space.
                for(int step=0;step<12+index*4;step++)
                    genome=genome.Mutate(seed*101+step*7919);
                var phenotype=OpenMorphDevelopment.Grow(genome);
                var pixels=OpenMorphRasterizer.Render(genome);
                int visible=0;
                int col=index%4,row=1-index/4;
                for(int y=0;y<h;y++)
                for(int x=0;x<w;x++)
                {
                    var pixel=pixels[y*w+x];
                    if(pixel.a==0)continue;
                    visible++;
                    board[(row*h+y)*(w*4)+col*w+x]=pixel;
                }
                csv.Append(index).Append(',').Append(seed).Append(',')
                    .Append(genome.genes.Count).Append(',')
                    .Append(phenotype.Length).Append(',').Append(visible)
                    .AppendLine();
            }

            var texture=new Texture2D(w*4,h*2,TextureFormat.RGBA32,false);
            texture.filterMode=FilterMode.Point;
            texture.SetPixels32(board);
            texture.Apply(false,false);
            try
            {
                File.WriteAllBytes(folder+"/actual_open_morphologies.png",
                    texture.EncodeToPNG());
                File.WriteAllText(folder+"/morphology_metrics.csv",csv.ToString());
                File.WriteAllText(folder+"/provenance.txt",
                    "Unity C# OpenMorphGenome -> OpenMorphDevelopment -> "+
                    "OpenMorphRasterizer; not concept artwork.\n"+
                    "Experimental modular segment raster; not biological "+
                    "tissue simulation or production creature sprites.\n");
            }
            finally {UnityEngine.Object.DestroyImmediate(texture);}
            Debug.Log("Open morphology experiments exported: "+folder);
        }
    }
}
#endif
