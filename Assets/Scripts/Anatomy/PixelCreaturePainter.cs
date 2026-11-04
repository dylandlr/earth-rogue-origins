using System;
using UnityEngine;

namespace EarthRogueOrigins.PixelCreatures
{
    /// <summary>
    /// Original procedural pixel artist: no source-image extraction, image models,
    /// rescaling of existing sprites, or Unity global randomness.
    /// 128x96 RGBA, exactly one logical pixel per texture pixel.
    /// </summary>
    public static class PixelCreaturePainter
    {
        public const int Width=128,Height=96;
        static readonly Color32 Clear=new Color32(0,0,0,0);
        static readonly Color32 Ink=new Color32(26,29,25,255);
        static readonly Color32 EyeWhite=new Color32(237,224,184,255);
        static readonly Color32 Black=new Color32(15,19,17,255);

        public static Texture2D Render(PixelCreatureRecipe recipe)
        {
            return Render(recipe,0,false);
        }

        public static Texture2D Render(PixelCreatureRecipe recipe,int frame,bool swimming)
        {
            recipe.Validate();
            return TextureFromPixels(RenderPixels(recipe,frame,swimming));
        }

        public static Color32[] RenderPixels(PixelCreatureRecipe recipe)
        {
            return RenderPixels(recipe,0,false);
        }

        public static Color32[] RenderPixels(PixelCreatureRecipe recipe,int frame,bool swimming)
        {
            recipe.Validate();
            var c=new Canvas(PixelArtDirection.Ramp(recipe.palette).ink);
            var parts=PixelCreaturePoses.Layout(recipe,frame,swimming);
            Array.Sort(parts,(a,b)=>a.drawOrder.CompareTo(b.drawOrder));
            foreach(var part in parts)
                DrawPart(c,part,recipe);
            return c.pixels;
        }

        public static Color32[] RenderLayerPixels(PixelCreatureRecipe recipe,int partIndex)
        {
            return RenderLayerPixels(recipe,partIndex,0,false);
        }

        public static Color32[] RenderLayerPixels(PixelCreatureRecipe recipe,int partIndex,
            int frame,bool swimming)
        {
            recipe.Validate();
            var parts=PixelCreaturePoses.Layout(recipe,frame,swimming);
            Array.Sort(parts,(a,b)=>a.drawOrder.CompareTo(b.drawOrder));
            if(partIndex<0 || partIndex>=parts.Length)throw new ArgumentOutOfRangeException(nameof(partIndex));
            // Context-sensitive parts (especially markings) need the preceding
            // layers as a mask. Output only pixels actually changed by this part.
            // This ensures ordered exported layers reproduce the assembled sprite.
            var canvas=new Canvas(PixelArtDirection.Ramp(recipe.palette).ink);
            for(int i=0;i<partIndex;i++)DrawPart(canvas,parts[i],recipe);
            var previous=(Color32[])canvas.pixels.Clone();
            DrawPart(canvas,parts[partIndex],recipe);
            var output=new Color32[Width*Height];
            for(int i=0;i<output.Length;i++)
                output[i]=canvas.pixels[i].Equals(previous[i])?Clear:canvas.pixels[i];
            return output;
        }

        /// <summary>
        /// Independent ground-contact shadow. It never becomes a creature layer:
        /// world rendering sorts it behind the organism, while export preserves
        /// full transparency everywhere else.
        /// </summary>
        public static Color32[] RenderGroundShadowPixels(PixelCreatureRecipe recipe)
        {
            recipe.Validate();
            var pixels=new Color32[Width*Height];
            if(recipe.plan==CreaturePlan.Fish)return pixels;
            // Resolve the anatomical ground plane from the same near-side
            // feet used by the gait renderer. A fixed Y=23 shadow floated
            // beneath short-legged animals and clipped long-legged ones.
            int cx=62,cy=23;
            if(recipe.plan==CreaturePlan.Tetrapod)
            {
                var parts=PixelCreatureDevelopment.Layout(recipe);
                int contacts=0,contactY=0;
                foreach(var part in parts)
                {
                    if((part.slot!=PartSlot.ForeLeg &&
                        part.slot!=PartSlot.HindLeg) || part.drawOrder!=9)
                        continue;
                    var foot=PixelLimbSilhouette.Build(part.anchor.x,
                        part.anchor.y,part.extent.y,part.style,0,
                        part.slot==PartSlot.ForeLeg,false);
                    contactY+=foot.ankle.y-3;
                    contacts++;
                }
                if(contacts>0)cy=Mathf.Clamp(
                    Mathf.RoundToInt(contactY/(float)contacts)-2,5,Height-6);
            }
            int rx=Mathf.RoundToInt(Mathf.Lerp(25f,38f,recipe.trunkLength));
            int ry=Mathf.RoundToInt(Mathf.Lerp(3f,5f,recipe.trunkDepth));
            for(int y=-ry-2;y<=ry+2;y++)
            for(int x=-rx-3;x<=rx+3;x++)
            {
                float u=x/(float)Mathf.Max(1,rx),v=y/(float)Mathf.Max(1,ry);
                float d=u*u+v*v;
                if(d>1.5f)continue;
                int px=cx+x,py=cy+y;
                if(px<0||px>=Width||py<0||py>=Height)continue;
                float opacity=Mathf.Clamp01((1.5f-d)*.50f);
                pixels[py*Width+px]=new Color32(20,26,19,
                    (byte)Mathf.RoundToInt(opacity*112f));
            }
            return pixels;
        }

        public static Texture2D TextureFromPixels(Color32[] data)
        {
            if(data==null || data.Length!=Width*Height)
                throw new ArgumentException("Expected 128x96 RGBA pixel layer.");
            var texture=new Texture2D(Width,Height,TextureFormat.RGBA32,false);
            texture.filterMode=FilterMode.Point;
            texture.wrapMode=TextureWrapMode.Clamp;
            texture.SetPixels32(data);
            texture.Apply(false,false);
            return texture;
        }

        static void DrawPart(Canvas c,PixelPartDescriptor d,PixelCreatureRecipe r)
        {
            int x=d.anchor.x,y=d.anchor.y,w=d.extent.x,h=d.extent.y;
            if(d.slot==PartSlot.Trunk)
            {
                if(d.style==PartStyle.Slender)h=Mathf.Max(11,h-6);
                if(d.style==PartStyle.Robust)h+=5;
            }
            var palette=PixelArtDirection.Ramp(r.palette);
            var dark=palette.shadow;
            var light=palette.light;
            switch(d.slot)
            {
                case PartSlot.Trunk:
                    PaintProfile(c,PartSlot.Trunk,d.style,x,y,w,h,palette,r.baseColor);
                    if(d.style==PartStyle.Segmented)
                    {
                        int segments=7+Mathf.RoundToInt(r.armor*5);
                        for(int i=1;i<segments;i++)
                        {
                            int sx=x-w/2+Mathf.RoundToInt(i*w/(float)segments);
                            int sy=y;
                            int height=Mathf.RoundToInt(h*.35f+
                                Mathf.Sin(Mathf.PI*i/segments)*h*.23f);
                            Line(c,sx,sy-height,sx+1,sy+height,c.ink,1);
                            Line(c,sx+2,sy-height+3,sx+2,sy-2,
                                Shade(r.accentColor,.9f),1);
                        }
                    }
                    break;

                case PartSlot.Head:
                    PaintProfile(c,PartSlot.Head,d.style,x,y,w,h,palette,r.baseColor);
                    // Organic cheek and raised brow preserve character at 32 px.
                    Ellipse(c,x-2,y+Mathf.Max(2,h/5),Mathf.Max(3,w/3),
                        Mathf.Max(2,h/6),palette.light);
                    break;

                case PartSlot.FaceDetails:
                    var face=PixelFaceGrammar.Landmarks(r.plan,d.style,x,y,w,h);
                    // Subtle dark cheek crescent follows the jaw's attachment.
                    Ellipse(c,face.cheek.x,face.cheek.y,
                        face.cheekRadius+1,face.jawDepth+1,palette.shadow);
                    Ellipse(c,face.cheek.x-1,face.cheek.y+1,
                        face.cheekRadius,face.jawDepth,r.baseColor);
                    if(r.plan!=CreaturePlan.Arthropod)
                    {
                        // Deliberately broken lip stroke reads as a smile at
                        // half-size and a distinct jaw at native resolution.
                        Line(c,face.mouthStart.x,face.mouthStart.y,
                            face.mouthEnd.x,face.mouthEnd.y,palette.ink,1);
                        Line(c,face.mouthStart.x+1,face.mouthStart.y-2,
                            face.mouthEnd.x-2,face.mouthEnd.y-2,
                            palette.highlight,1);
                        c.Put(face.nostril.x,face.nostril.y,palette.shadow);
                        c.Put(face.nostril.x,face.nostril.y+1,palette.ink);
                    }
                    break;

                case PartSlot.TailRootBlend:
                    // Overlap covers the dark tail/body join with an inset,
                    // tapered skin bridge. Drawn before markings and armor.
                    Polygon(c,new[]{
                        new Vector2Int(x-w/2-2,y-2),
                        new Vector2Int(x+w/2+1,y+4),
                        new Vector2Int(x+w/2+1,y-3),
                        new Vector2Int(x-w/2-2,y-5)
                    },palette.shadow);
                    Polygon(c,new[]{
                        new Vector2Int(x-w/2,y),
                        new Vector2Int(x+w/2-1,y+4),
                        new Vector2Int(x+w/2-1,y-1),
                        new Vector2Int(x-w/2,y-3)
                    },r.baseColor);
                    Line(c,x-2,y+3,x+w/2-2,y+3,palette.light,1);
                    break;

                case PartSlot.NeckBlend:
                    // A controlled occlusion patch unifies the back of the head
                    // with the forward shoulder without erasing the snout.
                    Ellipse(c,x,y,Mathf.Max(3,w/2),Mathf.Max(2,h/2),palette.shadow);
                    Ellipse(c,x-1,y+2,Mathf.Max(2,w/2-2),Mathf.Max(2,h/2-2),r.baseColor);
                    Line(c,x-w/2+2,y+Mathf.Max(1,h/2-1),x+1,y+Mathf.Max(1,h/2-1),
                        palette.light,1);
                    break;

                case PartSlot.ShoulderBlend:
                case PartSlot.HipBlend:
                    // Joint cap is painted *after* the foreground limb, hiding
                    // its polygonal origin in the torso instead of leaving a seam.
                    Ellipse(c,x,y,Mathf.Max(3,w/2),Mathf.Max(3,h/2),palette.shadow);
                    Ellipse(c,x-1,y+2,Mathf.Max(2,w/2-2),Mathf.Max(2,h/2-2),r.baseColor);
                    break;

                case PartSlot.Eye:
                    // Small raised brow, dark socket, glossy pupil and a single
                    // light glint: no white sclera at normal game scale.
                    int eyeRadius=Mathf.Max(2,Mathf.RoundToInt(1+r.eyeSize*2f));
                    Ellipse(c,x,y,eyeRadius+1,eyeRadius+1,palette.shadow);
                    Ellipse(c,x,y,eyeRadius,eyeRadius,palette.ink);
                    if(eyeRadius>=3)c.Put(x+1,y-1,Black);
                    c.Put(x-1,y+1,EyeWhite);
                    Line(c,x-eyeRadius,y+eyeRadius+1,
                        x+eyeRadius-1,y+eyeRadius+1,palette.light,1);
                    break;

                case PartSlot.ForeLeg:
                case PartSlot.HindLeg:
                    if(r.plan==CreaturePlan.Arthropod)
                    {
                        for(int i=0;i<3;i++)
                        {
                            int shift=i*8-8;
                            int knee=x+shift+(d.slot==PartSlot.HindLeg?3:-3);
                            int foot=knee+(i%2==0?7:-8);
                            Line(c,x+shift,y,knee,y-h/2,c.ink,4);
                            Line(c,knee,y-h/2,foot,y-h,c.ink,3);
                            Line(c,x+shift,y+1,knee,y-h/2+1,r.secondaryColor,2);
                        }
                    }
                    else
                    {
                        bool farSide=d.drawOrder<2;
                        bool foreleg=d.slot==PartSlot.ForeLeg;
                        var shape=PixelLimbSilhouette.Build(
                            x,y,h,d.style,d.mirrorGroup,foreleg,farSide,d.footLift);
                        // The far leg is darker and narrower. Near legs use a
                        // shaded shoulder and an independently bent shin.
                        Color32 skin=farSide?palette.shadow:r.baseColor;
                        Color32 underside=farSide?palette.ink:palette.shadow;
                        Polygon(c,PixelLimbSilhouette.UpperPolygon(shape),c.ink);
                        Polygon(c,PixelLimbSilhouette.LowerPolygon(shape),c.ink);
                        // Inset material follows knee and ankle directions,
                        // avoiding the single straight limb-bar silhouette.
                        Line(c,shape.root.x,shape.root.y,
                            shape.knee.x,shape.knee.y,skin,
                            Mathf.Max(2,shape.upperRadius*2-3));
                        Line(c,shape.knee.x,shape.knee.y,
                            shape.ankle.x,shape.ankle.y,underside,
                            Mathf.Max(2,shape.lowerRadius*2-2));
                        if(!farSide)
                        {
                            Line(c,shape.root.x-1,shape.root.y+1,
                                shape.knee.x-1,shape.knee.y+2,
                                palette.light,2);
                            c.Put(shape.knee.x-1,shape.knee.y+1,r.baseColor);
                        }
                        int ground=shape.ankle.y-2;
                        int footX=shape.ankle.x;
                        int toeSpread=d.style==PartStyle.Webbed?8:
                            d.style==PartStyle.Robust?7:5;
                        // Three distinct digit clusters taper toward the
                        // ground and stay connected to the ankle.
                        Ellipse(c,footX,ground+1,toeSpread-2,3,c.ink);
                        Ellipse(c,footX,ground+2,
                            Mathf.Max(2,toeSpread-4),2,skin);
                        var toes=PixelDetailGrammar.FootToes(footX,ground,d.style);
                        foreach(var toe in toes)
                        {
                            Line(c,footX,ground+1,toe.x,toe.y,c.ink,2);
                            c.Put(toe.x,toe.y+1,
                                farSide?palette.shadow:palette.highlight);
                        }
                        if(d.style==PartStyle.Webbed)
                        {
                            Polygon(c,new[]{
                                new Vector2Int(footX-toeSpread,ground+1),
                                new Vector2Int(footX+toeSpread,ground+1),
                                new Vector2Int(footX,ground+5)
                            },palette.shadow);
                            Line(c,footX-toeSpread+2,ground+2,
                                footX+toeSpread-2,ground+2,
                                palette.light,1);
                        }
                    }
                    break;

                case PartSlot.Tail:
                    int tailWave=d.mirrorGroup;
                    if(r.plan==CreaturePlan.Fish)
                    {
                        Polygon(c,new[]{
                            new Vector2Int(x-w/2-2,y+3+tailWave),new Vector2Int(x+w/2,y+7),
                            new Vector2Int(x+w/2,y-7),new Vector2Int(x-w/2-2,y-3+tailWave)
                        },c.ink);
                        Polygon(c,new[]{
                            new Vector2Int(x-w/2,y+2+tailWave),new Vector2Int(x+w/2-2,y+5),
                            new Vector2Int(x+w/2-2,y-5),new Vector2Int(x-w/2,y-2+tailWave)
                        },r.secondaryColor);
                    }
                    else if(d.style==PartStyle.Fan)
                    {
                        Polygon(c,new[]{
                            new Vector2Int(x-w/2-5,y+12+tailWave),
                            new Vector2Int(x+w/2,y+7),
                            new Vector2Int(x+w/2,y-4),
                            new Vector2Int(x-w/2-5,y-12+tailWave)
                        },r.secondaryColor);
                        Line(c,x-w/2,y+tailWave,x+w/2,y,r.accentColor,2);
                    }
                    else
                    {
                        // Tapered fleshy root and softened blunt end instead
                        // of a sharp wedge disconnected from the rump.
                        Polygon(c,new[]{
                            new Vector2Int(x-w/2-5,y+7+tailWave),
                            new Vector2Int(x-w/2+2,y+9+tailWave),
                            new Vector2Int(x+w/2-5,y+4),
                            new Vector2Int(x+w/2+2,y+2),
                            new Vector2Int(x+w/2+2,y-2),
                            new Vector2Int(x+w/2-5,y-4),
                            new Vector2Int(x-w/2+1,y-7+tailWave),
                            new Vector2Int(x-w/2-5,y-5+tailWave)
                        },c.ink);
                        Polygon(c,new[]{
                            new Vector2Int(x-w/2-3,y+5+tailWave),
                            new Vector2Int(x-w/2+1,y+7+tailWave),
                            new Vector2Int(x+w/2-5,y+3),
                            new Vector2Int(x+w/2,y+1),
                            new Vector2Int(x+w/2,y-1),
                            new Vector2Int(x+w/2-5,y-2),
                            new Vector2Int(x-w/2+1,y-5+tailWave),
                            new Vector2Int(x-w/2-3,y-3+tailWave)
                        },r.baseColor);
                        Line(c,x-w/2,y+4+tailWave,x+2,y+3,
                            palette.light,1);
                    }
                    break;

                case PartSlot.Fin:
                    Polygon(c,new[]{
                        new Vector2Int(x-w/2,y),new Vector2Int(x+w/2,y+h/2),
                        new Vector2Int(x+w/2-3,y),
                        new Vector2Int(x+w/2,y-h/2)
                    },c.ink);
                    Polygon(c,new[]{
                        new Vector2Int(x-w/2+3,y),new Vector2Int(x+w/2-3,y+h/2-3),
                        new Vector2Int(x+w/2-5,y),
                        new Vector2Int(x+w/2-3,y-h/2+3)
                    },r.accentColor);
                    break;

                case PartSlot.Armor:
                    int n=Mathf.RoundToInt(r.armor*(r.plan==CreaturePlan.Arthropod?11:8));
                    if(r.plan==CreaturePlan.Arthropod)n=Mathf.Max(6,n);
                    for(int i=0;i<n;i++)
                    {
                        int ax=x-w/2+Mathf.RoundToInt((i+.5f)*w/Mathf.Max(1,n));
                        float t=(ax-(x-w/2))/(float)Mathf.Max(1,w);
                        int curve=Mathf.RoundToInt(Mathf.Sin(t*Mathf.PI)*h*.16f);
                        int ay=y+curve;
                        Polygon(c,new[]{
                            new Vector2Int(ax-4,ay),new Vector2Int(ax-1,ay+4+Mathf.RoundToInt(r.armor*5)),
                            new Vector2Int(ax+4,ay)
                        },c.ink);
                        Polygon(c,new[]{
                            new Vector2Int(ax-2,ay+1),new Vector2Int(ax-1,ay+3+Mathf.RoundToInt(r.armor*4)),
                            new Vector2Int(ax+2,ay+1)
                        },r.accentColor);
                    }
                    break;

                case PartSlot.Markings:
                    DrawMarkings(c,x,y,w,h,r,palette);
                    break;
            }
        }

        /// <summary>
        /// Shape-aware rasterization: define an exact silhouette mask, dilate
        /// that mask by 1px for ink and paint only within the silhouette.
        /// Avoids the rectangular ink bars produced by per-column borders.
        /// Each pixel is still an original integer-grid pixel.
        /// </summary>
        static void PaintProfile(Canvas canvas,PartSlot slot,PartStyle style,
            int cx,int cy,int width,int height,PixelColorRamp ramp,Color32 baseColor)
        {
            int left=cx-width/2,right=cx+width/2;
            int minX=Mathf.Max(1,left-3),maxX=Mathf.Min(Width-2,right+3);
            int minY=Mathf.Max(1,cy-height/2-5);
            int maxY=Mathf.Min(Height-2,cy+height/2+5);
            var mask=new bool[Width*Height];
            var interior=new bool[Width*Height];

            float[] top,bottom;
            PixelSilhouetteTemplates.Profile(slot,style,out top,out bottom);
            for(int x=left;x<=right;x++)
            {
                float u=(x-left)/(float)Mathf.Max(1,right-left);
                int high=cy+Mathf.RoundToInt(
                    PixelSilhouetteTemplates.Sample(top,u)*height*.5f);
                int low=cy+Mathf.RoundToInt(
                    PixelSilhouetteTemplates.Sample(bottom,u)*height*.5f);
                for(int y=Mathf.Max(minY,low);y<=Mathf.Min(maxY,high);y++)
                    mask[y*Width+x]=true;
            }

            // Native pixel outline is a true morphological dilation of the
            // anatomical mask, with no rectangular corner artifacts.
            for(int y=minY;y<=maxY;y++)
            for(int x=minX;x<=maxX;x++)
            {
                int index=y*Width+x;
                if(mask[index])continue;
                bool near=false;
                for(int oy=-1;oy<=1&&!near;oy++)
                for(int ox=-1;ox<=1;ox++)
                    if(mask[(y+oy)*Width+x+ox]){near=true;break;}
                if(near)canvas.Put(x,y,ramp.ink);
            }

            for(int y=minY;y<=maxY;y++)
            for(int x=minX;x<=maxX;x++)
            {
                int index=y*Width+x;
                if(!mask[index])continue;
                bool edge=false;
                for(int oy=-1;oy<=1&&!edge;oy++)
                for(int ox=-1;ox<=1;ox++)
                    if(!mask[(y+oy)*Width+x+ox]){edge=true;break;}
                if(edge)
                {
                    canvas.Put(x,y,ramp.ink);
                    continue;
                }
                interior[index]=true;
                // This ramp uses coherent material zones rather than random
                // texture noise: underside, midtone, dorsal and lit shoulder.
                Color32 fill=y<cy-height*.08f?ramp.shadow:baseColor;
                if(y>cy+height*.12f && x<cx+width*.24f)fill=ramp.light;
                if(y>cy+height*.28f && x<cx-width*.12f)fill=ramp.highlight;
                canvas.Put(x,y,fill);
            }

            // Broken, organic dorsal flecks are constrained to the painted
            // interior. Never paint through the black contour into empty air.
            if(slot==PartSlot.Trunk)
            {
                for(int i=0;i<4;i++)
                {
                    int x=cx-width/3+i*Mathf.Max(2,width/7);
                    int y=cy+Mathf.RoundToInt(height*(.22f+.04f*(i%2)));
                    int index=y*Width+x;
                    if(x>0&&x<Width&&y>0&&y<Height&&interior[index])
                        canvas.Put(x,y,ramp.highlight);
                }
            }
        }

        static void DrawMarkings(Canvas c,int x,int y,int w,int h,
            PixelCreatureRecipe r,PixelColorRamp palette)
        {
            if(r.pattern==MarkingPattern.None || r.spots<=.01f)return;
            var rng=new System.Random(unchecked(r.seed*7919+23));
            if(r.pattern==MarkingPattern.DorsalStripe)
            {
                int start=x-w/2+7,end=x+w/2-7;
                for(int px=start;px<=end;px++)
                {
                    float t=(px-start)/(float)Mathf.Max(1,end-start);
                    int py=y+Mathf.RoundToInt(Mathf.Sin(t*Mathf.PI)*h*.21f);
                    for(int dy=0;dy<=Mathf.RoundToInt(1+r.spots*3);dy++)
                        c.MarkBody(px,py+dy,palette.pattern);
                }
                return;
            }
            if(r.pattern==MarkingPattern.TailBands)
            {
                for(int i=1;i<=5;i++)
                {
                    int px=x-w/2+i*w/6;
                    int halfHeight=Mathf.RoundToInt((h*.28f)*Mathf.Sin(Mathf.PI*i/6f));
                    for(int yy=y-halfHeight;yy<=y+halfHeight;yy++)
                        c.MarkBody(px+(yy%2==0?0:1),yy,palette.pattern);
                }
                return;
            }
            if(r.pattern==MarkingPattern.MottledBack)
            {
                foreach(var mark in PixelDetailGrammar.DorsalMottle(r,x,y,w,h))
                {
                    for(int yy=-mark.radius;yy<=mark.radius;yy++)
                    for(int xx=-mark.radius;xx<=mark.radius;xx++)
                        if(xx*xx+yy*yy<=mark.radius*mark.radius)
                            c.MarkBody(mark.x+xx,mark.y+yy,mark.pigment);
                }
                return;
            }
            int count=Mathf.RoundToInt(r.spots*(r.pattern==MarkingPattern.Sparse?9:28));
            for(int i=0;i<count;i++)
            {
                int px=x-w/2+5+rng.Next(Mathf.Max(1,w-10));
                int py=y-h/2+4+rng.Next(Mathf.Max(1,h-8));
                float dx=(px-x)/(float)Mathf.Max(1,w/2-4);
                float dy=(py-y)/(float)Mathf.Max(1,h/2-4);
                if(dx*dx+dy*dy>.72f)continue;
                if(r.pattern==MarkingPattern.MottledBack && py<y)continue;
                if(r.pattern==MarkingPattern.SideFreckles && (py<y-h*.28f || py>y+h*.18f))
                    continue;
                int radius=r.pattern==MarkingPattern.SideFreckles?1:1+rng.Next(3);
                for(int dy=-radius;dy<=radius;dy++)
                for(int dx=-radius;dx<=radius;dx++)
                    if(dx*dx+dy*dy<=radius*radius)
                        c.MarkBody(px+dx,py+dy,palette.pattern);
                if(radius>=2)c.MarkBody(px-1,py+1,palette.light);
            }
        }

        static Color32 Shade(Color32 c,float mul)
        {
            return new Color32((byte)Mathf.Clamp(c.r*mul,0,255),
                (byte)Mathf.Clamp(c.g*mul,0,255),(byte)Mathf.Clamp(c.b*mul,0,255),255);
        }

        sealed class Canvas
        {
            public readonly Color32[] pixels=new Color32[Width*Height];
            public readonly Color32 ink;
            public Canvas(Color32 contour)
            {
                ink=contour;
                for(int i=0;i<pixels.Length;i++)pixels[i]=Clear;
            }
            public void Put(int x,int y,Color32 c)
            {
                if(x>=0&&x<Width&&y>=0&&y<Height)pixels[y*Width+x]=c;
            }
            public void MarkBody(int x,int y,Color32 color)
            {
                if(x<0||x>=Width||y<0||y>=Height)return;
                int index=y*Width+x;
                var existing=pixels[index];
                if(existing.a==0 || (existing.r==ink.r && existing.g==ink.g &&
                    existing.b==ink.b))return;
                pixels[index]=color;
            }
        }
        static void Dot(Canvas c,int x,int y,int r,Color32 color){Ellipse(c,x,y,r,r,color);}
        static void Ellipse(Canvas c,int cx,int cy,int rx,int ry,Color32 color)
        {
            rx=Mathf.Max(1,rx);ry=Mathf.Max(1,ry);
            for(int y=-ry;y<=ry;y++)
            for(int x=-rx;x<=rx;x++)
                if(x*x/(float)(rx*rx)+y*y/(float)(ry*ry)<=1f)
                    c.Put(cx+x,cy+y,color);
        }
        static void Line(Canvas c,int x0,int y0,int x1,int y1,Color32 color,int thickness)
        {
            int n=Mathf.Max(1,Mathf.Max(Mathf.Abs(x1-x0),Mathf.Abs(y1-y0)));
            for(int i=0;i<=n;i++)
            {
                float t=i/(float)n;
                Ellipse(c,Mathf.RoundToInt(Mathf.Lerp(x0,x1,t)),
                    Mathf.RoundToInt(Mathf.Lerp(y0,y1,t)),
                    Mathf.Max(1,thickness/2),Mathf.Max(1,thickness/2),color);
            }
        }
        static void Polygon(Canvas c,Vector2Int[] points,Color32 color)
        {
            if(points.Length<3)return;
            for(int i=1;i<points.Length-1;i++)
                Triangle(c,points[0],points[i],points[i+1],color);
        }
        static void Triangle(Canvas c,Vector2Int a,Vector2Int b,Vector2Int d,Color32 color)
        {
            int minX=Mathf.Max(0,Mathf.Min(a.x,Mathf.Min(b.x,d.x)));
            int maxX=Mathf.Min(Width-1,Mathf.Max(a.x,Mathf.Max(b.x,d.x)));
            int minY=Mathf.Max(0,Mathf.Min(a.y,Mathf.Min(b.y,d.y)));
            int maxY=Mathf.Min(Height-1,Mathf.Max(a.y,Mathf.Max(b.y,d.y)));
            float area=Edge(a,b,d);
            if(Mathf.Abs(area)<.01f)return;
            for(int y=minY;y<=maxY;y++)
            for(int x=minX;x<=maxX;x++)
            {
                var p=new Vector2Int(x,y);
                float u=Edge(b,d,p)/area,v=Edge(d,a,p)/area,w=1-u-v;
                if(u>=0&&v>=0&&w>=0)c.Put(x,y,color);
            }
        }
        static float Edge(Vector2Int a,Vector2Int b,Vector2Int p)
        {
            return (p.x-a.x)*(float)(b.y-a.y)-(p.y-a.y)*(float)(b.x-a.x);
        }
    }
}
