#if UNITY_EDITOR
using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace EarthRogueOrigins.Editor
{
    /// <summary>
    /// Art-directed, seamless-at-the-edges tile generator for the Carboniferous swamp.
    /// Stable seeds + disciplined shared palette. Generated PNGs are starter art and
    /// never overwrite hand-painted replacements unless explicitly requested.
    /// </summary>
    public static class CarboniferousTerrainPainter
    {
        public const int Size=192;
        const string Root="Assets/Art/CarboniferousTerrain";
        static readonly string[] Names={"forest","mud","water","rock","floodplain"};
        static readonly Color32 Ink=C(31,37,30);
        static readonly Color32 Fern=C(135,162,71);
        static readonly Color32 DarkFern=C(66,102,51);
        static readonly Color32 PaleFern=C(180,187,93);

        [MenuItem("Earth: Rogue Origins/Art/Paint Carboniferous Terrain Tiles")]
        public static void Generate(){Generate(false);}

        [MenuItem("Earth: Rogue Origins/Art/Repaint Carboniferous Tiles (Overwrite)")]
        public static void GenerateOverwrite(){Generate(true);}

        public static void Generate(bool overwrite)
        {
            Directory.CreateDirectory(Root);
            foreach(string name in Names)Directory.CreateDirectory(Root+"/"+name);
            for(int biome=0;biome<Names.Length;biome++)
            for(int variant=0;variant<8;variant++)
            {
                string path=TilePath(biome,variant);
                if(!overwrite && File.Exists(path))continue;
                var texture=Paint(biome,variant,4061+biome*733+variant*109);
                File.WriteAllBytes(path,texture.EncodeToPNG());
                UnityEngine.Object.DestroyImmediate(texture);
            }
            AssetDatabase.Refresh();
            for(int biome=0;biome<Names.Length;biome++)
            for(int variant=0;variant<8;variant++)
            {
                var importer=AssetImporter.GetAtPath(TilePath(biome,variant)) as TextureImporter;
                if(importer==null)continue;
                importer.textureType=TextureImporterType.Sprite;
                importer.spritePixelsPerUnit=Size;
                importer.filterMode=FilterMode.Bilinear;
                importer.alphaIsTransparency=true;
                importer.mipmapEnabled=false;
                importer.SaveAndReimport();
            }
            AssetDatabase.SaveAssets();
            Debug.Log("Carboniferous terrain ready: 40 art-directed 192px tiles at "+Root);
        }

        public static string TilePath(int biome,int variant)
        {
            return Root+"/"+Names[Mathf.Clamp(biome,0,Names.Length-1)]+"/"+
                Names[Mathf.Clamp(biome,0,Names.Length-1)]+"_"+variant.ToString("00")+".png";
        }

        static Texture2D Paint(int biome,int variant,int seed)
        {
            var rng=new System.Random(seed);
            var px=new Color32[Size*Size];
            Color32 primary=biome==0?C(76,88,48):biome==1?C(117,91,76):
                biome==2?C(32,102,111):biome==3?C(87,74,69):C(115,103,75);
            Color32 secondary=biome==0?C(112,111,66):biome==1?C(141,110,89):
                biome==2?C(54,130,131):biome==3?C(122,108,95):C(146,132,94);
            for(int y=0;y<Size;y++)for(int x=0;x<Size;x++)
            {
                float broad=(Mathf.Sin((x+variant*39)*.053f)+Mathf.Cos((y-variant*17)*.061f))*.085f;
                float grain=((float)rng.NextDouble()-.5f)*.12f;
                float mix=Mathf.Clamp01(.43f+broad+grain);
                px[y*Size+x]=Lerp(primary,secondary,mix);
            }
            // Keep major motifs away from tile edges: adjoining tiles read as coherent ground.
            int pebbles=biome==3?130:biome==2?11:65;
            for(int i=0;i<pebbles;i++)
            {
                int x=rng.Next(12,Size-12),y=rng.Next(12,Size-12);
                int rx=rng.Next(2,biome==3?11:7),ry=Mathf.Max(1,rx/2);
                var shadow=biome==2?C(19,75,83,150):C(37,41,32,180);
                Oval(px,x+2,y-2,rx+2,ry+2,shadow);
                Oval(px,x,y,rx,ry,biome==3?C(143,131,109):
                    biome==2?C(81,155,152,130):C(146,130,95));
                if(i%3==0)Stroke(px,x-rx/2,y+ry/2,x,y+ry,1,C(209,185,131,160));
            }
            if(biome==2)
            {
                for(int i=0;i<65;i++)
                {
                    int x=rng.Next(8,Size-8),y=rng.Next(8,Size-8);
                    Stroke(px,x-rng.Next(4,13),y,x+rng.Next(5,15),y+1,1,C(112,176,165,120));
                }
                for(int i=0;i<7;i++)
                {
                    int x=rng.Next(24,Size-24),y=rng.Next(24,Size-24);
                    Oval(px,x+2,y-2,12,5,C(24,71,75,110));
                    Oval(px,x,y,11,5,C(96,140,52));
                    Stroke(px,x-5,y+2,x+3,y+3,1,C(159,173,80));
                }
            }
            else
            {
                int vegetation=biome==0?22:biome==4?15:biome==1?7:3;
                for(int i=0;i<vegetation;i++)
                {
                    int x=rng.Next(15,Size-15),y=rng.Next(17,Size-15);
                    FernCluster(px,x,y,rng,biome==0?1.0f:.72f);
                }
                // Bark fragments, spore moss and little fern seedlings give ecosystem scale.
                for(int i=0;i<(biome==0?180:80);i++)
                {
                    int x=rng.Next(8,Size-8),y=rng.Next(8,Size-8);
                    Dot(px,x,y,1+i%2,biome==3?C(71,77,52):C(91,116,50));
                }
            }
            var texture=new Texture2D(Size,Size,TextureFormat.RGBA32,false);
            texture.SetPixels32(px);texture.Apply(false,false);
            return texture;
        }

        static void FernCluster(Color32[] px,int x,int y,System.Random rng,float scale)
        {
            int arms=4+rng.Next(3), length=Mathf.RoundToInt((11+rng.Next(11))*scale);
            Oval(px,x+2,y-4,10,4,C(26,36,25,120));
            for(int a=0;a<arms;a++)
            {
                float angle=Mathf.PI*.25f+a*Mathf.PI*1.5f/Mathf.Max(1,arms-1);
                int ex=x+Mathf.RoundToInt(Mathf.Cos(angle)*length);
                int ey=y+Mathf.RoundToInt(Mathf.Sin(angle)*length*.65f);
                Stroke(px,x,y,ex,ey,2,Ink);
                Stroke(px,x,y+1,ex,ey+1,1,DarkFern);
                int pinnae=4;
                for(int k=1;k<=pinnae;k++)
                {
                    float t=k/(float)(pinnae+1);
                    int bx=Mathf.RoundToInt(Mathf.Lerp(x,ex,t)),by=Mathf.RoundToInt(Mathf.Lerp(y,ey,t));
                    int span=3+(pinnae-k);
                    Oval(px,bx-span/2,by+span/2,span,2,Fern);
                    Oval(px,bx+span/2,by-span/2,span,2,k%2==0?PaleFern:DarkFern);
                }
            }
            Oval(px,x,y,3,2,C(169,152,83));
        }

        static Color32 Lerp(Color32 a,Color32 b,float t)
        {
            return new Color32((byte)Mathf.Lerp(a.r,b.r,t),(byte)Mathf.Lerp(a.g,b.g,t),
                (byte)Mathf.Lerp(a.b,b.b,t),255);
        }
        static Color32 C(byte r,byte g,byte b,byte a=255){return new Color32(r,g,b,a);}
        static void Dot(Color32[] p,int x,int y,int radius,Color32 c){Oval(p,x,y,radius,radius,c);}
        static void Pixel(Color32[] px,int x,int y,Color32 c)
        {
            if(x<0 || x>=Size || y<0 || y>=Size)return;
            // Compositing transparent accents over opaque ground (not dropping alpha).
            int i=y*Size+x;
            float a=c.a/255f;
            px[i]=new Color32(
                (byte)Mathf.Lerp(px[i].r,c.r,a),
                (byte)Mathf.Lerp(px[i].g,c.g,a),
                (byte)Mathf.Lerp(px[i].b,c.b,a),255);
        }
        static void Oval(Color32[] px,int x,int y,int rx,int ry,Color32 c)
        {
            for(int oy=-ry;oy<=ry;oy++)for(int ox=-rx;ox<=rx;ox++)
                if(ox*ox/(float)(rx*rx)+oy*oy/(float)(ry*ry)<=1f)
                    Pixel(px,x+ox,y+oy,c);
        }
        static void Stroke(Color32[] px,int x0,int y0,int x1,int y1,int radius,Color32 c)
        {
            int steps=Mathf.Max(1,Mathf.Max(Mathf.Abs(x1-x0),Mathf.Abs(y1-y0)));
            for(int s=0;s<=steps;s++)
            {
                float t=s/(float)steps;
                Oval(px,Mathf.RoundToInt(Mathf.Lerp(x0,x1,t)),
                    Mathf.RoundToInt(Mathf.Lerp(y0,y1,t)),Mathf.Max(1,radius),Mathf.Max(1,radius),c);
            }
        }
    }
}
#endif
