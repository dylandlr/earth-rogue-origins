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
            // The neck bridge now closes the head/torso seam after the
            // head outline, without painting over the face or eye.
            Assert.Less(headOrder,neckOrder);
            Assert.Less(neckOrder,faceOrder);
            Assert.Less(faceOrder,eyeOrder);
            Assert.Less(hipOrder,eyeOrder);
        }

        [Test]
        public void BentLegKeepsItsAttachmentRootAcrossAllGaitFrames()
        {
            var recipe=PixelArtDirection.Hero(CreatureArchetype.MarshCrawler,103);
            foreach(int frame in new[]{0,1,2,3})
            foreach(bool swimming in new[]{false,true})
            {
                var parts=PixelCreaturePoses.Layout(recipe,frame,swimming);
                foreach(var part in parts)
                {
                    if(part.slot!=PartSlot.ForeLeg&&part.slot!=PartSlot.HindLeg)
                        continue;
                    var shape=PixelLimbSilhouette.Build(
                        part.anchor.x,part.anchor.y,part.extent.y,
                        part.style,part.mirrorGroup,
                        part.slot==PartSlot.ForeLeg,part.drawOrder<2);
                    Assert.AreEqual(part.anchor,shape.root);
                    Assert.Less(shape.knee.y,shape.root.y);
                    Assert.Less(shape.ankle.y,shape.knee.y);
                    Assert.LessOrEqual(Mathf.Abs(shape.ankle.x-shape.root.x),9);
                    Assert.GreaterOrEqual(shape.upperRadius,shape.lowerRadius);
                }
            }
        }

        [Test]
        public void LimbContoursRemainWithinCanvasAtMorphologyExtremes()
        {
            var recipe=PixelArtDirection.Hero(CreatureArchetype.MarshCrawler,518);
            foreach(float length in new[]{0f,.5f,1f})
            foreach(PartStyle style in new[]{PartStyle.Slender,
                PartStyle.Rounded,PartStyle.Robust,PartStyle.Webbed})
            {
                recipe.legLength=length;recipe.limbStyle=style;
                foreach(int frame in new[]{0,1,2,3})
                {
                    var layout=PixelCreaturePoses.Layout(recipe,frame,false);
                    foreach(var p in layout)
                    {
                        if(p.slot!=PartSlot.ForeLeg&&p.slot!=PartSlot.HindLeg)
                            continue;
                        var shape=PixelLimbSilhouette.Build(
                            p.anchor.x,p.anchor.y,p.extent.y,p.style,
                            p.mirrorGroup,p.slot==PartSlot.ForeLeg,p.drawOrder<2);
                        var upper=PixelLimbSilhouette.UpperPolygon(shape);
                        var lower=PixelLimbSilhouette.LowerPolygon(shape);
                        foreach(var point in upper)
                        {
                            Assert.That(point.x,Is.InRange(0,127));
                            Assert.That(point.y,Is.InRange(0,95));
                        }
                        foreach(var point in lower)
                        {
                            Assert.That(point.x,Is.InRange(0,127));
                            Assert.That(point.y,Is.InRange(0,95));
                        }
                    }
                    var pixels=PixelCreaturePainter.RenderPixels(recipe,frame,false);
                    Assert.AreEqual(128*96,pixels.Length);
                }
            }
        }

        [Test]
        public void FarSideLegHasSmallerShoulderThanNearSide()
        {
            var foreground=PixelLimbSilhouette.Build(
                44,43,18,PartStyle.Robust,3,true,false);
            var background=PixelLimbSilhouette.Build(
                44,43,18,PartStyle.Robust,3,true,true);
            Assert.Less(background.upperRadius,foreground.upperRadius);
            Assert.AreEqual(background.root,foreground.root);
            Assert.AreEqual(background.ankle,foreground.ankle);
        }

        [Test]
        public void WalkAlwaysHasPlantedSupportAndAlternatingSwingFeet()
        {
            var recipe=PixelArtDirection.Hero(CreatureArchetype.MarshCrawler,661);
            int previousContactY=int.MinValue;
            for(int phase=0;phase<PixelCreaturePoses.Frames;phase++)
            {
                var parts=PixelCreaturePoses.Layout(recipe,phase,false);
                int planted=0,swings=0;
                foreach(var p in parts)
                {
                    if(p.slot!=PartSlot.ForeLeg&&p.slot!=PartSlot.HindLeg)continue;
                    if(p.footLift==0 && p.drawOrder==9)planted++;
                    if(p.footLift>0 && p.drawOrder==9)swings++;
                    var shape=PixelLimbSilhouette.Build(p.anchor.x,p.anchor.y,
                        p.extent.y,p.style,p.mirrorGroup,
                        p.slot==PartSlot.ForeLeg,p.drawOrder<2,p.footLift);
                    if(p.footLift==0 && p.drawOrder==9)
                    {
                        if(previousContactY==int.MinValue)previousContactY=shape.ankle.y;
                        Assert.AreEqual(previousContactY,shape.ankle.y,
                            "Planted foot contact height drifted at phase "+phase);
                    }
                }
                Assert.GreaterOrEqual(planted,1,"No support foot at phase "+phase);
                if(phase==1 || phase==3)Assert.GreaterOrEqual(swings,1);
            }
        }

        [Test]
        public void SwingFootLiftsWithoutDisplacingTheShoulder()
        {
            var still=PixelLimbSilhouette.Build(
                44,43,18,PartStyle.Robust,3,true,false,0);
            var raised=PixelLimbSilhouette.Build(
                44,43,18,PartStyle.Robust,3,true,false,2);
            Assert.AreEqual(still.root,raised.root);
            Assert.AreEqual(still.ankle.x,raised.ankle.x);
            Assert.AreEqual(still.ankle.y+2,raised.ankle.y);
            Assert.Less(raised.ankle.y,raised.root.y);
        }

        [Test]
        public void MidStanceBodyCompressionMaintainsGroundContact()
        {
            var r=PixelArtDirection.Hero(CreatureArchetype.MarshCrawler,314);
            foreach(float length in new[]{0f,.5f,1f})
            foreach(float depth in new[]{0f,.5f,1f})
            {
                r.trunkLength=length;r.trunkDepth=depth;
                var neutral=PixelCreaturePoses.Layout(r,0,false);
                var squat=PixelCreaturePoses.Layout(r,2,false);
                PixelPartDescriptor trunk0=null,trunk2=null;
                foreach(var p in neutral)if(p.slot==PartSlot.Trunk)trunk0=p;
                foreach(var p in squat)if(p.slot==PartSlot.Trunk)trunk2=p;
                Assert.NotNull(trunk0);Assert.NotNull(trunk2);
                Assert.AreEqual(trunk0.anchor.y-1,trunk2.anchor.y);
                for(int i=0;i<neutral.Length;i++)
                {
                    var a=neutral[i];var b=squat[i];
                    Assert.AreEqual(a.slot,b.slot);
                    if(a.slot!=PartSlot.ForeLeg&&a.slot!=PartSlot.HindLeg)continue;
                    var original=PixelLimbSilhouette.Build(
                        a.anchor.x,a.anchor.y,a.extent.y,a.style,0,
                        a.slot==PartSlot.ForeLeg,a.drawOrder<2);
                    var lowered=PixelLimbSilhouette.Build(
                        b.anchor.x,b.anchor.y,b.extent.y,b.style,0,
                        b.slot==PartSlot.ForeLeg,b.drawOrder<2,b.footLift);
                    Assert.AreEqual(original.ankle.y,lowered.ankle.y,
                        "Compression shifted ground contact.");
                    Assert.AreEqual(original.root.y-1,lowered.root.y);
                }
            }
        }

        [Test]
        public void GroundShadowTracksInheritedLegLength()
        {
            var r=PixelArtDirection.Hero(CreatureArchetype.MarshCrawler,650);
            r.legLength=0f;
            var shortLegs=PixelCreaturePainter.RenderGroundShadowPixels(r);
            r.legLength=1f;
            var longLegs=PixelCreaturePainter.RenderGroundShadowPixels(r);
            int sumShort=0,countShort=0,sumLong=0,countLong=0;
            for(int y=0;y<96;y++)
            for(int x=0;x<128;x++)
            {
                int i=y*128+x;
                if(shortLegs[i].a>0){sumShort+=y;countShort++;}
                if(longLegs[i].a>0){sumLong+=y;countLong++;}
            }
            Assert.Greater(countShort,50);
            Assert.Greater(countLong,50);
            Assert.Greater(sumShort/(float)countShort,
                sumLong/(float)countLong+4f,
                "Short-legged creature shadow should sit higher on the canvas.");
        }

        [Test]
        public void JointSkinFoldsRemainAttachedAndInsideCanvas()
        {
            var recipe=PixelArtDirection.Hero(CreatureArchetype.MarshCrawler,2046);
            foreach(float depth in new[]{0f,.5f,1f})
            foreach(float length in new[]{0f,.5f,1f})
            foreach(int frame in new[]{0,1,2,3})
            {
                recipe.trunkLength=length;
                recipe.trunkDepth=depth;
                var parts=PixelCreaturePoses.Layout(recipe,frame,false);
                PixelPartDescriptor shoulder=null,hip=null,fore=null,rear=null;
                foreach(var p in parts)
                {
                    if(p.slot==PartSlot.ShoulderBlend)shoulder=p;
                    if(p.slot==PartSlot.HipBlend)hip=p;
                    if(p.slot==PartSlot.ForeLeg&&p.drawOrder==9)fore=p;
                    if(p.slot==PartSlot.HindLeg&&p.drawOrder==9)rear=p;
                }
                Assert.NotNull(shoulder);Assert.NotNull(hip);
                Assert.NotNull(fore);Assert.NotNull(rear);
                Assert.AreEqual(fore.anchor,shoulder.anchor);
                Assert.AreEqual(rear.anchor,hip.anchor);
                foreach(var fold in new[]{
                    PixelJointTransition.Build(shoulder.anchor.x,
                        shoulder.anchor.y,shoulder.extent.x,
                        shoulder.extent.y,false),
                    PixelJointTransition.Build(hip.anchor.x,
                        hip.anchor.y,hip.extent.x,hip.extent.y,true)})
                {
                    Assert.GreaterOrEqual(fold.outer.Length,5);
                    Assert.AreEqual(fold.outer.Length,fold.inner.Length);
                    foreach(var vertex in fold.outer)
                    {
                        Assert.That(vertex.x,Is.InRange(0,127));
                        Assert.That(vertex.y,Is.InRange(0,95));
                    }
                    foreach(var vertex in fold.inner)
                    {
                        Assert.That(vertex.x,Is.InRange(0,127));
                        Assert.That(vertex.y,Is.InRange(0,95));
                    }
                }
                var pixels=PixelCreaturePainter.RenderPixels(recipe,frame,false);
                Assert.AreEqual(128*96,pixels.Length);
            }
        }

        [Test]
        public void ShoulderAndHipFoldsHaveDistinctDirections()
        {
            var shoulder=PixelJointTransition.Build(44,41,11,8,false);
            var hip=PixelJointTransition.Build(44,41,11,8,true);
            CollectionAssert.AreNotEqual(shoulder.outer,hip.outer);
            Assert.AreNotEqual(shoulder.highlightEnd,hip.highlightEnd);
            Assert.AreEqual(shoulder.outer.Length,hip.outer.Length);
        }

        [Test]
        public void FishFinsFollowTailAndDorsalSurfaceWithoutClipping()
        {
            foreach(float tailLength in new[]{0f,.25f,.5f,.75f,1f})
            foreach(float finSize in new[]{0f,.5f,1f})
            foreach(float trunkDepth in new[]{0f,.5f,1f})
            {
                var r=PixelArtDirection.Hero(CreatureArchetype.PaddleFish,2046);
                r.tailLength=tailLength;
                r.finSize=finSize;
                r.trunkDepth=trunkDepth;
                var parts=PixelCreatureDevelopment.Layout(r);
                PixelPartDescriptor tail=null,caudal=null,dorsal=null,trunk=null;
                foreach(var p in parts)
                {
                    if(p.slot==PartSlot.Tail)tail=p;
                    if(p.slot==PartSlot.Trunk)trunk=p;
                    if(p.slot==PartSlot.Fin && p.drawOrder==1)caudal=p;
                    if(p.slot==PartSlot.Fin && p.drawOrder==3)dorsal=p;
                }
                Assert.NotNull(tail);Assert.NotNull(caudal);
                Assert.NotNull(trunk);Assert.NotNull(dorsal);
                int tailEnd=tail.anchor.x+tail.extent.x/2;
                int caudalStart=caudal.anchor.x-caudal.extent.x/2;
                int caudalEnd=caudal.anchor.x+caudal.extent.x/2;
                Assert.LessOrEqual(caudalStart,tailEnd);
                Assert.Less(caudalEnd,127,
                    "Caudal fin clipped by right edge");
                Assert.That(dorsal.anchor.x,Is.InRange(
                    trunk.anchor.x-trunk.extent.x/2,
                    trunk.anchor.x+trunk.extent.x/2));
                Assert.Greater(dorsal.anchor.y,trunk.anchor.y);
                // Dorsal fin must overlap the torso's upper edge,
                // rather than floating below the belly as before.
                Assert.GreaterOrEqual(
                    dorsal.anchor.y-dorsal.extent.y/2,
                    trunk.anchor.y);
                var pixels=PixelCreaturePainter.RenderPixels(r);
                var metrics=PixelCreatureVisualMetrics.Analyze(pixels);
                Assert.Less(metrics.maxX,127);
                Assert.Greater(metrics.nonTransparentPixels,150);
            }
        }

        [Test]
        public void TorsoLightBandDoesNotCoverMostOfTheAnimalBack()
        {
            foreach(CreatureArchetype archetype in new[]{
                CreatureArchetype.MarshCrawler,CreatureArchetype.ClayRunner,
                CreatureArchetype.SlateNewt,CreatureArchetype.PaleJuvenile,
                CreatureArchetype.ArmoredCrawler})
            {
                var recipe=PixelArtDirection.Hero(archetype,2046);
                var parts=PixelCreatureDevelopment.Layout(recipe);
                Array.Sort(parts,(a,b)=>a.drawOrder.CompareTo(b.drawOrder));
                int trunkIndex=-1;
                for(int i=0;i<parts.Length;i++)
                    if(parts[i].slot==PartSlot.Trunk){trunkIndex=i;break;}
                Assert.GreaterOrEqual(trunkIndex,0);
                var layer=PixelCreaturePainter.RenderLayerPixels(recipe,trunkIndex);
                var ramp=PixelArtDirection.Ramp(recipe.palette);
                int lit=0,opaque=0;
                foreach(var pixel in layer)
                {
                    if(pixel.a==0)continue;
                    opaque++;
                    if(pixel.Equals(ramp.light) || pixel.Equals(ramp.highlight))
                        lit++;
                }
                Assert.Greater(opaque,100);
                Assert.Less(lit,opaque*.22f,
                    archetype+" has an oversized pale dorsal plate");
            }
        }

        [Test]
        public void OrganicTetrapodTailTapersAndBendsWithoutMovingItsRoot()
        {
            foreach(int width in new[]{13,19,25,29})
            foreach(int wave in new[]{-6,-3,0,3,6})
            {
                var rest=PixelTetrapodTail.Build(94,52,width,0);
                var tail=PixelTetrapodTail.Build(94,52,width,wave);
                Assert.AreEqual(12,tail.outer.Length);
                Assert.Greater(tail.skin.Length,8);
                // Waving happens near the tip; the first station is fixed.
                Assert.AreEqual(rest.outer[0],tail.outer[0]);
                Assert.AreEqual(rest.outer[tail.outer.Length-1],
                    tail.outer[tail.outer.Length-1]);
                Assert.AreEqual(rest.outer[5].y+wave,tail.outer[5].y);
                Assert.Greater(tail.outer[1].y-tail.outer[10].y,
                    tail.outer[4].y-tail.outer[7].y,
                    "Distal tail should taper relative to its fleshy base");
                foreach(var vertex in tail.outer)
                {
                    Assert.That(vertex.x,Is.InRange(0,127));
                    Assert.That(vertex.y,Is.InRange(0,95));
                }
                foreach(var vertex in tail.skin)
                {
                    Assert.That(vertex.x,Is.InRange(0,127));
                    Assert.That(vertex.y,Is.InRange(0,95));
                }
            }
        }

        [Test]
        public void TetrapodTailRootStaysInsideInheritedRump()
        {
            foreach(float length in new[]{0f,.25f,.5f,.75f,1f})
            foreach(float tailLength in new[]{0f,.5f,1f})
            {
                var r=PixelArtDirection.Hero(CreatureArchetype.MarshCrawler,2046);
                r.trunkLength=length;r.tailLength=tailLength;
                var parts=PixelCreatureDevelopment.Layout(r);
                PixelPartDescriptor torso=null,tail=null;
                foreach(var p in parts)
                {
                    if(p.slot==PartSlot.Trunk)torso=p;
                    if(p.slot==PartSlot.Tail)tail=p;
                }
                Assert.NotNull(torso);Assert.NotNull(tail);
                int rump=torso.anchor.x+torso.extent.x/2;
                var shape=PixelTetrapodTail.Build(tail.anchor.x,
                    tail.anchor.y,tail.extent.x,0);
                Assert.LessOrEqual(shape.outer[0].x,rump);
                Assert.Greater(shape.outer[5].x,rump);
                var pixels=PixelCreaturePainter.RenderPixels(r);
                var metrics=PixelCreatureVisualMetrics.Analyze(pixels);
                Assert.Less(metrics.maxX,127);
                Assert.Greater(metrics.dominantFraction,.9f);
            }
        }

        [Test]
        public void WalkingToesStayCompactAcrossAnatomyStyles()
        {
            foreach(PartStyle style in new[]{
                PartStyle.Rounded,PartStyle.Robust,
                PartStyle.Slender,PartStyle.Webbed})
            {
                var toes=PixelDetailGrammar.FootToes(64,26,style);
                Assert.AreEqual(4,toes.Length);
                int left=128,right=0;
                foreach(var toe in toes)
                {
                    left=Mathf.Min(left,toe.x);
                    right=Mathf.Max(right,toe.x);
                    Assert.That(toe.y,Is.InRange(24,26));
                }
                int maxSpread=style==PartStyle.Webbed?14:
                    style==PartStyle.Robust?10:
                    style==PartStyle.Slender?6:8;
                Assert.LessOrEqual(right-left,maxSpread);
            }
        }

        [Test]
        public void SquatTetrapodLegRootIsBroaderThanDistalShin()
        {
            foreach(PartStyle style in new[]{
                PartStyle.Rounded,PartStyle.Robust,PartStyle.Slender})
            foreach(bool farSide in new[]{false,true})
            {
                var limb=PixelLimbSilhouette.Build(42,44,15,
                    style,0,true,farSide);
                Assert.GreaterOrEqual(limb.upperRadius,limb.lowerRadius);
                Assert.Greater(limb.root.y,limb.knee.y);
                Assert.Greater(limb.knee.y,limb.ankle.y);
                var upper=PixelLimbSilhouette.UpperPolygon(limb);
                Assert.GreaterOrEqual(upper.Length,5);
            }
        }

        [Test]
        public void TetrapodBellyIsRaisedOnlyBetweenLimbStations()
        {
            foreach(PartStyle style in new[]{
                PartStyle.Rounded,PartStyle.Robust,PartStyle.Slender})
            {
                float[] upper,lower;
                PixelSilhouetteTemplates.Profile(
                    PartSlot.Trunk,style,out upper,out lower);
                foreach(float u in new[]{0f,.1f,.25f,.29f,.73f,.9f,1f})
                {
                    float original=PixelSilhouetteTemplates.Sample(lower,u);
                    Assert.That(PixelSilhouetteTemplates.TetrapodBellyLower(
                        style,u),Is.EqualTo(original).Within(.0001f));
                }
                float middle=PixelSilhouetteTemplates.TetrapodBellyLower(
                    style,.51f);
                Assert.Greater(middle,
                    PixelSilhouetteTemplates.Sample(lower,.51f)+.1f);
                for(int step=0;step<=100;step++)
                {
                    float u=step/100f;
                    float low=PixelSilhouetteTemplates.TetrapodBellyLower(
                        style,u);
                    float high=PixelSilhouetteTemplates.Sample(upper,u);
                    Assert.Less(low,high);
                    Assert.That(low,Is.InRange(-1f,1f));
                }
            }
        }

        [Test]
        public void BellyRefinementDoesNotDisconnectTetrapodSprites()
        {
            foreach(CreatureArchetype archetype in new[]{
                CreatureArchetype.MarshCrawler,CreatureArchetype.ClayRunner,
                CreatureArchetype.SlateNewt,CreatureArchetype.PaleJuvenile,
                CreatureArchetype.ArmoredCrawler})
            foreach(float depth in new[]{0f,.5f,1f})
            {
                var recipe=PixelArtDirection.Hero(archetype,2046);
                recipe.trunkDepth=depth;
                var metrics=PixelCreatureVisualMetrics.Analyze(
                    PixelCreaturePainter.RenderPixels(recipe));
                Assert.Greater(metrics.nonTransparentPixels,150);
                Assert.Greater(metrics.dominantFraction,.9f,
                    archetype+" disconnected with belly depth "+depth);
                Assert.Less(metrics.maxX,127);
            }
        }

        [Test]
        public void ForegroundThighCatchlightStaysNearShoulder()
        {
            foreach(PartStyle style in new[]{
                PartStyle.Slender,PartStyle.Rounded,
                PartStyle.Robust,PartStyle.Webbed})
            foreach(int legHeight in new[]{10,14,18,23})
            foreach(int stride in new[]{-3,0,3})
            {
                var shape=PixelLimbSilhouette.Build(
                    45,43,legHeight,style,stride,true,false);
                var highlight=PixelLimbSilhouette.ShoulderHighlight(shape);
                Assert.AreEqual(2,highlight.Length);
                Assert.AreEqual(shape.root.y,highlight[0].y);
                Assert.Greater(highlight[0].y,highlight[1].y);
                int thighHeight=shape.root.y-shape.knee.y;
                Assert.LessOrEqual(highlight[0].y-highlight[1].y,
                    Mathf.Max(2,thighHeight/3));
                Assert.GreaterOrEqual(highlight[1].y,shape.knee.y+1);
                Assert.LessOrEqual(Mathf.Abs(highlight[1].x-shape.root.x),
                    Mathf.Abs(shape.knee.x-shape.root.x)+1);
            }
        }

        [Test]
        public void CompactThighHighlightsKeepFullCreatureConnected()
        {
            foreach(CreatureArchetype archetype in new[]{
                CreatureArchetype.MarshCrawler,CreatureArchetype.ClayRunner,
                CreatureArchetype.SlateNewt,CreatureArchetype.PaleJuvenile,
                CreatureArchetype.ArmoredCrawler})
            foreach(int frame in new[]{0,1,2,3})
            {
                var r=PixelArtDirection.Hero(archetype,2046);
                var pixels=PixelCreaturePainter.RenderPixels(r,frame,false);
                var metrics=PixelCreatureVisualMetrics.Analyze(pixels);
                Assert.Greater(metrics.nonTransparentPixels,150);
                Assert.Greater(metrics.dominantFraction,.9f,
                    archetype+" frame "+frame+" has detached anatomy");
            }
        }

        [Test]
        public void ForeAndHindLimbsLeanInOppositeAnatomicalDirections()
        {
            foreach(PartStyle style in new[]{
                PartStyle.Rounded,PartStyle.Robust,
                PartStyle.Slender,PartStyle.Webbed})
            foreach(int height in new[]{10,15,19,23})
            foreach(int stride in new[]{-3,0,3})
            {
                var front=PixelLimbSilhouette.Build(
                    45,44,height,style,stride,true,false);
                var rear=PixelLimbSilhouette.Build(
                    75,44,height,style,stride,false,false);
                Assert.Less(front.knee.x,front.root.x);
                Assert.Greater(rear.knee.x,rear.root.x);
                Assert.That(front.ankle.x-front.root.x,Is.InRange(-9,9));
                Assert.That(rear.ankle.x-rear.root.x,Is.InRange(-9,9));
                Assert.Greater(front.root.y,front.knee.y);
                Assert.Greater(front.knee.y,front.ankle.y);
                Assert.Greater(rear.root.y,rear.knee.y);
                Assert.Greater(rear.knee.y,rear.ankle.y);
                Assert.AreEqual(8,
                    PixelLimbSilhouette.UpperPolygon(front).Length);
                Assert.AreEqual(8,
                    PixelLimbSilhouette.LowerPolygon(rear).Length);
            }
        }

        [Test]
        public void CurvedLimbsPreserveCreatureConnectivityAcrossGait()
        {
            foreach(CreatureArchetype archetype in new[]{
                CreatureArchetype.MarshCrawler,
                CreatureArchetype.ClayRunner,
                CreatureArchetype.SlateNewt,
                CreatureArchetype.PaleJuvenile,
                CreatureArchetype.ArmoredCrawler})
            foreach(int frame in new[]{0,1,2,3})
            {
                var recipe=PixelArtDirection.Hero(archetype,2046);
                var pixels=PixelCreaturePainter.RenderPixels(recipe,frame,false);
                var metrics=PixelCreatureVisualMetrics.Analyze(pixels);
                Assert.Greater(metrics.nonTransparentPixels,150);
                Assert.Greater(metrics.dominantFraction,.90f,
                    archetype+" gait frame "+frame+" detached the limbs");
                Assert.Greater(metrics.minX,0);
                Assert.Less(metrics.maxX,127);
            }
        }

        [Test]
        public void StockyBodyPlanHasShorterLegsAndWiderStanceThanRunner()
        {
            var stocky=PixelArtDirection.Hero(CreatureArchetype.MarshCrawler,2046);
            var runner=stocky.Copy();
            runner.trunkStyle=PartStyle.Slender;
            // Hold inherited lengths constant to isolate the body-plan rule.
            foreach(float legGene in new[]{0f,.25f,.5f,.75f,1f})
            foreach(float bodyGene in new[]{0f,.5f,1f})
            {
                stocky.legLength=runner.legLength=legGene;
                stocky.trunkLength=runner.trunkLength=bodyGene;
                var stout=PixelCreatureDevelopment.Layout(stocky);
                var slim=PixelCreatureDevelopment.Layout(runner);
                PixelPartDescriptor sf=null,sh=null,rf=null,rh=null;
                foreach(var part in stout)
                {
                    if(part.slot==PartSlot.ForeLeg && part.drawOrder==9)sf=part;
                    if(part.slot==PartSlot.HindLeg && part.drawOrder==9)sh=part;
                }
                foreach(var part in slim)
                {
                    if(part.slot==PartSlot.ForeLeg && part.drawOrder==9)rf=part;
                    if(part.slot==PartSlot.HindLeg && part.drawOrder==9)rh=part;
                }
                Assert.NotNull(sf);Assert.NotNull(sh);
                Assert.NotNull(rf);Assert.NotNull(rh);
                Assert.LessOrEqual(sf.extent.y,rf.extent.y);
                Assert.LessOrEqual(sh.extent.y,rh.extent.y);
                Assert.Greater(sh.anchor.x-sf.anchor.x,
                    rh.anchor.x-rf.anchor.x,
                    "Stocky fore/hind spacing should exceed the runner's");
            }
        }

        [Test]
        public void SquatAndRunnerRenderingsStayInsideCanvas()
        {
            foreach(CreatureArchetype archetype in new[]{
                CreatureArchetype.MarshCrawler,
                CreatureArchetype.ArmoredCrawler,
                CreatureArchetype.ClayRunner,
                CreatureArchetype.PaleJuvenile})
            foreach(int phase in new[]{0,1,2,3})
            {
                var recipe=PixelArtDirection.Hero(archetype,2046);
                var metrics=PixelCreatureVisualMetrics.Analyze(
                    PixelCreaturePainter.RenderPixels(recipe,phase,false));
                Assert.Greater(metrics.nonTransparentPixels,150);
                Assert.Greater(metrics.dominantFraction,.9f);
                Assert.Greater(metrics.minX,0);
                Assert.Less(metrics.maxX,127);
                Assert.Greater(metrics.minY,0);
                Assert.Less(metrics.maxY,95);
            }
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
