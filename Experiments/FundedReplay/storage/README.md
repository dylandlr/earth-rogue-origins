# Physical storage changes the conclusion

This prototype separates physical capacity C from uptake target G and diffuses concentration mass/C. Incoming transfer is bounded by receiver free space. Incremental construction is paid from the original budget; maintenance uses available food and contributes to starvation shortfall. Extra capacity is imposed, not evolved.

| Condition | C / G | Final alive, ordinary | Final alive, transport |
|---|---|---:|---:|
| Baseline | 1 / 1 | 13.64% | 14.87% |
| Storage only, free | 10 / 1 | 4.56% | 4.56% |
| Storage and uptake, free | 10 / 10 | 9.91% | 10.81% |
| Storage and uptake, costed | 10 / 10 | 9.15% | 10.09% |

Every enlarged-storage condition underperformed baseline on all24 reused seeds in both transport arms. For costed storage with transport, final survival falls4.775 percentage points; paired bootstrap95% interval[-5.474,-4.085]. Results are exploratory, unadjusted, with directions averaged within seed.

The decline already occurs before costs. Interpreting capacity as dilution volume reduces concentration for a given resource mass and slows redistribution relative to the earlier mass-based model. Thus the earlier48.3% result from enlarging an uptake gate cannot be treated as evidence that realistic storage improves survival. Construction and maintenance further reduce survival in this particular parameterization. Neither concentration law is biologically validated; changing equations changes the mechanism under test.

The costed variant spends95.4 units on construction on average. Mean storage maintenance consumed is9.25 units with transport, paid from the same initial resource budget. Construction material remains in a sealed ledger after death; nothing is recycled. These numerical prices are declared assumptions, not fitted biological estimates.

## Verification and reproduction

Five tests passed: exact cost accounting, independent uptake gate, physical capacity bound, unaffordable construction, and no-patch equivalence. Every step checks resource conservation, capacity bounds and nonnegative external/internal food. All768 intervention runs completed720 steps. No tuning or candidate promotion followed inspection. Inputs are the already inspected701-724 maps; no fresh-confirmation claim.

Requires Python3,numpy,pandas. Keep storage beside normalized/maps.json. From storage run:

```
python -m unittest -v test_storage.py
python run.py
```

Archive includes exact maps, source and raw outcomes. The repository preserves reports and per-seed summaries. This remains a Python post-development assay using Mono-derived tissue, not Unity integration.

The conservative incoming-flux limiter uses space before simultaneous outflow; this may underuse vacated space. Capacity C is uniform across cells and also acts as dilution volume. These assumptions require validation before evolutionary selection. Next define separate mobile nutrient and stored reserve pools, with explicitly costed exchange, so increased storage need not automatically enlarge the mobile diffusion volume. Preserve this failed model as a control. Reproduction and adaptive morphogenesis remain untested.
