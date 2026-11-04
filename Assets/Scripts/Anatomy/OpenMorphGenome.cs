using System;
using System.Collections.Generic;
using UnityEngine;

namespace EarthRogueOrigins.PixelCreatures
{
    /// <summary>
    /// Variable-length, topology-evolvable developmental genome. Components
    /// have no semantic limb/head/torso tags. Their role is an emergent
    /// consequence of spatial placement, shape, articulation and control.
    /// A node can spawn arbitrary other nodes, not merely animal templates.
    /// </summary>
    [Serializable]
    public sealed class OpenMorphGenome
    {
        public const int MaxGenes=96;
        public const int MaxDevelopedNodes=256;
        public const int MaxDepth=12;

        [Serializable]
        public sealed class Gene
        {
            public int innovation;
            public int parentInnovation; // -1 is the developmental origin
            public int repeat=1;
            public bool mirror;
            public float attachX,attachY; // normalized parent coordinates
            public float angle;
            public float length=.2f,width=.15f;
            public float bend,rigidity=.5f;
            public float pigment=.5f;
            // Evolvable gene regulation, not named anatomical structures.
            public float growthResponse=.6f;
            public float tipResponse=.55f;
            public float inhibitionResponse=.45f;
            // Fate response is separate from cell division and geometry.
            public float fateResponse=.5f;
            public Gene Copy() { return (Gene)MemberwiseClone(); }
        }

        public readonly List<Gene> genes=new List<Gene>();
        public int seed;

        public static OpenMorphGenome Founder(int seed)
        {
            var genome=new OpenMorphGenome {seed=seed};
            genome.genes.Add(new Gene {
                innovation=0,parentInnovation=-1,length=.42f,width=.26f,
                rigidity=.75f
            });
            return genome;
        }

        public OpenMorphGenome Copy()
        {
            var clone=new OpenMorphGenome {seed=seed};
            foreach(var gene in genes)clone.genes.Add(gene.Copy());
            return clone;
        }

        public void Validate()
        {
            if(genes.Count<1 || genes.Count>MaxGenes)
                throw new InvalidOperationException("Invalid gene count.");
            var seen=new HashSet<int>();
            for(int i=0;i<genes.Count;i++)
            {
                var g=genes[i];
                if(g==null || g.innovation<0 || !seen.Add(g.innovation))
                    throw new InvalidOperationException("Invalid gene innovation.");
                if(i==0)
                {
                    if(g.parentInnovation!=-1)
                        throw new InvalidOperationException("First gene must be root.");
                }
                else if(!seen.Contains(g.parentInnovation))
                    throw new InvalidOperationException(
                        "Parent must occur earlier in developmental order.");
                if(g.repeat<1 || g.repeat>5 ||
                   !FiniteRange(g.attachX,-1f,1f) ||
                   !FiniteRange(g.attachY,-1f,1f) ||
                   !FiniteRange(g.angle,-180f,180f) ||
                   !FiniteRange(g.length,.02f,1f) ||
                   !FiniteRange(g.width,.02f,1f) ||
                   !FiniteRange(g.bend,-1f,1f) ||
                   !FiniteRange(g.rigidity,0f,1f) ||
                   !FiniteRange(g.pigment,0f,1f) ||
                   !FiniteRange(g.growthResponse,0f,1f) ||
                   !FiniteRange(g.tipResponse,0f,1f) ||
                   !FiniteRange(g.inhibitionResponse,0f,1f) ||
                   !FiniteRange(g.fateResponse,0f,1f))
                    throw new InvalidOperationException("Invalid developmental parameter.");
            }
        }

        static bool FiniteRange(float n,float a,float b)
        {
            return !float.IsNaN(n) && !float.IsInfinity(n) && n>=a && n<=b;
        }

        /// <summary>
        /// Reproducible structural and parameter mutations. Deletion removes
        /// the selected gene's descendants, keeping parent links well formed.
        /// </summary>
        public OpenMorphGenome Mutate(int randomSeed)
        {
            Validate();
            var rng=new System.Random(randomSeed);
            var result=Copy();
            result.seed=randomSeed;
            int operation=rng.Next(5);
            if(operation==0 && result.genes.Count<MaxGenes)
            {
                var parent=result.genes[rng.Next(result.genes.Count)];
                var child=parent.Copy();
                int max=0;
                foreach(var gene in result.genes)max=Math.Max(max,gene.innovation);
                child.innovation=max+1;
                child.parentInnovation=parent.innovation;
                child.repeat=1;
                child.mirror=rng.Next(2)==0;
                child.attachX=(float)(rng.NextDouble()*1.6-.8);
                child.attachY=(float)(rng.NextDouble()*1.6-.8);
                child.angle=(float)(rng.NextDouble()*140-70);
                child.length=Mathf.Clamp(parent.length*.65f,.02f,1f);
                child.width=Mathf.Clamp(parent.width*.65f,.02f,1f);
                result.genes.Add(child);
            }
            else if(operation==1 && result.genes.Count>1)
            {
                int chosen=result.genes[rng.Next(1,result.genes.Count)].innovation;
                var removed=new HashSet<int> {chosen};
                // Parent-before-child ordering makes a single forward pass
                // enough to remove a complete branch.
                foreach(var g in result.genes)
                    if(removed.Contains(g.parentInnovation))
                        removed.Add(g.innovation);
                result.genes.RemoveAll(g=>removed.Contains(g.innovation));
            }
            else if(operation==2)
            {
                var g=result.genes[rng.Next(result.genes.Count)];
                g.length=Mathf.Clamp(g.length+
                    (float)(rng.NextDouble()-.5)*.18f,.02f,1f);
                g.width=Mathf.Clamp(g.width+
                    (float)(rng.NextDouble()-.5)*.18f,.02f,1f);
                g.angle=Mathf.Clamp(g.angle+
                    (float)(rng.NextDouble()-.5)*45f,-180f,180f);
                g.pigment=Mathf.Clamp01(g.pigment+
                    (float)(rng.NextDouble()-.5)*.22f);
                g.growthResponse=Mathf.Clamp01(g.growthResponse+
                    (float)(rng.NextDouble()-.5)*.40f);
                g.tipResponse=Mathf.Clamp01(g.tipResponse+
                    (float)(rng.NextDouble()-.5)*.40f);
                g.inhibitionResponse=Mathf.Clamp01(g.inhibitionResponse+
                    (float)(rng.NextDouble()-.5)*.40f);
                g.fateResponse=Mathf.Clamp01(g.fateResponse+
                    (float)(rng.NextDouble()-.5)*.40f);
            }
            else if(operation==3 && result.genes.Count>1)
            {
                // Structural reconnection only chooses earlier genes, so
                // mutations cannot introduce a cyclic growth graph.
                int childIndex=rng.Next(1,result.genes.Count);
                int parentIndex=rng.Next(childIndex);
                var child=result.genes[childIndex];
                child.parentInnovation=result.genes[parentIndex].innovation;
                child.attachX=(float)(rng.NextDouble()*1.8-.9);
                child.attachY=(float)(rng.NextDouble()*1.8-.9);
            }
            else
            {
                var g=result.genes[rng.Next(result.genes.Count)];
                g.repeat=1+rng.Next(3);
                g.mirror=rng.Next(2)==0;
                g.bend=Mathf.Clamp(g.bend+
                    (float)(rng.NextDouble()-.5),-1f,1f);
            }
            result.Validate();
            return result;
        }
    }
}
