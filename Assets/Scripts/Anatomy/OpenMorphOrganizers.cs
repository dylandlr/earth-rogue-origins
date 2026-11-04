using System;
using UnityEngine;

namespace EarthRogueOrigins.PixelCreatures
{
    /// <summary>
    /// Experimental evolvable developmental organizers. This is a spatial
    /// regulatory field, NOT a list of pre-assigned animal body parts.
    /// Each grown segment expresses local axial, proximal-distal, and
    /// inhibitory signals. No limb/head/tail labels are required.
    /// </summary>
    public static class OpenMorphOrganizers
    {
        public sealed class Field
        {
            public readonly int width,height;
            public readonly float[] axial,distal,activation,inhibition;
            public readonly int[] innovation;
            public Field(int width,int height)
            {
                this.width=width;this.height=height;
                axial=new float[width*height];
                distal=new float[width*height];
                activation=new float[width*height];
                inhibition=new float[width*height];
                innovation=new int[width*height];
            }
        }

        public static Field Build(OpenMorphGenome genome,int width,int height)
        {
            if(genome==null)throw new ArgumentNullException(nameof(genome));
            if(width<8||height<8||width>256||height>256)
                throw new ArgumentOutOfRangeException("Organizers require 8..256 cells per axis.");
            var segments=OpenMorphDevelopment.Grow(genome);
            var field=new Field(width,height);
            Vector2 offset=new Vector2(width/2f,height/2f);
            const float scale=.36f;
            for(int y=0;y<height;y++)
            for(int x=0;x<width;x++)
            {
                int i=y*width+x;
                var point=new Vector2(x,y);
                // Continuous inherited body axis, not a named region.
                field.axial[x+y*width]=width>1?
                    2f*x/(width-1f)-1f:0f;
                float best=0f,bestTip=0f;
                int bestInnovation=0;
                foreach(var segment in segments)
                {
                    Vector2 a=offset+segment.start*scale;
                    Vector2 b=offset+segment.end*scale;
                    Vector2 delta=b-a;
                    float denom=Mathf.Max(.00001f,Vector2.Dot(delta,delta));
                    float t=Mathf.Clamp01(Vector2.Dot(point-a,delta)/denom);
                    Vector2 nearest=a+t*delta;
                    float distance=(point-nearest).magnitude;
                    float radius=1.1f+segment.radius*scale;
                    float along=Mathf.Abs(t-.5f)*2f;
                    // Sharper than the previous 1/(1+d^2) response, which
                    // left distant empty cells permissive to blob growth.
                    float activation=Mathf.Exp(
                        -.5f*distance*distance/(radius*radius));
                    // Tip organizer is a gradient, not a painted appendage.
                    float distal=t*t*activation;
                    if(activation>best)
                    {
                        best=activation;
                        bestTip=distal;
                        bestInnovation=segment.geneInnovation;
                    }
                    // A wide, mild suppressor competes with the organizer.
                    // Broad tissue can thus inhibit nearby lateral buds;
                    // stronger active tips can still continue outward.
                    float shell=Mathf.Exp(-.5f*
                        Mathf.Pow(distance/(radius*2.8f),2f));
                    field.inhibition[i]=Mathf.Max(field.inhibition[i],
                        shell*(.28f+.22f*(1f-segment.rigidity)));
                }
                field.activation[i]=Mathf.Clamp01(best);
                field.distal[i]=Mathf.Clamp01(bestTip);
                field.innovation[i]=bestInnovation;
            }
            return field;
        }
    }
}
