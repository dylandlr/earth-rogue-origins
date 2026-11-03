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
    /// Verifiable full-creature 3x3 morphology QA rendered by the SAME
    /// authoritative C# painter used in the ecosystem preview.
    /// Contains no sampled/generated concept imagery.
    /// </summary>
    public static class CreatureMorphologyAudit
    {
        const string Folder="ValidationResults/CreatureMorphology";
        static readonly float[] Dimensions={0f,.5f,1f};

        [MenuItem("Earth: Rogue Origins/Art/Pixel Creatures/Export Full Creature Morphology Audit")]
        public static void Run()
        {
            Directory.CreateDirectory(Folder);
            const int tileW=PixelCreaturePainter.Width,tileH=PixelCreaturePainter.Height;
            int width=tileW*3,height=tileH*3;
            var image=new Color32[width*height];
            var background=new Color32(32,43,34,255);
            for(int i=0;i<image.Length;i++)image[i]=background;
            var metrics=new StringBuilder("trunk_length,trunk_depth,visible_pixels,head_x,head_y,fore_leg_x,rear_leg_x,tail_x\n");
            for(int row=0;row<3;row++)
            for(int col=0;col<3;col++)
            {
                var recipe=PixelArtDirection.Hero(CreatureArchetype.MarshCrawler,2046);
                recipe.trunkLength=Dimensions[col];
                recipe.trunkDepth=Dimensions[row];
                var parts=PixelCreatureDevelopment.Layout(recipe);
                PixelPartDescriptor head=null,fore=null,rear=null,tail=null;
                foreach(var part in parts)
                {
                    if(part.slot==PartSlot.Head)head=part;
                    if(part.slot==PartSlot.ForeLeg&&part.drawOrder==9)fore=part;
                    if(part.slot==PartSlot.HindLeg&&part.drawOrder==9)rear=part;
                    if(part.slot==PartSlot.Tail)tail=part;
                }
                if(head==null||fore==null||rear==null||tail==null)
                    throw new InvalidOperationException("Required anatomy not found.");
                var pixels=PixelCreaturePainter.RenderPixels(recipe);
                int visible=0;
                foreach(var p in pixels)if(p.a>0)visible++;
                if(visible<250)throw new InvalidOperationException(
                    "Morphology generated near-blank sprite.");
                for(int y=0;y<tileH;y++)
                for(int x=0;x<tileW;x++)
                {
                    var pixel=pixels[y*tileW+x];
                    if(pixel.a==0)continue;
                    int yy=2-row;
                    image[(yy*tileH+y)*width+col*tileW+x]=pixel;
                }
                metrics.Append(Dimensions[col]).Append(',').Append(Dimensions[row])
                    .Append(',').Append(visible).Append(',').Append(head.anchor.x)
                    .Append(',').Append(head.anchor.y).Append(',')
                    .Append(fore.anchor.x).Append(',').Append(rear.anchor.x)
                    .Append(',').Append(tail.anchor.x).AppendLine();
            }
            var texture=new Texture2D(width,height,TextureFormat.RGBA32,false);
            texture.filterMode=FilterMode.Point;
            texture.SetPixels32(image);texture.Apply(false,false);
            try
            {
                File.WriteAllBytes(Folder+"/actual_csharp_morphology.png",
                    texture.EncodeToPNG());
                File.WriteAllText(Folder+"/morphology_metrics.csv",metrics.ToString());
            }
            finally {UnityEngine.Object.DestroyImmediate(texture);}
            Debug.Log("Actual C# morphology visual audit exported: "+Folder);
        }
    }
}
#endif
