#if UNITY_EDITOR
using System;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;
using EarthRogueOrigins.PixelCreatures;

namespace EarthRogueOrigins.Editor
{
    /// <summary>
    /// Starter library and reproducible per-layer export for wholly original pixel
    /// sprites. PNGs remain designer-editable: reruns do not overwrite by default.
    /// </summary>
    public static class PixelCreatureAssetBuilder
    {
        const string Root="Assets/Art/PixelCreatures";
        const int Columns=4,Rows=3;

        [MenuItem("Earth: Rogue Origins/Art/Pixel Creatures/Generate Starter Library")]
        public static void Generate(){Generate(false);}

        [MenuItem("Earth: Rogue Origins/Art/Pixel Creatures/Rebuild Starter Library (Overwrite)")]
        public static void Regenerate(){Generate(true);}

        public static void Generate(bool overwrite)
        {
            Directory.CreateDirectory(Root);
            var preview=new Texture2D(Columns*PixelCreaturePainter.Width,
                Rows*PixelCreaturePainter.Height,TextureFormat.RGBA32,false);
            var background=new Color32(246,240,225,255);
            var sheet=new Color32[preview.width*preview.height];
            for(int i=0;i<sheet.Length;i++)sheet[i]=background;
            var csv=new StringBuilder("id,body_plan,seed,layer_count,armor,fin,legs,tail,spots\n");

            for(int index=0;index<Columns*Rows;index++)
            {
                var r=Starter(index);
                string folder=Root+"/"+r.id;
                Directory.CreateDirectory(folder);
                string json=JsonUtility.ToJson(r,true);
                WriteText(folder+"/recipe.json",json,overwrite);
                var pixels=PixelCreaturePainter.RenderPixels(r);
                WriteImage(folder+"/assembled.png",pixels,overwrite);
                WriteImage(folder+"/contact_shadow.png",
                    PixelCreaturePainter.RenderGroundShadowPixels(r),overwrite);
                var layout=PixelCreatureDevelopment.Layout(r);
                Array.Sort(layout,(a,b)=>a.drawOrder.CompareTo(b.drawOrder));
                var descriptors=new StringBuilder("layer,slot,style,anchor_x,anchor_y,width,height,order\n");
                for(int layer=0;layer<layout.Length;layer++)
                {
                    var part=layout[layer];
                    string layerName=layer.ToString("00")+"_"+part.slot.ToString().ToLowerInvariant();
                    WriteImage(folder+"/"+layerName+".png",
                        PixelCreaturePainter.RenderLayerPixels(r,layer),overwrite);
                    descriptors.Append(layer).Append(',').Append(part.slot).Append(',')
                        .Append(part.style).Append(',').Append(part.anchor.x).Append(',')
                        .Append(part.anchor.y).Append(',').Append(part.extent.x).Append(',')
                        .Append(part.extent.y).Append(',').Append(part.drawOrder).AppendLine();
                }
                WriteText(folder+"/layers.csv",descriptors.ToString(),overwrite);
                // Export all poses from the exact same layered recipe, including
                // the neutral frame. Keep each PNG individually artist-editable.
                string motionFolder=folder+"/Animation";
                Directory.CreateDirectory(motionFolder);
                for(int mode=0;mode<2;mode++)
                for(int frame=0;frame<PixelCreaturePoses.Frames;frame++)
                {
                    string animationName=(mode==0?"walk_":"swim_")+frame.ToString("00")+".png";
                    WriteImage(motionFolder+"/"+animationName,
                        PixelCreaturePainter.RenderPixels(r,frame,mode==1),overwrite);
                }
                csv.Append(r.id).Append(',').Append(r.plan).Append(',').Append(r.seed)
                    .Append(',').Append(layout.Length).Append(',').Append(r.armor)
                    .Append(',').Append(r.finSize).Append(',').Append(r.legLength)
                    .Append(',').Append(r.tailLength).Append(',').Append(r.spots).AppendLine();
                int x0=(index%Columns)*PixelCreaturePainter.Width;
                int y0=(Rows-1-index/Columns)*PixelCreaturePainter.Height;
                for(int y=0;y<PixelCreaturePainter.Height;y++)
                for(int x=0;x<PixelCreaturePainter.Width;x++)
                {
                    Color32 color=pixels[y*PixelCreaturePainter.Width+x];
                    if(color.a==0)continue;
                    sheet[(y0+y)*preview.width+x0+x]=color;
                }
            }
            preview.SetPixels32(sheet);preview.Apply(false,false);
            string sheetPath=Root+"/starter_contact_sheet.png";
            if(overwrite || !File.Exists(sheetPath))
                File.WriteAllBytes(sheetPath,preview.EncodeToPNG());
            UnityEngine.Object.DestroyImmediate(preview);
            WriteText(Root+"/starter_manifest.csv",csv.ToString(),overwrite);

            AssetDatabase.Refresh();
            string[] guids=AssetDatabase.FindAssets("t:Texture2D",new[]{Root});
            foreach(string guid in guids)
            {
                string path=AssetDatabase.GUIDToAssetPath(guid);
                var importer=AssetImporter.GetAtPath(path) as TextureImporter;
                if(importer==null)continue;
                importer.textureType=TextureImporterType.Sprite;
                importer.spritePixelsPerUnit=64;
                importer.filterMode=FilterMode.Point;
                importer.textureCompression=TextureImporterCompression.Uncompressed;
                importer.mipmapEnabled=false;
                importer.alphaIsTransparency=true;
                importer.SaveAndReimport();
            }
            AssetDatabase.SaveAssets();
            Debug.Log("Generated 12 reproducible, layered pixel creatures at "+Root+
                (overwrite?" (overwriting originals)":" (preserving existing files)"));
        }

        static void WriteImage(string path,Color32[] pixels,bool overwrite)
        {
            if(!overwrite && File.Exists(path))return;
            var tex=PixelCreaturePainter.TextureFromPixels(pixels);
            File.WriteAllBytes(path,tex.EncodeToPNG());
            UnityEngine.Object.DestroyImmediate(tex);
        }
        static void WriteText(string path,string value,bool overwrite)
        {
            if(overwrite || !File.Exists(path))File.WriteAllText(path,value);
        }

        public static PixelCreatureRecipe Starter(int index)
        {
            if(index<0 || index>=12)throw new ArgumentOutOfRangeException(nameof(index));
            var archetype=(CreatureArchetype)(index%8);
            var r=PixelArtDirection.Hero(archetype,7900+index*137);
            r.id="starter_"+index.ToString("00")+"_"+archetype.ToString().ToLowerInvariant();
            if(index>=8)
            {
                // Second generation of related variants: conserve the selected
                // family silhouette while changing localized inherited controls.
                r.headSize=Mathf.Clamp01(r.headSize+(index%2==0?.12f:-.1f));
                r.tailLength=Mathf.Clamp01(r.tailLength+(index%2==0?.18f:-.12f));
                r.spots=Mathf.Clamp01(r.spots+.2f);
            }
            r.Validate();
            return r;
        }

        static Color32 C(byte r,byte g,byte b){return new Color32(r,g,b,255);}
        static Color32 Darker(Color32 c)
        {
            return new Color32((byte)(c.r*.68f),(byte)(c.g*.68f),(byte)(c.b*.68f),255);
        }
    }
}
#endif
