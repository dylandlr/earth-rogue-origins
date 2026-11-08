# Branch placement search: no promoted candidate

The predeclared search evaluated 135 combinations of branch attachment offsets, fate response and transfer capacity. Training seeds301-308 selected three finalists; validation309-312 chose attachX offset0, attachY offset-0.2, fateResponse0.9, capacity4. The winner was committed at c6ee351ce52161123c643193f9a07832dafab30c before generating test maps401-424.

## Fresh-seed result

Mean delivered nutrient out of100, averaged across both vertical source directions per seed:

| Candidate | Delivery |
|---|---:|
| Previous fixed candidate | 7.889957 |
| New frozen placement | 7.665735 |
| New placement with capacity disabled | 6.938393 |

New minus previous: -0.224222 (-2.84%), positive on4/24 seeds, paired bootstrap95% interval[-0.919991,0.591696]. New minus capacity-disabled: +0.727342, positive on24/24, interval[0.497897,1.006528]. Retain prior candidate; no improvement established. The interval includes both benefit and harm. Test conditions change developmental seeds and source direction, so the result does not isolate why generalization failed.

## Reproduction

Use Python3 with numpy,pandas and Mono6.12 compiler/runtime. Place Runner.cs, Shim.cs, capacity.py and all eight OpenMorph*.cs files together. The downloadable archive includes those dependencies. C# simulation source is pinned to8524cb46a22b7fb5e7ee5e6a69af71fac3351d9a; Shim.cs is identical to ../joint_selection/Shim.cs and capacity.py to ../results/scarcity/capacity.py. Run:

```
bash run_portable.sh 301 312
python search.py select
bash run_portable.sh 401 424
python search.py test
```

The repository retains summaries, frozen decision, per-seed results and source hashes. The archive additionally retains all raw maps and assay rows. Test scoring only evaluates frozen winner and previous candidate morphologies, with all five capacity levels calculated by the shared vectorized assay. No retuning follows test inspection.

## Verification and scope

All972 maps passed length/alphabet validation; all assay pulses checked nonnegative material and conservation to1e-8. Compilation succeeded with an existing unused-variable warning. Development used the Mono Unity math shim, not the Unity editor. Results concern artificial fixed-tissue nutrient delivery; they do not demonstrate survival, reproduction, organ evolution or Unity runtime parity. Changing attachment alters both geometry and fate, so it is not a pure transport-location intervention.

Next experiment should train on both axes and use independently held-out source conditions and seeds, with matched morphology controls. Seeds401-424 are now consumed and must not be reused as a fresh holdout.
