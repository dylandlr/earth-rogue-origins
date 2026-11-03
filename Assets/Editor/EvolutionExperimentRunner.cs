#if UNITY_EDITOR
using System;
using System.Globalization;
using System.IO;
using System.Text;
using EarthRogueOrigins.Evolution;
using UnityEditor;
using UnityEngine;

namespace EarthRogueOrigins.Editor
{
    /// <summary>
    /// Produces auditable CSV evidence from the deterministic genetics experiment.
    /// Results are simulation demonstrations, not an empirical biological model.
    /// </summary>
    public static class EvolutionExperimentRunner
    {
        [MenuItem("Earth: Rogue Origins/Experiments/Run Predator Selection Comparison")]
        public static void Run()
        {
            const int replicates = 20;
            const int generations = 45;
            const int organisms = 80;
            string folder = Path.Combine(Directory.GetParent(Application.dataPath).FullName, "ExperimentResults");
            Directory.CreateDirectory(folder);
            string output = Path.Combine(folder, "predator_selection.csv");
            var csv = new StringBuilder("seed,generations,predators,population,mean_speed,mean_armor,mean_metabolism,mean_fecundity,mean_caution,mean_thermal_tolerance\n");
            double exposedArmor = 0, controlArmor = 0;
            for (int seed = 0; seed < replicates; ++seed)
            {
                foreach (bool predators in new[] { false, true })
                {
                    PopulationSnapshot result = SelectionExperiment.Run(seed, generations, predators, organisms);
                    csv.Append(seed).Append(',').Append(generations).Append(',').Append(predators ? 1 : 0)
                        .Append(',').Append(result.population);
                    foreach (float trait in result.meanTraits)
                        csv.Append(',').Append(trait.ToString("F6", CultureInfo.InvariantCulture));
                    csv.AppendLine();
                    if (predators) exposedArmor += result.meanTraits[(int)Locus.Armor];
                    else controlArmor += result.meanTraits[(int)Locus.Armor];
                }
            }
            File.WriteAllText(output, csv.ToString());
            Debug.Log(string.Format(CultureInfo.InvariantCulture,
                "Evolution experiment: armor without predators = {0:F4}, with predators = {1:F4}, difference = {2:F4}. CSV: {3}",
                controlArmor / replicates, exposedArmor / replicates,
                (exposedArmor - controlArmor) / replicates, output));
        }
    }
}
#endif
