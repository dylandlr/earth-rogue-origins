using System;
using NUnit.Framework;
using UnityEngine;
using EarthRogueOrigins.Anatomy;

namespace EarthRogueOrigins.Tests
{
    public sealed class ArticulatedCreatureViewTests
    {
        [Test]
        public void GeneratedRigContainsAnchoredHeadTailAndFourLimbs()
        {
            var p=Development.Express(DevelopmentalGenome.Founder(new System.Random(2046)));
            var obj=new GameObject("Test creature rig");
            try
            {
                var view=obj.AddComponent<ArticulatedCreatureView>();
                view.Initialize(p);
                Assert.NotNull(view.HeadJoint);
                Assert.NotNull(view.TailJoint);
                Assert.AreEqual(4,view.LegJoints.Length);
                Assert.Greater(view.AllocatedMeshes,10);
                Assert.That(view.HeadJoint.localPosition.x,
                    Is.EqualTo(p.head.x).Within(.001f));
                Assert.That(view.TailJoint.localPosition.x,
                    Is.EqualTo(p.tailRoot.x).Within(.001f));
            }
            finally { UnityEngine.Object.DestroyImmediate(obj); }
        }

        [Test]
        public void SwimmingTailRotatesAndIdleReturnsToNeutral()
        {
            var p=Development.Express(DevelopmentalGenome.Founder(new System.Random(87)));
            var obj=new GameObject("Test swim rig");
            try
            {
                var view=obj.AddComponent<ArticulatedCreatureView>();
                view.Initialize(p);
                view.SetMotion(.11f,true,true,false);
                Assert.IsTrue(view.Swimming);
                Assert.Greater(Mathf.Abs(view.TailJoint.localEulerAngles.z),.1f);
                view.SetMotion(0f,false,false,false);
                Assert.IsFalse(view.Swimming);
                Assert.That(Mathf.DeltaAngle(0,view.TailJoint.localEulerAngles.z),
                    Is.EqualTo(0).Within(.001f));
            }
            finally { UnityEngine.Object.DestroyImmediate(obj); }
        }

        [Test]
        public void RepeatedMotionDoesNotCreateAdditionalMeshes()
        {
            var p=Development.Express(DevelopmentalGenome.Founder(new System.Random(32)));
            var obj=new GameObject("Test gait rig");
            try
            {
                var view=obj.AddComponent<ArticulatedCreatureView>();
                view.Initialize(p);
                int count=view.AllocatedMeshes;
                for(int i=0;i<200;i++)
                    view.SetMotion(.06f,i%3==0,true,i%2==0);
                Assert.AreEqual(count,view.AllocatedMeshes);
            }
            finally { UnityEngine.Object.DestroyImmediate(obj); }
        }

        [Test]
        public void ArmorGeometryDependsOnInheritedCoverage()
        {
            var g=DevelopmentalGenome.Founder(new System.Random(65));
            g.regulatory[(int)BodyGene.ArmorCoverage]=0f;
            var low=Development.Express(g);
            g.regulatory[(int)BodyGene.ArmorCoverage]=1f;
            var high=Development.Express(g);
            var a=new GameObject("Low armor");
            var b=new GameObject("High armor");
            try
            {
                var first=a.AddComponent<ArticulatedCreatureView>();
                var second=b.AddComponent<ArticulatedCreatureView>();
                first.Initialize(low);
                second.Initialize(high);
                Assert.Greater(second.AllocatedMeshes,first.AllocatedMeshes);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(a);
                UnityEngine.Object.DestroyImmediate(b);
            }
        }
    }
}
