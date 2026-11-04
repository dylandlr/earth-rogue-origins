using System;
using UnityEngine;

namespace EarthRogueOrigins.PixelCreatures
{
    /// <summary>
    /// Maps the heritable developmental graph to a local morphogen source
    /// field. It is a regulatory cue, not a pre-painted body mask: births
    /// still require neighboring tissue, chemical signals, energy and time.
    /// </summary>
    public static class OpenMorphGrowthGuide
    {
        public struct Field
        {
            public int width,height;
            public float[] activation;
            public int[] innovation;
            public float At(int x,int y) { return activation[y*width+x]; }
        }

        public static Field Build(OpenMorphGenome genome,int width,int height)
        {
            if(genome==null)throw new ArgumentNullException(nameof(genome));
            if(width<8 || height<8 || width>256 || height>256)
                throw new ArgumentOutOfRangeException("Invalid guidance grid.");
            var segments=OpenMorphDevelopment.Grow(genome);
            var guide=new Field {
                width=width,height=height,
                activation=new float[width*height],
                innovation=new int[width*height]
            };
            // The root remains fixed at the tissue seed. Avoid fitting
            // each body to a different scale: that would mask inherited
            // growth in body size and truncate developmental comparisons.
            var offset=new Vector2(width/2f,height/2f);
            const float scale=.36f;
            for(int y=0;y<height;y++)
            for(int x=0;x<width;x++)
            {
                float best=0f;
                int label=0;
                var location=new Vector2(x,y);
                foreach(var segment in segments)
                {
                    Vector2 a=offset+segment.start*scale;
                    Vector2 b=offset+segment.end*scale;
                    var edge=b-a;
                    float lengthSquared=Mathf.Max(.001f,Vector2.Dot(edge,edge));
                    float t=Mathf.Clamp01(Vector2.Dot(location-a,edge)/lengthSquared);
                    var d=location-(a+edge*t);
                    float distanceSquared=Vector2.Dot(d,d);
                    float radius=1.5f+segment.radius*scale;
                    // Concentration profile falls gradually toward the
                    // edges, encouraging growth to follow derived branches.
                    float strength=1f/(1f+distanceSquared/(radius*radius));
                    strength*=Mathf.Lerp(.78f,1f,segment.rigidity);
                    if(strength>best)
                    {
                        best=strength;
                        label=segment.geneInnovation;
                    }
                }
                int i=y*width+x;
                guide.activation[i]=best;
                guide.innovation[i]=label;
            }
            return guide;
        }
    }
}
