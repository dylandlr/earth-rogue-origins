using System;
using System.Collections.Generic;

namespace EarthRogueOrigins.PixelCreatures
{
    /// <summary>
    /// Measures which inherited regulatory innovations actually occupy
    /// developed tissue. A painted segment or a genome entry alone does
    /// not count as an expressed anatomical branch.
    /// </summary>
    public static class OpenMorphBranchExpression
    {
        public struct GeneEvidence
        {
            public int innovation;
            public int guidedCells;
            public int occupiedCells;
            public float expressionFraction;
            public bool expressed;
        }

        public sealed class Report
        {
            public GeneEvidence[] genes;
            public int nonRootGenes,expressedNonRootGenes;
            public int unusedGuidanceCells;
            public float nonRootExpressionFraction;
        }

        public static Report Analyze(OpenMorphGenome genome,
            OpenMorphTissue.Field tissue,
            float threshold=.25f,int minimumOccupied=4,
            float minimumFraction=.10f)
        {
            if(genome==null || tissue==null)
                throw new ArgumentNullException("Genome and tissue required.");
            if(threshold<=0f || threshold>=1f ||
                minimumOccupied<1 || minimumFraction<0f ||
                minimumFraction>1f)
                throw new ArgumentOutOfRangeException("Invalid expression gates.");
            var organizer=OpenMorphOrganizers.Build(
                genome,tissue.width,tissue.height);
            var counts=new Dictionary<int,int>();
            var observed=new Dictionary<int,int>();
            foreach(var gene in genome.genes)
            {
                counts[gene.innovation]=0;
                observed[gene.innovation]=0;
            }
            var result=new Report();
            for(int i=0;i<tissue.cells.Length;i++)
            {
                if(organizer.activation[i]<threshold)continue;
                int label=organizer.innovation[i];
                if(!counts.ContainsKey(label))continue;
                counts[label]++;
                if(tissue.cells[i].fate!=OpenMorphTissue.Fate.Empty)
                    observed[label]++;
                else result.unusedGuidanceCells++;
            }
            var evidence=new List<GeneEvidence>();
            foreach(var gene in genome.genes)
            {
                int guided=counts[gene.innovation];
                int occupied=observed[gene.innovation];
                float fraction=guided==0?0f:occupied/(float)guided;
                bool expressed=occupied>=minimumOccupied &&
                    fraction>=minimumFraction;
                evidence.Add(new GeneEvidence {
                    innovation=gene.innovation,
                    guidedCells=guided,
                    occupiedCells=occupied,
                    expressionFraction=fraction,
                    expressed=expressed
                });
                if(gene.parentInnovation<0)continue;
                result.nonRootGenes++;
                if(expressed)result.expressedNonRootGenes++;
            }
            result.genes=evidence.ToArray();
            result.nonRootExpressionFraction=result.nonRootGenes==0?0f:
                result.expressedNonRootGenes/(float)result.nonRootGenes;
            return result;
        }
    }
}
