using UnityEngine;

namespace EarthRogueOrigins.PixelCreatures
{
    /// <summary>
    /// Authoritative integer-grid silhouette stations for a planted, bent
    /// tetrapod leg. The shoulder stays fixed; the ankle follows gait phase.
    /// Foreground and background legs share joints but differ in visual depth.
    /// This is morphology drawing, not locomotion physics or IK.
    /// </summary>
    public static class PixelLimbSilhouette
    {
        public struct Shape
        {
            public Vector2Int root,knee,ankle;
            public int upperRadius,lowerRadius;
        }

        public static Shape Build(int x,int y,int extentHeight,
            PartStyle style,int distalStride,bool foreleg,bool farSide,
            int footLift=0)
        {
            int h=Mathf.Max(9,extentHeight);
            int upper=style==PartStyle.Robust?5:
                style==PartStyle.Slender?3:4;
            if(farSide)upper=Mathf.Max(2,upper-1);
            int lower=Mathf.Max(2,upper-1);
            int ankleX=x+distalStride+(foreleg?-2:2);
            int kneeX=Mathf.RoundToInt(Mathf.Lerp(x,ankleX,.46f))+
                (foreleg?2:-2);
            int lift=Mathf.Clamp(footLift,0,3);
            int kneeY=y-Mathf.RoundToInt(h*.55f)+lift/2;
            return new Shape {
                root=new Vector2Int(x,y),
                knee=new Vector2Int(kneeX,kneeY),
                ankle=new Vector2Int(ankleX,y-h+3+lift),
                upperRadius=upper,lowerRadius=lower
            };
        }

        public static Vector2Int[] UpperPolygon(Shape s)
        {
            // An asymmetric tapered shoulder, not a cylindrical line.
            return new[]{
                new Vector2Int(s.root.x-s.upperRadius,s.root.y+2),
                new Vector2Int(s.root.x+s.upperRadius,s.root.y+1),
                new Vector2Int(s.knee.x+s.lowerRadius,s.knee.y+1),
                new Vector2Int(s.knee.x+s.lowerRadius-1,s.knee.y-2),
                new Vector2Int(s.knee.x-s.lowerRadius,s.knee.y-2),
                new Vector2Int(s.root.x-s.upperRadius-1,s.root.y-2)
            };
        }

        public static Vector2Int[] LowerPolygon(Shape s)
        {
            return new[]{
                new Vector2Int(s.knee.x-s.lowerRadius,s.knee.y+1),
                new Vector2Int(s.knee.x+s.lowerRadius,s.knee.y),
                new Vector2Int(s.ankle.x+Mathf.Max(2,s.lowerRadius-1),s.ankle.y-1),
                new Vector2Int(s.ankle.x+2,s.ankle.y-3),
                new Vector2Int(s.ankle.x-3,s.ankle.y-3),
                new Vector2Int(s.knee.x-s.lowerRadius-1,s.knee.y-2)
            };
        }
    }
}
