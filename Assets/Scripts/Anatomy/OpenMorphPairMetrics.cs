using System;

namespace EarthRogueOrigins.PixelCreatures
{
    /// <summary>
    /// Pairwise comparison of two tissue occupancy masks at exactly the
    /// same world coordinates. No cropping, scaling, or image interpolation:
    /// resizing would hide changes in spatial developmental outcomes.
    /// </summary>
    public static class OpenMorphPairMetrics
    {
        public struct Result
        {
            public int common,onlyA,onlyB,union;
            public float jaccard,symmetricDifferenceFraction;
            public int aOccupied,bOccupied;
        }

        public static Result Compare(
            OpenMorphTissue.Field a,OpenMorphTissue.Field b)
        {
            if(a==null || b==null)throw new ArgumentNullException(
                "Both developmental fields are required.");
            if(a.width!=b.width || a.height!=b.height)
                throw new ArgumentException("Cannot compare unequal grid sizes.");
            var result=new Result();
            for(int i=0;i<a.cells.Length;i++)
            {
                bool inA=a.cells[i].fate!=OpenMorphTissue.Fate.Empty;
                bool inB=b.cells[i].fate!=OpenMorphTissue.Fate.Empty;
                if(inA)result.aOccupied++;
                if(inB)result.bOccupied++;
                if(inA&&inB)result.common++;
                else if(inA)result.onlyA++;
                else if(inB)result.onlyB++;
            }
            result.union=result.common+result.onlyA+result.onlyB;
            if(result.union>0)
            {
                result.jaccard=result.common/(float)result.union;
                result.symmetricDifferenceFraction=
                    (result.onlyA+result.onlyB)/(float)result.union;
            }
            else
            {
                result.jaccard=1f;
                result.symmetricDifferenceFraction=0f;
            }
            return result;
        }
    }
}
