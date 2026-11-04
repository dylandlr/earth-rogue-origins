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
        // Back rises gradually from the neck, crests over the shoulders, and
        // falls into the rump. Lower contour has asymmetric shoulder/hip
        // transitions rather than a centered geometric ellipse.
        static readonly float[] TorsoTop={
            .32f,.47f,.63f,.81f,.92f,.97f,1f,.99f,.96f,.90f,.81f,.65f,.43f};
        static readonly float[] TorsoBottom={
            -.26f,-.43f,-.60f,-.77f,-.86f,-.94f,-.97f,-1f,-.98f,-.89f,-.80f,-.64f,-.39f};
        static readonly float[] StockyTop={
            .48f,.59f,.74f,.89f,.97f,1f,1f,.99f,.98f,.95f,.86f,.73f,.58f};
        static readonly float[] StockyBottom={
            -.41f,-.59f,-.74f,-.87f,-.95f,-.98f,-1f,-1f,-.98f,-.94f,-.87f,-.73f,-.51f};
        static readonly float[] SlenderTop={
            .16f,.33f,.51f,.69f,.84f,.93f,.97f,.95f,.89f,.79f,.65f,.49f,.31f};
        static readonly float[] SlenderBottom={
            -.11f,-.27f,-.44f,-.61f,-.71f,-.80f,-.86f,-.88f,-.82f,-.72f,-.59f,-.45f,-.27f};

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

        /// <summary>
        /// A shallow, smooth tuck between the fore- and hind-leg stations.
        /// Positive Y is dorsal in the sprite coordinate system: making the
        /// normalized lower contour less negative raises the belly.
        /// Only used on tetrapods; fish retain their aquatic contour.
        /// </summary>
        public static float TetrapodBellyLower(PartStyle style,float u)
        {
            float[] upper,lower;
            Profile(PartSlot.Trunk,style,out upper,out lower);
            float original=Sample(lower,u);
            const float front=.29f,rear=.73f;
            if(u<=front || u>=rear)return original;
            float t=(u-front)/(rear-front);
            float arch=Mathf.Sin(t*Mathf.PI);
            arch*=arch;
            float strength=style==PartStyle.Robust?.24f:
                style==PartStyle.Slender?.13f:.19f;
            return Mathf.Min(original+strength*arch,
                Sample(upper,u)-.12f);
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
