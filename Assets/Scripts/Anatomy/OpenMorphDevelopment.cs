using System;
using System.Collections.Generic;
using UnityEngine;

namespace EarthRogueOrigins.PixelCreatures
{
    /// <summary>
    /// Decodes a variable graph into an articulated hierarchy of geometric
    /// material segments, without enumerating named animal body parts.
    /// Identical genomes decode deterministically and may branch, repeat,
    /// mirror, and change their number of segments across generations.
    /// </summary>
    public static class OpenMorphDevelopment
    {
        public struct Segment
        {
            public int geneInnovation,parentIndex,depth;
            public Vector2 start,end;
            public float radius,rigidity,pigment;
        }

        public static Segment[] Grow(OpenMorphGenome genome)
        {
            if(genome==null)throw new ArgumentNullException(nameof(genome));
            genome.Validate();
            var segments=new List<Segment>();
            var instances=new Dictionary<int,List<int>>();
            foreach(var gene in genome.genes)
            {
                var made=new List<int>();
                var parents=new List<int>();
                if(gene.parentInnovation==-1)parents.Add(-1);
                else if(instances.ContainsKey(gene.parentInnovation))
                    parents.AddRange(instances[gene.parentInnovation]);
                foreach(int parentIndex in parents)
                {
                    int depth=parentIndex<0?0:segments[parentIndex].depth+1;
                    if(depth>=OpenMorphGenome.MaxDepth)continue;
                    for(int repeat=0;repeat<gene.repeat;repeat++)
                    {
                        for(int side=0;side<(gene.mirror?2:1);side++)
                        {
                            if(segments.Count>=OpenMorphGenome.MaxDevelopedNodes)
                                break;
                            Vector2 origin=Vector2.zero;
                            float parentAngle=0f;
                            if(parentIndex>=0)
                            {
                                var parent=segments[parentIndex];
                                Vector2 direction=(parent.end-parent.start).normalized;
                                Vector2 normal=new Vector2(-direction.y,direction.x);
                                origin=Vector2.Lerp(parent.start,parent.end,
                                    (gene.attachX+1f)*.5f)+
                                    normal*gene.attachY*parent.radius;
                                parentAngle=Mathf.Atan2(direction.y,direction.x)*
                                    Mathf.Rad2Deg;
                            }
                            float spread=(repeat-(gene.repeat-1)*.5f)*22f;
                            float turn=gene.angle+spread+gene.bend*20f;
                            if(gene.mirror && side==1)turn=-turn;
                            float radians=(parentAngle+turn)*Mathf.Deg2Rad;
                            var direction2=new Vector2(
                                Mathf.Cos(radians),Mathf.Sin(radians));
                            float segmentLength=5f+gene.length*45f;
                            var segment=new Segment {
                                geneInnovation=gene.innovation,
                                parentIndex=parentIndex,
                                depth=depth,
                                start=origin,
                                end=origin+direction2*segmentLength,
                                radius=1.5f+gene.width*13f,
                                rigidity=gene.rigidity,
                                pigment=gene.pigment
                            };
                            made.Add(segments.Count);
                            segments.Add(segment);
                        }
                    }
                }
                instances.Add(gene.innovation,made);
            }
            return segments.ToArray();
        }

        /// <summary>
        /// Fits an organism to the image for visual diagnostics. The physical
        /// segment positions returned by Grow remain unmodified.
        /// </summary>
        public static Segment[] FitToCanvas(Segment[] input,
            int width=128,int height=96)
        {
            if(input==null)throw new ArgumentNullException(nameof(input));
            if(width<8 || height<8)throw new ArgumentOutOfRangeException();
            if(input.Length==0)return new Segment[0];
            float minX=float.PositiveInfinity,minY=float.PositiveInfinity;
            float maxX=float.NegativeInfinity,maxY=float.NegativeInfinity;
            foreach(var s in input)
            {
                minX=Mathf.Min(minX,Mathf.Min(s.start.x,s.end.x)-s.radius);
                maxX=Mathf.Max(maxX,Mathf.Max(s.start.x,s.end.x)+s.radius);
                minY=Mathf.Min(minY,Mathf.Min(s.start.y,s.end.y)-s.radius);
                maxY=Mathf.Max(maxY,Mathf.Max(s.start.y,s.end.y)+s.radius);
            }
            float scale=Mathf.Min((width-12f)/Mathf.Max(1,maxX-minX),
                (height-12f)/Mathf.Max(1,maxY-minY));
            var center=new Vector2((minX+maxX)*.5f,(minY+maxY)*.5f);
            var target=new Vector2(width*.5f,height*.5f);
            var result=new Segment[input.Length];
            for(int i=0;i<input.Length;i++)
            {
                result[i]=input[i];
                result[i].start=(input[i].start-center)*scale+target;
                result[i].end=(input[i].end-center)*scale+target;
                result[i].radius=input[i].radius*scale;
            }
            return result;
        }
    }
}
