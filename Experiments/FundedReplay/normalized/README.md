# Fresh-seed normalized supply: modest boundary transport benefit

Protocol committed before generating seeds701-724. All conditions use the retained genome29 morphology,0.3 starting reserve/cell deducted from the total budget,720 steps and fixed prior damage rules. No post-test tuning or candidate search.

| Access | Potential supply / baseline maintenance | Final alive, ordinary | Final alive, transport |
|---|---:|---:|---:|
| Boundary | 0.5 | 13.637% | 14.814% |
| Boundary | 1.0 | 13.637% | 14.867% |
| Boundary | 1.5 | 13.637% | 14.867% |
| Uniform | 0.5 | 0.000% | 0.000% |
| Uniform | 1.0 | 100.000% | 100.000% |
| Uniform | 1.5 | 100.000% | 100.000% |

The primary boundary/1.5 condition meets its predeclared within-model benefit criterion: transport increases final survival1.2306 percentage points, paired bootstrap95% interval[0.9142,1.5620], with22/24 seeds improved. Alive cell-time increases1.0459 points. This supports the intervention under these fixed assumptions; it does not establish evolved placement or biological organ function.

Uniform access at ratios1 and1.5 maintains100% survival through720 steps for both interventions. Uniform access at0.5 ends at0% survival. Boundary survival barely changes as the nominal supply ceiling rises: potential intake does not guarantee absorption or delivery to interior cells. Internal uptake saturation, diffusion, feeding geometry and irreversible starvation remain coupled constraints. The controls point to a spatial delivery bottleneck, but do not isolate its components. Uniform access is artificial and cannot be treated as an evolved solution.

At ratio1, nominal supply covers baseline maintenance only; starting reserves and redistribution can temporarily cover transport overhead. Survival through720 is not a claim of indefinite persistence. Total food budget was scaled to exceed whole-tissue demand over this horizon, not to simulate ecological scarcity or competition.

## Verification and reproduction

All eight pinned simulation source hashes match earlier studies. Development used Mono6.12 and the existing Unity math shim; compilation passed with one existing unused-variable warning. Three new contract tests passed for normalized uptake ceilings, reserve accounting and uniform-surplus viability. All1152 condition evaluations enforced resource conservation and nonnegative resources at every step. No source rules or assay parameters were adjusted after seeing these outcomes.

Python3 requires numpy,pandas. The archive includes exact fresh maps and all model sources. Run:

```
python -m unittest -v test_normalized.py
python run.py
```

If using repository files only, unpack maps.json first:

```
python -c "import json,pathlib; p=pathlib.Path('maps'); p.mkdir(exist_ok=True); [(p/k).write_text(v) for k,v in json.load(open('maps.json')).items()]"
```

To regenerate maps from the archive, run `bash run_portable.sh 701 724` with Mono mcs installed. C# source pin8524cb46a22b7fb5e7ee5e6a69af71fac3351d9a; Runner.cs matches ../survival/Runner.cs. hashes.json records all source inputs. Per-seed results and reports are committed; raw outputs also accompany the archive.

Next: diagnose the boundary delivery ceiling with explicit flux and connectivity measurements before changing developmental rules. Preserve this benchmark; seeds701-724 are now inspected and must not be labeled a fresh holdout again. Unity integration, reproduction, competition and evolutionary adaptation remain outstanding.
