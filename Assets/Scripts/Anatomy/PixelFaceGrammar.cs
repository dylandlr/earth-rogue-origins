using System;
using UnityEngine;

namespace EarthRogueOrigins.PixelCreatures
{
    /// <summary>
    /// Small, deliberate facial landmarks for side-view organisms. Coordinates
    /// are relative to the inherited head profile, never the source concept art.
    /// Rendering distinguishes a cheek, near-nostril, lip separation and
    /// single-pixel eye glint without adding random facial noise.
    /// </summary>
    public static class PixelFaceGrammar
    {
        public struct FaceLandmarks
        {
            public Vector2Int eye, nostril, mouthStart, mouthEnd, cheek;
            public int cheekRadius, jawDepth;
        }

        public static FaceLandmarks Landmarks(
            CreaturePlan plan,PartStyle head,int centerX,int centerY,int width,int height)
        {
            int radius=Mathf.Max(6,width/2);
            int length=head==PartStyle.LongSnout?5:head==PartStyle.Wedge?2:0;
            int front=centerX-radius-length;
            int eyeX=centerX-Mathf.Max(2,radius/3);
            int eyeY=centerY+Mathf.Max(2,height/5);
            if(plan==CreaturePlan.Arthropod)
                return new FaceLandmarks {
                    eye=new Vector2Int(eyeX,eyeY),
                    nostril=new Vector2Int(front+3,centerY),
                    mouthStart=new Vector2Int(front+3,centerY-2),
                    mouthEnd=new Vector2Int(front+6,centerY-2),
                    cheek=new Vector2Int(centerX,centerY),
                    cheekRadius=2,jawDepth=1
                };

            int lower=centerY-Mathf.Max(3,height/4);
            return new FaceLandmarks {
                eye=new Vector2Int(eyeX,eyeY),
                nostril=new Vector2Int(front+3,centerY),
                mouthStart=new Vector2Int(front+1,lower+1),
                mouthEnd=new Vector2Int(centerX+Mathf.Max(1,radius/5),lower),
                cheek=new Vector2Int(centerX+Mathf.Max(1,radius/4),centerY-1),
                cheekRadius=Mathf.Max(2,width/7),
                jawDepth=Mathf.Max(2,height/6)
            };
        }
    }
}
