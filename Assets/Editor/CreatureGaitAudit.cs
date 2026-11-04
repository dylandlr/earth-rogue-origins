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
    /// Renders actual PixelCreaturePainter C# output across the four
    /// gait phases, and writes physical foot contact diagnostics.
    /// This is the authoritative artifact; external diagrams are not.
    /// </summary>
    public static class CreatureGaitAudit
    {
        const string Folder="ValidationResults/CreatureGait";

        [MenuItem("Earth: Rogue Origins/Art/Pixel Creatures/Export Walking Gait Audit")]
        public static void Run()
        {
            Directory.CreateDirectory(Folder);
            var recipe=PixelArtDirection.Hero(CreatureArchetype.MarshCrawler,2046);
            int w=PixelCreaturePainter.Width,h=PixelCreaturePainter.Height;
            var board=new Color32[w*PixelCreaturePoses.Frames*h];
            var bg=new Color32(33,41,32,255);
            for(int i=0;i<board.Length;i++)board[i]=bg;
            var rows=new StringBuilder("frame,slot,root_x,root_y,knee_x,knee_y,ankle_x,ankle_y,lift,draw_order\n");
            for(int frame=0;frame<PixelCreaturePoses.Frames;frame++)
            {
                var pixels=PixelCreaturePainter.RenderPixels(recipe,frame,false);
                for(int y=0;y<h;y++)
                for(int x=0;x<w;x++)
                {
                    var p=pixels[y*w+x];
                    if(p.a>0)board[y*(w*PixelCreaturePoses.Frames)+frame*w+x]=p;
                }
                var parts=PixelCreaturePoses.Layout(recipe,frame,false);
                int groundedForeground=0;
                foreach(var part in parts)
                {
                    if(part.slot!=PartSlot.ForeLeg&&part.slot!=PartSlot.HindLeg)
                        continue;
                    var shape=PixelLimbSilhouette.Build(part.anchor.x,part.anchor.y,
                        part.extent.y,part.style,part.mirrorGroup,
                        part.slot==PartSlot.ForeLeg,part.drawOrder<2,part.footLift);
                    if(part.drawOrder==9 && part.footLift==0)groundedForeground++;
                    rows.Append(frame).Append(',').Append(part.slot).Append(',')
                        .Append(shape.root.x).Append(',').Append(shape.root.y).Append(',')
                        .Append(shape.knee.x).Append(',').Append(shape.knee.y).Append(',')
                        .Append(shape.ankle.x).Append(',').Append(shape.ankle.y).Append(',')
                        .Append(part.footLift).Append(',').Append(part.drawOrder).AppendLine();
                }
                if(groundedForeground<1)
                    throw new InvalidOperationException(
                        "Gait frame "+frame+" has no foreground planted support.");
            }
            var texture=new Texture2D(w*PixelCreaturePoses.Frames,h,
                TextureFormat.RGBA32,false);
            texture.filterMode=FilterMode.Point;
            texture.SetPixels32(board);texture.Apply(false,false);
            try
            {
                File.WriteAllBytes(Folder+"/actual_csharp_walk_strip.png",
                    texture.EncodeToPNG());
                File.WriteAllText(Folder+"/joint_positions.csv",rows.ToString());
            }
            finally {UnityEngine.Object.DestroyImmediate(texture);}
            Debug.Log("C# walking gait exported: "+Folder);
        }
    }
}
#endif
