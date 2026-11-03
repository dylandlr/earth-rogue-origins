using System;
using NUnit.Framework;
using UnityEngine;
using EarthRogueOrigins.Ecology;

namespace EarthRogueOrigins.Tests
{
    public sealed class EcosystemWorldTests
    {
        [Test]
        public void WorldGenerationIsDeterministic()
        {
            var a=new EcosystemWorld(18,14,2026);
            var b=new EcosystemWorld(18,14,2026);
            for(int i=0;i<a.cells.Length;i++)
            {
                Assert.AreEqual(a.cells[i].terrain,b.cells[i].terrain);
                Assert.AreEqual(a.cells[i].plants,b.cells[i].plants);
            }
        }

        [Test]
        public void TickIsDeterministicForSameSeed()
        {
            var a=SeedWorld(43);
            var b=SeedWorld(43);
            for(int i=0;i<120;i++){a.Step();b.Step();}
            var x=a.Snapshot();var y=b.Snapshot();
            Assert.AreEqual(x.population,y.population);
            Assert.AreEqual(x.births,y.births);
            Assert.AreEqual(x.deaths,y.deaths);
            Assert.AreEqual(x.totalPlants,y.totalPlants);
            Assert.AreEqual(x.meanArmor,y.meanArmor);
        }

        [Test]
        public void ResourceStocksRemainNonNegative()
        {
            var w=SeedWorld(81);
            for(int i=0;i<350;i++)w.Step();
            foreach(var tile in w.cells)
            {
                Assert.GreaterOrEqual(tile.plants,0f);
                Assert.GreaterOrEqual(tile.detritus,0f);
                Assert.GreaterOrEqual(tile.soilNutrients,0f);
            }
        }

        [Test]
        public void SnapshotReportsLivingOrganisms()
        {
            var w=new EcosystemWorld(10,10,7);
            w.AddFounder(new Vector2(2,2),DietKind.Grazer,1);
            w.AddFounder(new Vector2(6,6),DietKind.Predator,2);
            var s=w.Snapshot();
            Assert.AreEqual(2,s.population);
            Assert.AreEqual(1,s.predatorCount);
            Assert.AreEqual(0,s.births);
        }

        [Test]
        public void InvalidTimestepsAreRejected()
        {
            var w=new EcosystemWorld(10,10,7);
            Assert.Throws<ArgumentOutOfRangeException>(()=>w.Step(0));
            Assert.Throws<ArgumentOutOfRangeException>(()=>w.Step(10));
        }

        static EcosystemWorld SeedWorld(int seed)
        {
            var world=new EcosystemWorld(24,16,seed);
            var random=new System.Random(seed);
            for(int i=0;i<38;i++)
            {
                var diet=i%9==0?DietKind.Predator:i%7==0?DietKind.Detritivore: DietKind.Grazer;
                var species=diet==DietKind.Predator?2:diet==DietKind.Detritivore?3:1;
                world.AddFounder(new Vector2(random.Next(2,22),random.Next(2,14)),diet,species);
            }
            return world;
        }
    }
}
