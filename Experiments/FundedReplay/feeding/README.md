# Equal-food schedules: early transport gains do not persist

The predeclared diagnostic used inspected seeds601-624, demand0.01, threshold30, and100 units of food offered by step180. Four source directions were averaged within each seed. No resource was offered afterward. All food was absorbed in these runs: the inaccessible-food ledger stayed zero under every schedule and intervention.

| Schedule | Horizon | Alive cell-time, ordinary | Alive cell-time, paid transport | Difference (percentage points) |
|---|---:|---:|---:|---:|
| gradual | 180 | 34.208% | 35.609% | +1.401 |
| gradual | 360 | 27.383% | 28.721% | +1.338 |
| gradual | 720 | 17.526% | 17.416% | -0.110 |
| pulses | 180 | 39.554% | 41.136% | +1.582 |
| pulses | 360 | 32.162% | 32.875% | +0.713 |
| pulses | 720 | 17.653% | 17.461% | -0.191 |
| upfront | 180 | 58.221% | 60.898% | +2.677 |
| upfront | 360 | 35.335% | 35.012% | -0.323 |
| upfront | 720 | 17.674% | 17.506% | -0.168 |

At720 steps, transport's alive-time difference is negative for all schedules. Paired bootstrap95% intervals, in percentage points: upfront[-0.184,-0.151], pulses[-0.212,-0.170], gradual[-0.189,-0.007]. These are exploratory, unadjusted intervals using reused seeds. Final living fractions (ordinary/transport): upfront0/0%, pulses0.140/0%, gradual0.917/0.359%. Almost all cells are dead.

The earlier180-step benefit should be described as a short-horizon effect under a finite food supply. The endpoint tradeoff changes with schedule and observation time. This does not show that transport is universally harmful; it shows that the previous short run did not establish persistence. Food timing also changes early starvation losses, so comparisons between schedules are whole interventions, not pure effects on late depletion.

## Verification and reproduction

Five feeding tests passed, including equal total food, no revival by delayed food, invalid schedule rejection and analytic single-cell resource accounting. All96 prior upfront180 per-seed outcomes reproduced to1e-12. Every step checked conservation including inaccessible food, dead material and upkeep, and nonnegative resources. The experiment comprises576 intervention-direction-seed-schedule evaluations, each720 steps.

Requires Python3,numpy,pandas. Keep feeding beside survival, with survival/maps.json and survival/paired.csv. From feeding run:

```
python -m unittest -v test_feeding.py
python run.py
```

The archive preserves these inputs, model, tests, raw outputs and paired summaries. No new developmental maps or tuning were used. Data are Mono-derived fixed tissues; Unity parity, biological validity, reproduction and evolved transport placement remain unverified.

Next work should evaluate food acquisition or sustained environmental supply with a finite external budget and meaningful resource competition. Reproduction should only be added with explicit material costs. Fresh seeds will be needed for confirmation once that ecology is specified; do not reuse601-624 as unseen data.
