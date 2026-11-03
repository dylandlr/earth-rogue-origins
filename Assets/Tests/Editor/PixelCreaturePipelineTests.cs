using System;
using NUnit.Framework;
using UnityEngine;
using EarthRogueOrigins.PixelCreatures;
using EarthRogueOrigins.Anatomy;

namespace EarthRogueOrigins.Tests
{
    public sealed class PixelCreaturePipelineTests
    {
        [Test]
        public void AllStarterPlansGenerateNonemptyRgbaSprites()
        {
            foreach(CreaturePlan plan in Enum.GetValues(typeof(CreaturePlan)))
            {
                var r=PixelCreatureRecipe.Founder(plan,2046);
                var pixels=PixelCreaturePainter.RenderPixels(r);
                Assert.AreEqual(PixelCreaturePainter.Width*PixelCreaturePainter.Height,pixels.Length);
                int visible=0;
                foreach(var pixel in pixels)if(pixel.a>0)visible++;
                Assert.Greater(visible,100,plan.ToString());
            }
        }

        [Test]
        public void RepeatedRecipeProducesExactSamePixels()
        {
            var recipe=PixelCreatureRecipe.Founder(CreaturePlan.Tetrapod,976);
            var a=PixelCreaturePainter.RenderPixels(recipe);
            var b=PixelCreaturePainter.RenderPixels(recipe.Copy());
            CollectionAssert.AreEqual(a,b);
        }

        [Test]
        public void AnatomyLayoutUsesOnlyValidSlots()
        {
            foreach(CreaturePlan plan in Enum.GetValues(typeof(CreaturePlan)))
            {
                var r=PixelCreatureRecipe.Founder(plan,3);
                var layout=PixelCreatureDevelopment.Layout(r);
                bool hasHead=false,hasTrunk=false,hasEye=false;
                foreach(var part in layout)
                {
                    Assert.Greater(part.extent.x,0);
                    Assert.Greater(part.extent.y,0);
                    Assert.IsTrue(part.anchor.x>=0 && part.anchor.x<128);
                    Assert.IsTrue(part.anchor.y>=0 && part.anchor.y<96);
                    if(part.slot==PartSlot.Head)hasHead=true;
                    if(part.slot==PartSlot.Trunk)hasTrunk=true;
                    if(part.slot==PartSlot.Eye)hasEye=true;
                }
                Assert.IsTrue(hasHead && hasTrunk && hasEye);
            }
        }

        [Test]
        public void AllOrderedLayersComposeToSameFinalSprite()
        {
            // Fully opaque top-layer pixel compositing is the defined rule.
            var recipe=PixelCreatureRecipe.Founder(CreaturePlan.Tetrapod,300);
            var target=PixelCreaturePainter.RenderPixels(recipe);
            var composed=new Color32[target.Length];
            var parts=PixelCreatureDevelopment.Layout(recipe);
            Array.Sort(parts,(a,b)=>a.drawOrder.CompareTo(b.drawOrder));
            for(int index=0;index<parts.Length;index++)
            {
                var layer=PixelCreaturePainter.RenderLayerPixels(recipe,index);
                for(int p=0;p<layer.Length;p++)
                    if(layer[p].a>0)composed[p]=layer[p];
            }
            CollectionAssert.AreEqual(target,composed);
        }

        [Test]
        public void OffspringPreservePlanAndMutationsStayBounded()
        {
            var rng=new System.Random(14);
            var mother=PixelCreatureRecipe.Founder(CreaturePlan.Arthropod,111);
            var father=PixelCreatureRecipe.Founder(CreaturePlan.Arthropod,112);
            for(int i=0;i<40;i++)
            {
                var child=PixelCreatureRecipe.Offspring(mother,father,rng,1f);
                child.Validate();
                Assert.AreEqual(CreaturePlan.Arthropod,child.plan);
                Assert.That(child.armor,Is.InRange(0,1));
                Assert.That(child.tailLength,Is.InRange(0,1));
                mother=father;father=child;
            }
        }

        [Test]
        public void CrossPlanReproductionIsRejected()
        {
            var a=PixelCreatureRecipe.Founder(CreaturePlan.Tetrapod,1);
            var b=PixelCreatureRecipe.Founder(CreaturePlan.Fish,2);
            Assert.Throws<InvalidOperationException>(()=>
                PixelCreatureRecipe.Offspring(a,b,new System.Random(5)));
        }

        [Test]
        public void InvalidControlsAndUnknownRecipeVersionsFail()
        {
            var r=PixelCreatureRecipe.Founder(CreaturePlan.Fish,9);
            r.finSize=float.NaN;
            Assert.Throws<ArgumentOutOfRangeException>(()=>r.Validate());
            r.finSize=.5f;r.version=3;
            Assert.Throws<NotSupportedException>(()=>r.Validate());
        }

        [Test]
        public void EcologicalGenomeControlsPixelArmorAndFins()
        {
            var g=DevelopmentalGenome.Founder(new System.Random(93));
            g.regulatory[(int)BodyGene.ArmorCoverage]=0f;
            g.regulatory[(int)BodyGene.TailFin]=0f;
            var low=PixelRecipeAdapter.FromDevelopment(g,Development.Express(g),14);
            g.regulatory[(int)BodyGene.ArmorCoverage]=1f;
            g.regulatory[(int)BodyGene.TailFin]=1f;
            var high=PixelRecipeAdapter.FromDevelopment(g,Development.Express(g),14);
            Assert.AreEqual(CreaturePlan.Tetrapod,high.plan);
            Assert.Greater(high.armor,low.armor);
            Assert.Greater(high.finSize,low.finSize);
        }

        [Test]
        public void SwappingHeadAndBodyModulesChangesRenderedPixels()
        {
            var r=PixelCreatureRecipe.Founder(CreaturePlan.Tetrapod,120);
            r.headStyle=PartStyle.Rounded;
            r.trunkStyle=PartStyle.Rounded;
            var before=PixelCreaturePainter.RenderPixels(r);
            r.headStyle=PartStyle.LongSnout;
            r.trunkStyle=PartStyle.Slender;
            var after=PixelCreaturePainter.RenderPixels(r);
            int differences=0;
            for(int i=0;i<before.Length;i++)
                if(!before[i].Equals(after[i]))differences++;
            Assert.Greater(differences,30);
        }

        [Test]
        public void HeroArchetypesHaveDistinctControlledOutputs()
        {
            var keys=new System.Collections.Generic.HashSet<string>();
            foreach(CreatureArchetype family in Enum.GetValues(typeof(CreatureArchetype)))
            {
                var hero=PixelArtDirection.Hero(family,462);
                hero.Validate();
                var pixels=PixelCreaturePainter.RenderPixels(hero);
                int visible=0;
                ulong hash=14695981039346656037UL;
                unchecked
                {
                    foreach(var color in pixels)
                    {
                        if(color.a!=0)visible++;
                        hash^=color.r;hash*=1099511628211UL;
                        hash^=color.g;hash*=1099511628211UL;
                        hash^=color.b;hash*=1099511628211UL;
                        hash^=color.a;hash*=1099511628211UL;
                    }
                }
                Assert.Greater(visible,100,family.ToString());
                Assert.IsTrue(keys.Add(hash.ToString()),"Duplicate hero artwork: "+family);
            }
            Assert.AreEqual(8,keys.Count);
        }

        [Test]
        public void PaletteRampsHaveOrderedLightness()
        {
            foreach(CreaturePalette palette in Enum.GetValues(typeof(CreaturePalette)))
            {
                var ramp=PixelArtDirection.Ramp(palette);
                Func<Color32,float> value=c=>c.r*.2126f+c.g*.7152f+c.b*.0722f;
                Assert.Less(value(ramp.ink),value(ramp.shadow));
                Assert.Less(value(ramp.shadow),value(ramp.mid));
                Assert.Less(value(ramp.mid),value(ramp.light));
                Assert.Less(value(ramp.light),value(ramp.highlight));
            }
        }

        [Test]
        public void MarkingDensityChangesPixelsWithoutChangingBodyPlan()
        {
            var recipe=PixelArtDirection.Hero(CreatureArchetype.MarshCrawler,1924);
            recipe.pattern=MarkingPattern.MottledBack;
            recipe.spots=0f;
            var before=PixelCreaturePainter.RenderPixels(recipe);
            recipe.spots=1f;
            var after=PixelCreaturePainter.RenderPixels(recipe);
            int changed=0;
            for(int i=0;i<before.Length;i++)
                if(!before[i].Equals(after[i]))changed++;
            Assert.Greater(changed,0);
            Assert.AreEqual(CreaturePlan.Tetrapod,recipe.plan);
        }

        [Test]
        public void AuthoredHeadProfilesHaveDistinctContours()
        {
            var recipe=PixelArtDirection.Hero(CreatureArchetype.MarshCrawler,2046);
            recipe.headStyle=PartStyle.Robust;
            var broad=PixelCreaturePainter.RenderPixels(recipe);
            recipe.headStyle=PartStyle.Wedge;
            var wedge=PixelCreaturePainter.RenderPixels(recipe);
            recipe.headStyle=PartStyle.LongSnout;
            var longSnout=PixelCreaturePainter.RenderPixels(recipe);
            int broadWedge=0,wedgeLong=0;
            for(int i=0;i<broad.Length;i++)
            {
                if(!broad[i].Equals(wedge[i]))broadWedge++;
                if(!wedge[i].Equals(longSnout[i]))wedgeLong++;
            }
            Assert.Greater(broadWedge,55);
            Assert.Greater(wedgeLong,55);
        }

        [Test]
        public void AuthoredProfilesRemainInsideCanvasAcrossMorphologyExtremes()
        {
            var r=PixelArtDirection.Hero(CreatureArchetype.MarshCrawler,491);
            foreach(PartStyle style in new[]{PartStyle.Rounded,PartStyle.Robust,
                PartStyle.Slender,PartStyle.Wedge,PartStyle.LongSnout})
            {
                r.headStyle=style;
                foreach(float value in new[]{0f,.5f,1f})
                {
                    r.trunkLength=value;r.trunkDepth=value;r.headSize=value;
                    var pixels=PixelCreaturePainter.RenderPixels(r);
                    int visible=0;
                    for(int y=1;y<PixelCreaturePainter.Height-1;y++)
                    for(int x=1;x<PixelCreaturePainter.Width-1;x++)
                        if(pixels[y*PixelCreaturePainter.Width+x].a>0)visible++;
                    Assert.Greater(visible,250);
                }
            }
        }

        [Test]
        public void ArchetypeLayoutsHaveOverlappingHeadAndBodyEnvelopes()
        {
            foreach(CreatureArchetype archetype in Enum.GetValues(typeof(CreatureArchetype)))
            {
                var r=PixelArtDirection.Hero(archetype,45);
                var parts=PixelCreatureDevelopment.Layout(r);
                PixelPartDescriptor head=null,trunk=null;
                foreach(var part in parts)
                {
                    if(part.slot==PartSlot.Head)head=part;
                    if(part.slot==PartSlot.Trunk)trunk=part;
                }
                Assert.NotNull(head);
                Assert.NotNull(trunk);
                Assert.IsTrue(PixelSilhouetteTemplates.AttachmentOverlap(head,trunk),
                    "Disconnected head envelope for "+archetype);
            }
        }

        [Test]
        public void RepaintingCustomPigmentChangesRenderedPixels()
        {
            var r=PixelArtDirection.Hero(CreatureArchetype.MarshCrawler,17);
            var before=PixelCreaturePainter.RenderPixels(r);
            r.baseColor=new Color32(191,114,105,255);
            var after=PixelCreaturePainter.RenderPixels(r);
            int changed=0;
            for(int i=0;i<before.Length;i++)if(!before[i].Equals(after[i]))changed++;
            Assert.Greater(changed,70);
        }

        [Test]
        public void AnimatedFramesAreDeterministicAndNotAllIdentical()
        {
            var r=PixelArtDirection.Hero(CreatureArchetype.MarshCrawler,731);
            var neutral=PixelCreaturePainter.RenderPixels(r,0,false);
            int changedFrames=0;
            for(int frame=0;frame<PixelCreaturePoses.Frames;frame++)
            {
                var walk=PixelCreaturePainter.RenderPixels(r,frame,false);
                var repeat=PixelCreaturePainter.RenderPixels(r,frame,false);
                CollectionAssert.AreEqual(walk,repeat);
                int different=0;
                for(int i=0;i<walk.Length;i++)
                    if(!walk[i].Equals(neutral[i]))different++;
                if(different>0)changedFrames++;
            }
            Assert.GreaterOrEqual(changedFrames,2);
        }

        [Test]
        public void AnimatedLayerCompositingMatchesFinalImage()
        {
            var r=PixelArtDirection.Hero(CreatureArchetype.ArmoredCrawler,812);
            var parts=PixelCreaturePoses.Layout(r,3,true);
            Array.Sort(parts,(a,b)=>a.drawOrder.CompareTo(b.drawOrder));
            var result=new Color32[PixelCreaturePainter.Width*PixelCreaturePainter.Height];
            for(int i=0;i<parts.Length;i++)
            {
                var layer=PixelCreaturePainter.RenderLayerPixels(r,i,3,true);
                for(int j=0;j<result.Length;j++)
                    if(layer[j].a>0)result[j]=layer[j];
            }
            CollectionAssert.AreEqual(PixelCreaturePainter.RenderPixels(r,3,true),result);
        }

        [Test]
        public void TetrapodSilhouetteHasSingleDominantConnectedBody()
        {
            var r=PixelArtDirection.Hero(CreatureArchetype.MarshCrawler,710);
            foreach(int frame in new[]{0,1,2,3})
            {
                var pixels=PixelCreaturePainter.RenderPixels(r,frame,false);
                int width=PixelCreaturePainter.Width,height=PixelCreaturePainter.Height;
                int count=0,maxComponent=0;
                var seen=new bool[pixels.Length];
                var queue=new int[pixels.Length];
                for(int index=0;index<pixels.Length;index++)
                {
                    if(pixels[index].a==0||seen[index])continue;
                    int first=0,last=0;
                    seen[index]=true;queue[last++]=index;
                    while(first<last)
                    {
                        int current=queue[first++];
                        int x=current%width,y=current/width;
                        int[] neighbor={
                            x>0?current-1:-1,x<width-1?current+1:-1,
                            y>0?current-width:-1,y<height-1?current+width:-1};
                        foreach(int next in neighbor)
                            if(next>=0&&!seen[next]&&pixels[next].a>0)
                            {
                                seen[next]=true;queue[last++]=next;
                            }
                    }
                    count+=last;
                    maxComponent=Math.Max(maxComponent,last);
                }
                Assert.Greater(count,200);
                Assert.Greater((float)maxComponent/count,.85f,
                    "Disconnected visible anatomy at frame "+frame);
            }
        }

        [Test]
        public void TailRootTransitionAndFootLayersMaintainExactCompositing()
        {
            foreach(int phase in new[]{0,1,2,3})
            {
                var recipe=PixelArtDirection.Hero(CreatureArchetype.MarshCrawler,400+phase);
                var layout=PixelCreaturePoses.Layout(recipe,phase,false);
                Array.Sort(layout,(a,b)=>a.drawOrder.CompareTo(b.drawOrder));
                bool foundTailBlend=false,foundHip=false,foundShoulder=false;
                var assembled=new Color32[PixelCreaturePainter.Width*PixelCreaturePainter.Height];
                for(int i=0;i<layout.Length;i++)
                {
                    if(layout[i].slot==PartSlot.TailRootBlend)foundTailBlend=true;
                    if(layout[i].slot==PartSlot.HipBlend)foundHip=true;
                    if(layout[i].slot==PartSlot.ShoulderBlend)foundShoulder=true;
                    var layer=PixelCreaturePainter.RenderLayerPixels(recipe,i,phase,false);
                    for(int j=0;j<assembled.Length;j++)
                        if(layer[j].a!=0)assembled[j]=layer[j];
                }
                Assert.IsTrue(foundTailBlend&&foundHip&&foundShoulder);
                CollectionAssert.AreEqual(
                    PixelCreaturePainter.RenderPixels(recipe,phase,false),assembled);
            }
        }

        [Test]
        public void GroundShadowIsTransparentSeparateAndScalesWithBody()
        {
            var recipe=PixelArtDirection.Hero(CreatureArchetype.MarshCrawler,712);
            recipe.trunkLength=0f;
            var narrow=PixelCreaturePainter.RenderGroundShadowPixels(recipe);
            recipe.trunkLength=1f;
            var wide=PixelCreaturePainter.RenderGroundShadowPixels(recipe);
            int narrowCount=0,wideCount=0;
            foreach(var p in narrow)
                if(p.a!=0)narrowCount++;
            foreach(var p in wide)
                if(p.a!=0)wideCount++;
            Assert.Greater(wideCount,narrowCount);
            Assert.AreEqual(0,wide[0].a);
            Assert.AreEqual(0,wide[wide.Length-1].a);
            Assert.Less(wide[23*PixelCreaturePainter.Width+62].a,160);
            var fish=PixelArtDirection.Hero(CreatureArchetype.PaddleFish,713);
            foreach(var p in PixelCreaturePainter.RenderGroundShadowPixels(fish))
                Assert.AreEqual(0,p.a);
        }

        [Test]
        public void GroundShadowGenerationIsSeedStable()
        {
            var r=PixelArtDirection.Hero(CreatureArchetype.ClayRunner,431);
            CollectionAssert.AreEqual(
                PixelCreaturePainter.RenderGroundShadowPixels(r),
                PixelCreaturePainter.RenderGroundShadowPixels(r.Copy()));
        }

        [Test]
        public void MottlingAndFootTemplatesStayWithinLogicalCanvas()
        {
            foreach(CreatureArchetype archetype in Enum.GetValues(typeof(CreatureArchetype)))
            {
                var recipe=PixelArtDirection.Hero(archetype,1018);
                var pixels=PixelCreaturePainter.RenderPixels(recipe);
                Assert.AreEqual(128*96,pixels.Length);
                var marks=PixelDetailGrammar.DorsalMottle(recipe,63,50,60,27);
                foreach(var mark in marks)
                {
                    Assert.That(mark.x,Is.InRange(0,127));
                    Assert.That(mark.y,Is.InRange(0,95));
                    Assert.That(mark.radius,Is.InRange(1,3));
                }
                foreach(var toe in PixelDetailGrammar.FootToes(44,31,recipe.limbStyle))
                {
                    Assert.That(toe.x,Is.InRange(0,127));
                    Assert.That(toe.y,Is.InRange(0,95));
                }
            }
        }

        [Test]
        public void FleshyTailRemainsDeterministicAcrossAllPoseFrames()
        {
            var r=PixelArtDirection.Hero(CreatureArchetype.MarshCrawler,981);
            for(int phase=0;phase<PixelCreaturePoses.Frames;phase++)
            {
                var first=PixelCreaturePainter.RenderPixels(r,phase,true);
                var second=PixelCreaturePainter.RenderPixels(r,phase,true);
                CollectionAssert.AreEqual(first,second);
                int visibleTail=0;
                for(int y=42;y<=68;y++)
                for(int x=98;x<125;x++)
                    if(first[y*128+x].a>0)visibleTail++;
                Assert.Greater(visibleTail,8);
            }
        }

        [Test]
        public void FaceLandmarksStayInsideCanvasForAllTetrapodHeadStyles()
        {
            foreach(PartStyle style in new[]{PartStyle.Rounded,PartStyle.Robust,
                PartStyle.Wedge,PartStyle.LongSnout})
            foreach(float size in new[]{0f,.5f,1f})
            foreach(float depth in new[]{0f,.5f,1f})
            {
                var recipe=PixelArtDirection.Hero(CreatureArchetype.MarshCrawler,901);
                recipe.headSize=size;
                recipe.trunkDepth=depth;
                recipe.headStyle=style;
                var layout=PixelCreatureDevelopment.Layout(recipe);
                PixelPartDescriptor head=null,eye=null,face=null;
                foreach(var part in layout)
                {
                    if(part.slot==PartSlot.Head)head=part;
                    if(part.slot==PartSlot.Eye)eye=part;
                    if(part.slot==PartSlot.FaceDetails)face=part;
                }
                Assert.NotNull(head);
                Assert.NotNull(eye);
                Assert.NotNull(face);
                var marks=PixelFaceGrammar.Landmarks(recipe.plan,head.style,
                    head.anchor.x,head.anchor.y,head.extent.x,head.extent.y);
                Assert.AreEqual(marks.eye,eye.anchor);
                foreach(var point in new[]{marks.eye,marks.nostril,
                    marks.mouthStart,marks.mouthEnd})
                {
                    float u=(point.x-(head.anchor.x-head.extent.x/2))/
                        (float)Mathf.Max(1,head.extent.x);
                    float v=(point.y-head.anchor.y)*2f/
                        Mathf.Max(1,head.extent.y);
                    Assert.IsTrue(PixelSilhouetteTemplates.Contains(
                        PartSlot.Head,style,u,v),
                        "Floating facial feature at "+point+" / "+style+
                        " / head size "+size+" / depth "+depth);
                }
                foreach(var point in new[]{marks.eye,marks.nostril,
                    marks.mouthStart,marks.mouthEnd,marks.cheek})
                {
                    Assert.That(point.x,Is.InRange(0,127));
                    Assert.That(point.y,Is.InRange(0,95));
                }
                var pixels=PixelCreaturePainter.RenderPixels(recipe);
                Assert.AreEqual(128*96,pixels.Length);
            }
        }

        [Test]
        public void FaceDetailLayerPreservesCompositingAcrossMorphologies()
        {
            foreach(CreatureArchetype hero in new[]{CreatureArchetype.MarshCrawler,
                CreatureArchetype.ClayRunner,CreatureArchetype.PaleJuvenile})
            {
                var recipe=PixelArtDirection.Hero(hero,1977);
                var parts=PixelCreatureDevelopment.Layout(recipe);
                Array.Sort(parts,(a,b)=>a.drawOrder.CompareTo(b.drawOrder));
                int faceIndex=-1;
                var assembled=new Color32[128*96];
                for(int i=0;i<parts.Length;i++)
                {
                    if(parts[i].slot==PartSlot.FaceDetails)faceIndex=i;
                    var layer=PixelCreaturePainter.RenderLayerPixels(recipe,i);
                    for(int pixel=0;pixel<layer.Length;pixel++)
                        if(layer[pixel].a!=0)assembled[pixel]=layer[pixel];
                }
                Assert.Greater(faceIndex,0);
                CollectionAssert.AreEqual(PixelCreaturePainter.RenderPixels(recipe),assembled);
            }
        }

        [Test]
        public void HandAuthoredHeadContoursStayOrderedAtAllSamples()
        {
            foreach(PartStyle head in new[]{PartStyle.Rounded,PartStyle.Robust,
                PartStyle.Wedge,PartStyle.LongSnout})
            {
                float[] upper,lower;
                PixelSilhouetteTemplates.Profile(PartSlot.Head,head,out upper,out lower);
                Assert.GreaterOrEqual(upper.Length,13);
                Assert.AreEqual(upper.Length,lower.Length);
                for(int step=0;step<=100;step++)
                {
                    float u=step/100f;
                    float hi=PixelSilhouetteTemplates.Sample(upper,u);
                    float lo=PixelSilhouetteTemplates.Sample(lower,u);
                    Assert.Greater(hi,lo,
                        "Head profile inverted at "+head+" u="+u);
                    Assert.That(hi,Is.InRange(-1f,1f));
                    Assert.That(lo,Is.InRange(-1f,1f));
                }
            }
        }

        [Test]
        public void HeadContoursDoNotCreateFilledRectangleCorners()
        {
            var recipe=PixelArtDirection.Hero(CreatureArchetype.MarshCrawler,2046);
            foreach(PartStyle style in new[]{PartStyle.Rounded,PartStyle.Robust,
                PartStyle.Wedge,PartStyle.LongSnout})
            {
                recipe.headStyle=style;
                var layout=PixelCreatureDevelopment.Layout(recipe);
                Array.Sort(layout,(a,b)=>a.drawOrder.CompareTo(b.drawOrder));
                int headIndex=-1;
                for(int i=0;i<layout.Length;i++)
                    if(layout[i].slot==PartSlot.Head){headIndex=i;break;}
                Assert.GreaterOrEqual(headIndex,0);
                var pixels=PixelCreaturePainter.RenderLayerPixels(recipe,headIndex);
                // Probe above the snout and below the rear upper contour.
                // A rectangular border fill would color these empty corners.
                var head=layout[headIndex];
                int left=head.anchor.x-head.extent.x/2;
                int top=head.anchor.y+head.extent.y/2+2;
                int background=0;
                for(int dx=-2;dx<=1;dx++)
                for(int dy=0;dy<=2;dy++)
                {
                    int x=left+dx,y=top+dy;
                    if(x>=0&&x<128&&y>=0&&y<96&&pixels[y*128+x].a==0)
                        background++;
                }
                Assert.Greater(background,0,"Rectangle border artifact for "+style);
            }
        }

        [Test]
        public void TorsoContourIsAsymmetricAndInBoundsAcrossStyles()
        {
            foreach(PartStyle style in new[]{PartStyle.Rounded,
                PartStyle.Robust,PartStyle.Slender})
            {
                float[] top,bottom;
                PixelSilhouetteTemplates.Profile(
                    PartSlot.Trunk,style,out top,out bottom);
                Assert.GreaterOrEqual(top.Length,13);
                Assert.AreEqual(top.Length,bottom.Length);
                // Anatomical dorsal rise and sloped rump must differ
                // on the left and right, unlike a mirrored ellipse.
                Assert.AreNotEqual(top[2],top[top.Length-3]);
                for(int i=0;i<=100;i++)
                {
                    float u=i/100f;
                    float hi=PixelSilhouetteTemplates.Sample(top,u);
                    float lo=PixelSilhouetteTemplates.Sample(bottom,u);
                    Assert.Greater(hi,lo,style.ToString());
                    Assert.That(hi,Is.InRange(-1f,1f));
                    Assert.That(lo,Is.InRange(-1f,1f));
                }
            }
        }

        [Test]
        public void LimbTailAndNeckAttachmentsTrackInheritedTrunkLength()
        {
            var r=PixelArtDirection.Hero(CreatureArchetype.MarshCrawler,2046);
            r.legLength=.5f;
            int earlierRearX=-1,earlierRootX=-1;
            foreach(float length in new[]{0f,.25f,.5f,.75f,1f})
            foreach(float depth in new[]{0f,.5f,1f})
            {
                r.trunkLength=length;r.trunkDepth=depth;
                var parts=PixelCreatureDevelopment.Layout(r);
                PixelPartDescriptor trunk=null,head=null,neck=null,
                    tail=null,tailRoot=null,fore=null,rear=null;
                foreach(var p in parts)
                {
                    if(p.slot==PartSlot.Trunk)trunk=p;
                    if(p.slot==PartSlot.Head)head=p;
                    if(p.slot==PartSlot.NeckBlend)neck=p;
                    if(p.slot==PartSlot.TailRootBlend)tailRoot=p;
                    if(p.slot==PartSlot.Tail)tail=p;
                    if(p.slot==PartSlot.ForeLeg&&p.drawOrder==9)fore=p;
                    if(p.slot==PartSlot.HindLeg&&p.drawOrder==9)rear=p;
                }
                Assert.NotNull(trunk);Assert.NotNull(head);
                Assert.NotNull(neck);Assert.NotNull(tailRoot);
                Assert.NotNull(tail);Assert.NotNull(fore);Assert.NotNull(rear);
                int left=trunk.anchor.x-trunk.extent.x/2;
                int right=trunk.anchor.x+trunk.extent.x/2;
                Assert.That(fore.anchor.x,Is.InRange(left,right));
                Assert.That(rear.anchor.x,Is.InRange(left,right));
                Assert.Greater(rear.anchor.x,fore.anchor.x);
                Assert.That(tailRoot.anchor.x,Is.InRange(right-5,right+1));
                Assert.Greater(tail.anchor.x,tailRoot.anchor.x);
                int headRight=head.anchor.x+head.extent.x/2;
                Assert.LessOrEqual(neck.anchor.x-neck.extent.x/2,headRight+1);
                Assert.GreaterOrEqual(neck.anchor.x+neck.extent.x/2,left-1);
                // Same inherited torso length must position the hip and
                // tail consistently regardless of depth changes.
                if(depth==0f && earlierRearX>=0)
                {
                    Assert.GreaterOrEqual(rear.anchor.x,earlierRearX);
                    Assert.GreaterOrEqual(tailRoot.anchor.x,earlierRootX);
                }
                if(depth==0f)
                {
                    earlierRearX=rear.anchor.x;earlierRootX=tailRoot.anchor.x;
                }
                var pixels=PixelCreaturePainter.RenderPixels(r);
                int painted=0;
                foreach(var pixel in pixels)if(pixel.a>0)painted++;
                Assert.Greater(painted,250);
            }
        }

        [Test]
        public void FaceRemainsAboveNeckAndLimbOcclusionLayers()
        {
            var r=PixelArtDirection.Hero(CreatureArchetype.MarshCrawler,47);
            var parts=PixelCreatureDevelopment.Layout(r);
            int eyeOrder=-1,faceOrder=-1,headOrder=-1,neckOrder=-1,hipOrder=-1;
            foreach(var p in parts)
            {
                if(p.slot==PartSlot.Eye)eyeOrder=p.drawOrder;
                if(p.slot==PartSlot.FaceDetails)faceOrder=p.drawOrder;
                if(p.slot==PartSlot.Head)headOrder=p.drawOrder;
                if(p.slot==PartSlot.NeckBlend)neckOrder=p.drawOrder;
                if(p.slot==PartSlot.HipBlend)hipOrder=p.drawOrder;
            }
            Assert.Less(neckOrder,headOrder);
            Assert.Less(headOrder,faceOrder);
            Assert.Less(faceOrder,eyeOrder);
            Assert.Less(hipOrder,eyeOrder);
        }

        [Test]
        public void LayerIndexMustBeInRange()
        {
            var r=PixelCreatureRecipe.Founder(CreaturePlan.Fish,4);
            Assert.Throws<ArgumentOutOfRangeException>(()=>
                PixelCreaturePainter.RenderLayerPixels(r,-1));
        }
    }
}
