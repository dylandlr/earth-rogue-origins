using System;
using NUnit.Framework;
using UnityEngine;
using EarthRogueOrigins.Anatomy;

namespace EarthRogueOrigins.Tests
{
    public sealed class IllustratedCreatureSkinTests
    {
        Texture2D texture;
        Sprite sprite;
        CreatureArtLibrary library;
        GameObject organism;

        [SetUp]
        public void Setup()
        {
            texture=new Texture2D(4,4,TextureFormat.RGBA32,false);
            for(int y=0;y<4;y++)for(int x=0;x<4;x++)
                texture.SetPixel(x,y,Color.white);
            texture.Apply();
            sprite=Sprite.Create(texture,new Rect(0,0,4,4),new Vector2(.5f,.5f),4);
            library=ScriptableObject.CreateInstance<CreatureArtLibrary>();
            organism=new GameObject("Organism");
        }

        [TearDown]
        public void Cleanup()
        {
            if(organism!=null)UnityEngine.Object.DestroyImmediate(organism);
            if(library!=null)UnityEngine.Object.DestroyImmediate(library);
            if(sprite!=null)UnityEngine.Object.DestroyImmediate(sprite);
            if(texture!=null)UnityEngine.Object.DestroyImmediate(texture);
        }

        ArticulatedCreatureView CreateRig()
        {
            var phenotype=Development.Express(
                DevelopmentalGenome.Founder(new System.Random(107)));
            var rig=organism.AddComponent<ArticulatedCreatureView>();
            rig.Initialize(phenotype);
            return rig;
        }

        void FillLibrary()
        {
            library.torso=sprite;
            library.head=sprite;
            library.forelimb=sprite;
            library.hindlimb=sprite;
            library.tail=sprite;
            library.foot=sprite;
            library.armorPlate=sprite;
            library.eye=sprite;
        }

        [Test]
        public void MissingArtLeavesProceduralRigVisible()
        {
            var rig=CreateRig();
            var skin=organism.AddComponent<IllustratedCreatureSkin>();
            Assert.IsFalse(skin.Apply(library));
            Assert.IsFalse(skin.IsIllustrated);
            foreach(var renderer in organism.GetComponentsInChildren<MeshRenderer>())
                Assert.IsTrue(renderer.enabled);
        }

        [Test]
        public void IllustratedSkinTracksRigAttachmentPoints()
        {
            var rig=CreateRig();
            FillLibrary();
            var skin=organism.AddComponent<IllustratedCreatureSkin>();
            Assert.IsTrue(skin.Apply(library));
            Assert.IsTrue(skin.IsIllustrated);
            Assert.NotNull(rig.HeadJoint.Find("Head"));
            Assert.NotNull(rig.TailJoint.Find("Tail"));
            foreach(var limb in rig.LegJoints)
                Assert.Greater(limb.GetComponentsInChildren<SpriteRenderer>().Length,0);
            foreach(var renderer in organism.GetComponentsInChildren<MeshRenderer>())
                Assert.IsFalse(renderer.enabled);
        }

        [Test]
        public void ReapplyingLibraryDoesNotDuplicateAttachedParts()
        {
            var rig=CreateRig();
            FillLibrary();
            var skin=organism.AddComponent<IllustratedCreatureSkin>();
            Assert.IsTrue(skin.Apply(library));
            int count=organism.GetComponentsInChildren<SpriteRenderer>().Length;
            Assert.IsTrue(skin.Apply(library));
            int updated=organism.GetComponentsInChildren<SpriteRenderer>().Length;
            Assert.AreEqual(count,updated);
            Assert.NotNull(rig.HeadJoint.Find("Head"));
        }
    }
}
