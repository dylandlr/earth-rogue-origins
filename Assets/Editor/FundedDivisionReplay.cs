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

    // One pulse, equal total supply in each orientation. Every exposed site
    // samples the same fraction of its local nutrient into a fresh ledger.
    private static (int exposed,double captured) Sample(OpenMorphTissue.Field field,
        int horizontal,int vertical)
    {
        int w=field.width,h=field.height;
        var ledger=new OpenMorphMaterialLedger(w,h);
        int exposed=0;
        for(int y=0;y<h;y++)
        for(int x=0;x<w;x++)
        {
            double nutrient=1+horizontal*(2.0*x/(w-1)-1)+vertical*(2.0*y/(h-1)-1);
            ledger.Seed(x,y,nutrient);
        }
        double before=ledger.TotalMaterial();
        for(int y=0;y<h;y++)
        for(int x=0;x<w;x++)
        {
            if(field.At(x,y).fate==OpenMorphTissue.Fate.Empty)continue;
            // Reflecting walls do not expose a cell to external material.
            bool edge=(x>0 && field.At(x-1,y).fate==OpenMorphTissue.Fate.Empty)
                || (x+1<w && field.At(x+1,y).fate==OpenMorphTissue.Fate.Empty)
                || (y>0 && field.At(x,y-1).fate==OpenMorphTissue.Fate.Empty)
                || (y+1<h && field.At(x,y+1).fate==OpenMorphTissue.Fate.Empty);
            if(!edge)continue;
            exposed++;
            ledger.Step(x,y,.5*ledger.nutrient[y*w+x],0,0);
        }
        double captured=ledger.reserve.Sum();
        if(Math.Abs(before-w*h)>1e-8 || Math.Abs(before-ledger.TotalMaterial())>1e-8 ||
            Math.Abs(before-ledger.nutrient.Sum()-captured)>1e-8)
            throw new InvalidOperationException("Frozen uptake balance failed.");
        return (exposed,captured);
    }

    private static void WriteUptake(string folder,OpenMorphTissue.Field intact,
        OpenMorphTissue.Field knockout)
    {
        using(var csv=new StreamWriter(Path.Combine(folder,"frozen_uptake.csv")))
        {
            csv.WriteLine("field,intact_exposed,knockout_exposed,intact_capture,knockout_capture,delta_capture");
            foreach(var gradient in new[]{("uniform",0,0),("right",1,0),("left",-1,0),
                ("up",0,1),("down",0,-1)})
            {
                var a=Sample(intact,gradient.Item2,gradient.Item3);
                var b=Sample(knockout,gradient.Item2,gradient.Item3);
                csv.WriteLine(FormattableString.Invariant($"{gradient.Item1},{a.exposed},{b.exposed},{a.captured:R},{b.captured:R},{a.captured-b.captured:R}"));
            }
        }
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
        WriteUptake(folder,intact,knockout);
        var metrics=OpenMorphPairMetrics.Compare(intact,knockout);
        string summary=FormattableString.Invariant($"seed={genome.seed} width={Width} height={Height} steps={Steps} cap={Cap} nutrient_per_site={Supply} first_different_decision_tick={first} intact_sites={intact.Occupied()} knockout_sites={knockout.Occupied()} intact_only={metrics.onlyA} knockout_only={metrics.onlyB}");
        File.WriteAllText(Path.Combine(folder,"summary.txt"),summary+Environment.NewLine);
        Debug.Log(summary+" Output: "+folder);
    }
}
