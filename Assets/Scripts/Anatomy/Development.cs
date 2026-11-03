using System;
using System.Collections.Generic;
using UnityEngine;
using EarthRogueOrigins.Evolution;

namespace EarthRogueOrigins.Anatomy
{
    public enum BodyGene
    {
        TrunkLength, TrunkDepth, HeadScale, TailLength, TailFin, LimbLength,
        LimbMuscle, FootWebbing, ArmorCoverage, ArmorHeight, PatternDensity,
        Hue, EyeScale, NeckLength
    }

    public enum Attachment { Head, TailRoot, FrontLeft, FrontRight, RearLeft, RearRight, Back }

    [Serializable]
    public sealed class DevelopmentalGenome
    {
        public const int GeneCount=14;
        public float[] regulatory=new float[GeneCount];
        public int bodyPlan=0; // 0: four-legged tetrapod; more plans require dedicated constraints

        public float Get(BodyGene gene)
        {
            if(regulatory==null || regulatory.Length!=GeneCount)
                throw new InvalidOperationException("Invalid developmental genome.");
            return Mathf.Clamp01(regulatory[(int)gene]);
        }

        public static DevelopmentalGenome Founder(System.Random rng)
        {
            if(rng==null)throw new ArgumentNullException(nameof(rng));
            var g=new DevelopmentalGenome();
            for(int i=0;i<GeneCount;i++)g.regulatory[i]=.25f+(float)rng.NextDouble()*.5f;
            return g;
        }

        public static DevelopmentalGenome Child(DevelopmentalGenome a,DevelopmentalGenome b,
            System.Random rng,float mutationChance=.025f)
        {
            if(a==null || b==null)throw new ArgumentNullException("parents");
            if(rng==null)throw new ArgumentNullException(nameof(rng));
            if(mutationChance<0 || mutationChance>1)throw new ArgumentOutOfRangeException(nameof(mutationChance));
            var child=new DevelopmentalGenome{bodyPlan=rng.Next(2)==0?a.bodyPlan:b.bodyPlan};
            if(child.bodyPlan!=0)throw new NotSupportedException("Only tetrapod anatomy is implemented.");
            for(int i=0;i<GeneCount;i++)
            {
                // Continuous regulatory expression is a blended inheritance toy model.
                // Not an allele-level representation; future work must replace this
                // with a regulatory network + sexual recombination.
                float value=(a.regulatory[i]+b.regulatory[i])*.5f;
                value+=(float)(rng.NextDouble()-.5)*.06f;
                if(rng.NextDouble()<mutationChance)
                    value+=(float)(rng.NextDouble()*2-1)*.22f;
                child.regulatory[i]=Mathf.Clamp01(value);
            }
            return child;
        }
    }

    [Serializable]
    public sealed class AnatomicalPhenotype
    {
        public int bodyPlan;
        public float trunkLength, trunkDepth, headRadius, tailLength, tailFinSize;
        public float limbLength, limbThickness, footWebbing, armorCoverage, armorHeight;
        public float patternDensity, hue, eyeRadius, neckLength;
        public Vector2 head, tailRoot, frontLeg, rearLeg;
        public Vector2[] dorsalSurface;
        public float bodyMass, terrestrialSpeed, swimmingSpeed, defense, upkeep;

        public Vector2 Anchor(Attachment attachment)
        {
            switch(attachment)
            {
                case Attachment.Head: return head;
                case Attachment.TailRoot: return tailRoot;
                case Attachment.FrontLeft: return frontLeg+Vector2.up*.05f;
                case Attachment.FrontRight: return frontLeg-Vector2.up*.05f;
                case Attachment.RearLeft: return rearLeg+Vector2.up*.05f;
                case Attachment.RearRight: return rearLeg-Vector2.up*.05f;
                default: return Vector2.up*trunkDepth;
            }
        }
    }

    public static class Development
    {
        public static AnatomicalPhenotype Express(DevelopmentalGenome g,DiploidGenome physiology=null)
        {
            if(g==null)throw new ArgumentNullException(nameof(g));
            if(g.bodyPlan!=0)throw new NotSupportedException("Only tetrapod body plan is available.");
            float speed=physiology == null ? .5f : physiology.Phenotype(Locus.Speed);
            float armor=physiology==null?.5f:physiology.Phenotype(Locus.Armor);
            var p=new AnatomicalPhenotype{bodyPlan=g.bodyPlan};
            p.trunkLength=Mathf.Lerp(.65f,1.35f,g.Get(BodyGene.TrunkLength));
            p.trunkDepth=Mathf.Lerp(.29f,.62f,g.Get(BodyGene.TrunkDepth));
            p.headRadius=Mathf.Lerp(.14f,.30f,g.Get(BodyGene.HeadScale));
            p.neckLength=Mathf.Lerp(.04f,.22f,g.Get(BodyGene.NeckLength));
            p.tailLength=Mathf.Lerp(.28f,1.15f,g.Get(BodyGene.TailLength));
            p.tailFinSize=Mathf.Lerp(0,.33f,g.Get(BodyGene.TailFin));
            p.limbLength=Mathf.Lerp(.19f,.70f,g.Get(BodyGene.LimbLength));
            p.limbThickness=Mathf.Lerp(.06f,.18f,g.Get(BodyGene.LimbMuscle));
            p.footWebbing=g.Get(BodyGene.FootWebbing);
            p.armorCoverage=Mathf.Clamp01(g.Get(BodyGene.ArmorCoverage)*.65f+armor*.35f);
            p.armorHeight=Mathf.Lerp(.03f,.19f,g.Get(BodyGene.ArmorHeight));
            p.patternDensity=g.Get(BodyGene.PatternDensity);
            p.hue=g.Get(BodyGene.Hue);
            p.eyeRadius=Mathf.Lerp(.018f,.055f,g.Get(BodyGene.EyeScale));
            p.tailRoot=new Vector2(-p.trunkLength*.50f,.02f);
            p.head=new Vector2(p.trunkLength*.54f+p.neckLength,.03f);
            p.rearLeg=new Vector2(-p.trunkLength*.30f,-p.trunkDepth*.20f);
            p.frontLeg=new Vector2(p.trunkLength*.31f,-p.trunkDepth*.20f);
            p.dorsalSurface=new Vector2[17];
            for(int i=0;i<p.dorsalSurface.Length;i++)
            {
                float t=i/(float)(p.dorsalSurface.Length-1);
                float x=Mathf.Lerp(-p.trunkLength*.48f,p.trunkLength*.48f,t);
                float y=p.trunkDepth*Mathf.Sqrt(Mathf.Max(0,1f-Mathf.Pow((t-.5f)*2,2)));
                p.dorsalSurface[i]=new Vector2(x,y);
            }
            // Costly armor / fins produce meaningful ecological tradeoffs.
            p.bodyMass=1f+p.trunkLength*p.trunkDepth*1.8f+
                p.armorCoverage*.65f+p.tailFinSize*.38f;
            p.terrestrialSpeed=(.7f+p.limbLength*1.3f+speed*.45f)/
                (1f+p.bodyMass*.18f+p.footWebbing*.12f);
            p.swimmingSpeed=(.55f+p.tailLength*.34f+p.tailFinSize*1.8f+p.footWebbing*.34f)/
                (1f+p.bodyMass*.12f);
            p.defense=Mathf.Clamp01(.1f+p.armorCoverage*.65f+p.trunkDepth*.14f);
            p.upkeep=.5f+p.bodyMass*.21f+p.armorCoverage*.16f+p.limbThickness*.30f;
            return p;
        }

        public static bool IsValid(AnatomicalPhenotype p)
        {
            if(p==null || p.dorsalSurface==null || p.dorsalSurface.Length<3)return false;
            if(p.trunkLength<=0 || p.trunkDepth<=0 || p.tailLength<=0 ||
                p.limbLength<=0 || p.bodyMass<=0 || p.upkeep<=0)return false;
            if(p.rearLeg.x>=p.frontLeg.x || p.tailRoot.x>=p.head.x)return false;
            for(int i=1;i<p.dorsalSurface.Length;i++)
                if(p.dorsalSurface[i].x<=p.dorsalSurface[i-1].x)return false;
            return true;
        }
    }
}
