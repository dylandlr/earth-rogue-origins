#if UNITY_EDITOR
using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace EarthRogueOrigins.Editor
{
    /// <summary>
    /// Generates transparent layered forest props whose silhouettes overlap
    /// terrain cells. Low-density prop placement is handled by LiveEcosystemPreview.
    /// </summary>
    public static class CarboniferousFoliagePainter
    {
        const string Folder="Assets/Art/CarboniferousFoliage";
        const int W=192,H=256;
        static readonly Color32 Ink=C(31,38,28);
        static readonly Color32 Shadow=C(46,69,37);
        static readonly Color32 Green=C(112,142,54);
        static readonly Color32 Gold=C(160,177,72);

        [MenuItem("Earth: Rogue Origins/Art/Paint Carboniferous Fern Props")]
        public static void Generate(){Generate(false);}

        public static void Generate(bool overwrite)
        {
            Directory.CreateDirectory(Folder);
            for(int i=0;i<8;i++)
            {
                string path=Folder+"/fern_"+i.ToString("00")+".png";
                if(!overwrite && File.Exists(path))continue;
                var tex=Paint(i);
                File.WriteAllBytes(path,tex.EncodeToPNG());
                UnityEngine.Object.DestroyImmediate(tex);
            }
            AssetDatabase.Refresh();
            for(int i=0;i<8;i++)
            {
                var importer=AssetImporter.GetAtPath(Folder+"/fern_"+i.ToString("00")+".png") as TextureImporter;
                if(importer==null)continue;
                importer.textureType=TextureImporterType.Sprite;
                importer.spritePixelsPerUnit=192;
                importer.filterMode=FilterMode.Bilinear;
                importer.alphaIsTransparency=true;
                importer.mipmapEnabled=false;
                importer.SaveAndReimport();
            }
        }

        static Texture2D Paint(int seed)
        {
            var rng=new System.Random(873+seed*175);
            var p=new Color32[W*H];
            int stem=seed%3==0?135:seed%3==1?97:77;
            // Cast shadow, woody stalk, dark crown, and layered upper leaflets.
            Oval(p,96,13,49,8,C(23,30,24,75));
            Stroke(p,94,17,96,stem,10,Ink);
            Stroke(p,94,19,96,stem,6,C(106,88,62));
            Stroke(p,91,29,93,stem-11,2,C(155,123,76));
            for(int a=0;a<7;a++)
            {
                float angle=(a+.25f)*Mathf.PI*2/7;
                int len=49+rng.Next(12,30);
                int ex=96+Mathf.RoundToInt(Mathf.Cos(angle)*len);
                int ey=stem+Mathf.RoundToInt(Mathf.Sin(angle)*len*.56f);
                Stroke(p,96,stem,ex,ey,7,Ink);
                Stroke(p,96,stem+1,ex,ey+2,3,Shadow);
                for(int k=1;k<=8;k++)
                {
                    float t=k/9f;
                    int x=Mathf.RoundToInt(Mathf.Lerp(96,ex,t));
                    int y=Mathf.RoundToInt(Mathf.Lerp(stem,ey,t));
                    int leaflet=8-Mathf.Abs(5-k);
                    Oval(p,x-leaflet/2,y+4,leaflet+4,5,Ink);
                    Oval(p,x-leaflet/2,y+5,leaflet+2,3,k%3==0?Gold:Green);
                    Oval(p,x+leaflet/2,y-3,leaflet+4,5,Ink);
                    Oval(p,x+leaflet/2,y-2,leaflet+2,3,k%2==0?Green:Shadow);
                }
            }
            Oval(p,96,stem,10,7,Ink);
            Oval(p,96,stem+2,8,5,Gold);
            var texture=new Texture2D(W,H,TextureFormat.RGBA32,false);
            texture.SetPixels32(p);texture.Apply(false,false);
            return texture;
        }

        static Color32 C(byte r,byte g,byte b,byte a=255){return new Color32(r,g,b,a);}
        static void Pixel(Color32[] p,int x,int y,Color32 color)
        {
            if(x>=0&&x<W&&y>=0&&y<H)p[y*W+x]=color;
        }
        static void Oval(Color32[] p,int cx,int cy,int rx,int ry,Color32 c)
        {
            for(int y=-ry;y<=ry;y++)for(int x=-rx;x<=rx;x++)
                if((x*x)/(float)(rx*rx)+(y*y)/(float)(ry*ry)<=1f)
                    Pixel(p,cx+x,cy+y,c);
        }
        static void Stroke(Color32[] p,int x0,int y0,int x1,int y1,int radius,Color32 c)
        {
            int n=Mathf.Max(1,Mathf.Max(Mathf.Abs(x1-x0),Mathf.Abs(y1-y0)));
            for(int i=0;i<=n;i++)
            {
                float t=i/(float)n;
                Oval(p,Mathf.RoundToInt(Mathf.Lerp(x0,x1,t)),
                    Mathf.RoundToInt(Mathf.Lerp(y0,y1,t)),Mathf.Max(1,radius),Mathf.Max(1,radius),c);
            }
        }
    }
}
#endif
