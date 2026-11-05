# Earth: Rogue Origins — Resource-Constrained Tissue v1.5

## Grounding

The actual Unity v1.4 growth-channel ablation yielded:

| Pair | Full | GrowthOnly | BrakeOnly | Neutral |
|---|---:|---:|---:|---:|
| 1 | 122 | 118 | 125 | 121 |
| 2 | 173 | 170 | 175 | 174 |
| 3 | 190 | 202 | 222 | 248 |
| 4 | 235 | 228 | 247 | 238 |

All sixteen developed tissue masks had one connected component and no
border contact. Pair 3 was the strongest growth-suppressing phenotype,
but growth inhibition does not automatically confer an advantage.

## Implemented first resource trial

`OpenMorphResourceTrial` runs on **frozen developed tissue**. It
does not change the existing authoritative growth, GRN, or mutation
rules, and therefore isolates postdevelopmental maintenance from
developmental history.

- Each initially occupied cell receives an explicit nutrient reserve.
- Environments have a finite starting nutrient pool and bounded,
  externally supplied replenishment per tick. The only new usable
  energy enters through that explicit supply.
- Exposed tissue surfaces can absorb a portion of the shared nutrient
  pool, capped by local capacity and uptake limits. Distribution
  is proportional to uptake demand, avoiding raster-order privilege.
- Adjacent live cells exchange energy conservatively. Transport-type
  tissues have a faster exchange rate, while storage-type tissues
  have higher energy capacity and lower upkeep.
- Tissue types have distinct upkeep. A cell that cannot pay upkeep
  for three consecutive ticks dies; any remaining reserve is
  explicitly accounted for as nonrecoverable discarded energy.
- The trial records `initialEnergy + initialPool + externalAdded`
  versus `consumed + discarded + remainingEnergy + remainingPool`.
  `conservationError` must remain near zero. "Consumed" is no
  longer usable but is included in the accounting ledger.
- No hidden nutrient recovery or supply appears within this trial.
  The prior developmental `Cell.energy` field is not used as a
  nutrient source, avoiding double-counting provisional developmental
  energy heuristics.

## Controlled experimental arms

Two previously analyzed baseline genomes are used: **pair 3** (strong
recurrent growth inhibition) and **pair 4** (comparative phenotype).
Each is developed with `Full` and `Neutral` recurrent GRN birth
channels (same genotype and stochastic field), creating four frozen
body masks. Each body is then placed in three environments:

| Environment | Initial nutrient pool | Replenishment/tick | Uptake limit / exposed edge |
|---|---:|---:|---:|
| Scarce | 6 | 0.25 | 0.09 |
| Moderate | 18 | 1.2 | 0.16 |
| Rich | 50 | 4.5 | 0.25 |

All 12 trials last 120 maintenance ticks.

## Actual Unity audit outputs

The standard `scripts/export-open-tissue.ps1` includes:

- `actual_resource_survival.png` — actual C# 4×3 contact sheet.
  Columns: pair 3 Full, pair 3 Neutral, pair 4 Full, pair 4 Neutral.
  Rows from top: rich, moderate, scarce. Green indicates more
  remaining nutrient reserve, red less, grey dead cells.
- `resource_survival_metrics.csv` — initial/surviving cells,
  deaths, nutrient intake, consumption, transport/storage cell
  counts, unmet maintenance, and energy-ledger residual.
- `resource_survival_trace.csv` — living cells, internal energy
  and environmental nutrient pool at each tick.

Run:

```powershell
git pull origin main
.\scripts\export-open-tissue.ps1 -UnityEditor "C:\Program Files\Unity\Hub\Editor\6000.6.5f1\Editor\Unity.exe"
```

The exporter checks that the new resource audit outputs and
required numeric columns exist in a **new** run directory.

## Evaluation gates

1. Unity compiles; all resource and existing anatomy tests pass.
2. Energy conservation ledger residual stays below numeric tolerance.
3. Identical genotype and environment produce identical trajectories.
4. Without external supply, cells starve instead of receiving free energy.
5. More nutrient supply does not automatically imply identical survival:
   morphology, exposed surface and tissue fate can alter survival.
6. Compare *fraction of cells surviving* and maintenance cost per
   initial cell. Absolute survivor count alone unfairly favors larger
   organisms.

## Scientific limitations

This is a **toy resource-maintenance model on frozen 2D tissue**, not
a working organism or evolutionary fitness benchmark. Exposed
surfaces can absorb nutrient irrespective of whether their candidate
tissue fate represents an authentic feeding mechanism. Cell death
does not trigger tissue mechanics or damage propagation, and there is
no reproduction, growth during maintenance, resource competition
between separate organisms, locomotion, natural selection or ecological
simulation. The finite pool is replenished exogenously and lacks
explicit environmental nutrient diffusion.

In particular, a lower maintenance burden does not by itself mean
that the evolution of inhibition is adaptive. To infer selection,
we will need lineage inheritance, reproduction, competition,
environmental replication, held-out seeds and lifetime fitness,
not just survival on fixed body masks.

## Next direction

After validating v1.5, add a dynamic developmental energy ledger
and couple nutrient flow to growth and programmed death without
creating energy. Then model genuine sensing and actuation from tissue
physics; only then start multi-generation environmental selection
with proper controls.
