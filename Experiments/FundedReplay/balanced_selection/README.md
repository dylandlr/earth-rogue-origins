# Balanced-direction selection: retain previous candidate

Training and validation used both source axes and signs. The 135-candidate grid selected attachment offsets (-0.2,0), fateResponse0.9, capacity4. This winner was committed at82b8dfe8f0dc709576b2342be7be45f0c8845ec1 before test seeds501-524 were generated. Development seeds301-312 were reused deliberately as development data.

On24 fresh seeds, mean delivered nutrient (out of100) was4.896270 for the selected candidate,4.776307 for the previous candidate and3.554450 for the selected morphology with capacity disabled. Paired selected-minus-previous mean was+0.119963 (+2.51%), positive on13/24 seeds, bootstrap95% interval[-0.264184,0.492894]. The predeclared promotion criterion was not met. Retain previous candidate.

Capacity increased delivery on24/24 seeds: paired mean+1.341819, interval[1.138635,1.573143]. Descriptive horizontal delta was+0.242307 and vertical delta-0.002381; both intervals included zero. These do not establish improved transport placement. Tissue geometry and fate both change with attachment.

## Reproduce

Use Mono6.12 and Python3 with numpy,pandas. The archive includes the eight pinned OpenMorph C# sources, Unity shim, Runner.cs, capacity.py and scripts. In the repository, obtain the unchanged Runner.cs/run_portable.sh from ../spatial_selection, Shim.cs from ../joint_selection, capacity.py from ../results/scarcity; simulation source remains pinned to8524cb46a22b7fb5e7ee5e6a69af71fac3351d9a. Source hashes are recorded.

```
bash run_portable.sh 301 312
python search.py select
bash run_portable.sh 501 524
python search.py test
python axis_report.py
```

All972 maps passed size/alphabet checks. Each diffusion pulse passed material conservation and nonnegativity assertions. Compilation succeeded with one existing unused-variable warning. Archive includes raw maps and assay rows; repository includes selection summaries, per-seed comparisons, separate-axis results and frozen decision.

This is Mono shim execution, not verified Unity parity. It tests artificial fixed-tissue delivery under familiar directions with fresh developmental seeds. Survival, reproduction and novel-environment generalization remain untested. Seeds501-524 are now consumed; no test-driven retuning was performed. The earlier spatial experiment and this experiment use different test distributions, so their headline percentages are not a controlled comparison of training methods.
