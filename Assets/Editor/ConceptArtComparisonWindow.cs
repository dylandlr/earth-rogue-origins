#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

namespace EarthRogueOrigins.Editor
{
    /// <summary>
    /// Side-by-side inspection against the user-approved reference. The right
    /// panel reads a REAL Unity render exported by MorphologyReviewBuilder.
    /// It does not synthesize or substitute a screenshot.
    /// </summary>
    public sealed class ConceptArtComparisonWindow : EditorWindow
    {
        const string ReferencePath="Assets/Art/ConceptReference/Environment/approved_concept.png";
        const string CurrentPath="ValidationResults/VisualReview/morphology_board.png";
        Texture2D reference,current;
        float split=.5f;
        Vector2 scroll;
        string warning;

        [MenuItem("Earth: Rogue Origins/Art/Compare Render to Approved Concept")]
        static void Open()
        {
            var window=GetWindow<ConceptArtComparisonWindow>("Concept Art Comparison");
            window.minSize=new Vector2(840,480);
            window.Reload();
        }

        void OnEnable(){Reload();}
        void OnDisable()
        {
            if(current!=null)DestroyImmediate(current);
            current=null;
        }

        void Reload()
        {
            reference=AssetDatabase.LoadAssetAtPath<Texture2D>(ReferencePath);
            if(current!=null)DestroyImmediate(current);
            current=null;
            warning="";
            if(!File.Exists(CurrentPath))
            {
                warning="A real Unity review render is not available. Run Art > Create Morphology Review Board first.";
                return;
            }
            try
            {
                current=new Texture2D(2,2,TextureFormat.RGBA32,false);
                if(!current.LoadImage(File.ReadAllBytes(CurrentPath),false))
                    throw new IOException("Unable to decode captured PNG.");
                current.filterMode=FilterMode.Bilinear;
            }
            catch(System.Exception e)
            {
                warning=e.Message;
                if(current!=null)DestroyImmediate(current);
                current=null;
            }
        }

        void OnGUI()
        {
            GUILayout.Label("EARTH: ROGUE ORIGINS — VISUAL FIDELITY REVIEW",EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "Left: approved illustrated world target. Right: real Unity morphology capture. "+
                "The images show different views; compare contour language, palette, shading, detail density, "+
                "and the rendering quality of organisms—not their scene layout.",MessageType.Info);
            using(new EditorGUILayout.HorizontalScope())
            {
                if(GUILayout.Button("Reload Capture",GUILayout.Width(170)))Reload();
                if(GUILayout.Button("Generate Review Capture",GUILayout.Width(190)))
                {
                    MorphologyReviewBuilder.BuildReview();
                    Reload();
                }
            }
            split=EditorGUILayout.Slider("Reference panel width",split,.25f,.75f);
            if(reference==null)
                EditorGUILayout.HelpBox("Reference art not imported at "+ReferencePath+
                    ". Copy the approved Concept Art Source Pack's Assets folder into this Unity project.",MessageType.Warning);
            if(!string.IsNullOrEmpty(warning))
                EditorGUILayout.HelpBox(warning,MessageType.Warning);

            Rect available=GUILayoutUtility.GetRect(0,10000,0,10000,GUILayout.ExpandHeight(true));
            float gap=12f;
            float leftWidth=(available.width-gap)*split;
            Rect left=new Rect(available.x,available.y,leftWidth,available.height);
            Rect right=new Rect(left.xMax+gap,available.y,available.width-leftWidth-gap,available.height);
            DrawPanel(left,reference,"APPROVED WORLD CONCEPT");
            DrawPanel(right,current,"UNITY ANATOMY OUTPUT");
        }

        static void DrawPanel(Rect rect,Texture2D image,string label)
        {
            EditorGUI.DrawRect(rect,new Color(.11f,.14f,.12f));
            Rect header=new Rect(rect.x+12,rect.y+8,rect.width-24,25);
            GUI.Label(header,label,EditorStyles.boldLabel);
            if(image==null)return;
            Rect canvas=new Rect(rect.x+10,rect.y+38,rect.width-20,rect.height-52);
            float width=image.width,height=image.height;
            float scale=Mathf.Min(canvas.width/width,canvas.height/height);
            var r=new Rect(canvas.x+(canvas.width-width*scale)*.5f,
                canvas.y+(canvas.height-height*scale)*.5f,width*scale,height*scale);
            GUI.DrawTexture(r,image,ScaleMode.ScaleToFit,false);
        }
    }
}
#endif
