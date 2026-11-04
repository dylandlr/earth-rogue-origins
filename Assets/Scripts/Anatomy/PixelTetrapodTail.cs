using UnityEngine;

namespace EarthRogueOrigins.PixelCreatures
{
    /// <summary>
    /// Original integer-grid amphibian tail contour. The root stays under
    /// the rump while the terminal third responds to the walk/swim wave.
    /// The dorsal and ventral contours are independently authored to avoid
    /// the straight triangular wedge seen in the first Unity contact sheet.
    /// </summary>
    public static class PixelTetrapodTail
    {
        public struct Shape
        {
            public Vector2Int[] outer;
            public Vector2Int[] skin;
            public Vector2Int highlightStart,highlightEnd;
        }

        public static Shape Build(int centerX,int centerY,int width,int wave)
        {
            int start=centerX-width/2-5;
            int end=centerX+width/2+2;
            int span=end-start;
            int x1=start+Mathf.RoundToInt(span*.22f);
            int x2=start+Mathf.RoundToInt(span*.48f);
            int x3=start+Mathf.RoundToInt(span*.72f);
            int x4=start+Mathf.RoundToInt(span*.89f);
            int bend=wave/2;

            return new Shape {
                outer=new[]{
                    new Vector2Int(start,centerY+6),
                    new Vector2Int(x1,centerY+8),
                    new Vector2Int(x2,centerY+7),
                    new Vector2Int(x3,centerY+5+bend),
                    new Vector2Int(x4,centerY+3+wave),
                    new Vector2Int(end,centerY+1+wave),
                    new Vector2Int(end,centerY-1+wave),
                    new Vector2Int(x4,centerY-2+wave),
                    new Vector2Int(x3,centerY-3+bend),
                    new Vector2Int(x2,centerY-5),
                    new Vector2Int(x1,centerY-6),
                    new Vector2Int(start,centerY-5)
                },
                skin=new[]{
                    new Vector2Int(start+2,centerY+4),
                    new Vector2Int(x1,centerY+6),
                    new Vector2Int(x2,centerY+5),
                    new Vector2Int(x3,centerY+3+bend),
                    new Vector2Int(x4,centerY+1+wave),
                    new Vector2Int(end-1,centerY+wave),
                    new Vector2Int(x4,centerY-1+wave),
                    new Vector2Int(x3,centerY-2+bend),
                    new Vector2Int(x2,centerY-3),
                    new Vector2Int(x1,centerY-4),
                    new Vector2Int(start+2,centerY-3)
                },
                highlightStart=new Vector2Int(x1,centerY+5),
                highlightEnd=new Vector2Int(x2,centerY+4)
            };
        }
    }
}
