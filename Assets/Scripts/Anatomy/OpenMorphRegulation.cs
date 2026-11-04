using System;
using UnityEngine;

namespace EarthRogueOrigins.PixelCreatures
{
    /// <summary>
    /// Evolvable regulatory reaction function. Active organizer identity
    /// selects a heritable instruction; chemical signals, tissue age and
    /// local crowding influence how strongly that instruction is expressed.
    /// No predefined organ types or target silhouettes.
    /// </summary>
    public static class OpenMorphRegulation
    {
        public struct Response
        {
            public float growthMultiplier,tipBoost,inhibition;
        }

        public static Response Evaluate(OpenMorphGenome.Gene gene,
            float signalA,float signalB,float tip,float suppressor,
            float neighborFraction,float age)
        {
            if(gene==null)throw new ArgumentNullException(nameof(gene));
            float activation=Mathf.Clamp01(signalA*.65f+
                signalB*.18f+gene.growthResponse*.55f);
            float maturation=Mathf.Clamp01(age);
            float crowding=Mathf.Clamp01(neighborFraction);
            return new Response {
                growthMultiplier=Mathf.Clamp01(
                    .18f+gene.growthResponse*1.35f*activation*
                    (1f-.35f*maturation)),
                tipBoost=Mathf.Clamp01(
                    tip*gene.tipResponse*(1f-.30f*crowding)),
                inhibition=Mathf.Clamp01(
                    suppressor*gene.inhibitionResponse+
                    .12f*crowding*gene.inhibitionResponse)
            };
        }

        public static OpenMorphGenome.Gene[] BuildLookup(
            OpenMorphGenome genome,OpenMorphOrganizers.Field organizers)
        {
            if(genome==null||organizers==null)
                throw new ArgumentNullException("Regulation requires a genome and organizers.");
            var genes=new System.Collections.Generic.Dictionary<int,OpenMorphGenome.Gene>();
            foreach(var gene in genome.genes)genes.Add(gene.innovation,gene);
            var lookup=new OpenMorphGenome.Gene[organizers.innovation.Length];
            var root=genome.genes[0];
            for(int i=0;i<lookup.Length;i++)
            {
                OpenMorphGenome.Gene match;
                lookup[i]=genes.TryGetValue(organizers.innovation[i],out match)?
                    match:root;
            }
            return lookup;
        }
    }
}
