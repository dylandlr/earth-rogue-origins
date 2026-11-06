using NUnit.Framework;
using System;
using EarthRogueOrigins.PixelCreatures;

namespace EarthRogueOrigins.Tests
{
    public sealed class OpenMorphMaterialLedgerTests
    {
        [Test]
        public void UptakeGrowthAndMaintenanceTransferButDoNotCreateMaterial()
        {
            var m=new OpenMorphMaterialLedger(4,4);
            m.Seed(1,1,10,0,1);
            double before=m.TotalMaterial();
            var r=m.Step(1,1,6,2,3);
            Assert.AreEqual(6d,r.uptake,1e-12);
            Assert.AreEqual(2d,r.maintenance,1e-12);
            Assert.AreEqual(3d,r.growth,1e-12);
            int i=m.Index(1,1);
            Assert.AreEqual(4d,m.nutrient[i],1e-12);
            Assert.AreEqual(1d,m.reserve[i],1e-12);
            Assert.AreEqual(4d,m.biomass[i],1e-12);
            Assert.AreEqual(2d,m.waste[i],1e-12);
            Assert.AreEqual(before,m.TotalMaterial(),1e-12);
        }

        [Test]
        public void EmptyEnvironmentCannotFundGrowth()
        {
            var m=new OpenMorphMaterialLedger(4,4);
            var r=m.Step(1,1,100,100,100);
            Assert.AreEqual(0d,r.uptake);
            Assert.AreEqual(0d,r.growth);
            Assert.AreEqual(0d,r.maintenance);
            Assert.AreEqual(0d,m.TotalMaterial());
        }

        [Test]
        public void DiffusionConservesMaterialAndRemainsNonnegative()
        {
            var m=new OpenMorphMaterialLedger(9,9);
            m.Seed(4,4,100);
            double before=m.TotalMaterial();
            for(int step=0;step<250;step++)m.Diffuse(1,0.2);
            Assert.AreEqual(before,m.TotalMaterial(),1e-8);
            foreach(double n in m.nutrient)
                Assert.That(n,Is.GreaterThanOrEqualTo(0d));
            Assert.Less(m.nutrient[m.Index(4,4)],100d);
            Assert.Greater(m.nutrient[m.Index(0,0)],0d);
        }

        [Test]
        public void RunsAreDeterministicGivenIdenticalRequests()
        {
            var a=new OpenMorphMaterialLedger(8,6);
            var b=new OpenMorphMaterialLedger(8,6);
            foreach(var m in new[]{a,b})
            {
                m.Seed(2,3,24,3,1);
                for(int step=0;step<20;step++)
                {
                    m.Diffuse(.5,.25);
                    m.Step(2,3,1,.12,.3);
                }
            }
            for(int i=0;i<a.nutrient.Length;i++)
            {
                Assert.AreEqual(a.nutrient[i],b.nutrient[i]);
                Assert.AreEqual(a.reserve[i],b.reserve[i]);
                Assert.AreEqual(a.biomass[i],b.biomass[i]);
                Assert.AreEqual(a.waste[i],b.waste[i]);
            }
        }

        [Test]
        public void RejectsUnsafeDiffusionStepAndInvalidRequests()
        {
            var m=new OpenMorphMaterialLedger(4,4);
            Assert.Throws<ArgumentOutOfRangeException>(()=>m.Diffuse(1,0.26));
            Assert.Throws<ArgumentOutOfRangeException>(()=>m.Step(0,0,-1,0,0));
            Assert.Throws<ArgumentOutOfRangeException>(()=>m.Seed(0,0,double.NaN));
        }
    }
}
