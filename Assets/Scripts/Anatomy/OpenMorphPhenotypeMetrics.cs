using System;
using System.Collections.Generic;
using UnityEngine;

namespace EarthRogueOrigins.PixelCreatures
{
    /// <summary>
    /// Measures expressed anatomy, not genome length or the presentation-
    /// scaled skeleton preview. Developmental guidance and final tissue are
    /// compared in the SAME original tissue-coordinate system.
    /// </summary>
    public static class OpenMorphPhenotypeMetrics
    {
        public struct Result
        {
            public int occupied,expressedGuidanceCells,supportedTissueCells;
            public int guidanceWithoutTissue,tissueOutsideGuidance;
            public float supportedTissueFraction,guidanceExpressionFraction;
            public float coverageJaccard;
        }

        public static Result Analyze(OpenMorphGenome genome,
            OpenMorphTissue.Field tissue,float threshold=.25f)
        {
            if(genome==null||tissue==null)
                throw new ArgumentNullException("Genome and developed tissue required.");
            if(float.IsNaN(threshold)||threshold<=0f||threshold>=1f)
                throw new ArgumentOutOfRangeException(nameof(threshold));
            var organizer=OpenMorphOrganizers.Build(
                genome,tissue.width,tissue.height);
            var result=new Result();
            for(int i=0;i<tissue.cells.Length;i++)
            {
                bool grown=tissue.cells[i].fate!=OpenMorphTissue.Fate.Empty;
                bool guided=organizer.activation[i]>=threshold;
                if(grown)result.occupied++;
                if(guided)result.expressedGuidanceCells++;
                if(grown && guided)result.supportedTissueCells++;
                else if(grown)result.tissueOutsideGuidance++;
                else if(guided)result.guidanceWithoutTissue++;
            }
            result.supportedTissueFraction=result.occupied==0?0f:
                result.supportedTissueCells/(float)result.occupied;
            result.guidanceExpressionFraction=
                result.expressedGuidanceCells==0?0f:
                result.supportedTissueCells/(float)result.expressedGuidanceCells;
            int union=result.supportedTissueCells+
                result.tissueOutsideGuidance+result.guidanceWithoutTissue;
            result.coverageJaccard=union==0?1f:
                result.supportedTissueCells/(float)union;
            return result;
        }

        /// <summary>
        /// Novelty is a phenotype occupancy difference, irrespective of
        /// body color, named anatomy or genome innovation count.
        /// Mean of the K closest neighbors prevents a single outlier
        /// from dominating the candidate pool.
        /// </summary>
        public static float Novelty(OpenMorphTissue.Field subject,
            IList<OpenMorphTissue.Field> reference,int neighbors=3)
        {
            if(subject==null||reference==null)
                throw new ArgumentNullException("Subject and references required.");
            if(neighbors<1)throw new ArgumentOutOfRangeException(nameof(neighbors));
            var distances=new List<float>();
            foreach(var other in reference)
            {
                if(other==null || Object.ReferenceEquals(other,subject))
                    continue;
                distances.Add(1f-OpenMorphPairMetrics.Compare(subject,other).jaccard);
            }
            if(distances.Count==0)return 0f;
            distances.Sort();
            int n=Math.Min(neighbors,distances.Count);
            float sum=0f;
            for(int i=0;i<n;i++)sum+=distances[i];
            return sum/n;
        }
    }
}
