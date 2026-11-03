#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;
using System.IO;

namespace EarthRogueOrigins.Editor
{
    [InitializeOnLoad]
    public static class ProjectSetup
    {
        static ProjectSetup()
        {
            EditorApplication.delayCall += () =>
            {
                PlayerSettings.companyName = "Rogue Origins Studio";
                PlayerSettings.productName = "Earth: Rogue Origins";
                PlayerSettings.defaultScreenWidth = 1672;
                PlayerSettings.defaultScreenHeight = 941;
                PlayerSettings.resizableWindow = true;
            };
        }

        [MenuItem("Earth: Rogue Origins/Build Windows Game")]
        public static void BuildWindows()
        {
            string folder = Path.Combine(Directory.GetParent(Application.dataPath).FullName, "Builds/Windows");
            Directory.CreateDirectory(folder);
            const string scene = "Assets/Scenes/EcosystemLab.unity";
            if (!File.Exists(scene))
            {
                throw new System.IO.FileNotFoundException(
                    "Ecosystem Lab scene is missing. Choose Earth: Rogue Origins > World > Create Ecosystem Lab Scene first.", scene);
            }
            var options = new BuildPlayerOptions
            {
                scenes = new[] { scene },
                locationPathName = Path.Combine(folder, "EarthRogueOrigins.exe"),
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.None
            };
            BuildReport result = BuildPipeline.BuildPlayer(options);
            Debug.Log("Earth Rogue Origins Windows build: " + result.summary.result + " at " + folder);
            if (result.summary.result != BuildResult.Succeeded)
                throw new System.Exception("Earth Rogue Origins build failed: " + result.summary.result);
        }
    }
}
#endif
