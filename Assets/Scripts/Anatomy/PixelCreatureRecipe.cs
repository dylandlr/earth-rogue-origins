using System;
using UnityEngine;

namespace EarthRogueOrigins.PixelCreatures
{
    public enum CreaturePlan { Tetrapod, Fish, Arthropod }
    public enum PartSlot { Tail, HindLeg, ForeLeg, Trunk, Head, Eye, Armor, Fin, Markings, NeckBlend, ShoulderBlend, HipBlend, TailRootBlend, FaceDetails }
    public enum PartStyle { Rounded, Slender, Robust, Plated, Segmented, Fan, Webbed, Wedge, LongSnout }

    [Serializable]
    public sealed class PixelCreatureRecipe
    {
        public int version = 1;
        public string id = "new_creature";
        public CreaturePlan plan = CreaturePlan.Tetrapod;
        public CreatureArchetype archetype = CreatureArchetype.MarshCrawler;
        public CreaturePalette palette = CreaturePalette.SwampOlive;
        public MarkingPattern pattern = MarkingPattern.MottledBack;
        public int seed = 2046;
        public Color32 baseColor = new Color32(143,150,106,255);
        public Color32 secondaryColor = new Color32(91,103,74,255);
        public Color32 accentColor = new Color32(190,184,141,255);
        public float trunkLength = .5f, trunkDepth = .5f, headSize = .5f;
        public float legLength = .5f, tailLength = .5f, finSize = .0f;
        public float armor = 0f, spots = .45f, eyeSize = .5f;
        public PartStyle headStyle = PartStyle.Rounded;
        public PartStyle trunkStyle = PartStyle.Rounded;
        public PartStyle tailStyle = PartStyle.Slender;
        public PartStyle limbStyle = PartStyle.Robust;

        public void Validate()
        {
            if(string.IsNullOrWhiteSpace(id))throw new ArgumentException("Recipe id is required.");
            if(version!=1)throw new NotSupportedException("Unsupported pixel recipe version "+version);
            if(!Enum.IsDefined(typeof(CreaturePlan),plan))throw new ArgumentException("Unknown body plan.");
            if(!Enum.IsDefined(typeof(CreaturePalette),palette) || !Enum.IsDefined(typeof(MarkingPattern),pattern) ||
                !Enum.IsDefined(typeof(CreatureArchetype),archetype))throw new ArgumentException("Unknown art style setting.");
            foreach(float v in new[]{trunkLength,trunkDepth,headSize,legLength,tailLength,
                finSize,armor,spots,eyeSize})
                if(float.IsNaN(v)||float.IsInfinity(v)||v<0||v>1)
                    throw new ArgumentOutOfRangeException("Morphology controls must be finite in [0,1].");
        }

        public PixelCreatureRecipe Copy()
        {
            return (PixelCreatureRecipe)MemberwiseClone();
        }

        public static PixelCreatureRecipe Founder(CreaturePlan plan,int seed)
        {
            var random=new System.Random(seed);
            var recipe=new PixelCreatureRecipe {plan=plan,seed=seed,id=plan+"_"+seed};
            recipe.trunkLength=.25f+(float)random.NextDouble()*.55f;
            recipe.trunkDepth=.3f+(float)random.NextDouble()*.45f;
            recipe.headSize=.28f+(float)random.NextDouble()*.42f;
            recipe.legLength=.25f+(float)random.NextDouble()*.45f;
            recipe.tailLength=.2f+(float)random.NextDouble()*.65f;
            recipe.finSize=plan==CreaturePlan.Fish?.5f:0f;
            recipe.armor=plan==CreaturePlan.Arthropod?.65f:.08f;
            recipe.spots=(float)random.NextDouble()*.8f;
            return recipe;
        }

        public static PixelCreatureRecipe Offspring(PixelCreatureRecipe mother,PixelCreatureRecipe father,
            System.Random rng,float mutation=.05f)
        {
            if(mother==null||father==null||rng==null)throw new ArgumentNullException("Parent or random is null.");
            mother.Validate();father.Validate();
            if(mother.plan!=father.plan)throw new InvalidOperationException("Incompatible body plan grammar.");
            if(mutation<0||mutation>1)throw new ArgumentOutOfRangeException(nameof(mutation));
            var child=mother.Copy();
            child.seed=rng.Next();
            child.id=mother.plan+"_"+child.seed;
            child.trunkLength=Cross(mother.trunkLength,father.trunkLength,rng,mutation);
            child.trunkDepth=Cross(mother.trunkDepth,father.trunkDepth,rng,mutation);
            child.headSize=Cross(mother.headSize,father.headSize,rng,mutation);
            child.legLength=Cross(mother.legLength,father.legLength,rng,mutation);
            child.tailLength=Cross(mother.tailLength,father.tailLength,rng,mutation);
            child.finSize=Cross(mother.finSize,father.finSize,rng,mutation);
            child.armor=Cross(mother.armor,father.armor,rng,mutation);
            child.spots=Cross(mother.spots,father.spots,rng,mutation);
            child.eyeSize=Cross(mother.eyeSize,father.eyeSize,rng,mutation);
            child.headStyle=rng.Next(2)==0?mother.headStyle:father.headStyle;
            child.palette=rng.Next(2)==0?mother.palette:father.palette;
            child.pattern=rng.Next(2)==0?mother.pattern:father.pattern;
            child.archetype=rng.Next(2)==0?mother.archetype:father.archetype;
            child.trunkStyle=rng.Next(2)==0?mother.trunkStyle:father.trunkStyle;
            child.tailStyle=rng.Next(2)==0?mother.tailStyle:father.tailStyle;
            child.limbStyle=rng.Next(2)==0?mother.limbStyle:father.limbStyle;
            child.baseColor=rng.Next(2)==0?mother.baseColor:father.baseColor;
            child.secondaryColor=rng.Next(2)==0?mother.secondaryColor:father.secondaryColor;
            child.accentColor=rng.Next(2)==0?mother.accentColor:father.accentColor;
            child.Validate();
            return child;
        }

        static float Cross(float a,float b,System.Random rng,float rate)
        {
            float result=rng.Next(2)==0?a:b;
            if(rng.NextDouble()<rate)result+=(float)(rng.NextDouble()*2-1)*.23f;
            return Mathf.Clamp01(result);
        }
    }

    [Serializable]
    public sealed class PixelPartDescriptor
    {
        public PartSlot slot;
        public PartStyle style;
        public Vector2Int anchor;
        public Vector2Int extent;
        public int drawOrder;
        public int mirrorGroup;
    }

    public static class PixelCreatureDevelopment
    {
        // Coordinates use a 128×96 logical canvas, left-facing head. Each body
        // plan has a constrained grammar rather than freely interchangeable slots.
        public static PixelPartDescriptor[] Layout(PixelCreatureRecipe r)
        {
            r.Validate();
            int bodyLength=Mathf.RoundToInt(Mathf.Lerp(43,61,r.trunkLength));
            int bodyDepth=Mathf.RoundToInt(Mathf.Lerp(15,27,r.trunkDepth));
            int headWidth=Mathf.RoundToInt(Mathf.Lerp(16,24,r.headSize));
            int tailWidth=Mathf.RoundToInt(Mathf.Lerp(13,29,r.tailLength));
            int limbHeight=Mathf.RoundToInt(Mathf.Lerp(10,23,r.legLength));
            if(r.plan==CreaturePlan.Fish)
                return new[]{
                    P(PartSlot.Tail,r.tailStyle,94,51,tailWidth,11,0),
                    P(PartSlot.Fin,PartStyle.Fan,94+tailWidth,51,8+Mathf.RoundToInt(r.finSize*13),15,1),
                    P(PartSlot.Trunk,r.trunkStyle,61,50,bodyLength,bodyDepth,2),
                    P(PartSlot.Fin,PartStyle.Fan,64,35,15,10,3),
                    P(PartSlot.Head,r.headStyle,32,50,headWidth,bodyDepth-4,4),
                    P(PartSlot.Eye,PartStyle.Rounded,25,46,3,3,6),
                    P(PartSlot.Markings,PartStyle.Rounded,62,50,bodyLength,bodyDepth,5)
                };
            if(r.plan==CreaturePlan.Arthropod)
                return new[]{
                    P(PartSlot.Trunk,PartStyle.Segmented,66,50,bodyLength+8,bodyDepth,1),
                    P(PartSlot.HindLeg,PartStyle.Slender,76,57,18,limbHeight,0),
                    P(PartSlot.ForeLeg,PartStyle.Slender,46,58,18,limbHeight,0),
                    P(PartSlot.Head,r.headStyle,27,52,headWidth,bodyDepth-3,3),
                    P(PartSlot.Armor,PartStyle.Plated,65,39,bodyLength+5,bodyDepth,4),
                    P(PartSlot.Eye,PartStyle.Rounded,23,47,3,3,6)
                };
            var face=PixelFaceGrammar.Landmarks(r.plan,r.headStyle,30,51,
                headWidth,bodyDepth-3);
            return new[]{
                P(PartSlot.Tail,r.tailStyle,90,53,tailWidth,12,0),
                P(PartSlot.HindLeg,r.limbStyle,78,45,12,limbHeight,1),
                P(PartSlot.ForeLeg,r.limbStyle,44,45,12,limbHeight,1),
                P(PartSlot.Trunk,r.trunkStyle,63,50,bodyLength,bodyDepth,2),
                P(PartSlot.TailRootBlend,PartStyle.Rounded,85,51,13,11,3),
                P(PartSlot.Markings,PartStyle.Rounded,63,50,bodyLength,bodyDepth,4),
                P(PartSlot.NeckBlend,PartStyle.Rounded,39,51,13,10,7),
                P(PartSlot.Armor,PartStyle.Plated,64,37,bodyLength,bodyDepth,5),
                P(PartSlot.Head,r.headStyle,30,51,headWidth,bodyDepth-3,6),
                P(PartSlot.FaceDetails,r.headStyle,30,51,headWidth,bodyDepth-3,7),
                P(PartSlot.Eye,PartStyle.Rounded,face.eye.x,face.eye.y,3,3,11),
                P(PartSlot.ForeLeg,r.limbStyle,44,44,12,limbHeight,8),
                P(PartSlot.ShoulderBlend,PartStyle.Rounded,44,44,11,8,9),
                P(PartSlot.HindLeg,r.limbStyle,76,44,12,limbHeight,8),
                P(PartSlot.HipBlend,PartStyle.Rounded,76,44,11,8,9)
            };
        }

        static PixelPartDescriptor P(PartSlot slot,PartStyle style,int x,int y,int w,int h,int order)
        {
            return new PixelPartDescriptor {slot=slot,style=style,
                anchor=new Vector2Int(x,y),extent=new Vector2Int(w,h),drawOrder=order};
        }
    }
}
