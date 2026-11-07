using System;

namespace EarthRogueOrigins.PixelCreatures
{
    /// <summary>
    /// Deterministic, no-flux 2D material-accounting assay.
    /// Units are arbitrary but consistent mass equivalents.
    /// This is NOT a metabolism or thermodynamic energy model.
    /// This standalone assay does not modify OpenMorphTissue.Grow.
    /// </summary>
    public sealed class OpenMorphMaterialLedger
    {
        public struct TransferReceipt
        {
            public double uptake, maintenance, growth;
            public double totalBefore, totalAfter;
        }

        public readonly int width, height;
        // Each array has one slot per lattice site.
        public readonly double[] nutrient;
        public readonly double[] reserve;
        public readonly double[] biomass;
        public readonly double[] waste;

        public OpenMorphMaterialLedger(int width, int height)
        {
            if(width<2 || height<2 || width>256 || height>256)
                throw new ArgumentOutOfRangeException(nameof(width));
            this.width=width;
            this.height=height;
            int n=checked(width*height);
            nutrient=new double[n];
            reserve=new double[n];
            biomass=new double[n];
            waste=new double[n];
        }

        public int Index(int x,int y)
        {
            if(x<0 || y<0 || x>=width || y>=height)
                throw new ArgumentOutOfRangeException(nameof(x));
            return y*width+x;
        }

        static void CheckNonnegative(double value, string name)
        {
            if(double.IsNaN(value) || double.IsInfinity(value) || value<0)
                throw new ArgumentOutOfRangeException(name);
        }

        public void Seed(int x,int y,double freeNutrient,
            double cellReserve=0,double cellBiomass=0)
        {
            CheckNonnegative(freeNutrient,nameof(freeNutrient));
            CheckNonnegative(cellReserve,nameof(cellReserve));
            CheckNonnegative(cellBiomass,nameof(cellBiomass));
            int index=Index(x,y);
            nutrient[index]+=freeNutrient;
            reserve[index]+=cellReserve;
            biomass[index]+=cellBiomass;
        }

        /// <summary>Funds an adjacent daughter from parent reserve. Positive biomass marks occupancy in this assay.</summary>
        public bool TryDivide(int x, int y, int daughterX, int daughterY,
            double daughterBiomass, double divisionWaste)
        {
            CheckNonnegative(daughterBiomass, nameof(daughterBiomass));
            CheckNonnegative(divisionWaste, nameof(divisionWaste));
            if (daughterBiomass == 0)
                throw new ArgumentOutOfRangeException(nameof(daughterBiomass));

            int parent = Index(x, y);
            int daughter = Index(daughterX, daughterY);
            if (Math.Abs(x - daughterX) + Math.Abs(y - daughterY) != 1)
                return false;

            double cost = daughterBiomass + divisionWaste;
            if (biomass[parent] <= 0 || biomass[daughter] != 0 ||
                reserve[daughter] != 0 || reserve[parent] < cost)
                return false;

            reserve[parent] -= cost;
            biomass[daughter] = daughterBiomass;
            waste[parent] += divisionWaste;
            return true;
        }

        public double TotalMaterial()
        {
            double total=0;
            for(int i=0;i<nutrient.Length;i++)
                total+=nutrient[i]+reserve[i]+biomass[i]+waste[i];
            return total;
        }

        /// <summary>
        /// Explicit 5-point diffusion; reflecting/no-flux boundaries.
        /// Enforces the 2D positivity stability condition D*dt/dx^2 <= 1/4.
        /// No cells are created or destroyed and total nutrient is conserved.
        /// </summary>
        public void Diffuse(double diffusion,double dt,double dx=1)
        {
            CheckNonnegative(diffusion,nameof(diffusion));
            CheckNonnegative(dt,nameof(dt));
            if(double.IsNaN(dx) || double.IsInfinity(dx) || dx<=0)
                throw new ArgumentOutOfRangeException(nameof(dx));
            double alpha=diffusion*dt/(dx*dx);
            if(double.IsNaN(alpha) || double.IsInfinity(alpha) || alpha>0.25)
                throw new ArgumentOutOfRangeException(nameof(dt),
                    "Explicit 2D diffusion requires D*dt/dx^2 <= 1/4.");
            var delta=new double[nutrient.Length];
            for(int y=0;y<height;y++)
            for(int x=0;x<width;x++)
            {
                int i=y*width+x;
                if(x+1<width)
                {
                    int j=i+1;
                    double flux=alpha*(nutrient[i]-nutrient[j]);
                    delta[i]-=flux;
                    delta[j]+=flux;
                }
                if(y+1<height)
                {
                    int j=i+width;
                    double flux=alpha*(nutrient[i]-nutrient[j]);
                    delta[i]-=flux;
                    delta[j]+=flux;
                }
            }
            for(int i=0;i<nutrient.Length;i++)
            {
                nutrient[i]+=delta[i];
                // Tolerate floating-point roundoff only.
                if(nutrient[i]<-1e-10)
                    throw new InvalidOperationException("Negative nutrient after stable diffusion.");
                if(nutrient[i]<0)nutrient[i]=0;
            }
        }

        /// <summary>
        /// Requests are material amounts per step. Uptake moves nutrient to
        /// reserve; maintenance moves reserve to waste; growth moves reserve
        /// to biomass. All three are limited by available material.
        /// No "free" GRN/organizer energy injections occur in this assay.
        /// </summary>
        public TransferReceipt Step(int x,int y,
            double requestedUptake,double requestedMaintenance,
            double requestedGrowth)
        {
            CheckNonnegative(requestedUptake,nameof(requestedUptake));
            CheckNonnegative(requestedMaintenance,nameof(requestedMaintenance));
            CheckNonnegative(requestedGrowth,nameof(requestedGrowth));
            int i=Index(x,y);
            double before=TotalMaterial();
            double uptake=Math.Min(requestedUptake,nutrient[i]);
            nutrient[i]-=uptake; reserve[i]+=uptake;
            double maintenance=Math.Min(requestedMaintenance,reserve[i]);
            reserve[i]-=maintenance; waste[i]+=maintenance;
            double growth=Math.Min(requestedGrowth,reserve[i]);
            reserve[i]-=growth; biomass[i]+=growth;
            return new TransferReceipt {
                uptake=uptake,maintenance=maintenance,growth=growth,
                totalBefore=before,totalAfter=TotalMaterial()
            };
        }
    }
}
