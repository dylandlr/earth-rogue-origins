#if UNITY_EDITOR
using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using EarthRogueOrigins.PixelCreatures;

namespace EarthRogueOrigins.Editor
{
    /// <summary>
    /// Recipe-first authoring: tune deterministic morphology, preview its real
    /// compositor output, save JSON and each part as editable RGBA PNG.
    /// </summary>
    public sealed class PixelCreatureWorkbench : EditorWindow
    {
        [SerializeField] PixelCreatureRecipe recipe;
        Texture2D preview;
        Vector2 scroll;
        const string ExportRoot="Assets/Art/PixelCreatures/Custom";

        [MenuItem("Earth: Rogue Origins/Art/Pixel Creatures/Open Composer")]
        static void ShowWindow()
        {
            var window=GetWindow<PixelCreatureWorkbench>("Pixel Creature Composer");
            window.minSize=new Vector2(510,620);
            window.Initialize();
        }

        void OnEnable(){Initialize();}
        void OnDisable()
        {
            if(preview!=null)DestroyImmediate(preview);
            preview=null;
        }

        void Initialize()
        {
            if(recipe==null)recipe=PixelCreatureRecipe.Founder(CreaturePlan.Tetrapod,2046);
            RepaintPreview();
        }

        void RepaintPreview()
        {
            if(recipe==null)return;
            if(preview!=null)DestroyImmediate(preview);
            try{preview=PixelCreaturePainter.Render(recipe);}
            catch(Exception e){Debug.LogError("Pixel creature recipe error: "+e.Message);}
            Repaint();
        }

        void OnGUI()
        {
            if(recipe==null)Initialize();
            GUILayout.Label("EARTH: ROGUE ORIGINS — PROCEDURAL PIXEL ASSET LAB",EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("128×96 transparent pixels. Original vector/bitmap primitives and composable layers. "+
                "No concept-art extraction. A starter for hand-authored anatomical parts.",MessageType.Info);
            scroll=EditorGUILayout.BeginScrollView(scroll);
            EditorGUI.BeginChangeCheck();
            recipe.id=EditorGUILayout.TextField("Asset ID",recipe.id);
            recipe.seed=EditorGUILayout.IntField("Appearance seed",recipe.seed);
            recipe.plan=(CreaturePlan)EditorGUILayout.EnumPopup("Body plan",recipe.plan);
            recipe.headStyle=(PartStyle)EditorGUILayout.EnumPopup("Head module",recipe.headStyle);
            recipe.trunkStyle=(PartStyle)EditorGUILayout.EnumPopup("Trunk module",recipe.trunkStyle);
            recipe.tailStyle=(PartStyle)EditorGUILayout.EnumPopup("Tail module",recipe.tailStyle);
            recipe.limbStyle=(PartStyle)EditorGUILayout.EnumPopup("Limb module",recipe.limbStyle);
            GUILayout.Space(6);
            recipe.trunkLength=EditorGUILayout.Slider("Trunk length",recipe.trunkLength,0,1);
            recipe.trunkDepth=EditorGUILayout.Slider("Trunk depth",recipe.trunkDepth,0,1);
            recipe.headSize=EditorGUILayout.Slider("Head size",recipe.headSize,0,1);
            recipe.legLength=EditorGUILayout.Slider("Leg length",recipe.legLength,0,1);
            recipe.tailLength=EditorGUILayout.Slider("Tail length",recipe.tailLength,0,1);
            recipe.finSize=EditorGUILayout.Slider("Fin development",recipe.finSize,0,1);
            recipe.armor=EditorGUILayout.Slider("Armor",recipe.armor,0,1);
            recipe.spots=EditorGUILayout.Slider("Pigment density",recipe.spots,0,1);
            recipe.eyeSize=EditorGUILayout.Slider("Eye size",recipe.eyeSize,0,1);
            recipe.baseColor=(Color32)EditorGUILayout.ColorField("Base pigment",recipe.baseColor);
            recipe.secondaryColor=(Color32)EditorGUILayout.ColorField("Secondary pigment",recipe.secondaryColor);
            recipe.accentColor=(Color32)EditorGUILayout.ColorField("Accent",recipe.accentColor);
            if(EditorGUI.EndChangeCheck())RepaintPreview();
            GUILayout.Space(10);
            if(preview!=null)
            {
                Rect area=GUILayoutUtility.GetRect(450,320,GUILayout.ExpandWidth(true));
                EditorGUI.DrawRect(area,new Color(.17f,.21f,.17f));
                float scale=Mathf.Min(area.width/128f,area.height/96f);
                var target=new Rect(area.x+(area.width-128*scale)/2,
                    area.y+(area.height-96*scale)/2,128*scale,96*scale);
                GUI.DrawTexture(target,preview,ScaleMode.ScaleToFit,true);
            }
            EditorGUILayout.BeginHorizontal();
            if(GUILayout.Button("New seeded founder",GUILayout.Height(32)))
            {
                recipe=PixelCreatureRecipe.Founder(recipe.plan,recipe.seed+1);
                RepaintPreview();
            }
            if(GUILayout.Button("Open JSON recipe",GUILayout.Height(32)))LoadRecipe();
            if(GUILayout.Button("Export layers + recipe",GUILayout.Height(32)))Export();
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndScrollView();
        }

        void LoadRecipe()
        {
            string path=EditorUtility.OpenFilePanel("Load pixel creature recipe",Application.dataPath,"json");
            if(string.IsNullOrEmpty(path))return;
            try
            {
                var loaded=JsonUtility.FromJson<PixelCreatureRecipe>(File.ReadAllText(path));
                loaded.Validate();
                recipe=loaded;
                RepaintPreview();
            }
            catch(Exception e){EditorUtility.DisplayDialog("Invalid recipe",e.Message,"OK");}
        }

        void Export()
        {
            try
            {
                recipe.Validate();
                string safe=Sanitize(recipe.id);
                string folder=ExportRoot+"/"+safe;
                Directory.CreateDirectory(folder);
                string recipeFile=folder+"/recipe.json";
                if(File.Exists(recipeFile) &&
                   !EditorUtility.DisplayDialog("Existing asset","Overwrite "+safe+"?","Overwrite","Cancel"))
                    return;
                File.WriteAllText(recipeFile,JsonUtility.ToJson(recipe,true));
                var all=PixelCreaturePainter.Render(recipe);
                try{File.WriteAllBytes(folder+"/assembled.png",all.EncodeToPNG());}
                finally{DestroyImmediate(all);}
                var parts=PixelCreatureDevelopment.Layout(recipe);
                Array.Sort(parts,(a,b)=>a.drawOrder.CompareTo(b.drawOrder));
                var csv=new System.Text.StringBuilder("part,slot,style,x,y,width,height,order\n");
                for(int i=0;i<parts.Length;i++)
                {
                    var part=parts[i];
                    var tex=PixelCreaturePainter.TextureFromPixels(
                        PixelCreaturePainter.RenderLayerPixels(recipe,i));
                    string filename=i.ToString("00")+"_"+part.slot.ToString().ToLowerInvariant()+".png";
                    try{File.WriteAllBytes(folder+"/"+filename,tex.EncodeToPNG());}
                    finally{DestroyImmediate(tex);}
                    csv.Append(filename).Append(',').Append(part.slot).Append(',')
                        .Append(part.style).Append(',').Append(part.anchor.x).Append(',')
                        .Append(part.anchor.y).Append(',').Append(part.extent.x).Append(',')
                        .Append(part.extent.y).Append(',').Append(part.drawOrder).AppendLine();
                }
                File.WriteAllText(folder+"/layers.csv",csv.ToString());
                AssetDatabase.Refresh();
                foreach(string guid in AssetDatabase.FindAssets("t:Texture2D",new[]{folder}))
                {
                    var path=AssetDatabase.GUIDToAssetPath(guid);
                    var importer=AssetImporter.GetAtPath(path) as TextureImporter;
                    if(importer==null)continue;
                    importer.textureType=TextureImporterType.Sprite;
                    importer.filterMode=FilterMode.Point;
                    importer.textureCompression=TextureImporterCompression.Uncompressed;
                    importer.spritePixelsPerUnit=64;
                    importer.mipmapEnabled=false;
                    importer.SaveAndReimport();
                }
                Debug.Log("Exported pixel creature asset and anatomy layers to "+folder);
            }
            catch(Exception e)
            {
                EditorUtility.DisplayDialog("Pixel asset export failed",e.Message,"OK");
                Debug.LogException(e);
            }
        }

        static string Sanitize(string id)
        {
            var chars=id.ToCharArray();
            for(int i=0;i<chars.Length;i++)
                if(!char.IsLetterOrDigit(chars[i]) && chars[i]!='-' && chars[i]!='_')
                    chars[i]='_';
            return new string(chars);
        }
    }
}
#endif
