#if UNITY_EDITOR
using System;
using System.Globalization;
using System.IO;
using System.Text;
using EarthRogueOrigins.Ecology;
using UnityEditor;
using UnityEngine;

namespace EarthRogueOrigins.Editor
{
    public static class EcosystemExperimentRunner
    {
        [MenuItem("Earth: Rogue Origins/Experiments/Run Ecosystem Seed Sweep")]
        public static void Run()
        {
            const int seeds=12, ticks=600;
            var csv=new StringBuilder("seed,treatment,tick,population,predators,births,deaths,plants,detritus,nutrients,mean_armor,mean_speed\n");
            for(int seed=0;seed<seeds;seed++)
            for(int treatment=0;treatment<2;treatment++)
            {
                var world=new EcosystemWorld(36,24,seed+4000);
                var random=new System.Random(seed+571);
                for(int i=0;i<50;i++)
                    world.AddFounder(new Vector2(2+random.Next(32),2+random.Next(20)),
                        i<40?DietKind.Grazer: i<45?DietKind.Detritivore: DietKind.Omnivore,
                        i<40?1:i<45?3:4);
                if(treatment==1)
                    for(int i=0;i<8;i++)
                        world.AddFounder(new Vector2(2+random.Next(32),2+random.Next(20)),DietKind.Predator,2);
                for(int tick=0;tick<=ticks;tick++)
                {
                    if(tick%25==0)
                    {
                        var s=world.Snapshot();
                        csv.Append(seed).Append(',').Append(treatment).Append(',').Append(s.tick)
                          .Append(',').Append(s.population).Append(',').Append(s.predatorCount)
                          .Append(',').Append(s.births).Append(',').Append(s.deaths);
                        foreach(float value in new[]{s.totalPlants,s.totalDetritus,s.totalNutrients,s.meanArmor,s.meanSpeed})
                            csv.Append(',').Append(value.ToString("F5",CultureInfo.InvariantCulture));
                        csv.AppendLine();
                    }
                    if(tick<ticks)world.Step();
                }
            }
            string folder=Path.Combine(Directory.GetParent(Application.dataPath).FullName,"ExperimentResults");
            Directory.CreateDirectory(folder);
            string path=Path.Combine(folder,"ecosystem_seed_sweep.csv");
            File.WriteAllText(path,csv.ToString());
            Debug.Log("Saved paired seed sweep (12 seeds, 2 conditions, 600 ticks each): "+path);
        }
    }
}
#endif
