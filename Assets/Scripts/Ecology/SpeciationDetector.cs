using System;
using System.Collections.Generic;
using UnityEngine;

namespace EarthRogueOrigins.Ecology
{
    /// <summary>
    /// Operational allopatric-divergence detector, not proof of biological
    /// reproductive isolation. Uses existing inherited physiology and
    /// regulatory genes, observed geography, and independently recorded
    /// consecutive qualifying windows. No RNG and no mutations.
    /// </summary>
    [Serializable]
    public sealed class SpeciationSettings
    {
        public bool enabled=true;
        public int evaluationInterval=50;
        public int requiredConsecutiveWindows=5;
        public int minimumPerPopulation=4;
        public int minimumGeneration=2;
        public float minimumGeographicGap=3f;
        public float minimumCentroidSeparation=8f;
        public float minimumGeneticDistance=.18f;
    }

    public sealed class SpeciationEvidence
    {
        public int parentSpeciesId;
        public int observations;
        public int leftCount,rightCount;
        public float geographicGap,centroidSeparation,geneticDistance;
        public Vector2 leftCentroid,rightCentroid;
        public int[] candidateIndividualIds;
    }

    public sealed class SpeciationDetector
    {
        readonly Dictionary<int,SpeciationEvidence> streaks=
            new Dictionary<int,SpeciationEvidence>();

        public void Clear(){streaks.Clear();}

        public SpeciationEvidence Observe(int speciesId,List<EvolvingCreature> organisms,
            SpeciationSettings settings)
        {
            if(settings==null)throw new ArgumentNullException(nameof(settings));
            if(organisms==null)throw new ArgumentNullException(nameof(organisms));
            if(settings.minimumPerPopulation<2 ||
               settings.requiredConsecutiveWindows<2 ||
               settings.minimumGeneticDistance<=0f ||
               settings.minimumGeographicGap<=0f)
                throw new ArgumentException("Invalid speciation settings.");

            var group=new List<EvolvingCreature>();
            foreach(var c in organisms)
                if(c.Alive && c.speciesId==speciesId && c.development!=null)
                    group.Add(c);
            group.Sort((a,b)=>{
                int x=a.position.x.CompareTo(b.position.x);
                return x!=0?x:a.id.CompareTo(b.id);
            });
            int minimum=settings.minimumPerPopulation;
            if(group.Count<minimum*2)return Fail(speciesId);
            float bestGap=float.NegativeInfinity;
            int cut=-1;
            for(int i=minimum;i<=group.Count-minimum;i++)
            {
                float gap=group[i].position.x-group[i-1].position.x;
                if(gap>bestGap){bestGap=gap;cut=i;}
            }
            if(bestGap<settings.minimumGeographicGap)return Fail(speciesId);

            float leftX=0,leftY=0,rightX=0,rightY=0;
            int leftGeneration=0,rightGeneration=0;
            for(int i=0;i<group.Count;i++)
            {
                var c=group[i];
                if(i<cut)
                {
                    leftX+=c.position.x;leftY+=c.position.y;
                    leftGeneration=Math.Max(leftGeneration,c.generation);
                }
                else
                {
                    rightX+=c.position.x;rightY+=c.position.y;
                    rightGeneration=Math.Max(rightGeneration,c.generation);
                }
            }
            var leftCenter=new Vector2(leftX/cut,leftY/cut);
            var rightCenter=new Vector2(rightX/(group.Count-cut),
                rightY/(group.Count-cut));
            float centroidDistance=Vector2.Distance(leftCenter,rightCenter);
            if(centroidDistance<settings.minimumCentroidSeparation ||
               leftGeneration<settings.minimumGeneration ||
               rightGeneration<settings.minimumGeneration)
                return Fail(speciesId);

            // Six diploid-locus means plus 14 regulatory gene means.
            // Continuous distances are descriptive, not reproductive proof.
            var left=new float[20];
            var right=new float[20];
            for(int i=0;i<group.Count;i++)
            {
                var c=group[i];
                var accumulator=i<cut?left:right;
                for(int locus=0;locus<6;locus++)
                    accumulator[locus]+=c.Trait(locus);
                for(int j=0;j<14;j++)
                    accumulator[6+j]+=c.development.Get((EarthRogueOrigins.Anatomy.BodyGene)j);
            }
            float difference=0f;
            for(int i=0;i<left.Length;i++)
                difference+=Mathf.Abs(left[i]/cut-
                    right[i]/(group.Count-cut));
            difference/=left.Length;
            if(difference<settings.minimumGeneticDistance)return Fail(speciesId);

            SpeciationEvidence previous;
            streaks.TryGetValue(speciesId,out previous);
            // Reject a different cohort substituting for the original one:
            // multiple distinct gaps do not constitute persistent isolation.
            if(previous!=null &&
               (Vector2.Distance(previous.leftCentroid,leftCenter)>3.5f ||
                Vector2.Distance(previous.rightCentroid,rightCenter)>3.5f))
                previous=null;
            var result=new SpeciationEvidence{
                parentSpeciesId=speciesId,
                observations=previous==null?1:previous.observations+1,
                leftCount=cut,rightCount=group.Count-cut,
                geographicGap=bestGap,
                centroidSeparation=centroidDistance,
                geneticDistance=difference,
                leftCentroid=leftCenter,rightCentroid=rightCenter,
                candidateIndividualIds=new int[group.Count-cut]
            };
            for(int i=cut;i<group.Count;i++)
                result.candidateIndividualIds[i-cut]=group[i].id;
            streaks[speciesId]=result;
            return result;
        }

        SpeciationEvidence Fail(int speciesId)
        {
            streaks.Remove(speciesId);
            return null;
        }
    }
}
