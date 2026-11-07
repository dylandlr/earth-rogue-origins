# Starvation pilot: longer cell survival time, endpoint tradeoff

The predeclared primary condition (maintenance0.005, starvation threshold15) passed its within-assay benefit criterion on fresh developmental seeds601-624. Both interventions use the same fixed candidate tissue; only transport capacity is enabled/disabled. No selection or retuning occurred.

| Primary outcome | Disabled | Enabled | Paired change |
|---|---:|---:|---:|
| Normalized alive cell-time | 47.2492% | 50.7265% | +3.4773 percentage points |
| Final surviving fraction | 42.4217% | 46.2156% | +3.7939 percentage points |

Alive cell-time improved on24/24 seeds, a7.36% increase relative to disabled. Paired bootstrap95% interval for its change: [2.9404,4.0075] percentage points. Final survival change interval: [3.2088,4.3723] points. Directional conditions are averaged within seed, not treated as independent samples.

All nine demand/threshold combinations had positive mean alive-time differences. However, demand0.01 with threshold30 produced fewer final survivors:44.0183% disabled versus41.2622% enabled, a-2.7561-point change (interval[-4.6802,-1.1146]). Alive-time still increased2.6766 points. Transport can extend cell-time while worsening the final survivor count under these mechanics; the cause of that tradeoff has not been isolated.

## Model and evidence limits

Starvation accumulates unmet demand as damage, with no healing. Death removes all adjacent transport edges; residual resource is sealed in the dead-material ledger. There is no reproduction, regrowth or evolutionary selection. The model supplies a one-time boundary resource pulse after development, with no internal starting reserve. This imposed initialization and irreversible damage rule can strongly affect outcomes. Parameters are uncalibrated. Results establish a benefit of imposed conductance within this assay, not evolved organ placement, biological survival validity, or whole-organism fitness. Sensitivity intervals are descriptive and unadjusted.

All eight C# source hashes were restored exactly from pinned commit8524cb46a22b7fb5e7ee5e6a69af71fac3351d9a (the archived copies include an extra trailing newline). Development used Mono6.12 and the recorded Unity math shim; Unity editor parity remains unverified. Compilation passed with one existing unused-variable warning. Six contract tests passed. All1728 condition evaluations enforced resource conservation and nonnegative resources at every step.

## Reproduction

Python3 requires numpy,pandas. Archive contains24 maps and all model sources. Repository maps.json preserves exact map contents; to unpack:

```
python -c "import json,pathlib; p=pathlib.Path('maps'); p.mkdir(exist_ok=True); [(p/k).write_text(v) for k,v in json.load(open('maps.json')).items()]"
python -m unittest -v test_survival.py
python survival.py maps
```

To regenerate maps, compile the included Runner.cs, Shim.cs and eight OpenMorph*.cs files with Mono mcs, then run `mono run.exe 601 624`. Runner uses the prior genome29 definition, fixed branch fateResponse0.9 and seeds passed on the command line. Hashes record source and map identities.

Next investigation: trace the high-demand endpoint reversal and compare alternative prespecified resource initialization/death-recovery rules on new seeds. Do not tune against601-624 as a fresh holdout.
