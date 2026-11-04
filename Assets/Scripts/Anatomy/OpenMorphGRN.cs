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

        // Experimental interventions on the birth decision only.
        // The recurrent expression dynamics and differentiated fates
        // remain identical at the same local state.
        public enum BirthChannel
        {
            Full=0,
            GrowthOnly=1,
            BrakeOnly=2,
            Neutral=3
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

        /// <summary>
        /// Adjusts an already organization-limited cell-division chance.
        /// A neutral network (growth=brake=.5) leaves it unchanged.
        /// This avoids multiplying two separate inhibition systems.
        /// </summary>
        public static float ModulateGrowth(float baseChance,State state)
        {
            return ModulateGrowth(baseChance,state,BirthChannel.Full);
        }

        public static float ModulateGrowth(float baseChance,State state,
            BirthChannel channel)
        {
            float growth=.18f*(Mathf.Clamp01(state.growth)-.5f);
            float brake=-.10f*(Mathf.Clamp01(state.brake)-.5f);
            switch(channel)
            {
                case BirthChannel.Full:break;
                case BirthChannel.GrowthOnly:brake=0f;break;
                case BirthChannel.BrakeOnly:growth=0f;break;
                case BirthChannel.Neutral:growth=0f;brake=0f;break;
                default:throw new ArgumentOutOfRangeException(nameof(channel));
            }
            return Mathf.Clamp01(Mathf.Clamp01(baseChance)+growth+brake);
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
