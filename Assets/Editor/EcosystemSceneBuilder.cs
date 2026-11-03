#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using EarthRogueOrigins.Preview;

namespace EarthRogueOrigins.Editor
{
    public static class EcosystemSceneBuilder
    {
        [MenuItem("Earth: Rogue Origins/World/Create Ecosystem Lab Scene")]
        public static void BuildScene()
        {
            const string scenePath="Assets/Scenes/EcosystemLab.unity";
            // Generate source sprites first so the saved scene contains direct sprite references.
            ProceduralArtGenerator.Generate();
            var illustratedLibrary=IllustratedPartsGenerator.Generate();
            var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
            var camObj=new GameObject("Orthographic World Camera");
            camObj.tag="MainCamera";
            var camera=camObj.AddComponent<Camera>();
            camera.orthographic=true;
            camera.orthographicSize=14.5f;
            camera.transform.position=new Vector3(21,14,-10);
            var controller=new GameObject("Live Ecosystem World");
            var preview=controller.AddComponent<LiveEcosystemPreview>();
            preview.worldCamera=camera;
            preview.useArticulatedRig=true;
            preview.creatureArt=illustratedLibrary;
            preview.forestTiles=LoadTiles("forest");
            preview.mudTiles=LoadTiles("mud");
            preview.waterTiles=LoadTiles("water");
            preview.rockTiles=LoadTiles("rock");
            preview.floodplainTiles=LoadTiles("floodplain");
            preview.organismSprites=new Sprite[24];
            for(int i=0;i<24;i++)
                preview.organismSprites[i]=AssetDatabase.LoadAssetAtPath<Sprite>(
                    "Assets/Art/Generated/Creatures/tetrapod_"+i.ToString("00")+".png");
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene,scenePath);
            EditorUtility.DisplayDialog("Ecosystem Lab Ready",
                "Generated 64 prototype tiles and creature sprites, illustrated anatomical parts, and EcosystemLab.unity. Open the scene and press Play."+
                "\nControls are shown at the upper-left.", "OK");
        }

        static Sprite[] LoadTiles(string biome)
        {
            var result=new Sprite[8];
            for(int i=0;i<8;i++)
                result[i]=AssetDatabase.LoadAssetAtPath<Sprite>(
                    "Assets/Art/Generated/Terrain/"+biome+"_"+i.ToString("00")+".png");
            return result;
        }
    }
}
#endif
