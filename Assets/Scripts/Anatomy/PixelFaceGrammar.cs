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
            int left=centerX-width/2;
            // An eye positioned from a fixed x offset can end up above the
            // steep wedge forehead at small sizes. Sample the actual authored
            // silhouette instead, with safe margins for the eye socket.
            int eyeX=centerX+(head==PartStyle.Wedge?
                Mathf.Max(1,width/10):head==PartStyle.LongSnout?0:
                -Mathf.Max(1,width/7));
            int eyeY=InteriorY(head,eyeX,centerX,centerY,width,height,.60f);
            int nostrilX=left+Mathf.Max(2,width/6);
            int nostrilY=InteriorY(head,nostrilX,centerX,centerY,width,height,.43f);
            int mouthX=left+Mathf.Max(3,width/5);
            int mouthEndX=centerX+Mathf.Max(1,radius/6);
            int mouthY=InteriorY(head,mouthX,centerX,centerY,width,height,.25f);
            int mouthEndY=InteriorY(head,mouthEndX,centerX,centerY,width,height,.25f);
            if(plan==CreaturePlan.Arthropod)
                return new FaceLandmarks {
                    eye=new Vector2Int(eyeX,eyeY),
                    nostril=new Vector2Int(nostrilX,nostrilY),
                    mouthStart=new Vector2Int(mouthX,mouthY),
                    mouthEnd=new Vector2Int(mouthEndX,mouthEndY),
                    cheek=new Vector2Int(centerX,centerY),
                    cheekRadius=2,jawDepth=1
                };

            return new FaceLandmarks {
                eye=new Vector2Int(eyeX,eyeY),
                nostril=new Vector2Int(front+3,centerY),
                mouthStart=new Vector2Int(mouthX,mouthY),
                mouthEnd=new Vector2Int(mouthEndX,mouthEndY),
                cheek=new Vector2Int(centerX+Mathf.Max(1,radius/4),centerY-1),
                cheekRadius=Mathf.Max(2,width/7),
                jawDepth=Mathf.Max(2,height/6)
            };
        }

        static int InteriorY(PartStyle style,int x,int centerX,int centerY,
            int width,int height,float position)
        {
            float[] upper,lower;
            PixelSilhouetteTemplates.Profile(PartSlot.Head,style,out upper,out lower);
            float u=Mathf.Clamp01((x-(centerX-width/2))/(float)Mathf.Max(1,width));
            int low=centerY+Mathf.RoundToInt(
                PixelSilhouetteTemplates.Sample(lower,u)*height*.5f);
            int high=centerY+Mathf.RoundToInt(
                PixelSilhouetteTemplates.Sample(upper,u)*height*.5f);
            return Mathf.RoundToInt(Mathf.Lerp(low+1,high-1,Mathf.Clamp01(position)));
        }
    }
}
