#if UNITY_EDITOR
using System;
using System.IO;
using System.Text;
using EarthRogueOrigins.PixelCreatures;
using UnityEngine;

namespace EarthRogueOrigins.Editor
{
    /// <summary>
    /// Three nutrient environments x two recurrent growth interventions
    /// for pair 3 (strong inhibition) and pair 4 (comparative control).
    /// This is a frozen-body maintenance trial, NOT reproduction/fitness.
    /// </summary>
    public static class OpenMorphResourceAudit
    {
        const int W=64,H=48;
        public static void Export(string folder,OpenMorphGenome[] genomes)
        {
            if(genomes==null || genomes.Length!=8)
                throw new ArgumentException("Expected the eight standard genomes.");
            var climates=new[]{
                OpenMorphResourceTrial.Climate.Scarce,
                OpenMorphResourceTrial.Climate.Moderate,
                OpenMorphResourceTrial.Climate.Rich
            };
            var modes=new[]{
                OpenMorphGRN.BirthChannel.Full,
                OpenMorphGRN.BirthChannel.Neutral
            };
            var csv=new StringBuilder(
                "pair,seed,mode,climate,initial_cells,surviving_cells,deaths,survival_fraction,maintenance_per_initial_cell,harvest_per_initial_cell,initial_energy,external_added,harvested,consumed,discarded,remaining_energy,remaining_pool,unmet_maintenance,conservation_error,transport_cells,storage_cells\n");
            var timeline=new StringBuilder(
                "pair,mode,climate,tick,living_cells,cell_energy,external_pool\n");
            var board=new Color32[W*4*H*3];
            var background=new Color32(31,38,36,255);
            for(int i=0;i<board.Length;i++)board[i]=background;
            foreach(int pair in new[]{3,4})
            {
                var genome=genomes[(pair-1)*2];
                for(int mode=0;mode<modes.Length;mode++)
                {
                    var morphology=OpenMorphTissue.Grow(
                        genome,W,H,72,800,true,true,modes[mode]);
                    for(int climate=0;climate<climates.Length;climate++)
                    {
                        var trial=OpenMorphResourceTrial.Run(
                            morphology,climates[climate],120);
                        csv.Append(pair).Append(',').Append(genome.seed).Append(',')
                            .Append(modes[mode]).Append(',')
                            .Append(climates[climate].name).Append(',')
                            .Append(trial.initialCells).Append(',')
                            .Append(trial.livingCells).Append(',')
                            .Append(trial.deaths).Append(',')
                            .Append(Format(trial.livingCells/(float)
                                Math.Max(1,trial.initialCells))).Append(',')
                            .Append(Format(trial.consumed/
                                Math.Max(1,trial.initialCells))).Append(',')
                            .Append(Format(trial.harvested/
                                Math.Max(1,trial.initialCells))).Append(',')
                            .Append(Format(trial.initialEnergy)).Append(',')
                            .Append(Format(trial.externalAdded)).Append(',')
                            .Append(Format(trial.harvested)).Append(',')
                            .Append(Format(trial.consumed)).Append(',')
                            .Append(Format(trial.discarded)).Append(',')
                            .Append(Format(trial.remainingEnergy)).Append(',')
                            .Append(Format(trial.remainingPool)).Append(',')
                            .Append(Format(trial.unmetMaintenance)).Append(',')
                            .Append(Format(trial.conservationError)).Append(',')
                            .Append(morphology.FateCount(
                                OpenMorphTissue.Fate.Transport)).Append(',')
                            .Append(morphology.FateCount(
                                OpenMorphTissue.Fate.Storage)).AppendLine();
                        for(int tick=0;tick<trial.livingByTick.Length;tick++)
                            timeline.Append(pair).Append(',')
                                .Append(modes[mode]).Append(',')
                                .Append(climates[climate].name).Append(',')
                                .Append(tick).Append(',')
                                .Append(trial.livingByTick[tick]).Append(',')
                                .Append(Format(trial.energyByTick[tick])).Append(',')
                                .Append(Format(trial.poolByTick[tick])).AppendLine();

                        // Four columns: pair3 Full/Neutral, pair4 Full/Neutral.
                        // Rows: scarce, moderate, rich, bottom to top.
                        int col=(pair-3)*2+mode;
                        int row=2-climate;
                        for(int y=0;y<H;y++)
                        for(int x=0;x<W;x++)
                        {
                            int i=y*W+x;
                            var cell=morphology.cells[i];
                            if(cell.fate==OpenMorphTissue.Fate.Empty)continue;
                            Color32 pixel;
                            if(!trial.living[i])
                                pixel=new Color32(76,69,71,255);
                            else
                            {
                                float fraction=Mathf.Clamp01(
                                    trial.cellEnergy[i]/
                                    OpenMorphResourceTrial.Capacity(cell.fate));
                                byte red=(byte)Mathf.RoundToInt(
                                    Mathf.Lerp(161f,84f,fraction));
                                byte green=(byte)Mathf.RoundToInt(
                                    Mathf.Lerp(72f,190f,fraction));
                                pixel=new Color32(red,111,green,255);
                            }
                            board[(row*H+y)*(W*4)+col*W+x]=pixel;
                        }
                    }
                }
            }
            var image=new Texture2D(W*4,H*3,TextureFormat.RGBA32,false);
            image.filterMode=FilterMode.Point;
            image.SetPixels32(board);image.Apply(false,false);
            try
            {
                File.WriteAllBytes(folder+"/actual_resource_survival.png",
                    image.EncodeToPNG());
                File.WriteAllText(folder+"/resource_survival_metrics.csv",
                    csv.ToString());
                File.WriteAllText(folder+"/resource_survival_trace.csv",
                    timeline.ToString());
            }
            finally {UnityEngine.Object.DestroyImmediate(image);}
            File.AppendAllText(folder+"/provenance.txt",
                "v1.5 frozen-body resource trial, 120 ticks. Columns: "+
                "pair3 Full/Neutral, pair4 Full/Neutral. Rows top to "+
                "bottom: rich/moderate/scarce. Green=high reserve, "+
                "red=low reserve, grey=dead tissue. Model has no "+
                "growth/reproduction and is NOT ecological fitness.\n");
        }

        static string Format(float value)
        {
            return value.ToString("F6",
                System.Globalization.CultureInfo.InvariantCulture);
        }
    }
}
#endif
