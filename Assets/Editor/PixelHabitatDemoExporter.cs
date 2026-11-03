#if UNITY_EDITOR
using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using EarthRogueOrigins.PixelCreatures;

namespace EarthRogueOrigins.Editor
{
    /// <summary>
    /// Deterministic offline gameplay-style composition using the exact C#
    /// animal painter. Not a screenshot from a running Unity scene.
    /// Renders creature pixels onto authored swamp-color ground with reeds,
    /// water and stones for legibility evaluation at realistic scale.
    /// </summary>
    public static class PixelHabitatDemoExporter
    {
        const int W=640,H=360;
        const string Folder="ValidationResults/PixelVisualAudit";
        static readonly Color32 Water=new Color32(47,104,108,255);
        static readonly Color32 Ground=new Color32(105,98,69,255);
        static readonly Color32 Fern=new Color32(113,143,63,255);
        static readonly Color32 Ink=new Color32(33,39,30,255);

        [MenuItem("Earth: Rogue Origins/Art/Pixel Creatures/Export Habitat Demo")]
        public static void Export()
        {
            Directory.CreateDirectory(Folder);
            var pixels=new Color32[W*H];
            var rng=new System.Random(2046);
            for(int y=0;y<H;y++)for(int x=0;x<W;x++)
            {
                float noise=((float)rng.NextDouble()-.5f)*16;
                bool wet=(x<205 && y<190+(int)(Mathf.Sin(x*.034f)*13)) ||
                    (x>483 && y<116+(int)(Mathf.Sin(x*.067f)*8));
                var baseColor=wet?Water:Ground;
                pixels[y*W+x]=new Color32(
                    (byte)Mathf.Clamp(baseColor.r+noise,0,255),
                    (byte)Mathf.Clamp(baseColor.g+noise,0,255),
                    (byte)Mathf.Clamp(baseColor.b+noise,0,255),255);
            }
            // Sparse shoreline highlights and pebble clusters: deterministic,
            // intentionally secondary to the anatomy readability test.
            for(int i=0;i<65;i++)
            {
                int x=rng.Next(W),y=rng.Next(H);
                int size=2+rng.Next(5);
                Oval(pixels,x,y,size,Mathf.Max(1,size/2),
                    new Color32(155,139,99,255));
                Oval(pixels,x-1,y+1,Mathf.Max(1,size-2),1,
                    new Color32(193,168,115,255));
            }
            for(int i=0;i<90;i++)
            {
                int x=rng.Next(W),y=rng.Next(H);
                if(y<155)continue;
                int height=10+rng.Next(20);
                Line(pixels,x,y,x+rng.Next(-6,7),y+height,Ink,2);
                Line(pixels,x,y,x-9,y+height/2,Fern,3);
                Line(pixels,x,y,x+10,y+height/2,Fern,3);
            }
            var families=new[]{
                CreatureArchetype.MarshCrawler,CreatureArchetype.ClayRunner,
                CreatureArchetype.SlateNewt,CreatureArchetype.ArmoredCrawler
            };
            int[] positionsX={142,325,450,548};
            int[] positionsY={152,184,95,235};
            for(int i=0;i<families.Length;i++)
            {
                var recipe=PixelArtDirection.Hero(families[i],2046+i*41);
                var shadow=PixelCreaturePainter.RenderGroundShadowPixels(recipe);
                Paste(pixels,shadow,positionsX[i],positionsY[i],true);
                var creature=PixelCreaturePainter.RenderPixels(recipe,i%4,i==2);
                Paste(pixels,creature,positionsX[i],positionsY[i],false);
            }
            var tex=new Texture2D(W,H,TextureFormat.RGBA32,false);
            tex.filterMode=FilterMode.Point;
            tex.SetPixels32(pixels);
            tex.Apply(false,false);
            try{File.WriteAllBytes(Folder+"/actual_csharp_habitat_demo.png",tex.EncodeToPNG());}
            finally{UnityEngine.Object.DestroyImmediate(tex);}
            Debug.Log("Gameplay-style C# pixel demo exported to "+Folder);
        }

        static void Paste(Color32[] target,Color32[] sprite,int centerX,int centerY,bool alpha)
        {
            const int sw=PixelCreaturePainter.Width,sh=PixelCreaturePainter.Height;
            for(int y=0;y<sh;y++)for(int x=0;x<sw;x++)
            {
                var src=sprite[y*sw+x];
                if(src.a==0)continue;
                int xx=centerX+x-sw/2,yy=centerY+y-sh/2;
                if(xx<0||xx>=W||yy<0||yy>=H)continue;
                int index=yy*W+xx;
                if(!alpha){target[index]=src;continue;}
                float opacity=src.a/255f;
                var bg=target[index];
                target[index]=new Color32(
                    (byte)Mathf.Lerp(bg.r,src.r,opacity),
                    (byte)Mathf.Lerp(bg.g,src.g,opacity),
                    (byte)Mathf.Lerp(bg.b,src.b,opacity),255);
            }
        }

        static void Plot(Color32[] p,int x,int y,Color32 color)
        {
            if(x>=0&&x<W&&y>=0&&y<H)p[y*W+x]=color;
        }
        static void Oval(Color32[] p,int cx,int cy,int rx,int ry,Color32 color)
        {
            for(int y=-ry;y<=ry;y++)for(int x=-rx;x<=rx;x++)
                if(x*x/(float)Mathf.Max(1,rx*rx)+y*y/(float)Mathf.Max(1,ry*ry)<=1)
                    Plot(p,cx+x,cy+y,color);
        }
        static void Line(Color32[] p,int x0,int y0,int x1,int y1,Color32 color,int radius)
        {
            int n=Mathf.Max(1,Mathf.Max(Mathf.Abs(x1-x0),Mathf.Abs(y1-y0)));
            for(int i=0;i<=n;i++)
            {
                float t=i/(float)n;
                Oval(p,Mathf.RoundToInt(Mathf.Lerp(x0,x1,t)),
                    Mathf.RoundToInt(Mathf.Lerp(y0,y1,t)),radius,radius,color);
            }
        }
    }
}
#endif
