#if UNITY_EDITOR
using System;
using System.IO;
using System.Text;
using EarthRogueOrigins.PixelCreatures;
using UnityEditor;
using UnityEngine;

namespace EarthRogueOrigins.Editor
{
    /// <summary>
    /// Exercises the authoritative C# renderer against the actual sampled
    /// v2.0 head profiles. Exports a 4x3 128px contact sheet and fails if any
    /// anatomical landmark leaves the head or an expected eye is invisible.
    /// </summary>
    public static class FaceContourAudit
    {
        const string Folder="ValidationResults/FaceContour";
        static readonly PartStyle[] Styles={
            PartStyle.Rounded,PartStyle.Robust,
            PartStyle.Wedge,PartStyle.LongSnout
        };
        static readonly float[] Sizes={0f,.5f,1f};

        [MenuItem("Earth: Rogue Origins/Art/Pixel Creatures/Verify Face Contours")]
        public static void Run()
        {
            Directory.CreateDirectory(Folder);
            const int tileW=PixelCreaturePainter.Width;
            const int tileH=PixelCreaturePainter.Height;
            int outputWidth=tileW*Styles.Length;
            int outputHeight=tileH*Sizes.Length;
            var canvas=new Color32[outputWidth*outputHeight];
            var background=new Color32(31,40,33,255);
            for(int i=0;i<canvas.Length;i++)canvas[i]=background;
            var report=new StringBuilder("style,head_size,eye_x,eye_y,nostril_x,nostril_y,mouth_x,mouth_y,landmarks_valid,eye_ink_visible\n");
            int failures=0;
            for(int row=0;row<Sizes.Length;row++)
            for(int col=0;col<Styles.Length;col++)
            {
                var recipe=PixelArtDirection.Hero(CreatureArchetype.MarshCrawler,2046);
                recipe.headStyle=Styles[col];
                recipe.headSize=Sizes[row];
                var layout=PixelCreatureDevelopment.Layout(recipe);
                PixelPartDescriptor head=null,eye=null;
                foreach(var part in layout)
                {
                    if(part.slot==PartSlot.Head)head=part;
                    if(part.slot==PartSlot.Eye)eye=part;
                }
                if(head==null||eye==null)
                    throw new InvalidOperationException("Head or eye part missing.");
                var marks=PixelFaceGrammar.Landmarks(recipe.plan,head.style,
                    head.anchor.x,head.anchor.y,head.extent.x,head.extent.y);
                bool valid=true;
                foreach(var point in new[]{marks.eye,marks.nostril,
                    marks.mouthStart,marks.mouthEnd})
                {
                    float u=(point.x-(head.anchor.x-head.extent.x/2))/
                        (float)Mathf.Max(1,head.extent.x);
                    float v=(point.y-head.anchor.y)*2f/
                        Mathf.Max(1,head.extent.y);
                    if(!PixelSilhouetteTemplates.Contains(PartSlot.Head,
                        head.style,u,v))valid=false;
                }
                var pixels=PixelCreaturePainter.RenderPixels(recipe);
                int eyeIndex=eye.anchor.y*tileW+eye.anchor.x;
                // The eye layer is topmost, so the eye center must be opaque
                // regardless of the palette.
                var ink=PixelArtDirection.Ramp(recipe.palette).ink;
                bool eyeVisible=eyeIndex>=0&&eyeIndex<pixels.Length&&
                    pixels[eyeIndex].a>0&&
                    pixels[eyeIndex].r==ink.r&&pixels[eyeIndex].g==ink.g&&
                    pixels[eyeIndex].b==ink.b;
                if(!valid || !eyeVisible)failures++;
                report.Append(Styles[col]).Append(',').Append(Sizes[row])
                    .Append(',').Append(marks.eye.x).Append(',').Append(marks.eye.y)
                    .Append(',').Append(marks.nostril.x).Append(',').Append(marks.nostril.y)
                    .Append(',').Append(marks.mouthStart.x).Append(',')
                    .Append(marks.mouthStart.y).Append(',')
                    .Append(valid).Append(',').Append(eyeVisible).AppendLine();
                for(int y=0;y<tileH;y++)
                for(int x=0;x<tileW;x++)
                {
                    var color=pixels[y*tileW+x];
                    if(color.a!=0)
                        canvas[(Sizes.Length-1-row)*tileH*outputWidth+
                            y*outputWidth+col*tileW+x]=color;
                }
            }
            var texture=new Texture2D(outputWidth,outputHeight,
                TextureFormat.RGBA32,false);
            texture.filterMode=FilterMode.Point;
            texture.SetPixels32(canvas);
            texture.Apply(false,false);
            try
            {
                File.WriteAllBytes(Folder+"/csharp_face_contours.png",
                    texture.EncodeToPNG());
                File.WriteAllText(Folder+"/face_landmarks.csv",report.ToString());
            }
            finally {UnityEngine.Object.DestroyImmediate(texture);}
            if(failures>0)
                throw new InvalidOperationException(failures+
                    " facial contour cases failed. See "+Folder+"/face_landmarks.csv");
            Debug.Log("Face contour audit PASS: 12 head size/style cases; "+Folder);
        }
    }
}
#endif
