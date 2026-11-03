using System;
using UnityEngine;

namespace EarthRogueOrigins.PixelCreatures
{
    /// <summary>
    /// Authored *original* top/bottom contour profiles, not extracted from reference
    /// images. Band interpolation preserves pixel steps and usable attachment roots.
    /// Coordinates are in each module's normalized left-to-right space.
    /// </summary>
    public static class PixelSilhouetteTemplates
    {
        // Each profile is y in normalized [-1,1] at seven x sample positions.
        // Top is deliberately asymmetric: shoulder humps and jaw read at tiny scale.
        static readonly float[] BroadHeadTop={-.16f,.35f,.79f,.98f,.93f,.69f,.33f};
        static readonly float[] BroadHeadBottom={-.40f,-.71f,-.93f,-1f,-.91f,-.77f,-.54f};
        static readonly float[] WedgeHeadTop={-.43f,-.30f,.18f,.63f,.91f,.84f,.68f};
        static readonly float[] WedgeHeadBottom={-.46f,-.65f,-.79f,-.97f,-.93f,-.77f,-.56f};
        static readonly float[] LongHeadTop={-.35f,.04f,.44f,.83f,.91f,.79f,.50f};
        static readonly float[] LongHeadBottom={-.51f,-.64f,-.81f,-.94f,-.95f,-.83f,-.60f};
        static readonly float[] RoundHeadTop={-.31f,.34f,.78f,.98f,.98f,.73f,.23f};
        static readonly float[] RoundHeadBottom={-.48f,-.78f,-.96f,-1f,-.89f,-.68f,-.39f};
        static readonly float[] TorsoTop={.23f,.55f,.83f,.99f,.96f,.78f,.51f};
        static readonly float[] TorsoBottom={-.22f,-.64f,-.92f,-1f,-.91f,-.74f,-.44f};
        static readonly float[] StockyTop={.46f,.76f,.96f,1f,.98f,.90f,.65f};
        static readonly float[] StockyBottom={-.42f,-.78f,-.96f,-1f,-.97f,-.86f,-.55f};
        static readonly float[] SlenderTop={.08f,.48f,.77f,.95f,.91f,.68f,.36f};
        static readonly float[] SlenderBottom={-.10f,-.48f,-.72f,-.86f,-.81f,-.57f,-.30f};

        public static void Profile(PartSlot slot,PartStyle style,
            out float[] upper,out float[] lower)
        {
            if(slot==PartSlot.Head)
            {
                switch(style)
                {
                    case PartStyle.Wedge:upper=WedgeHeadTop;lower=WedgeHeadBottom;return;
                    case PartStyle.LongSnout:upper=LongHeadTop;lower=LongHeadBottom;return;
                    case PartStyle.Robust:upper=BroadHeadTop;lower=BroadHeadBottom;return;
                    default:upper=RoundHeadTop;lower=RoundHeadBottom;return;
                }
            }
            if(slot!=PartSlot.Trunk)throw new ArgumentException("No silhouette profile for "+slot);
            switch(style)
            {
                case PartStyle.Robust:upper=StockyTop;lower=StockyBottom;return;
                case PartStyle.Slender:upper=SlenderTop;lower=SlenderBottom;return;
                default:upper=TorsoTop;lower=TorsoBottom;return;
            }
        }

        public static float Sample(float[] values,float u)
        {
            if(values==null||values.Length<2)throw new ArgumentException("Profile needs samples.");
            float index=Mathf.Clamp01(u)*(values.Length-1);
            int left=Mathf.Min(values.Length-2,Mathf.FloorToInt(index));
            return Mathf.Lerp(values[left],values[left+1],index-left);
        }

        public static bool Contains(PartSlot slot,PartStyle style,float u,float v)
        {
            float[] top,bottom;
            Profile(slot,style,out top,out bottom);
            float upper=Sample(top,u),lower=Sample(bottom,u);
            return v>=lower && v<=upper;
        }

        public static bool AttachmentOverlap(PixelPartDescriptor a,PixelPartDescriptor b)
        {
            // Conservative envelope test used as an export diagnostic, not anatomy physics.
            return Math.Abs(a.anchor.x-b.anchor.x)*2<=a.extent.x+b.extent.x+12 &&
                   Math.Abs(a.anchor.y-b.anchor.y)*2<=a.extent.y+b.extent.y+12;
        }
    }
}
