using System;
using UnityEngine;

namespace EarthRogueOrigins.PixelCreatures
{
    /// <summary>
    /// Experimental post-development nutrient maintenance on the actual
    /// grown tissue mask. External nutrient is finite; internal transfers
    /// move existing energy and maintenance destroys usable energy.
    /// Does NOT model growth, photosynthesis, reproduction or selection.
    /// </summary>
    public static class OpenMorphResourceTrial
    {
        public struct Climate
        {
            public string name;
            public float startingPool,replenishment,uptakePerEdge;
            public static Climate Scarce { get { return new Climate {
                name="scarce",startingPool=6f,replenishment=.25f,
                uptakePerEdge=.09f
            }; } }
            public static Climate Moderate { get { return new Climate {
                name="moderate",startingPool=18f,replenishment=1.2f,
                uptakePerEdge=.16f
            }; } }
            public static Climate Rich { get { return new Climate {
                name="rich",startingPool=50f,replenishment=4.5f,
                uptakePerEdge=.25f
            }; } }
        }

        public sealed class Outcome
        {
            public int width,height,initialCells,livingCells,deaths,steps;
            public float initialEnergy,externalAdded,harvested,consumed;
            public float discarded,remainingEnergy,remainingPool;
            public float unmetMaintenance;
            public float[] cellEnergy;
            public bool[] living;
            public int[] livingByTick;
            public float[] energyByTick,poolByTick;
            public float conservationError;
        }

        public static float Capacity(OpenMorphTissue.Fate fate)
        {
            return fate==OpenMorphTissue.Fate.Storage?2f:1f;
        }

        public static float Maintenance(OpenMorphTissue.Fate fate)
        {
            switch(fate)
            {
                case OpenMorphTissue.Fate.Contractile:return .025f;
                case OpenMorphTissue.Fate.Sensory:return .023f;
                case OpenMorphTissue.Fate.Transport:return .018f;
                case OpenMorphTissue.Fate.Storage:return .012f;
                default:return .016f;
            }
        }

        public static Outcome Run(OpenMorphTissue.Field tissue,
            Climate climate,int steps=120)
        {
            if(tissue==null)throw new ArgumentNullException(nameof(tissue));
            if(steps<0||steps>1000)
                throw new ArgumentOutOfRangeException(nameof(steps));
            if(!Valid(climate.startingPool)||
                !Valid(climate.replenishment)||
                !Valid(climate.uptakePerEdge))
                throw new ArgumentOutOfRangeException(nameof(climate));
            int w=tissue.width,h=tissue.height,n=w*h;
            var state=new Outcome {
                width=w,height=h,steps=steps,
                cellEnergy=new float[n],
                living=new bool[n],
                livingByTick=new int[steps+1],
                energyByTick=new float[steps+1],
                poolByTick=new float[steps+1],
                remainingPool=climate.startingPool
            };
            var deficits=new int[n];
            for(int i=0;i<n;i++)
            {
                if(tissue.cells[i].fate==OpenMorphTissue.Fate.Empty)continue;
                state.living[i]=true;
                state.cellEnergy[i]=.45f*Capacity(tissue.cells[i].fate);
                state.initialEnergy+=state.cellEnergy[i];
                state.initialCells++;
            }
            state.livingCells=state.initialCells;
            state.livingByTick[0]=state.initialCells;
            state.energyByTick[0]=state.initialEnergy;
            state.poolByTick[0]=state.remainingPool;

            var demand=new float[n];
            for(int tick=0;tick<steps;tick++)
            {
                // External supply is the only new usable nutrient.
                state.remainingPool+=climate.replenishment;
                state.externalAdded+=climate.replenishment;
                float totalDemand=0f;
                for(int i=0;i<n;i++)
                {
                    demand[i]=0f;
                    if(!state.living[i])continue;
                    int x=i%w,y=i/w;
                    int edges=0;
                    if(x==0 || !state.living[i-1])edges++;
                    if(x==w-1 || !state.living[i+1])edges++;
                    if(y==0 || !state.living[i-w])edges++;
                    if(y==h-1 || !state.living[i+w])edges++;
                    float capacity=Capacity(tissue.cells[i].fate);
                    demand[i]=Mathf.Min(
                        Mathf.Max(0f,capacity-state.cellEnergy[i]),
                        edges*climate.uptakePerEdge);
                    totalDemand+=demand[i];
                }
                // Proportional allocation makes uptake independent of
                // scan direction when an environmental pool is depleted.
                float fraction=totalDemand>0f?
                    Mathf.Min(1f,state.remainingPool/totalDemand):0f;
                for(int i=0;i<n;i++)
                {
                    if(demand[i]<=0f)continue;
                    float intake=demand[i]*fraction;
                    state.cellEnergy[i]+=intake;
                    state.harvested+=intake;
                }
                state.remainingPool=Mathf.Max(0f,
                    state.remainingPool-totalDemand*fraction);

                // Conservative neighbor transfers. Both endpoints are
                // updated together; no energy is created or destroyed.
                // Transport cells permit greater local exchange.
                for(int y=0;y<h;y++)
                for(int x=0;x<w;x++)
                {
                    int i=y*w+x;
                    if(!state.living[i])continue;
                    if(x+1<w && state.living[i+1])
                        Exchange(i,i+1,tissue,state.cellEnergy);
                    if(y+1<h && state.living[i+w])
                        Exchange(i,i+w,tissue,state.cellEnergy);
                }

                // Fixed candidate tissue fates now have measurable,
                // differing upkeep. Three consecutive deficient ticks
                // eliminate a cell; remaining energy is discarded as
                // nonrecoverable waste, not silently deleted.
                for(int i=0;i<n;i++)
                {
                    if(!state.living[i])continue;
                    float cost=Maintenance(tissue.cells[i].fate);
                    float paid=Mathf.Min(cost,state.cellEnergy[i]);
                    state.cellEnergy[i]-=paid;
                    state.consumed+=paid;
                    if(paid+1e-6f<cost)
                    {
                        state.unmetMaintenance+=cost-paid;
                        deficits[i]++;
                    }
                    else deficits[i]=0;
                    if(deficits[i]<3)continue;
                    state.living[i]=false;
                    state.livingCells--;
                    state.deaths++;
                    state.discarded+=state.cellEnergy[i];
                    state.cellEnergy[i]=0f;
                }
                float sum=0f;
                foreach(var energy in state.cellEnergy)sum+=energy;
                state.energyByTick[tick+1]=sum;
                state.livingByTick[tick+1]=state.livingCells;
                state.poolByTick[tick+1]=state.remainingPool;
            }
            state.remainingEnergy=state.energyByTick[steps];
            state.conservationError=(state.initialEnergy+
                climate.startingPool+state.externalAdded)-
                (state.consumed+state.discarded+
                state.remainingEnergy+state.remainingPool);
            return state;
        }

        static bool Valid(float value)
        {
            return !float.IsNaN(value)&&!float.IsInfinity(value)&&value>=0f;
        }

        static void Exchange(int a,int b,OpenMorphTissue.Field field,
            float[] energy)
        {
            float difference=energy[a]-energy[b];
            if(Mathf.Abs(difference)<.000001f)return;
            bool aDonor=difference>0f;
            int source=aDonor?a:b,dest=aDonor?b:a;
            var sourceFate=field.cells[source].fate;
            var destFate=field.cells[dest].fate;
            float rate=(sourceFate==OpenMorphTissue.Fate.Transport ||
                destFate==OpenMorphTissue.Fate.Transport)? .22f:.07f;
            float amount=Mathf.Min(
                Mathf.Abs(difference)*rate,
                energy[source],
                Mathf.Max(0f,Capacity(destFate)-energy[dest]));
            energy[source]-=amount;
            energy[dest]+=amount;
        }
    }
}
