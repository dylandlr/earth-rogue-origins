#if UNITY_EDITOR
using System;
using System.Globalization;
using System.IO;
using System.Text;
using EarthRogueOrigins.Anatomy;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace EarthRogueOrigins.Editor
{
    /// <summary>
    /// Deterministic visual acceptance board generated using the actual runtime
    /// anatomical rig and illustrated skin. No fake screenshots or mockups.
    /// Outputs a scene, screenshot, and per-variant phenotype measurements.
    /// </summary>
    public static class MorphologyReviewBuilder
    {
        const int Columns=5;
        const int Rows=4;
        const int Width=1600;
        const int Height=1000;
        const string OutputFolder="ValidationResults/VisualReview";

        [MenuItem("Earth: Rogue Origins/Art/Create Morphology Review Board")]
        public static void BuildReview()
        {
            Directory.CreateDirectory("Assets/Scenes");
            Directory.CreateDirectory(OutputFolder);
            var art=IllustratedPartsGenerator.Generate(false);
            if(art==null || !art.HasCoreParts)
                throw new InvalidOperationException("Illustrated creature library is missing its core parts.");
            var scene=EditorSceneManager.NewPreviewScene();

            Shader shader=Shader.Find("Sprites/Default");
            if(shader==null)shader=Shader.Find("Unlit/Color");
            if(shader==null)throw new InvalidOperationException("No suitable creature material shader found.");

            var material=new Material(shader);
            material.name="Temporary morphology review material";
            var root=new GameObject("REVIEW — Real phenotype-driven anatomy");
            SceneManager.MoveGameObjectToScene(root,scene);
            const float stepX=4.4f, stepY=3.2f;
            var csv=new StringBuilder("variant,adaptation,intensity,trunk_length,tail_length,limb_length,fin,armor,swim_speed,land_speed,upkeep\n");

            for(int row=0;row<Rows;row++)
            for(int col=0;col<Columns;col++)
            {
                float intensity=col/(float)(Columns-1);
                var g=DevelopmentalGenome.Founder(new System.Random(1200+row*197));
                g.regulatory[(int)BodyGene.Hue]=row/3f;
                string adaptation;
                switch(row)
                {
                    case 0:
                        adaptation="defensive armor";
                        g.regulatory[(int)BodyGene.ArmorCoverage]=intensity;
                        g.regulatory[(int)BodyGene.ArmorHeight]=intensity;
                        g.regulatory[(int)BodyGene.TrunkDepth]=.3f+intensity*.5f;
                        break;
                    case 1:
                        adaptation="aquatic specialization";
                        g.regulatory[(int)BodyGene.TailLength]=.28f+.72f*intensity;
                        g.regulatory[(int)BodyGene.TailFin]=intensity;
                        g.regulatory[(int)BodyGene.FootWebbing]=intensity;
                        g.regulatory[(int)BodyGene.LimbLength]=.65f-.45f*intensity;
                        break;
                    case 2:
                        adaptation="terrestrial mobility";
                        g.regulatory[(int)BodyGene.LimbLength]=intensity;
                        g.regulatory[(int)BodyGene.LimbMuscle]=.2f+.8f*intensity;
                        g.regulatory[(int)BodyGene.TrunkLength]=.35f+.6f*intensity;
                        break;
                    default:
                        adaptation="cranial and body size";
                        g.regulatory[(int)BodyGene.HeadScale]=.18f+.82f*intensity;
                        g.regulatory[(int)BodyGene.TrunkDepth]=.22f+.72f*intensity;
                        g.regulatory[(int)BodyGene.PatternDensity]=intensity;
                        break;
                }
                var phenotype=Development.Express(g);
                if(!Development.IsValid(phenotype))
                    throw new InvalidOperationException("Generated an invalid phenotype at "+row+","+col);
                var go=new GameObject("Variant "+row+"-"+col+" | "+adaptation+" "+intensity.ToString("F2",CultureInfo.InvariantCulture));
                go.transform.SetParent(root.transform,false);
                go.transform.localPosition=new Vector3((col-(Columns-1)*.5f)*stepX,
                    ((Rows-1)*.5f-row)*stepY,0);
                var rig=go.AddComponent<ArticulatedCreatureView>();
                rig.Initialize(phenotype,material);
                var skin=go.AddComponent<IllustratedCreatureSkin>();
                skin.Apply(art);
                // Alternate swim/stride poses, using the same runtime motion implementation.
                rig.SetMotion(.12f+col*.04f,row==1,true,false);
                csv.Append(row*Columns+col).Append(',').Append('"').Append(adaptation).Append('"')
                    .Append(',').Append(intensity.ToString("F3",CultureInfo.InvariantCulture));
                foreach(float value in new[]{phenotype.trunkLength,phenotype.tailLength,
                    phenotype.limbLength,phenotype.tailFinSize,phenotype.armorCoverage,
                    phenotype.swimmingSpeed,phenotype.terrestrialSpeed,phenotype.upkeep})
                    csv.Append(',').Append(value.ToString("F4",CultureInfo.InvariantCulture));
                csv.AppendLine();
            }
            var camObj=new GameObject("Visual Review Camera");
            SceneManager.MoveGameObjectToScene(camObj,scene);
            var camera=camObj.AddComponent<Camera>();
            camObj.tag="MainCamera";
            camera.orthographic=true;
            camera.orthographicSize=Rows*stepY*.59f;
            camera.aspect=Width/(float)Height;
            camera.transform.position=new Vector3(0,0,-20);
            camera.clearFlags=CameraClearFlags.SolidColor;
            camera.backgroundColor=new Color(.17f,.21f,.17f,1);
            File.WriteAllText(OutputFolder+"/morphology_metrics.csv",csv.ToString());

            // Actual editor-camera render, not generated illustration. Correctly restore
            // render state and release GPU resources when screenshots are captured.
            var previous=RenderTexture.active;
            var rt=new RenderTexture(Width,Height,24,RenderTextureFormat.ARGB32);
            Texture2D capture=null;
            try
            {
                camera.targetTexture=rt;
                camera.Render();
                RenderTexture.active=rt;
                capture=new Texture2D(Width,Height,TextureFormat.RGB24,false);
                capture.ReadPixels(new Rect(0,0,Width,Height),0,0);
                capture.Apply(false,false);
                var sample=capture.GetPixels32();
                int foregroundPixels=0;
                var background=(Color32)camera.backgroundColor;
                foreach(var color in sample)
                {
                    if(Mathf.Abs(color.r-background.r)>12 ||
                       Mathf.Abs(color.g-background.g)>12 ||
                       Mathf.Abs(color.b-background.b)>12)foregroundPixels++;
                }
                if(foregroundPixels<Width*Height/500)
                    throw new InvalidOperationException("Morphology capture is blank or almost empty ("+
                        foregroundPixels+" distinct foreground pixels). Inspect camera and sprite shaders.");
                File.WriteAllBytes(OutputFolder+"/morphology_board.png",capture.EncodeToPNG());
                Debug.Log("Morphology screenshot distinct foreground pixels: "+foregroundPixels);
            }
            finally
            {
                camera.targetTexture=null;
                RenderTexture.active=previous;
                if(capture!=null)UnityEngine.Object.DestroyImmediate(capture);
                UnityEngine.Object.DestroyImmediate(rt);
                EditorSceneManager.ClosePreviewScene(scene);
                UnityEngine.Object.DestroyImmediate(material);
            }
            Debug.Log("Morphology review saved: "+OutputFolder+
                "/morphology_board.png and morphology_metrics.csv");
        }
    }
}
#endif
