#if UNITY_EDITOR
using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using EarthRogueOrigins.Anatomy;

namespace EarthRogueOrigins.Editor
{
    public static class AnatomyAtlasBuilder
    {
        const string PathRoot="Assets/Art/Generated/Anatomy";
        const int Columns=6,Rows=4,TileWidth=384,TileHeight=260;

        [MenuItem("Earth: Rogue Origins/Art/Generate Anatomy Atlas and Contact Sheet")]
        public static void GenerateAtlas()
        {
            Directory.CreateDirectory(PathRoot);
            var contact=new Texture2D(Columns*TileWidth,Rows*TileHeight,TextureFormat.RGBA32,false);
            contact.filterMode=FilterMode.Point;
            var background=new Color32(42,49,39,255);
            var pixels=new Color32[contact.width*contact.height];
            for(int i=0;i<pixels.Length;i++)pixels[i]=background;

            // Four genetic lineages, each at six different adaptation intensities.
            // Adaptations are correlated: swimming changes fin, webbing, limbs and trunk.
            for(int row=0;row<Rows;row++)
            for(int col=0;col<Columns;col++)
            {
                int seed=2046+row*601+col*29;
                var genome=DevelopmentalGenome.Founder(new System.Random(2046+row*601));
                float intensity=col/(float)(Columns-1);
                genome.regulatory[(int)BodyGene.Hue]=row/3f;
                if(row==0)
                {
                    genome.regulatory[(int)BodyGene.ArmorCoverage]=intensity;
                    genome.regulatory[(int)BodyGene.ArmorHeight]=intensity;
                    genome.regulatory[(int)BodyGene.TrunkDepth]=.3f+intensity*.45f;
                }
                else if(row==1)
                {
                    genome.regulatory[(int)BodyGene.TailFin]=intensity;
                    genome.regulatory[(int)BodyGene.TailLength]=.32f+intensity*.65f;
                    genome.regulatory[(int)BodyGene.FootWebbing]=intensity;
                    genome.regulatory[(int)BodyGene.LimbLength]=.6f-intensity*.32f;
                }
                else if(row==2)
                {
                    genome.regulatory[(int)BodyGene.LimbLength]=intensity;
                    genome.regulatory[(int)BodyGene.TrunkLength]=.32f+intensity*.49f;
                    genome.regulatory[(int)BodyGene.LimbMuscle]=.18f+intensity*.72f;
                }
                else
                {
                    genome.regulatory[(int)BodyGene.HeadScale]=.35f+intensity*.55f;
                    genome.regulatory[(int)BodyGene.TrunkDepth]=.25f+intensity*.59f;
                    genome.regulatory[(int)BodyGene.PatternDensity]=intensity;
                }
                var body=Development.Express(genome);
                Texture2D sprite=AnatomySpriteBaker.Bake(body,seed);
                byte[] png=sprite.EncodeToPNG();
                string file=PathRoot+"/morph_"+row+"_"+col+".png";
                File.WriteAllBytes(file,png);
                var image=sprite.GetPixels32();
                for(int y=0;y<sprite.height;y++)
                for(int x=0;x<sprite.width;x++)
                {
                    var color=image[y*sprite.width+x];
                    if(color.a==0)continue;
                    int xx=col*TileWidth+x;
                    int yy=(Rows-1-row)*TileHeight+y+12;
                    if(yy<contact.height)pixels[yy*contact.width+xx]=color;
                }
                UnityEngine.Object.DestroyImmediate(sprite);
            }
            contact.SetPixels32(pixels);
            contact.Apply();
            File.WriteAllBytes(PathRoot+"/morphology_contact_sheet.png",contact.EncodeToPNG());
            UnityEngine.Object.DestroyImmediate(contact);
            AssetDatabase.Refresh();
            foreach(string guid in AssetDatabase.FindAssets("t:Texture2D",new[]{PathRoot}))
            {
                string path=AssetDatabase.GUIDToAssetPath(guid);
                var imp=AssetImporter.GetAtPath(path) as TextureImporter;
                if(imp==null)continue;
                imp.textureType=TextureImporterType.Sprite;
                imp.spritePixelsPerUnit=88;
                imp.filterMode=FilterMode.Point;
                imp.alphaIsTransparency=true;
                imp.mipmapEnabled=false;
                imp.SaveAndReimport();
            }
            Debug.Log("Baked 24 anatomy-driven morphs and contact sheet to "+PathRoot+
                ". These are blockout sprites pending hand-painted production material layers.");
        }
    }
}
#endif
