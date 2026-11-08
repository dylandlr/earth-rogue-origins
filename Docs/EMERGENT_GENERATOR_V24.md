# v2.4 — Causal branch export acceptance gate

This milestone adds an **offline, standard-library-only** integrity check for the v2.3 Unity generation output. It does not change the game's evolution policy.

## Run the actual Unity experiment on Windows

```powershell
git pull origin main
.\scripts\export-emergent-creatures.ps1 -UnityEditor "C:\Program Files\Unity\Hub\Editor\6000.6.5f1\Editor\Unity.exe"
```

When the exporter has finished, locate the folder containing `population.csv` and `causal_branch_ablation.csv` and run:

```powershell
python scripts/audit-causal-branches.py "PATH_TO_EXPORT_FOLDER"
python -m unittest discover -s scripts/tests -p "test_audit_causal_branches.py"
```

The audit writes `audit/causal_audit.json` and (when interventions exist) `audit/branch_effect_types.csv`. Exit code 1 indicates a failed accounting check. It validates branch scores against final-generation population rows and checks exported occupancy masks via their count identities.

**Important distinction:** `redistribution_cells > 0` means that two occupancy masks must have both gains and losses, rather than simply adding or removing cells. It does not mean a novel appendage or an organ has formed. `pure_extent_compatible` means the counts are consistent with an extent-only change, but cannot prove it without the masks.

## Follow-on engineering gate: actual functional tissue

Do not promote a knockout to a functional innovation based on shape alone. Next intervention should record masks in the same world coordinates and test:

- tissue connected components and their positions relative to the main body,
- viable-cell maintenance and growth costs (nutrients and energy),
- a mechanically simulated force or displacement,
- sensing and resource capture in a repeatable world trial,
- fitness changes under matched environmental seeds and disabled-component controls.

For every intervention, preserve genome ID, genetic seed, environmental seed, cell budget, development steps, and simulation version. Run multiple seeds and compare distributions, including failed forms. No visual geometry should be added as a cosmetic substitute for simulated tissue.

**Verification status:** audit source and synthetic tests are committed on a feature branch; Windows Unity rerun and local Python test execution have not yet been observed.
