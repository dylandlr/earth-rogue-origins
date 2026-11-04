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
                    mirrorGroup=src.mirrorGroup,footLift=src.footLift
                };
                // At mid-stance the entire body settles by one logical
                // pixel. Shortening the legs by the same amount keeps their
                // contact points on the identical ground plane rather than
                // translating the whole creature through the terrain.
                if(!swimming && phase==2 && recipe.plan==CreaturePlan.Tetrapod)
                {
                    p.anchor=new Vector2Int(p.anchor.x,p.anchor.y-1);
                    if(p.slot==PartSlot.ForeLeg || p.slot==PartSlot.HindLeg)
                        p.extent=new Vector2Int(p.extent.x,
                            Mathf.Max(5,p.extent.y-1));
                }
                // All other gait motion bends distal anatomy while retaining
                // the attachment stations of the current body pose.
                if(p.slot==PartSlot.ForeLeg || p.slot==PartSlot.HindLeg)
                {
                    int sign=p.slot==PartSlot.ForeLeg?1:-1;
                    int stride=phase==1?3:phase==2?0:-3;
                    if(swimming)stride=phase==1?2:phase==3?-2:0;
                    p.mirrorGroup=stride*sign;
                    // Fore and hind support alternate; the far-side limbs
                    // counterphase their near-side partner. At every phase
                    // at least one limb retains a planted ground contact.
                    if(!swimming)
                    {
                        bool frontSwing=phase==1;
                        bool rearSwing=phase==3;
                        bool lift=p.slot==PartSlot.ForeLeg?frontSwing:rearSwing;
                        if(p.drawOrder<2)lift=!lift && phase!=2;
                        p.footLift=lift?2:0;
                    }
                    else p.footLift=1;
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
