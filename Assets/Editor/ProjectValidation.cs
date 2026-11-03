#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using EarthRogueOrigins.Anatomy;
using EarthRogueOrigins.Ecology;
using EarthRogueOrigins.Preview;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace EarthRogueOrigins.Editor
{
    /// <summary>
    /// Editor-only structural smoke test. This is deliberately independent of
    /// the NUnit suite so it can run in one -executeMethod invocation.
    /// </summary>
    public static class ProjectValidation
    {
        const string ScenePath = "Assets/Scenes/EcosystemLab.unity";
        const string LibraryPath = "Assets/Art/IllustratedParts/CarboniferousTetrapod.asset";

        [MenuItem("Earth: Rogue Origins/Validation/Validate Scene & Art")]
        public static void Validate()
        {
            var errors = new System.Collections.Generic.List<string>();
            if (!File.Exists(ScenePath))
                errors.Add("Missing scene: " + ScenePath + " (run Create Ecosystem Lab Scene)");
            var library = AssetDatabase.LoadAssetAtPath<CreatureArtLibrary>(LibraryPath);
            if (library == null || !library.HasCoreParts)
                errors.Add("Missing or incomplete illustrated creature library: " + LibraryPath);

            if (errors.Count == 0)
            {
                var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
                var preview = UnityEngine.Object.FindFirstObjectByType<LiveEcosystemPreview>();
                if (preview == null)
                    errors.Add("Scene is missing LiveEcosystemPreview controller.");
                else
                {
                    if (preview.worldCamera == null)
                        errors.Add("Preview has no world camera.");
                    if (preview.creatureArt == null || !preview.creatureArt.HasCoreParts)
                        errors.Add("Preview is not assigned a complete illustrated art library.");
                    foreach (var sprites in new [] {
                        preview.forestTiles,preview.mudTiles,preview.waterTiles,
                        preview.rockTiles,preview.floodplainTiles
                    })
                    {
                        if (sprites == null || sprites.Length < 1 || sprites.Any(x => x == null))
                            errors.Add("Missing generated terrain sprites in preview.");
                    }
                    if (!preview.useArticulatedRig)
                        errors.Add("Articulated creature rendering is not enabled.");
                }
                if (!scene.IsValid())
                    errors.Add("Unity did not load EcosystemLab scene.");
            }

            // Run minimal deterministic model checks without modifying the open scene.
            var worldA = new EcosystemWorld(12, 8, 2046);
            var worldB = new EcosystemWorld(12, 8, 2046);
            for (int i = 0; i < 30; ++i)
            {
                worldA.Step();
                worldB.Step();
            }
            if (Math.Abs(worldA.Snapshot().totalPlants-worldB.Snapshot().totalPlants)>0.00001f)
                errors.Add("Seeded ecosystem did not reproduce plant stocks.");

            var anatomy=Development.Express(DevelopmentalGenome.Founder(new System.Random(42)));
            if (!Development.IsValid(anatomy))
                errors.Add("Developmental anatomy failed validity check.");

            string report=errors.Count==0?
                "PASS — all scene, artwork and deterministic smoke checks passed.":
                "FAIL — " + errors.Count + " validation issue(s):\n- " + string.Join("\n- ",errors);
            if(errors.Count==0)Debug.Log(report);
            else
            {
                Debug.LogError(report);
                throw new InvalidOperationException(report);
            }
        }

        public static void GenerateAndValidate()
        {
            EcosystemSceneBuilder.BuildScene();
            Validate();
        }
    }
}
#endif
