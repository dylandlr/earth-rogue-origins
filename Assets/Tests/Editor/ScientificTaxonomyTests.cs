using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEngine;
using EarthRogueOrigins.Ecology;
using EarthRogueOrigins.PixelCreatures;

namespace EarthRogueOrigins.Tests
{
    public sealed class ScientificTaxonomyTests
    {
        [Test]
        public void DescriptiveBinomialsRespondToFoundingMorphology()
        {
            var broad=PixelArtDirection.Hero(CreatureArchetype.MarshCrawler,17);
            var armored=PixelArtDirection.Hero(CreatureArchetype.ArmoredCrawler,17);
            var aquatic=PixelArtDirection.Hero(CreatureArchetype.PaddleFish,17);
            var a=ScientificNameGenerator.Describe(broad,1);
            var b=ScientificNameGenerator.Describe(armored,2);
            var c=ScientificNameGenerator.Describe(aquatic,3);
            Assert.AreNotEqual(a.genus,b.genus);
            Assert.AreNotEqual(a.genus,c.genus);
            Assert.IsTrue(b.diagnosticTraits.Contains("armor"));
            Assert.IsTrue(c.diagnosticTraits.Contains("fin"));
            foreach(var t in new[]{a,b,c})
                Assert.IsTrue(Regex.IsMatch(t.Binomial,@"^[A-Z][a-z]+ [a-z]+$"),
                    "Invalid binomial form: "+t.Binomial);
        }

        [Test]
        public void UnrelatedSpeciesWithSameTraitsReceiveUniqueNames()
        {
            var recipe=PixelArtDirection.Hero(CreatureArchetype.MarshCrawler,2046);
            var first=ScientificNameGenerator.Describe(recipe,10);
            var collision=ScientificNameGenerator.Distinguish(
                ScientificNameGenerator.Describe(recipe,11),11);
            Assert.AreNotEqual(first.Binomial,collision.Binomial);
            Assert.IsTrue(collision.epithet.StartsWith(first.epithet,
                StringComparison.Ordinal));
            Assert.IsTrue(Regex.IsMatch(collision.Binomial,@"^[A-Z][a-z]+ [a-z]+$"));
            Assert.AreEqual(collision.Binomial,
                ScientificNameGenerator.Distinguish(
                    ScientificNameGenerator.Describe(recipe,11),11).Binomial);
        }

        [Test]
        public void FirstFounderAssignsTaxonOnceForAllDescendantsAndIndividuals()
        {
            var world=new EcosystemWorld(16,12,9021);
            var first=world.AddFounder(new Vector2(5,5),DietKind.Grazer,41);
            var second=world.AddFounder(new Vector2(6,5),DietKind.Grazer,41);
            Assert.AreNotEqual(first.id,second.id);
            Assert.AreEqual(world.ScientificName(first),world.ScientificName(second));
            string fixedName=world.ScientificName(first);
            var originalRecord=world.GetSpecies(41);
            first.genes[0]=1f;
            first.genes[1]=0f;
            first.development.regulatory[0]=1f;
            world.Step();
            Assert.AreSame(originalRecord,world.GetSpecies(41));
            Assert.AreEqual(fixedName,world.ScientificName(first));
            foreach(var animal in world.creatures)
                if(animal.speciesId==41)
                    Assert.AreEqual(fixedName,world.ScientificName(animal));
        }

        [Test]
        public void SpeciesCatalogRemainsDeterministicAndUniquelyNamed()
        {
            var a=new EcosystemWorld(16,12,720);
            var b=new EcosystemWorld(16,12,720);
            for(int id=1;id<=7;id++)
            {
                a.AddFounder(new Vector2(id,5),DietKind.Grazer,id);
                b.AddFounder(new Vector2(id,5),DietKind.Grazer,id);
                Assert.AreEqual(a.GetSpecies(id).Binomial,b.GetSpecies(id).Binomial);
            }
            var names=new HashSet<string>();
            foreach(var species in a.NamedSpecies)
                Assert.IsTrue(names.Add(species.Binomial),
                    "Duplicate scientific species name: "+species.Binomial);
            Assert.AreEqual(7,names.Count);
            Assert.Throws<KeyNotFoundException>(()=>a.GetSpecies(999));
        }
    }
}
