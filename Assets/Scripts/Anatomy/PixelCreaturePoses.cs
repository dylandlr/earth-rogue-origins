using System;
using UnityEngine;

namespace EarthRogueOrigins.PixelCreatures
{
    /// <summary>
    /// Pose is a visual-only transformation of the validated part layout.
    /// No changes to physiology, navigation, selection or world state.
    /// Grounded walking alternates near/far limb positions; swimming moves
    /// tail/fins with reduced limb reach. Output is always integer pixels.
    /// </summary>
    public static class PixelCreaturePoses
    {
        public const int Frames=4;

        public static PixelPartDescriptor[] Layout(PixelCreatureRecipe recipe,int frame,bool swimming)
        {
            recipe.Validate();
            var parts=PixelCreatureDevelopment.Layout(recipe);
            int phase=((frame%Frames)+Frames)%Frames;
            if(phase==0)return parts;
            for(int i=0;i<parts.Length;i++)
            {
                var src=parts[i];
                var p=new PixelPartDescriptor {
                    slot=src.slot,style=src.style,anchor=src.anchor,
                    extent=src.extent,drawOrder=src.drawOrder,
                    mirrorGroup=src.mirrorGroup
                };
                // Preserve joint root position. The pose offset is applied to
                // each appendage's distal end by the painter, never by moving
                // the attachment anchor away from the body.
                if(p.slot==PartSlot.ForeLeg || p.slot==PartSlot.HindLeg)
                {
                    int sign=p.slot==PartSlot.ForeLeg?1:-1;
                    int stride=phase==1?3:phase==2?0:-3;
                    if(swimming)stride=phase==1?2:phase==3?-2:0;
                    p.mirrorGroup=stride*sign;
                }
                else if(p.slot==PartSlot.Tail ||
                        (p.slot==PartSlot.Fin && recipe.plan==CreaturePlan.Fish &&
                         p.anchor.x>95))
                {
                    int wave=phase==1?3:phase==3?-3:0;
                    p.mirrorGroup=wave*(swimming?2:1);
                }
                parts[i]=p;
            }
            return parts;
        }
    }
}
