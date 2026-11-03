using System;
using UnityEngine;

namespace EarthRogueOrigins.PixelCreatures
{
    /// <summary>
    /// Original procedural pixel artist: no source-image extraction, image models,
    /// rescaling of existing sprites, or Unity global randomness.
    /// 128x96 RGBA, exactly one logical pixel per texture pixel.
    /// </summary>
    public static class PixelCreaturePainter
    {
        public const int Width=128,Height=96;
        static readonly Color32 Clear=new Color32(0,0,0,0);
        static readonly Color32 Ink=new Color32(26,29,25,255);
        static readonly Color32 EyeWhite=new Color32(237,224,184,255);
        static readonly Color32 Black=new Color32(15,19,17,255);

        public static Texture2D Render(PixelCreatureRecipe recipe)
        {
            recipe.Validate();
            Color32[] pixels=RenderPixels(recipe);
            var texture=new Texture2D(Width,Height,TextureFormat.RGBA32,false);
            texture.filterMode=FilterMode.Point;
            texture.wrapMode=TextureWrapMode.Clamp;
            texture.SetPixels32(pixels);
            texture.Apply(false,false);
            return texture;
        }

        public static Color32[] RenderPixels(PixelCreatureRecipe recipe)
        {
            recipe.Validate();
            var c=new Canvas();
            var parts=PixelCreatureDevelopment.Layout(recipe);
            Array.Sort(parts,(a,b)=>a.drawOrder.CompareTo(b.drawOrder));
            foreach(var part in parts)
                DrawPart(c,part,recipe);
            return c.pixels;
        }

        static void DrawPart(Canvas c,PixelPartDescriptor d,PixelCreatureRecipe r)
        {
            int x=d.anchor.x,y=d.anchor.y,w=d.extent.x,h=d.extent.y;
            var dark=Shade(r.baseColor,.64f);
            var light=Shade(r.baseColor,1.24f);
            switch(d.slot)
            {
                case PartSlot.Trunk:
                    Ellipse(c,x,y,w/2+2,h/2+2,Ink);
                    Ellipse(c,x,y,w/2,h/2,dark);
                    Ellipse(c,x,y+2,w/2-2,Mathf.Max(2,h/2-3),r.baseColor);
                    Ellipse(c,x-5,y+Mathf.Max(2,h/5),Mathf.Max(3,w/3),
                        Mathf.Max(2,h/6),light);
                    if(d.style==PartStyle.Segmented)
                    {
                        int segments=7+Mathf.RoundToInt(r.armor*5);
                        for(int i=1;i<segments;i++)
                        {
                            int sx=x-w/2+Mathf.RoundToInt(i*w/(float)segments);
                            int sy=y;
                            int height=Mathf.RoundToInt(h*.35f+
                                Mathf.Sin(Mathf.PI*i/segments)*h*.23f);
                            Line(c,sx,sy-height,sx+1,sy+height,Ink,1);
                            Line(c,sx+2,sy-height+3,sx+2,sy-2,
                                Shade(r.accentColor,.9f),1);
                        }
                    }
                    break;

                case PartSlot.Head:
                    Ellipse(c,x,y,w/2+2,h/2+2,Ink);
                    Ellipse(c,x,y,w/2,h/2,r.baseColor);
                    Ellipse(c,x-2,y+3,Mathf.Max(3,w/3),Mathf.Max(2,h/5),light);
                    // Small snout and jaw keep the face readable at 32 px.
                    if(r.plan!=CreaturePlan.Arthropod)
                    {
                        Ellipse(c,x-w/3,y-1,Mathf.Max(4,w/3),Mathf.Max(3,h/4),r.baseColor);
                        Line(c,x-w/2-4,y-4,x-w/3,y-4,Ink,2);
                        Line(c,x-w/2-3,y-6,x-w/3+2,y-6,EyeWhite,1);
                    }
                    break;

                case PartSlot.Eye:
                    Ellipse(c,x,y,Mathf.Max(2,w/2+1),Mathf.Max(2,h/2+1),Ink);
                    Dot(c,x,y,Mathf.Max(1,Mathf.RoundToInt(r.eyeSize*2)),Black);
                    Dot(c,x-1,y+1,1,EyeWhite);
                    break;

                case PartSlot.ForeLeg:
                case PartSlot.HindLeg:
                    if(r.plan==CreaturePlan.Arthropod)
                    {
                        for(int i=0;i<3;i++)
                        {
                            int shift=i*8-8;
                            int knee=x+shift+(d.slot==PartSlot.HindLeg?3:-3);
                            int foot=knee+(i%2==0?7:-8);
                            Line(c,x+shift,y,knee,y-h/2,Ink,4);
                            Line(c,knee,y-h/2,foot,y-h,Ink,3);
                            Line(c,x+shift,y+1,knee,y-h/2+1,r.secondaryColor,2);
                        }
                    }
                    else
                    {
                        Line(c,x,y,x+2,y-h+3,Ink,Mathf.Max(5,w/2));
                        Line(c,x,y,x+2,y-h+3,r.baseColor,Mathf.Max(3,w/2-2));
                        Line(c,x+2,y-h+3,x-4,y-h,Ink,4);
                        Line(c,x+2,y-h+4,x-3,y-h+1,light,2);
                    }
                    break;

                case PartSlot.Tail:
                    if(r.plan==CreaturePlan.Fish)
                    {
                        Polygon(c,new[]{
                            new Vector2Int(x-w/2-2,y+3),new Vector2Int(x+w/2,y+7),
                            new Vector2Int(x+w/2,y-7),new Vector2Int(x-w/2-2,y-3)
                        },Ink);
                        Polygon(c,new[]{
                            new Vector2Int(x-w/2,y+2),new Vector2Int(x+w/2-2,y+5),
                            new Vector2Int(x+w/2-2,y-5),new Vector2Int(x-w/2,y-2)
                        },r.secondaryColor);
                    }
                    else
                    {
                        Polygon(c,new[]{
                            new Vector2Int(x-w/2-5,y+7),new Vector2Int(x+w/2,y+9),
                            new Vector2Int(x+w/2+2,y-3),new Vector2Int(x-w/2+5,y-5)
                        },Ink);
                        Polygon(c,new[]{
                            new Vector2Int(x-w/2-3,y+5),new Vector2Int(x+w/2-2,y+7),
                            new Vector2Int(x+w/2,y-1),new Vector2Int(x-w/2+5,y-3)
                        },r.baseColor);
                    }
                    break;

                case PartSlot.Fin:
                    Polygon(c,new[]{
                        new Vector2Int(x-w/2,y),new Vector2Int(x+w/2,y+h/2),
                        new Vector2Int(x+w/2-3,y),
                        new Vector2Int(x+w/2,y-h/2)
                    },Ink);
                    Polygon(c,new[]{
                        new Vector2Int(x-w/2+3,y),new Vector2Int(x+w/2-3,y+h/2-3),
                        new Vector2Int(x+w/2-5,y),
                        new Vector2Int(x+w/2-3,y-h/2+3)
                    },r.accentColor);
                    break;

                case PartSlot.Armor:
                    int n=Mathf.RoundToInt(r.armor*(r.plan==CreaturePlan.Arthropod?11:8));
                    if(r.plan==CreaturePlan.Arthropod)n=Mathf.Max(6,n);
                    for(int i=0;i<n;i++)
                    {
                        int ax=x-w/2+Mathf.RoundToInt((i+.5f)*w/Mathf.Max(1,n));
                        float t=(ax-(x-w/2))/(float)Mathf.Max(1,w);
                        int curve=Mathf.RoundToInt(Mathf.Sin(t*Mathf.PI)*h*.16f);
                        int ay=y+curve;
                        Polygon(c,new[]{
                            new Vector2Int(ax-4,ay),new Vector2Int(ax-1,ay+4+Mathf.RoundToInt(r.armor*5)),
                            new Vector2Int(ax+4,ay)
                        },Ink);
                        Polygon(c,new[]{
                            new Vector2Int(ax-2,ay+1),new Vector2Int(ax-1,ay+3+Mathf.RoundToInt(r.armor*4)),
                            new Vector2Int(ax+2,ay+1)
                        },r.accentColor);
                    }
                    break;

                case PartSlot.Markings:
                    // Draw within a well-defined elliptical skin mask.
                    var rng=new System.Random(unchecked(r.seed*7919+23));
                    int count=Mathf.RoundToInt(r.spots*28);
                    for(int i=0;i<count;i++)
                    {
                        int px=x-w/2+4+rng.Next(Mathf.Max(1,w-8));
                        int py=y-h/2+3+rng.Next(Mathf.Max(1,h-6));
                        float dx=(px-x)/(float)Mathf.Max(1,w/2-3);
                        float dy=(py-y)/(float)Mathf.Max(1,h/2-3);
                        if(dx*dx+dy*dy>=.84f)continue;
                        int radius=1+rng.Next(3);
                        Ellipse(c,px,py,radius,radius,r.secondaryColor);
                        if(radius>=2)Dot(c,px-1,py+1,1,Shade(r.secondaryColor,1.24f));
                    }
                    break;
            }
        }

        static Color32 Shade(Color32 c,float mul)
        {
            return new Color32((byte)Mathf.Clamp(c.r*mul,0,255),
                (byte)Mathf.Clamp(c.g*mul,0,255),(byte)Mathf.Clamp(c.b*mul,0,255),255);
        }

        sealed class Canvas
        {
            public readonly Color32[] pixels=new Color32[Width*Height];
            public Canvas(){for(int i=0;i<pixels.Length;i++)pixels[i]=Clear;}
            public void Put(int x,int y,Color32 c)
            {
                if(x>=0&&x<Width&&y>=0&&y<Height)pixels[y*Width+x]=c;
            }
        }
        static void Dot(Canvas c,int x,int y,int r,Color32 color){Ellipse(c,x,y,r,r,color);}
        static void Ellipse(Canvas c,int cx,int cy,int rx,int ry,Color32 color)
        {
            rx=Mathf.Max(1,rx);ry=Mathf.Max(1,ry);
            for(int y=-ry;y<=ry;y++)
            for(int x=-rx;x<=rx;x++)
                if(x*x/(float)(rx*rx)+y*y/(float)(ry*ry)<=1f)
                    c.Put(cx+x,cy+y,color);
        }
        static void Line(Canvas c,int x0,int y0,int x1,int y1,Color32 color,int thickness)
        {
            int n=Mathf.Max(1,Mathf.Max(Mathf.Abs(x1-x0),Mathf.Abs(y1-y0)));
            for(int i=0;i<=n;i++)
            {
                float t=i/(float)n;
                Ellipse(c,Mathf.RoundToInt(Mathf.Lerp(x0,x1,t)),
                    Mathf.RoundToInt(Mathf.Lerp(y0,y1,t)),
                    Mathf.Max(1,thickness/2),Mathf.Max(1,thickness/2),color);
            }
        }
        static void Polygon(Canvas c,Vector2Int[] points,Color32 color)
        {
            if(points.Length<3)return;
            for(int i=1;i<points.Length-1;i++)
                Triangle(c,points[0],points[i],points[i+1],color);
        }
        static void Triangle(Canvas c,Vector2Int a,Vector2Int b,Vector2Int d,Color32 color)
        {
            int minX=Mathf.Max(0,Mathf.Min(a.x,Mathf.Min(b.x,d.x)));
            int maxX=Mathf.Min(Width-1,Mathf.Max(a.x,Mathf.Max(b.x,d.x)));
            int minY=Mathf.Max(0,Mathf.Min(a.y,Mathf.Min(b.y,d.y)));
            int maxY=Mathf.Min(Height-1,Mathf.Max(a.y,Mathf.Max(b.y,d.y)));
            float area=Edge(a,b,d);
            if(Mathf.Abs(area)<.01f)return;
            for(int y=minY;y<=maxY;y++)
            for(int x=minX;x<=maxX;x++)
            {
                var p=new Vector2Int(x,y);
                float u=Edge(b,d,p)/area,v=Edge(d,a,p)/area,w=1-u-v;
                if(u>=0&&v>=0&&w>=0)c.Put(x,y,color);
            }
        }
        static float Edge(Vector2Int a,Vector2Int b,Vector2Int p)
        {
            return (p.x-a.x)*(float)(b.y-a.y)-(p.y-a.y)*(float)(b.x-a.x);
        }
    }
}
