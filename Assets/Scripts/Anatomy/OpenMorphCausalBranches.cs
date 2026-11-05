using System;
using System.Collections.Generic;

namespace EarthRogueOrigins.PixelCreatures
{
    /// <summary>
    /// Counterfactual expression test: ablate a non-root developmental
    /// gene and all descendants, rerun development using the SAME seed,
    /// and measure actual changes to occupied tissue.
    /// Counts causal effects, not coincidental spatial overlap.
    /// </summary>
    public static class OpenMorphCausalBranches
    {
        public struct Evidence
        {
            public int innovation,removedGenes;
            public int originalCells,ablatedCells,changedCells;
            public float occupancyJaccard,changedFraction;
            public bool morphologicallyActive;
        }

        public sealed class Report
        {
            public Evidence[] branches;
            public int activeBranches,totalBranches;
            public float greatestChangedFraction;
        }

        public static OpenMorphGenome Ablate(OpenMorphGenome original,
            int innovation)
        {
            if(original==null)throw new ArgumentNullException(nameof(original));
            original.Validate();
            if(innovation==original.genes[0].innovation)
                throw new ArgumentException("Cannot ablate root gene.");
            var clone=original.Copy();
            var removed=new HashSet<int> {innovation};
            bool found=false;
            foreach(var gene in clone.genes)
            {
                if(gene.innovation==innovation)found=true;
                if(removed.Contains(gene.parentInnovation))
                    removed.Add(gene.innovation);
            }
            if(!found)throw new ArgumentException("Unknown innovation.");
            clone.genes.RemoveAll(g=>removed.Contains(g.innovation));
            clone.Validate();
            return clone;
        }

        public static Report Analyze(OpenMorphGenome original,
            OpenMorphTissue.Field developed,
            int steps=36,int cellBudget=360,
            float minimumEffect=.035f)
        {
            if(original==null||developed==null)
                throw new ArgumentNullException("Genome and original tissue required.");
            if(minimumEffect<=0f||minimumEffect>1f)
                throw new ArgumentOutOfRangeException(nameof(minimumEffect));
            var evidence=new List<Evidence>();
            foreach(var gene in original.genes)
            {
                if(gene.parentInnovation<0)continue;
                var ablated=Ablate(original,gene.innovation);
                var tissue=OpenMorphTissue.Grow(
                    ablated,developed.width,developed.height,
                    steps,cellBudget,true,true);
                var pair=OpenMorphPairMetrics.Compare(developed,tissue);
                float effect=pair.symmetricDifferenceFraction;
                var item=new Evidence {
                    innovation=gene.innovation,
                    removedGenes=original.genes.Count-ablated.genes.Count,
                    originalCells=pair.aOccupied,
                    ablatedCells=pair.bOccupied,
                    changedCells=pair.onlyA+pair.onlyB,
                    occupancyJaccard=pair.jaccard,
                    changedFraction=effect,
                    morphologicallyActive=effect>=minimumEffect
                };
                evidence.Add(item);
            }
            var report=new Report {branches=evidence.ToArray()};
            report.totalBranches=evidence.Count;
            foreach(var e in evidence)
            {
                if(e.morphologicallyActive)report.activeBranches++;
                report.greatestChangedFraction=Math.Max(
                    report.greatestChangedFraction,e.changedFraction);
            }
            return report;
        }
    }
}
