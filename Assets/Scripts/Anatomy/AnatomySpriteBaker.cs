using System;
using UnityEngine;

namespace EarthRogueOrigins.Anatomy
{
    /// <summary>
    /// Anatomy-anchored illustrated-style sprite rasterizer. Draws silhouettes first,
    /// then shadow/fill/highlight and plates on the curved dorsal surface.
    /// The CPU renderer is for preview and baking; production sprites can be hand-painted
    /// and skinned onto the same anchors.
    /// </summary>
    public static class AnatomySpriteBaker
    {
        const int W=384,H=224;
        const float Scale=88f;
        static readonly Color32 Ink=new Color32(33,35,29,255);
        static readonly Color32 Clear=new Color32(0,0,0,0);

        public static Texture2D Bake(AnatomicalPhenotype anatomy,int seed)
        {
            if(!Development.IsValid(anatomy))throw new ArgumentException("Invalid anatomy.");
            var texture=new Texture2D(W,H,TextureFormat.RGBA32,false);
            texture.filterMode=FilterMode.Point;
            var pixels=new Color32[W*H];
            for(int i=0;i<pixels.Length;i++)pixels[i]=Clear;
            var baseColor=Palette(anatomy.hue);
            var dark=Shade(baseColor,.66f);
            var light=Shade(baseColor,1.25f);
            var rng=new System.Random(seed);
            // Body local coordinates: x horizontal, y vertical; feet below the torso.
            var center=new Vector2(0,.14f);
            Ellipse(pixels,0,-.67f,.78f,.11f,new Color32(20,20,17,65));
            // Tail behind the hind leg, continuously attached to the trunk.
            Vector2 baseTail=anatomy.tailRoot+center;
            var tailTip=baseTail+new Vector2(-anatomy.tailLength,-.10f);
            Segment(pixels,baseTail,tailTip,.115f,Ink);
            Segment(pixels,baseTail,tailTip,.077f,dark);
            if(anatomy.tailFinSize>.035f)
            {
                float fin=anatomy.tailFinSize;
                Triangle(pixels,tailTip+Vector2.left*.10f,tailTip+new Vector2(.25f,fin),
                    tailTip+new Vector2(.23f,-fin),Ink);
                Triangle(pixels,tailTip+Vector2.left*.065f,tailTip+new Vector2(.20f,fin*.75f),
                    tailTip+new Vector2(.18f,-fin*.75f),new Color32(75,146,147,255));
            }
            // Four independently anchored limbs are drawn behind and then foreground.
            DrawLimbs(pixels,anatomy,center,true,dark,Ink);
            Ellipse(pixels,0,center.y,anatomy.trunkLength*.52f,anatomy.trunkDepth,Ink);
            Ellipse(pixels,-.012f,center.y+.015f,anatomy.trunkLength*.51f,anatomy.trunkDepth*.94f,dark);
            Ellipse(pixels,-.018f,center.y+.08f,anatomy.trunkLength*.48f,anatomy.trunkDepth*.74f,baseColor);
            Ellipse(pixels,-.09f,center.y+anatomy.trunkDepth*.37f,
                anatomy.trunkLength*.30f,anatomy.trunkDepth*.19f,light);
            // Pattern positions depend on genotype-defined density and a stable appearance seed.
            int spots=Mathf.RoundToInt(8+anatomy.patternDensity*24);
            for(int i=0;i<spots;i++)
            {
                float x=((float)rng.NextDouble()*1.62f-.81f)*anatomy.trunkLength*.50f;
                float y=((float)rng.NextDouble()*1.45f-.72f)*anatomy.trunkDepth;
                float norm=x*x/Mathf.Pow(anatomy.trunkLength*.50f,2)+
                           y*y/Mathf.Pow(anatomy.trunkDepth,2);
                if(norm<.76f)
                    Ellipse(pixels,x,center.y+y,.017f+(float)rng.NextDouble()*.037f,.018f,Shade(baseColor,.57f));
            }
            // Armor plates have paired occlusion and light edges; their roots lie on the actual back curve.
            int plates=Mathf.RoundToInt(anatomy.armorCoverage*9f);
            for(int i=0;i<plates;i++)
            {
                int index=Mathf.Clamp(Mathf.RoundToInt((i+1f)/(plates+1f)*16),1,15);
                var root=anatomy.dorsalSurface[index]+center;
                Vector2 tangent=(anatomy.dorsalSurface[index+1]-anatomy.dorsalSurface[index-1]).normalized;
                var normal=new Vector2(-tangent.y,tangent.x);
                Vector2 tip=root+normal*anatomy.armorHeight;
                Vector2 left=root-tangent*.045f;
                Vector2 right=root+tangent*.045f;
                Triangle(pixels,left,tip,right,Ink);
                Triangle(pixels,left+tangent*.009f,root+normal*(anatomy.armorHeight*.83f),
                    right-tangent*.009f,new Color32(170,156,119,255));
                Segment(pixels,root,tip,0.012f,light);
            }
            DrawLimbs(pixels,anatomy,center,false,baseColor,Ink);
            // Head overlaps and conceals the neck connection.
            var head=anatomy.head+center;
            Segment(pixels,new Vector2(anatomy.trunkLength*.40f,center.y),head,
                anatomy.headRadius*.71f,Ink);
            Segment(pixels,new Vector2(anatomy.trunkLength*.40f,center.y),head,
                anatomy.headRadius*.61f,baseColor);
            Ellipse(pixels,head.x,head.y,anatomy.headRadius*1.22f,anatomy.headRadius,Ink);
            Ellipse(pixels,head.x+.012f,head.y+.015f,anatomy.headRadius*1.12f,anatomy.headRadius*.89f,baseColor);
            Ellipse(pixels,head.x-anatomy.headRadius*.24f,head.y+anatomy.headRadius*.39f,
                anatomy.headRadius*.47f,anatomy.headRadius*.18f,light);
            var eye=head+new Vector2(anatomy.headRadius*.32f,anatomy.headRadius*.21f);
            Ellipse(pixels,eye.x,eye.y,anatomy.eyeRadius*1.8f,anatomy.eyeRadius*1.7f,Ink);
            Ellipse(pixels,eye.x+.012f,eye.y+.014f,anatomy.eyeRadius*.60f,
                anatomy.eyeRadius*.56f,new Color32(245,233,191,255));
            Segment(pixels,head+new Vector2(anatomy.headRadius*.41f,-anatomy.headRadius*.45f),
                head+new Vector2(anatomy.headRadius*1.04f,-anatomy.headRadius*.34f),.012f,Ink);
            texture.SetPixels32(pixels);
            texture.Apply(false,false);
            return texture;
        }

        public static Sprite BakeSprite(AnatomicalPhenotype p,int seed)
        {
            var texture=Bake(p,seed);
            return Sprite.Create(texture,new Rect(0,0,W,H),new Vector2(.5f,.42f),Scale);
        }

        static void DrawLimbs(Color32[] pixels,AnatomicalPhenotype p,Vector2 center,bool rear,
            Color32 color,Color32 ink)
        {
            foreach(var anchor in new[]{p.rearLeg,p.frontLeg})
            {
                var start=anchor+center+new Vector2(rear ? .04f : -.025f,rear ? .07f : -.05f);
                var knee=start+new Vector2(anchor.x<0?-.10f:.10f,-p.limbLength*.60f);
                var foot=knee+new Vector2(anchor.x<0?-.07f:.08f,-p.limbLength*.40f);
                float radius=p.limbThickness*(rear ? .78f : 1f);
                Segment(pixels,start,knee,radius+.025f,ink);
                Segment(pixels,knee,foot,radius*.72f+.016f,ink);
                Segment(pixels,start,knee,radius,color);
                Segment(pixels,knee,foot,radius*.65f,color);
                if(p.footWebbing>.52f)
                {
                    Triangle(pixels,foot+Vector2.left*.12f,foot+new Vector2(.10f,.02f),
                        foot+new Vector2(.04f,.10f),ink);
                    Triangle(pixels,foot+Vector2.left*.075f,foot+new Vector2(.07f,.03f),
                        foot+new Vector2(.035f,.08f),new Color32(103,151,134,255));
                }
                else
                {
                    Segment(pixels,foot+Vector2.left*.04f,foot+Vector2.right*.10f,.027f,ink);
                }
            }
        }

        static Color32 Palette(float h)
        {
            var samples=new[]{
                new Color32(152,166,116,255),new Color32(176,118,92,255),
                new Color32(130,143,152,255),new Color32(199,183,144,255)};
            float coordinate=Mathf.Clamp01(h)*(samples.Length-1);
            int i=Mathf.Clamp(Mathf.FloorToInt(coordinate),0,samples.Length-2);
            return Color32.Lerp(samples[i],samples[i+1],coordinate-i);
        }
        static Color32 Shade(Color32 c,float k)
        {
            return new Color32((byte)Mathf.Clamp(c.r*k,0,255),
                (byte)Mathf.Clamp(c.g*k,0,255),(byte)Mathf.Clamp(c.b*k,0,255),c.a);
        }
        static int X(float x){return Mathf.RoundToInt(W*.53f+x*Scale);}
        static int Y(float y){return Mathf.RoundToInt(H*.52f+y*Scale);}
        static void Pixel(Color32[] a,int x,int y,Color32 color)
        {
            if(x>=0&&x<W&&y>=0&&y<H)a[y*W+x]=color;
        }
        static void Ellipse(Color32[] a,float x,float y,float rx,float ry,Color32 c)
        {
            int cx=X(x),cy=Y(y),sx=Mathf.Max(1,Mathf.RoundToInt(rx*Scale)),sy=Mathf.Max(1,Mathf.RoundToInt(ry*Scale));
            for(int j=-sy;j<=sy;j++)for(int i=-sx;i<=sx;i++)
                if((i*i)/(float)(sx*sx)+(j*j)/(float)(sy*sy)<=1f)Pixel(a,cx+i,cy+j,c);
        }
        static void Segment(Color32[] a,Vector2 from,Vector2 to,float radius,Color32 c)
        {
            int steps=Mathf.Max(1,Mathf.CeilToInt(Vector2.Distance(from,to)*Scale*.8f));
            for(int i=0;i<=steps;i++)
            {
                var p=Vector2.Lerp(from,to,i/(float)steps);
                Ellipse(a,p.x,p.y,Mathf.Max(.012f,radius),Mathf.Max(.012f,radius),c);
            }
        }
        static void Triangle(Color32[] a,Vector2 v1,Vector2 v2,Vector2 v3,Color32 c)
        {
            int minX=Mathf.Max(0,Mathf.Min(X(v1.x),Mathf.Min(X(v2.x),X(v3.x))));
            int maxX=Mathf.Min(W-1,Mathf.Max(X(v1.x),Mathf.Max(X(v2.x),X(v3.x))));
            int minY=Mathf.Max(0,Mathf.Min(Y(v1.y),Mathf.Min(Y(v2.y),Y(v3.y))));
            int maxY=Mathf.Min(H-1,Mathf.Max(Y(v1.y),Mathf.Max(Y(v2.y),Y(v3.y))));
            float area=Edge(v1,v2,v3);
            if(Mathf.Abs(area)<1e-6f)return;
            for(int y=minY;y<=maxY;y++)for(int x=minX;x<=maxX;x++)
            {
                var p=new Vector2((x-W*.44f)/Scale,(y-H*.52f)/Scale);
                float u=Edge(v2,v3,p)/area,v=Edge(v3,v1,p)/area,w=1-u-v;
                if(u>=0&&v>=0&&w>=0)Pixel(a,x,y,c);
            }
        }
        static float Edge(Vector2 a,Vector2 b,Vector2 c)
        {
            return (c.x-a.x)*(b.y-a.y)-(c.y-a.y)*(b.x-a.x);
        }
    }
}
