using System;
using UnityEngine;

namespace EarthRogueOrigins.PixelCreatures
{
    /// <summary>
    /// Three-channel, per-cell recurrent gene regulatory network.
    /// All couplings are inherited signed alleles; each step receives
    /// local morphogens, organism organizer cues and earlier expression.
    /// No assigned animal anatomy or training labels are used.
    /// </summary>
    public static class OpenMorphGRN
    {
        public struct State
        {
            public float growth,fate,brake;
        }

        public static State Step(OpenMorphGenome.Gene gene,
            State previous,float signalA,float signalB,
            float tip,float crowding)
        {
            if(gene==null)throw new ArgumentNullException(nameof(gene));
            float a=Mathf.Clamp01(signalA),b=Mathf.Clamp01(signalB);
            float t=Mathf.Clamp01(tip),c=Mathf.Clamp01(crowding);
            // Signed interactions alter the internal gene-expression
            // network over developmental time. Previous state is always
            // read from the preceding tick (synchronous update).
            float growth=gene.growthResponse*.95f+
                previous.growth*.35f+
                gene.fateToGrowth*(previous.fate-.5f)+
                gene.brakeToGrowth*(previous.brake-.5f)+
                .30f*a+.22f*t-.12f*c-.35f;
            float fate=gene.fateResponse*.95f+
                previous.fate*.35f+
                gene.growthToFate*(previous.growth-.5f)+
                gene.brakeToFate*(previous.brake-.5f)+
                .12f*b+.12f*t-.34f;
            float brake=gene.inhibitionResponse*.95f+
                previous.brake*.35f+
                gene.growthToBrake*(previous.growth-.5f)+
                gene.fateToBrake*(previous.fate-.5f)+
                .25f*b+.18f*c-.36f;
            return new State {
                growth=Mathf.Clamp01(growth),
                fate=Mathf.Clamp01(fate),
                brake=Mathf.Clamp01(brake)
            };
        }

        public static OpenMorphTissue.Fate Differentiate(State state)
        {
            float x=Mathf.Clamp01(state.fate);
            if(x<.20f)return OpenMorphTissue.Fate.Storage;
            if(x<.40f)return OpenMorphTissue.Fate.Structural;
            if(x<.60f)return OpenMorphTissue.Fate.Contractile;
            if(x<.80f)return OpenMorphTissue.Fate.Transport;
            return OpenMorphTissue.Fate.Sensory;
        }
    }
}
