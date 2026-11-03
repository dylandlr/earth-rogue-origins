using System;
using UnityEngine;

namespace EarthRogueOrigins.PixelCreatures
{
    /// <summary>
    /// Original authored detail placements for the 128x96 side-view pixel
    /// grammar. These are geometry plans, not clipped/borrowed source pixels.
    /// All coordinates track each part's bounds and obey deterministic seeds.
    /// </summary>
    public static class PixelDetailGrammar
    {
        public struct Detail
        {
            public int x,y,radius;
            public Color32 pigment;
            public Detail(int x,int y,int radius,Color32 pigment)
            {this.x=x;this.y=y;this.radius=radius;this.pigment=pigment;}
        }

        public static Detail[] DorsalMottle(PixelCreatureRecipe r,int cx,int cy,int w,int h)
        {
            r.Validate();
            int count=Mathf.RoundToInt(r.spots*23f);
            var result=new Detail[count];
            var rng=new System.Random(unchecked(r.seed*883+227));
            var ramp=PixelArtDirection.Ramp(r.palette);
            for(int i=0;i<count;i++)
            {
                // Deterministic rows of organic grouped spots; not white noise.
                float u=(i+.5f)/(float)Mathf.Max(1,count);
                float arc=Mathf.Sin(Mathf.PI*u);
                int x=cx-w/2+4+Mathf.RoundToInt(u*(w-8));
                int y=cy+Mathf.RoundToInt(h*(.10f+.21f*arc))+
                    rng.Next(-3,4);
                result[i]=new Detail(x,y,1+(i%5==0?1:0),
                    i%5==0?ramp.shadow:ramp.pattern);
            }
            return result;
        }

        public static Vector2Int[] FootToes(int rootX,int groundY,PartStyle style)
        {
            int spread=style==PartStyle.Webbed?8:style==PartStyle.Robust?7:5;
            return new[]{
                new Vector2Int(rootX-spread,groundY),
                new Vector2Int(rootX-spread/2,groundY-2),
                new Vector2Int(rootX+1,groundY-2),
                new Vector2Int(rootX+spread-1,groundY)
            };
        }
    }
}
