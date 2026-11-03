#if UNITY_EDITOR
using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using EarthRogueOrigins.PixelCreatures;

namespace EarthRogueOrigins.Editor
{
    /// <summary>
    /// Exports a PNG of *actual C# PixelCreaturePainter output* alongside a
    /// four-frame walk strip. Never substitutes an image generator or concept
    /// image. Usable from Unity menu and -executeMethod batch mode.
    /// </summary>
    public static class PixelCreatureVisualAudit
    {
        const int Width=128,Height=96;
        const string Folder="ValidationResults/PixelVisualAudit";

        [MenuItem("Earth: Rogue Origins/Art/Pixel Creatures/Export Visual Progress Board")]
        public static void Export()
        {
            Directory.CreateDirectory(Folder);
            const int columns=5,rows=2;
            Color32[] canvas=new Color32[Width*columns*Height*rows];
            var background=new Color32(24,33,28,255);
            for(int i=0;i<canvas.Length;i++)canvas[i]=background;
            var families=new[]{
                CreatureArchetype.MarshCrawler,CreatureArchetype.ClayRunner,
                CreatureArchetype.SlateNewt,CreatureArchetype.PaleJuvenile,
                CreatureArchetype.ArmoredCrawler
            };
            int[] visiblePerHero=new int[families.Length];
            for(int i=0;i<families.Length;i++)
            {
                var recipe=PixelArtDirection.Hero(families[i],2046);
                var pixels=PixelCreaturePainter.RenderPixels(recipe);
                int count=0;
                foreach(var pixel in pixels)if(pixel.a>0)count++;
                visiblePerHero[i]=count;
                if(count<250)
                    throw new InvalidOperationException(
                        "Sprite is nearly blank: "+families[i]+" ("+count+" px)");
                Paste(canvas,pixels,columns*Width,i*Width,Height);
            }
            var crawler=PixelArtDirection.Hero(CreatureArchetype.MarshCrawler,2046);
            for(int frame=0;frame<PixelCreaturePoses.Frames;frame++)
                Paste(canvas,PixelCreaturePainter.RenderPixels(crawler,frame,false),
                    columns*Width,frame*Width,0);
            var texture=new Texture2D(Width*columns,Height*rows,TextureFormat.RGBA32,false);
            texture.filterMode=FilterMode.Point;
            texture.SetPixels32(canvas);
            texture.Apply(false,false);
            try
            {
                File.WriteAllBytes(Folder+"/actual_unity_csharp_creatures.png",
                    texture.EncodeToPNG());
            }
            finally {UnityEngine.Object.DestroyImmediate(texture);}
            var report=new System.Text.StringBuilder("archetype,visible_pixels\n");
            for(int i=0;i<families.Length;i++)
                report.Append(families[i]).Append(',').Append(visiblePerHero[i]).AppendLine();
            File.WriteAllText(Folder+"/visibility.csv",report.ToString());
            Debug.Log("Actual C# pixel creature audit saved: "+Folder);
        }

        static void Paste(Color32[] destination,Color32[] source,int sheetWidth,int x0,int y0)
        {
            if(source==null||source.Length!=Width*Height)
                throw new ArgumentException("Invalid source pixel array");
            for(int y=0;y<Height;y++)
            for(int x=0;x<Width;x++)
            {
                var pixel=source[y*Width+x];
                if(pixel.a==0)continue;
                destination[(y0+y)*sheetWidth+x0+x]=pixel;
            }
        }
    }
}
#endif
