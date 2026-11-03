using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using EarthRogueOrigins.Ecology;
using EarthRogueOrigins.Anatomy;

namespace EarthRogueOrigins.Tests
{
    public sealed class SpeciationTests
    {
        static List<EvolvingCreature> MakeSeparatedPopulation(
            int species=4,bool different=true)
        {
            var population=new List<EvolvingCreature>();
            for(int i=0;i<10;i++)
            {
                bool right=i>=5;
                var c=new EvolvingCreature {
                    id=i+1,speciesId=species,generation=3,
                    position=new Vector2((right?24f:3f)+i%5*.16f,5f),
                    development=new DevelopmentalGenome()
                };
                for(int j=0;j<c.genes.Length;j++)
                    c.genes[j]=right&&different?.88f:.12f;
                for(int j=0;j<DevelopmentalGenome.GeneCount;j++)
                    c.development.regulatory[j]=right&&different?.88f:.12f;
                population.Add(c);
            }
            return population;
        }

        static SpeciationSettings Settings()
        {
            return new SpeciationSettings {
                enabled=true,evaluationInterval=1,
                requiredConsecutiveWindows=4,minimumPerPopulation=4,
                minimumGeneration=2,minimumGeographicGap=3f,
                minimumCentroidSeparation=7f,minimumGeneticDistance=.20f
            };
        }

        [Test]
        public void StableDivergenceAccumulatesObservations()
        {
            var detector=new SpeciationDetector();
            var population=MakeSeparatedPopulation();
            SpeciationEvidence result=null;
            for(int window=1;window<=4;window++)
            {
                result=detector.Observe(4,population,Settings());
                Assert.NotNull(result);
                Assert.AreEqual(window,result.observations);
            }
            Assert.Greater(result.geographicGap,15f);
            Assert.Greater(result.geneticDistance,.5f);
            Assert.AreEqual(5,result.leftCount);
            Assert.AreEqual(5,result.rightCount);
            Assert.AreEqual(5,result.candidateIndividualIds.Length);
        }

        [Test]
        public void NoSplitOnColorOrLocationAlone()
        {
            var detector=new SpeciationDetector();
            Assert.IsNull(detector.Observe(4,MakeSeparatedPopulation(4,false),Settings()));
            var near=MakeSeparatedPopulation();
            for(int i=5;i<near.Count;i++)
                near[i].position=new Vector2(5f+(i-5)*.2f,5f);
            Assert.IsNull(detector.Observe(4,near,Settings()));
        }

        [Test]
        public void NoSplitForTinyOrYoungGroups()
        {
            var detector=new SpeciationDetector();
            var population=MakeSeparatedPopulation();
            population.RemoveAt(9);
            population.RemoveAt(8);
            Assert.IsNull(detector.Observe(4,population,Settings()));
            population=MakeSeparatedPopulation();
            foreach(var c in population)c.generation=1;
            Assert.IsNull(detector.Observe(4,population,Settings()));
        }

        [Test]
        public void InterruptedIsolationResetsStreak()
        {
            var detector=new SpeciationDetector();
            var population=MakeSeparatedPopulation();
            Assert.AreEqual(1,detector.Observe(4,population,Settings()).observations);
            Assert.AreEqual(2,detector.Observe(4,population,Settings()).observations);
            for(int i=5;i<population.Count;i++)
                population[i].position=new Vector2(5f,5f);
            Assert.IsNull(detector.Observe(4,population,Settings()));
            for(int i=5;i<population.Count;i++)
                population[i].position=new Vector2(24f+(i-5)*.16f,5f);
            Assert.AreEqual(1,detector.Observe(4,population,Settings()).observations);
        }

        [Test]
        public void WorldRecordsBranchWithoutDiscardingPedigreesOrRenamingParent()
        {
            var world=new EcosystemWorld(32,12,2718);
            var conf=world.speciationSettings;
            conf.evaluationInterval=1;
            conf.requiredConsecutiveWindows=4;
            conf.minimumPerPopulation=4;
            conf.minimumGeneration=2;
            conf.minimumGeneticDistance=.2f;
            conf.minimumGeographicGap=3f;
            conf.minimumCentroidSeparation=7f;
            var left=new List<EvolvingCreature>();
            var right=new List<EvolvingCreature>();
            for(int i=0;i<10;i++)
            {
                bool rhs=i>=5;
                var c=world.AddFounder(new Vector2(
                    (rhs?24f:3f)+(i%5)*.16f,5f),DietKind.Grazer,14);
                c.generation=3;
                c.energy=55f; // Prevent births changing the test cohort.
                c.motherId=i+100;
                for(int j=0;j<c.genes.Length;j++)
                    c.genes[j]=rhs?.88f:.12f;
                for(int j=0;j<c.development.regulatory.Length;j++)
                    c.development.regulatory[j]=rhs?.88f:.12f;
                if(rhs)right.Add(c);else left.Add(c);
            }
            string original=world.GetSpecies(14).Binomial;
            for(int i=0;i<4;i++)world.Step(.01f);
            Assert.AreEqual(original,world.GetSpecies(14).Binomial);
            int descendant=right[0].speciesId;
            Assert.AreNotEqual(14,descendant);
            var newTaxon=world.GetSpecies(descendant);
            Assert.AreEqual(14,newTaxon.parentSpeciesId);
            Assert.AreEqual(4,newTaxon.foundedTick);
            Assert.Greater(newTaxon.foundingGeneticDistance,.2f);
            Assert.AreNotEqual(original,newTaxon.Binomial);
            foreach(var c in right)
            {
                Assert.AreEqual(descendant,c.speciesId);
                Assert.AreEqual(c.id+99,c.motherId);
            }
            foreach(var c in left)Assert.AreEqual(14,c.speciesId);
        }
    }
}
