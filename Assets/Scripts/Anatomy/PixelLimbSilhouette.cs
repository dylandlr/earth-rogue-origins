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
            int upper=style==PartStyle.Robust?6:
                style==PartStyle.Slender?3:5;
            if(farSide)upper=Mathf.Max(2,upper-1);
            int lower=Mathf.Max(2,upper-1);
            // In the approved amphibian reference, limbs curve away from
            // their roots instead of forming narrow vertical pillars.
            // Forelimbs lean toward the head (left); hindlimbs sweep
            // backward toward the rump (right).
            int anatomicalLean=foreleg?-4:4;
            int ankleX=x+distalStride+anatomicalLean;
            int kneeX=x+(foreleg?-3:3)+
                Mathf.RoundToInt(distalStride*.35f);
            int lift=Mathf.Clamp(footLift,0,3);
            int kneeY=y-Mathf.RoundToInt(h*.55f)+lift/2;
            return new Shape {
                root=new Vector2Int(x,y),
                knee=new Vector2Int(kneeX,kneeY),
                ankle=new Vector2Int(ankleX,y-h+3+lift),
                upperRadius=upper,lowerRadius=lower
            };
        }

        /// <summary>
        /// A short broken highlight confined to the upper shoulder/thigh.
        /// Full-length light strokes made the legs read as rigid poles in
        /// the first actual Unity contact sheets.
        /// </summary>
        public static Vector2Int[] ShoulderHighlight(Shape s)
        {
            int dy=s.root.y-s.knee.y;
            int span=Mathf.Max(2,dy/3);
            int dx=s.knee.x-s.root.x;
            return new[]{
                new Vector2Int(s.root.x-1,s.root.y),
                new Vector2Int(
                    s.root.x-1+Mathf.RoundToInt(
                        dx*(span/(float)Mathf.Max(1,dy))),
                    s.root.y-span)
            };
        }

        public static Vector2Int[] UpperPolygon(Shape s)
        {
            // Rounded muscle at the shoulder with an inset knee. Extra
            // contour stations prevent the vertical six-sided pole shape.
            int thirdY=s.root.y-(s.root.y-s.knee.y)/3;
            int thirdX=Mathf.RoundToInt(
                Mathf.Lerp(s.root.x,s.knee.x,.33f));
            return new[]{
                new Vector2Int(s.root.x-s.upperRadius,s.root.y+2),
                new Vector2Int(s.root.x+s.upperRadius,s.root.y+1),
                new Vector2Int(thirdX+s.upperRadius,thirdY+1),
                new Vector2Int(s.knee.x+s.lowerRadius,s.knee.y+1),
                new Vector2Int(s.knee.x+s.lowerRadius-1,s.knee.y-2),
                new Vector2Int(s.knee.x-s.lowerRadius,s.knee.y-2),
                new Vector2Int(thirdX-s.upperRadius,thirdY-1),
                new Vector2Int(s.root.x-s.upperRadius-1,s.root.y-2)
            };
        }

        public static Vector2Int[] LowerPolygon(Shape s)
        {
            // The calf curves into the ankle rather than continuing a
            // single-width tube. Endpoints still match the planted foot.
            int midY=Mathf.RoundToInt(
                Mathf.Lerp(s.knee.y,s.ankle.y,.55f));
            int midX=Mathf.RoundToInt(
                Mathf.Lerp(s.knee.x,s.ankle.x,.55f));
            int calf=Mathf.Max(2,s.lowerRadius-1);
            return new[]{
                new Vector2Int(s.knee.x-s.lowerRadius,s.knee.y+1),
                new Vector2Int(s.knee.x+s.lowerRadius,s.knee.y),
                new Vector2Int(midX+calf,midY),
                new Vector2Int(s.ankle.x+calf,s.ankle.y-1),
                new Vector2Int(s.ankle.x+2,s.ankle.y-3),
                new Vector2Int(s.ankle.x-3,s.ankle.y-3),
                new Vector2Int(midX-calf,midY-1),
                new Vector2Int(s.knee.x-s.lowerRadius-1,s.knee.y-2)
            };
        }

    }
}
