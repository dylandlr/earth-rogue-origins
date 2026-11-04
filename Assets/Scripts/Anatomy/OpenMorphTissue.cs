using System;
using UnityEngine;

namespace EarthRogueOrigins.PixelCreatures
{
    /// <summary>
    /// Experimental local tissue rules expressed by a variable-length
    /// morphology genome. A rule can affect every occupied cell in its
    /// spatial/chemical domain: no named head, limb or organ slots.
    /// </summary>
    public static class OpenMorphTissue
    {
        public enum Fate : byte
        {
            Empty=0, Structural=1, Contractile=2, Sensory=3,
            Transport=4, Storage=5
        }

        public struct Cell
        {
            public Fate fate;
            public float signalA,signalB;
            public float energy;
            public int lineage;
        }

        public sealed class Field
        {
            public readonly int width,height;
            public readonly Cell[] cells;
            public int steps;
            public Field(int w,int h)
            {
                if(w<8 || h<8 || w>256 || h>256)
                    throw new ArgumentOutOfRangeException("Tissue grid must fit 8..256.");
                width=w;height=h;cells=new Cell[w*h];
            }
            public Cell At(int x,int y) { return cells[y*width+x]; }
            public int Occupied()
            {
                int count=0;
                foreach(var cell in cells)if(cell.fate!=Fate.Empty)count++;
                return count;
            }
            public int FateCount(Fate target)
            {
                int count=0;
                foreach(var cell in cells)if(cell.fate==target)count++;
                return count;
            }
        }

        /// <summary>
        /// A small deterministic reaction-diffusion-and-growth simulation.
        /// Signals diffuse in all cells; tissue grows only adjacent to
        /// existing tissue and spends a bounded internal energy reserve.
        /// </summary>
        public static Field Grow(OpenMorphGenome genome,
            int width=64,int height=48,int steps=32)
        {
            if(genome==null)throw new ArgumentNullException(nameof(genome));
            if(steps<0 || steps>256)throw new ArgumentOutOfRangeException(nameof(steps));
            genome.Validate();
            var field=new Field(width,height);
            int origin=(height/2)*width+width/2;
            field.cells[origin]=new Cell {
                fate=Fate.Structural,signalA=1f,signalB=.15f,
                energy=1f,lineage=0
            };
            var next=new Cell[field.cells.Length];
            var rng=new System.Random(genome.seed);
            for(int tick=0;tick<steps;tick++)
            {
                for(int y=0;y<height;y++)
                for(int x=0;x<width;x++)
                {
                    int i=y*width+x;
                    var current=field.cells[i];
                    float sumA=0f,sumB=0f;
                    int neighbors=0,parents=0;
                    float bestEnergy=-1f;
                    int lineage=0;
                    int[] dx={-1,1,0,0};
                    int[] dy={0,0,-1,1};
                    for(int k=0;k<4;k++)
                    {
                        int nx=x+dx[k],ny=y+dy[k];
                        if(nx<0 || ny<0 || nx>=width || ny>=height)continue;
                        var other=field.cells[ny*width+nx];
                        sumA+=other.signalA;
                        sumB+=other.signalB;
                        neighbors++;
                        if(other.fate!=Fate.Empty)
                        {
                            parents++;
                            if(other.energy>bestEnergy)
                            {
                                bestEnergy=other.energy;
                                lineage=other.lineage;
                            }
                        }
                    }
                    float a=Mathf.Clamp01(current.signalA+
                        .18f*(sumA-neighbors*current.signalA)-.025f*current.signalA);
                    float b=Mathf.Clamp01(current.signalB+
                        .12f*(sumB-neighbors*current.signalB)-.02f*current.signalB);
                    if(current.fate!=Fate.Empty)
                    {
                        // Occupied cells express signals according to inherited
                        // rules, rather than arbitrary named body parts.
                        float bias=RuleBias(genome,x,y,width,height,a,b);
                        a=Mathf.Clamp01(a+.12f+.10f*bias);
                        b=Mathf.Clamp01(b+.035f+.07f*(1f-bias));
                        next[i]=new Cell {
                            fate=ChooseFate(genome,x,y,width,height,a,b),
                            signalA=a,signalB=b,
                            energy=Mathf.Clamp01(current.energy-.012f+.022f*bias),
                            lineage=current.lineage
                        };
                    }
                    else
                    {
                        // No isolated births: new tissue requires a living
                        // neighbor and an inherited response to local signals.
                        float bias=RuleBias(genome,x,y,width,height,a,b);
                        bool canGrow=parents>0 && bestEnergy>.12f &&
                            bias>.22f && a>.14f;
                        // Deterministic seeded stochasticity permits distinct
                        // protrusions while retaining exact replay by seed.
                        if(canGrow && rng.NextDouble()<.23+.22*bias)
                            next[i]=new Cell {
                                fate=ChooseFate(genome,x,y,width,height,a,b),
                                signalA=Mathf.Clamp01(a+.18f),
                                signalB=b,energy=Mathf.Min(.8f,bestEnergy*.84f),
                                lineage=lineage
                            };
                        else next[i]=new Cell {
                            fate=Fate.Empty,signalA=a,signalB=b
                        };
                    }
                }
                Array.Copy(next,field.cells,next.Length);
                field.steps++;
            }
            return field;
        }

        static float RuleBias(OpenMorphGenome genome,int x,int y,
            int width,int height,float a,float b)
        {
            float nx=2f*x/(width-1f)-1f;
            float ny=2f*y/(height-1f)-1f;
            float response=0f,weight=0f;
            foreach(var gene in genome.genes)
            {
                float angle=gene.angle*Mathf.Deg2Rad;
                float rx=nx-gene.attachX,ry=ny-gene.attachY;
                float along=rx*Mathf.Cos(angle)+ry*Mathf.Sin(angle);
                float cross=-rx*Mathf.Sin(angle)+ry*Mathf.Cos(angle);
                float reach=.12f+gene.length*.8f;
                float thickness=.1f+gene.width*.8f;
                float distance=along*along/(reach*reach)+
                    cross*cross/(thickness*thickness);
                float spatial=1f/(1f+distance);
                float chemistry=Mathf.Clamp01(.35f+gene.bend*a-
                    (gene.rigidity-.5f)*b+.5f*gene.pigment);
                response+=spatial*chemistry*(1f+gene.repeat*.1f);
                weight+=spatial*(1f+gene.repeat*.1f);
            }
            return Mathf.Clamp01(weight>0?response/weight:0f);
        }

        static Fate ChooseFate(OpenMorphGenome genome,int x,int y,
            int width,int height,float a,float b)
        {
            float expression=RuleBias(genome,x,y,width,height,a,b);
            float contrast=a-b;
            if(expression>.68f && contrast>.38f)return Fate.Sensory;
            if(expression>.56f && b>.20f)return Fate.Contractile;
            if(contrast<-.08f)return Fate.Transport;
            if(expression<.30f)return Fate.Storage;
            return Fate.Structural;
        }

        public static Color32[] Render(Field field)
        {
            if(field==null)throw new ArgumentNullException(nameof(field));
            var pixels=new Color32[field.cells.Length];
            for(int i=0;i<pixels.Length;i++)
            {
                switch(field.cells[i].fate)
                {
                    case Fate.Structural:pixels[i]=new Color32(113,138,94,255);break;
                    case Fate.Contractile:pixels[i]=new Color32(194,117,93,255);break;
                    case Fate.Sensory:pixels[i]=new Color32(225,195,121,255);break;
                    case Fate.Transport:pixels[i]=new Color32(94,138,167,255);break;
                    case Fate.Storage:pixels[i]=new Color32(151,119,157,255);break;
                }
            }
            return pixels;
        }
    }
}
