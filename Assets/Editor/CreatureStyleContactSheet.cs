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
    /// Real, authoritative source of full-character previews.
    /// Calls the same PixelCreaturePainter as live ecosystem play.
    /// </summary>
    public static class CreatureStyleContactSheet
    {
        const string Folder="ValidationResults/CreatureStyle";
        static readonly CreatureArchetype[] Archetypes={
            CreatureArchetype.MarshCrawler,
            CreatureArchetype.ClayRunner,
            CreatureArchetype.SlateNewt,
            CreatureArchetype.PaleJuvenile,
            CreatureArchetype.ArmoredCrawler,
            CreatureArchetype.PaddleFish,
            CreatureArchetype.MarshFish,
            CreatureArchetype.SegmentedBurrower
        };

        [MenuItem("Earth: Rogue Origins/Art/Pixel Creatures/Export Actual Full Creature Art Review")]
        public static void Run()
        {
            Directory.CreateDirectory(Folder);
            int tileWidth=PixelCreaturePainter.Width;
            int tileHeight=PixelCreaturePainter.Height;
            int width=tileWidth*4,height=tileHeight*2;
            var sheet=new Color32[width*height];
            var backdrop=new Color32(43,55,42,255);
            for(int i=0;i<sheet.Length;i++)sheet[i]=backdrop;
            var report=new StringBuilder(
                "archetype,seed,visible_pixels,dominant_fraction,components,distinct_colors,min_x,min_y,max_x,max_y,aspect_ratio\n");
            int problems=0;
            var failures=new StringBuilder();
            foreach(var archetype in Archetypes)
            {
                int index=Array.IndexOf(Archetypes,archetype);
                var recipe=PixelArtDirection.Hero(archetype,2046);
                var pixels=PixelCreaturePainter.RenderPixels(recipe);
                var metrics=PixelCreatureVisualMetrics.Analyze(pixels);
                report.Append(archetype).Append(',').Append(recipe.seed)
                    .Append(',').Append(metrics.nonTransparentPixels)
                    .Append(',').Append(metrics.dominantFraction.ToString("F4",
                        System.Globalization.CultureInfo.InvariantCulture))
                    .Append(',').Append(metrics.components)
                    .Append(',').Append(metrics.distinctOpaqueColors)
                    .Append(',').Append(metrics.minX).Append(',').Append(metrics.minY)
                    .Append(',').Append(metrics.maxX).Append(',').Append(metrics.maxY)
                    .Append(',').Append(metrics.horizontalToVerticalRatio.ToString("F3",
                        System.Globalization.CultureInfo.InvariantCulture)).AppendLine();

                var reasons=new StringBuilder();
                if(metrics.nonTransparentPixels<150)
                    reasons.Append("near-blank sprite; ");
                if(metrics.dominantFraction<.80f)
                    reasons.Append("disconnected silhouette (largest component "+
                        metrics.dominantFraction.ToString("P1",
                            System.Globalization.CultureInfo.InvariantCulture)+"); ");
                if(metrics.minX<=0 || metrics.maxX>=tileWidth-1 ||
                   metrics.minY<=0 || metrics.maxY>=tileHeight-1)
                    reasons.Append("sprite touches image boundary; ");
                if(reasons.Length>0)
                {
                    problems++;
                    failures.Append(archetype).Append(": ")
                        .Append(reasons).AppendLine();
                }
                int column=index%4,row=1-index/4;
                for(int y=0;y<tileHeight;y++)
                for(int x=0;x<tileWidth;x++)
                {
                    Color32 pixel=pixels[y*tileWidth+x];
                    if(pixel.a!=0)
                        sheet[(row*tileHeight+y)*width+column*tileWidth+x]=pixel;
                }
                // Export individual transparent sprites without changing
                // their generated pixels or applying artistic enhancement.
                var sprite=PixelCreaturePainter.TextureFromPixels(pixels);
                try
                {
                    File.WriteAllBytes(Folder+"/"+archetype+".png",sprite.EncodeToPNG());
                }
                finally{UnityEngine.Object.DestroyImmediate(sprite);}
            }

            var texture=new Texture2D(width,height,TextureFormat.RGBA32,false);
            texture.filterMode=FilterMode.Point;
            texture.SetPixels32(sheet);texture.Apply(false,false);
            try
            {
                File.WriteAllBytes(Folder+"/actual_csharp_full_creatures.png",
                    texture.EncodeToPNG());
                File.WriteAllText(Folder+"/visual_metrics.csv",report.ToString());
                File.WriteAllText(Folder+"/qa_failures.txt",failures.ToString());
                File.WriteAllText(Folder+"/provenance.txt",
                    "Engine: Unity C# PixelCreaturePainter.RenderPixels\n"+
                    "Seed: 2046\nPixel format: 128x96 point-filtered RGBA32\n"+
                    "This is actual generator output, not AI concept art.\n");
            }
            finally{UnityEngine.Object.DestroyImmediate(texture);}
            if(problems>0)
                throw new InvalidOperationException(problems+
                    " creature(s) failed initial full-sprite QA:\n"+
                    failures.ToString()+"Images and metrics were exported to "+Folder);
            Debug.Log("Actual full creature art review exported: "+Folder);
        }
    }
}
#endif
