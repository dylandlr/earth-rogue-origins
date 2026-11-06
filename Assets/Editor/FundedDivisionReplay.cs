using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using EarthRogueOrigins.PixelCreatures;
using UnityEditor;
using UnityEngine;

public static class FundedDivisionReplay
{
    [Serializable]
    private sealed class Export
    {
        public int id, seed;
        public OpenMorphGenome.Gene[] genes;
    }

    private const int Width=64, Height=48, Steps=36, Cap=360;
    private const double Supply=4;
    private static OpenMorphTissue.Field Replay(OpenMorphGenome genome)
    {
        var field=OpenMorphTissue.Grow(genome,Width,Height,Steps,Cap,true,true,
            fundedDivision:true,nutrientPerSite:Supply);
        if(Math.Abs(field.material.TotalMaterial()-(Width*Height*Supply+1))>1e-7)
            throw new InvalidOperationException("Material balance failed.");
        return field;
    }

    private static int Key(OpenMorphTissue.DivisionAttempt a)
    {
        return (a.tick*Height+a.daughterY)*Width+a.daughterX;
    }

    private static string Detail(OpenMorphTissue.DivisionAttempt a)
    {
        return FormattableString.Invariant($"{a.parentX},{a.parentY},{a.reserveBefore:R},{a.outcome}");
    }

    [MenuItem("Tools/Earth Rogue Origins/Replay funded genome 29")]
    public static void Run()
    {
        string root=Path.GetDirectoryName(Application.dataPath);
        var input=JsonUtility.FromJson<Export>(File.ReadAllText(
            Path.Combine(root,"Experiments/FundedReplay/genome_29.json")));
        if(input.id!=29 || input.seed!=1548701035 || input.genes==null)
            throw new InvalidDataException("Unexpected genome fixture.");
        var genome=new OpenMorphGenome {seed=input.seed};
        genome.genes.AddRange(input.genes);
        var intact=Replay(genome);
        var knockout=Replay(OpenMorphCausalBranches.Ablate(genome,1));
        var left=intact.divisionAttempts.ToDictionary(Key);
        var right=knockout.divisionAttempts.ToDictionary(Key);
        string folder=Path.Combine(root,"Temp","FundedReplay29",
            Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(folder);
        int first=-1;
        using(var csv=new StreamWriter(Path.Combine(folder,"division_pairs.csv")))
        {
            csv.WriteLine("tick,x,y,intact_parent_x,intact_parent_y,intact_reserve,intact_outcome,knockout_parent_x,knockout_parent_y,knockout_reserve,knockout_outcome,difference");
            foreach(int k in left.Keys.Union(right.Keys).OrderBy(k=>k))
            {
                bool l=left.TryGetValue(k,out var a), r=right.TryGetValue(k,out var b);
                string difference=!l || !r?"proposal":a.parentX!=b.parentX || a.parentY!=b.parentY?
                    "parent":a.outcome!=b.outcome?"outcome":"same_decision";
                var site=l?a:b;
                if(first<0 && difference!="same_decision")first=site.tick;
                csv.WriteLine(FormattableString.Invariant($"{site.tick},{site.daughterX},{site.daughterY},{(l?Detail(a):",,,absent")},{(r?Detail(b):",,,absent")},{difference}"));
            }
        }
        var metrics=OpenMorphPairMetrics.Compare(intact,knockout);
        string summary=FormattableString.Invariant($"seed={genome.seed} width={Width} height={Height} steps={Steps} cap={Cap} nutrient_per_site={Supply} first_different_decision_tick={first} intact_sites={intact.Occupied()} knockout_sites={knockout.Occupied()} intact_only={metrics.onlyA} knockout_only={metrics.onlyB}");
        File.WriteAllText(Path.Combine(folder,"summary.txt"),summary+Environment.NewLine);
        Debug.Log(summary+" Output: "+folder);
    }
}
