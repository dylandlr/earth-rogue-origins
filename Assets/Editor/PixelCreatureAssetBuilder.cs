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
            var plan=index<5?CreaturePlan.Tetrapod:index<9?CreaturePlan.Fish:CreaturePlan.Arthropod;
            var r=PixelCreatureRecipe.Founder(plan,7900+index*137);
            r.id="starter_"+index.ToString("00")+"_"+plan.ToString().ToLowerInvariant();
            // Muted palettes deliberately follow the recreated game sprite study.
            Color32[] colors={
                C(151,156,113), C(191,124,98), C(143,143,145), C(196,183,160),
                C(178,167,138), C(94,128,119), C(119,144,135), C(172,159,119),
                C(91,113,96), C(155,94,75), C(119,120,78), C(154,115,81)
            };
            r.baseColor=colors[index];
            r.secondaryColor=Darker(colors[index]);
            r.accentColor=C(191,178,132);
            r.armor=index==4?.85f:index>=9?.75f:.03f;
            r.finSize=plan==CreaturePlan.Fish?.72f:index==3?.28f:0f;
            r.spots=index%4==0?.66f:index%3==0?.21f:.36f;
            r.legLength=index==4?.65f:.45f;
            r.tailLength=index>=5&&index<9?.82f:.53f;
            if(index>=9)r.trunkStyle=PartStyle.Segmented;
            if(index==1 || index==6)r.headStyle=PartStyle.Wedge;
            if(index==3 || index==7)r.headStyle=PartStyle.LongSnout;
            if(index==2)r.trunkStyle=PartStyle.Slender;
            if(index==4)r.trunkStyle=PartStyle.Robust;
            if(index==3)r.tailStyle=PartStyle.Fan;
            if(index==4)r.limbStyle=PartStyle.Robust;
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
