#if UNITY_EDITOR
using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using EarthRogueOrigins.Anatomy;

namespace EarthRogueOrigins.Editor
{
    /// <summary>
    /// Seeds an editable anatomical library with palette-consistent painted blockouts.
    /// These PNGs are placeholders for artist-painted production parts.
    /// </summary>
    public static class IllustratedPartsGenerator
    {
        const string Folder="Assets/Art/IllustratedParts";
        const int W=192,H=128;
        static readonly string[] Names={
            "torso","torsoHighlight","head","eye","forelimb","hindlimb",
            "foot","tail","tailFin","armorPlate","groundShadow","skinMarking"
        };

        [MenuItem("Earth: Rogue Origins/Art/Generate Illustrated Parts Library")]
        public static CreatureArtLibrary Generate()
        {
            return Generate(false);
        }

        [MenuItem("Earth: Rogue Origins/Art/Regenerate Starter Parts (Overwrite PNGs)")]
        public static void RegenerateStarterParts()
        {
            Generate(true);
        }

        public static CreatureArtLibrary Generate(bool overwrite)
        {
            Directory.CreateDirectory(Folder);
            for(int part=0;part<Names.Length;part++)
            {
                string output=Folder+"/"+Names[part]+".png";
                if(!overwrite && File.Exists(output))continue;
                var pixels=new Color32[W*H];
                var rng=new System.Random(9803+part*451);
                for(int y=0;y<H;y++)
                for(int x=0;x<W;x++)
                {
                    float u=(x+0.5f)/W*2f-1f,v=(y+0.5f)/H*2f-1f;
                    // SDFs allow anti-aliased edges and a stable dark contour.
                    float distance=SignedDistance(part,u,v);
                    float alpha=Mathf.Clamp01(.045f-distance)*25f;
                    if(alpha<=0)continue;
                    float inner=Mathf.Clamp01((-distance-.025f)*40f);
                    float stipple=(float)rng.NextDouble()*.09f-.045f;
                    float light=Mathf.Clamp01(.73f+.17f*(-u+v)+stipple);
                    Color basePaint=part==3?new Color(.14f,.18f,.14f):
                        part==8?new Color(.36f,.63f,.63f):
                        part==9?new Color(.72f,.65f,.49f):
                        part==10?new Color(.12f,.16f,.12f):
                        part==11?new Color(.25f,.31f,.23f):
                        part==1?new Color(.82f,.85f,.69f):
                        new Color(.53f,.62f,.43f);
                    Color ink=new Color(.12f,.14f,.11f);
                    Color paint=Color.Lerp(ink,basePaint*light,inner);
                    float texture=(float)rng.NextDouble();
                    if(texture>.975f && part!=10)paint=Color.Lerp(paint,Color.white,.12f);
                    if(part==10)alpha*=.42f;
                    pixels[y*W+x]=new Color(paint.r,paint.g,paint.b,alpha);
                }
                var texture=new Texture2D(W,H,TextureFormat.RGBA32,false);
                texture.SetPixels32(pixels);texture.Apply(false,false);
                File.WriteAllBytes(output,texture.EncodeToPNG());
                UnityEngine.Object.DestroyImmediate(texture);
            }
            AssetDatabase.Refresh();
            var libraryPath=Folder+"/CarboniferousTetrapod.asset";
            var library=AssetDatabase.LoadAssetAtPath<CreatureArtLibrary>(libraryPath);
            if(library==null)
            {
                library=ScriptableObject.CreateInstance<CreatureArtLibrary>();
                AssetDatabase.CreateAsset(library,libraryPath);
            }
            for(int i=0;i<Names.Length;i++)
            {
                string path=Folder+"/"+Names[i]+".png";
                var importer=AssetImporter.GetAtPath(path) as TextureImporter;
                if(importer==null)continue;
                importer.textureType=TextureImporterType.Sprite;
                importer.spritePixelsPerUnit=96;
                importer.filterMode=FilterMode.Bilinear;
                importer.mipmapEnabled=false;
                importer.alphaIsTransparency=true;
                importer.SaveAndReimport();
            }
            if(overwrite || library.torso==null)library.torso=SpriteAt(0);
            if(overwrite || library.torsoHighlight==null)library.torsoHighlight=SpriteAt(1);
            if(overwrite || library.head==null)library.head=SpriteAt(2);
            if(overwrite || library.eye==null)library.eye=SpriteAt(3);
            if(overwrite || library.forelimb==null)library.forelimb=SpriteAt(4);
            if(overwrite || library.hindlimb==null)library.hindlimb=SpriteAt(5);
            if(overwrite || library.foot==null)library.foot=SpriteAt(6);
            if(overwrite || library.tail==null)library.tail=SpriteAt(7);
            if(overwrite || library.tailFin==null)library.tailFin=SpriteAt(8);
            if(overwrite || library.armorPlate==null)library.armorPlate=SpriteAt(9);
            if(overwrite || library.groundShadow==null)library.groundShadow=SpriteAt(10);
            if(overwrite || library.skinMarking==null)library.skinMarking=SpriteAt(11);
            EditorUtility.SetDirty(library);
            AssetDatabase.SaveAssets();
            Debug.Log("Illustrated anatomical starter library created at "+libraryPath);
            return library;
        }

        static Sprite SpriteAt(int index)
        {
            return AssetDatabase.LoadAssetAtPath<Sprite>(Folder+"/"+Names[index]+".png");
        }

        static float SignedDistance(int part,float u,float v)
        {
            float ellipse=Mathf.Sqrt(u*u+.0f+v*v*2.4f)-.83f;
            switch(part)
            {
                case 0: return Mathf.Sqrt(u*u*.85f+v*v*2.3f)-.81f; // torso
                case 1: return Mathf.Sqrt(u*u*1.1f+(v+.14f)*(v+.14f)*3.8f)-.69f;
                case 2: return Mathf.Sqrt((u-.08f)*(u-.08f)*1.4f+v*v*1.8f)-.75f;
                case 3: return Mathf.Sqrt(u*u+v*v)-.43f;
                case 4:
                case 5: return Mathf.Sqrt(u*u*4.8f+v*v*.85f)-.78f;
                case 6: return Mathf.Sqrt(u*u*.94f+v*v*5.8f)-.70f;
                case 7: return Mathf.Abs(v)-.19f- .23f*Mathf.Clamp01(1f-u*.6f);
                case 8: return Mathf.Max(Mathf.Abs(u)*.72f+Mathf.Abs(v)*.88f-.74f,-u-.80f);
                case 9: return Mathf.Max(Mathf.Abs(u)*.8f+v*.65f-.55f,-v-.65f);
                case 10: return Mathf.Sqrt(u*u*.7f+v*v*7f)-.75f;
                case 11: return Mathf.Sqrt(u*u*.8f+v*v*2.1f)-.64f;
                default: return ellipse;
            }
        }
    }
}
#endif
