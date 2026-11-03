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
            var options = new BuildPlayerOptions
            {
                scenes = new[] { "Assets/Scenes/EmberDelta.unity" },
                locationPathName = Path.Combine(folder, "EarthRogueOrigins.exe"),
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.None
            };
            BuildReport result = BuildPipeline.BuildPlayer(options);
            Debug.Log("Earth Rogue Origins Windows build: " + result.summary.result + " at " + folder);
        }
    }
}
#endif
