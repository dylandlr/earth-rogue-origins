using UnityEngine;

namespace EarthRogueOrigins.PixelCreatures
{
    /// <summary>
    /// A small original, shape-directed skin fold at the shoulder or hip.
    /// The fold descends from the torso surface over the leg root, rather
    /// than masking the joint with a detached elliptical coin.
    /// </summary>
    public static class PixelJointTransition
    {
        public struct Shape
        {
            public Vector2Int[] outer,inner;
            public Vector2Int highlightStart,highlightEnd;
        }

        public static Shape Build(int x,int rootY,int width,int height,bool hip)
        {
            int half=Mathf.Max(4,width/2);
            int rise=Mathf.Max(3,height/2);
            int direction=hip?1:-1;
            // The shoulder fold leans into the neck; hip folds taper toward
            // the tail. Both connect at the body's lower contour.
            return new Shape {
                outer=new[]{
                    new Vector2Int(x-half-direction*2,rootY+rise),
                    new Vector2Int(x+half+direction*2,rootY+rise-1),
                    new Vector2Int(x+half+direction,rootY+1),
                    new Vector2Int(x+2*direction,rootY-3),
                    new Vector2Int(x-half+direction,rootY-2)
                },
                inner=new[]{
                    new Vector2Int(x-half+1-direction,rootY+rise-1),
                    new Vector2Int(x+half-1+direction,rootY+rise-2),
                    new Vector2Int(x+half-2+direction,rootY+1),
                    new Vector2Int(x+direction,rootY-1),
                    new Vector2Int(x-half+2+direction,rootY)
                },
                highlightStart=new Vector2Int(x-half+2,rootY+rise-1),
                highlightEnd=new Vector2Int(x+direction,rootY+rise-1)
            };
        }
    }
}
