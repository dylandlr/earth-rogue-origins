using NUnit.Framework;
using UnityEngine;
using EarthRogueOrigins.PixelCreatures;

namespace EarthRogueOrigins.Tests
{
    public sealed class OpenMorphResourceTests
    {
        static OpenMorphTissue.Field Block(int size,
            OpenMorphTissue.Fate fate)
        {
            var field=new OpenMorphTissue.Field(12,12);
            int offset=(12-size)/2;
            for(int y=0;y<size;y++)
            for(int x=0;x<size;x++)
                field.cells[(y+offset)*12+x+offset].fate=fate;
            return field;
        }

        [Test]
        public void EnergyLedgerConservesExternalSupplyAndInternalTransfers()
        {
            var organism=Block(5,OpenMorphTissue.Fate.Storage);
            organism.cells[5*12+5].fate=OpenMorphTissue.Fate.Transport;
            foreach(var climate in new[]{
                OpenMorphResourceTrial.Climate.Scarce,
                OpenMorphResourceTrial.Climate.Moderate,
                OpenMorphResourceTrial.Climate.Rich,
                new OpenMorphResourceTrial.Climate {
                    name="zero",startingPool=0f,replenishment=0f,
                    uptakePerEdge=0f
                }
            })
            {
                var a=OpenMorphResourceTrial.Run(organism,climate,120);
                var b=OpenMorphResourceTrial.Run(organism,climate,120);
                Assert.AreEqual(25,a.initialCells);
                Assert.AreEqual(a.livingCells,b.livingCells);
                Assert.AreEqual(a.harvested,b.harvested);
                Assert.AreEqual(a.consumed,b.consumed);
                Assert.AreEqual(a.remainingEnergy,b.remainingEnergy);
                Assert.AreEqual(a.remainingPool,b.remainingPool);
                Assert.Less(Mathf.Abs(a.conservationError),.02f,
                    "Energy silently created or destroyed.");
                Assert.That(a.livingCells,Is.InRange(0,25));
                Assert.That(a.deaths,Is.InRange(0,25));
                Assert.AreEqual(a.initialCells,
                    a.deaths+a.livingCells);
                foreach(float energy in a.cellEnergy)
                    Assert.GreaterOrEqual(energy,-.00001f);
            }
        }

        [Test]
        public void NoExternalNutrientsEventuallyExhaustTheColony()
        {
            var organism=Block(3,OpenMorphTissue.Fate.Contractile);
            var empty=new OpenMorphResourceTrial.Climate {
                name="no_source",startingPool=0f,
                replenishment=0f,uptakePerEdge=.2f
            };
            var result=OpenMorphResourceTrial.Run(organism,empty,120);
            Assert.AreEqual(0,result.harvested);
            Assert.AreEqual(0,result.livingCells);
            Assert.AreEqual(result.initialCells,result.deaths);
            Assert.Greater(result.unmetMaintenance,0f);
            Assert.Less(Mathf.Abs(result.conservationError),.02f);
        }

        [Test]
        public void RichEnvironmentSustainsSingleSurfaceCell()
        {
            var organism=new OpenMorphTissue.Field(8,8);
            organism.cells[4*8+4].fate=OpenMorphTissue.Fate.Structural;
            var poor=OpenMorphResourceTrial.Run(organism,
                new OpenMorphResourceTrial.Climate {
                    name="empty",startingPool=0f,
                    replenishment=0f,uptakePerEdge=0f
                },100);
            var rich=OpenMorphResourceTrial.Run(organism,
                OpenMorphResourceTrial.Climate.Rich,100);
            Assert.AreEqual(0,poor.livingCells);
            Assert.AreEqual(1,rich.livingCells);
            Assert.Greater(rich.harvested,0f);
            Assert.Less(Mathf.Abs(rich.conservationError),.02f);
        }

        [Test]
        public void TransportAndStorageHaveDistinctResourceProperties()
        {
            Assert.Greater(OpenMorphResourceTrial.Capacity(
                OpenMorphTissue.Fate.Storage),
                OpenMorphResourceTrial.Capacity(
                    OpenMorphTissue.Fate.Structural));
            Assert.Less(OpenMorphResourceTrial.Maintenance(
                OpenMorphTissue.Fate.Storage),
                OpenMorphResourceTrial.Maintenance(
                    OpenMorphTissue.Fate.Contractile));
            var tissue=Block(3,OpenMorphTissue.Fate.Storage);
            var outcome=OpenMorphResourceTrial.Run(
                tissue,OpenMorphResourceTrial.Climate.Moderate,20);
            Assert.AreEqual(9,outcome.initialCells);
            Assert.AreEqual(21,outcome.livingByTick.Length);
            Assert.AreEqual(outcome.livingCells,outcome.livingByTick[20]);
            Assert.AreEqual(outcome.remainingEnergy,outcome.energyByTick[20]);
        }

        [Test]
        public void InvalidResourceInputsAreRejected()
        {
            var tissue=Block(3,OpenMorphTissue.Fate.Structural);
            Assert.Throws<System.ArgumentOutOfRangeException>(()=>
                OpenMorphResourceTrial.Run(tissue,
                    OpenMorphResourceTrial.Climate.Rich,-1));
            Assert.Throws<System.ArgumentOutOfRangeException>(()=>
                OpenMorphResourceTrial.Run(tissue,
                    new OpenMorphResourceTrial.Climate {
                        replenishment=-1f
                    },10));
        }
    }
}
