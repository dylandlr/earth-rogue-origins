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
        // Each profile is y in normalized [-1,1] at authored x stations.
        // Top is deliberately asymmetric: shoulder humps and jaw read at tiny scale.
        // Thirteen author-controlled stations give the snout, raised brow,
        // cheek bulge and rear-neck connection different rhythms. These
        // profiles are original hand-authored numeric masks, not image samples.
        static readonly float[] BroadHeadTop={
            -.22f,-.15f,.05f,.40f,.65f,.83f,1f,1f,.95f,.90f,.78f,.62f,.44f};
        static readonly float[] BroadHeadBottom={
            -.37f,-.51f,-.62f,-.79f,-.90f,-.97f,-1f,-.96f,-.89f,-.82f,-.76f,-.68f,-.52f};
        static readonly float[] WedgeHeadTop={
            -.48f,-.39f,-.31f,-.14f,.15f,.48f,.73f,.90f,.95f,.91f,.84f,.74f,.59f};
        static readonly float[] WedgeHeadBottom={
            -.49f,-.57f,-.63f,-.73f,-.82f,-.89f,-.97f,-.97f,-.93f,-.86f,-.78f,-.67f,-.54f};
        static readonly float[] LongHeadTop={
            -.37f,-.33f,-.27f,-.17f,.08f,.41f,.72f,.89f,.98f,.97f,.90f,.76f,.49f};
        static readonly float[] LongHeadBottom={
            -.48f,-.53f,-.61f,-.67f,-.74f,-.82f,-.90f,-.96f,-.98f,-.95f,-.85f,-.72f,-.53f};
        static readonly float[] RoundHeadTop={
            -.32f,-.07f,.30f,.62f,.85f,.97f,1f,1f,.98f,.89f,.76f,.52f,.22f};
        static readonly float[] RoundHeadBottom={
            -.44f,-.61f,-.78f,-.87f,-.96f,-1f,-1f,-.97f,-.87f,-.77f,-.64f,-.51f,-.39f};
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
