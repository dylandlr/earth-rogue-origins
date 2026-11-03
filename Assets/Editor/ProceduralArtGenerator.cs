#if UNITY_EDITOR
using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace EarthRogueOrigins.Editor
{
    /// <summary>
    /// Builds editable source PNGs (not flattened UI mockups) with consistent
    /// dark contour, layered shade/highlight and warm, restrained colors.
    /// Intended for rapid variants/blockouts; hand-authored sprites replace
    /// these as the production atlas matures.
    /// </summary>
    public static class ProceduralArtGenerator
    {
        const string Root = "Assets/Art/Generated";
        const int CreatureWidth = 128;
        const int CreatureHeight = 96;
        static readonly Color32 Outline = C(34,35,30);
        static readonly Color32 Transparent = new Color32(0,0,0,0);

        [MenuItem("Earth: Rogue Origins/Art/Generate Creature Variations and Terrain")]
        public static void Generate()
        {
            Directory.CreateDirectory(Root+"/Creatures");
            Directory.CreateDirectory(Root+"/Terrain");
            var random = new System.Random(2046);
            for (int i=0;i<24;i++)
            {
                float armor=(i%6)/5f;
                float limbs=((i/6)%4)/3f;
                float aquatic=(i%3)/2f;
                var palette = i%4==0?C(150,162,114):i%4==1?C(176,126,101):
                    i%4==2?C(120,145,150):C(191,177,132);
                Texture2D sprite=GenerateCreature(palette,armor,limbs,aquatic,i);
                Write(sprite,Root+"/Creatures/tetrapod_"+i.ToString("00")+".png");
            }
            string[] biomes={"forest","mud","water","rock","floodplain"};
            for(int i=0;i<biomes.Length;i++)
            for(int variant=0;variant<8;variant++)
            {
                Texture2D tile=GenerateTile(i,new System.Random(8046+i*173+variant*13));
                Write(tile,Root+"/Terrain/"+biomes[i]+"_"+variant.ToString("00")+".png");
            }
            AssetDatabase.Refresh();
            string[] paths=AssetDatabase.FindAssets("t:Texture2D",new[]{Root});
            foreach(string guid in paths)
            {
                string path=AssetDatabase.GUIDToAssetPath(guid);
                var importer=AssetImporter.GetAtPath(path) as TextureImporter;
                if(importer==null)continue;
                importer.textureType=TextureImporterType.Sprite;
                importer.spritePixelsPerUnit=64;
                importer.filterMode=FilterMode.Point;
                importer.alphaIsTransparency=true;
                importer.mipmapEnabled=false;
                importer.SaveAndReimport();
            }
            AssetDatabase.SaveAssets();
            Debug.Log("Generated 24 organism morphs and 40 terrain tiles at "+Root);
        }

        public static Texture2D GenerateCreature(Color32 baseColor,float armor,float limbs,float aquatic,int seed)
        {
            var t=Blank(CreatureWidth,CreatureHeight);
            Color32 shadow=Shade(baseColor,.61f), highlight=Shade(baseColor,1.24f);
            int bodyY=53, bodyX=59;
            // Drop shadow grounds the organism in a 2.5D orthographic world.
            Oval(t,bodyX,22,39,6,C(16,18,15,76));
            // Tail shape and hind limbs; feet are separated by outline so that silhouettes read at zoom.
            if(aquatic>.6f)
            {
                Triangle(t,19,48,1,35,3,63,Outline);
                Triangle(t,20,48,5,39,5,58,shadow);
            }
            else
            {
                Stroke(t,31,49,8,40,15,Outline);
                Stroke(t,31,49,8,41,10,shadow);
            }
            int legs=limbs>.4f?4:2;
            for(int i=0;i<legs;i++)
            {
                int x=i%2==0?42:81,y=i<2?36:47;
                int reach=limbs>.6f?18:11;
                Stroke(t,x,y,x+(i%2==0?-8:7),y-reach,12,Outline);
                Stroke(t,x,y,x+(i%2==0?-8:7),y-reach,8,shadow);
                Oval(t,x+(i%2==0?-9:8),y-reach,7,3,Outline);
                Oval(t,x+(i%2==0?-9:8),y-reach+1,5,2,baseColor);
            }
            // Black outline, upper shadow, body fill, upper-left reflected light.
            Oval(t,62,50,40,22,Outline);
            Oval(t,63,50,37,19,shadow);
            Oval(t,60,55,34,15,baseColor);
            Oval(t,56,61,29,7,highlight);
            // Head, snout, eye and jaw, with a readable character silhouette.
            Oval(t,94,55,24,15,Outline);
            Oval(t,94,55,21,12,baseColor);
            Oval(t,106,52,16,7,baseColor);
            Stroke(t,96,45,119,44,2,Outline);
            Oval(t,98,61,3,3,Outline);
            Oval(t,99,62,1,1,C(255,252,215));
            // Inherited armor becomes visible overlapping plates, not simply a color multiplier.
            int plates=2+Mathf.RoundToInt(armor*7);
            for(int i=0;i<plates;i++)
            {
                int x=35+i*8;
                int rise=(int)(5*Mathf.Sin(i*.55f));
                Oval(t,x,66+rise,6+Mathf.RoundToInt(armor*2),5,Outline);
                Oval(t,x,67+rise,4+Mathf.RoundToInt(armor*2),3,
                    i%2==0?Shade(baseColor,.8f):highlight);
            }
            // Scattered pigmentation—deterministic per genotype/variant seed.
            var rng=new System.Random(seed+850);
            for(int i=0;i<10;i++)
            {
                int x=38+rng.Next(53),y=42+rng.Next(24);
                if(Mathf.Pow((x-62)/38f,2)+Mathf.Pow((y-50)/20f,2)<.8f)
                    Oval(t,x,y,rng.Next(2,5),rng.Next(2,4),Shade(baseColor,.69f));
            }
            t.Apply(false,false);
            return t;
        }

        static Texture2D GenerateTile(int biome,System.Random rng)
        {
            int size=96;
            var t=Blank(size,size);
            Color32 ground=biome==0?C(75,93,54):biome==1?C(105,79,67):
                biome==2?C(39,91,101):biome==3?C(89,82,78):C(105,111,66);
            Fill(t,ground);
            for(int i=0;i<1200;i++)
            {
                int x=rng.Next(size),y=rng.Next(size);
                Color32 variation=Shade(ground,(float)(.73+rng.NextDouble()*.53));
                Set(t,x,y,variation);
                if(rng.NextDouble()<.20)Set(t,x+1,y,variation);
            }
            for(int i=0;i<20;i++)
            {
                int x=rng.Next(size),y=rng.Next(size);
                if(biome==2)
                {
                    Stroke(t,x-8,y,x+12,y+1,1,C(80,137,141,125));
                    Oval(t,x,y,3,1,C(95,157,155,160));
                }
                else if(biome==0 || biome==4)
                {
                    Stroke(t,x,y,x,y+4,2,C(47,58,32));
                    Oval(t,x,y+6,4,3,C(110,138,59));
                    Oval(t,x-2,y+7,2,2,C(153,167,70));
                }
                else if(biome==3)
                {
                    Oval(t,x,y,rng.Next(2,7),rng.Next(2,4),C(57,59,55));
                    Oval(t,x+1,y+2,2,1,C(133,131,116));
                }
                else
                {
                    Oval(t,x,y,rng.Next(2,5),1,C(75,58,46));
                }
            }
            t.Apply(false,false);
            return t;
        }

        static Color32 C(byte r,byte g,byte b,byte a=255){return new Color32(r,g,b,a);}
        static Color32 Shade(Color32 c,float amount)
        {
            return C((byte)Mathf.Clamp(c.r*amount,0,255),(byte)Mathf.Clamp(c.g*amount,0,255),
                (byte)Mathf.Clamp(c.b*amount,0,255),c.a);
        }
        static Texture2D Blank(int w,int h)
        {
            var t=new Texture2D(w,h,TextureFormat.RGBA32,false);
            var a=new Color32[w*h];
            for(int i=0;i<a.Length;i++)a[i]=Transparent;
            t.SetPixels32(a);return t;
        }
        static void Fill(Texture2D t,Color32 c)
        {
            var data=new Color32[t.width*t.height];
            for(int i=0;i<data.Length;i++)data[i]=c;
            t.SetPixels32(data);
        }
        static void Set(Texture2D t,int x,int y,Color32 c)
        {
            if(x>=0 && y>=0 && x<t.width && y<t.height)t.SetPixel(x,y,c);
        }
        static void Oval(Texture2D t,int cx,int cy,int rx,int ry,Color32 col)
        {
            for(int y=-ry;y<=ry;y++)
            for(int x=-rx;x<=rx;x++)
                if((x*x)/(float)(rx*rx)+(y*y)/(float)(ry*ry)<=1f)
                    Set(t,cx+x,cy+y,col);
        }
        static void Stroke(Texture2D t,int x0,int y0,int x1,int y1,int thickness,Color32 c)
        {
            int steps=Mathf.Max(1,Mathf.Max(Mathf.Abs(x1-x0),Mathf.Abs(y1-y0)));
            for(int i=0;i<=steps;i++)
            {
                float a=i/(float)steps;
                Oval(t,Mathf.RoundToInt(Mathf.Lerp(x0,x1,a)),
                    Mathf.RoundToInt(Mathf.Lerp(y0,y1,a)),
                    Mathf.Max(1,thickness/2),Mathf.Max(1,thickness/2),c);
            }
        }
        static void Triangle(Texture2D t,int ax,int ay,int bx,int by,int cx,int cy,Color32 col)
        {
            int minX=Mathf.Min(ax,Mathf.Min(bx,cx)),maxX=Mathf.Max(ax,Mathf.Max(bx,cx));
            int minY=Mathf.Min(ay,Mathf.Min(by,cy)),maxY=Mathf.Max(ay,Mathf.Max(by,cy));
            float area=Edge(ax,ay,bx,by,cx,cy);
            if(Mathf.Abs(area)<1e-5f)return;
            for(int y=minY;y<=maxY;y++)for(int x=minX;x<=maxX;x++)
            {
                float a=Edge(bx,by,cx,cy,x,y)/area;
                float b=Edge(cx,cy,ax,ay,x,y)/area;
                float c=1-a-b;
                if(a>=0&&b>=0&&c>=0)Set(t,x,y,col);
            }
        }
        static float Edge(int ax,int ay,int bx,int by,int px,int py)
        {
            return (px-ax)*(float)(by-ay)-(py-ay)*(float)(bx-ax);
        }
        static void Write(Texture2D t,string path)
        {
            File.WriteAllBytes(path,t.EncodeToPNG());
            UnityEngine.Object.DestroyImmediate(t);
        }
    }
}
#endif
