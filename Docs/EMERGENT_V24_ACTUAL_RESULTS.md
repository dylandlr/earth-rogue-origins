# v2.4 measured Unity result — seed 3452

Evidence received: `actual_emergent_generation.png`, `actual_emergent_structures.png`, `actual_expression_alignment(2).png`, `population.csv`, `champions.csv`, `branch_expression.csv`, `causal_branch_ablation.csv`, `provenance.txt`, and final-generation genomes 24–31.

## Experiment

Native Unity C# recurrent GRN, four generations, eight evaluated individuals per generation, no prewritten species templates. The presentation-fitted structural image is **not in the same coordinate frame** as developed tissue. This is survival-weighted experimental selection, not ecosystem locomotion or autonomous organism reproduction.

## Measured population trajectory

| Generation | Mean raw fitness | Mean novelty |
| --- | ---: | ---: |
| 0 | 0.585409 | 0.2852 |
| 1 | 0.724295 | 0.2482 |
| 2 | 0.821954 | 0.2229 |
| 3 | 0.830667 | 0.1983 |

Champion #31 fitness: 0.966667; 152 tissue cells, three genes, two causally active branches. The selected champion's fitness uses an engineered proxy, not a functional ecology test.

## Six measured non-root branch interventions

| Organism | Innovation | Original | Ablated | Symmetric difference | Net occupied-cell loss | Mixed gain/loss cells | Change fraction |
| --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: |
| 24 | 1 | 182 | 145 | 41 | 37 | 4 | 0.222826 |
| 27 | 1 | 197 | 184 | 13 | 13 | 0 | 0.065990 |
| 27 | 2 | 197 | 163 | 34 | 34 | 0 | 0.172589 |
| 29 | 1 | 151 | 151 | 8 | 0 | 8 | 0.051613 |
| 31 | 1 | 152 | 144 | 10 | 8 | 2 | 0.065359 |
| 31 | 2 | 152 | 141 | 11 | 11 | 0 | 0.072368 |

All six have `morphologically_active=true` under the 3.5% effect threshold. Counts, IoU, and changed fractions satisfy the occupancy-mask identities within CSV rounding precision. `mixed gain/loss cells` is `changed_cells - abs(original_cells - ablated_cells)`, **not** a direct count of independently emerging appendage cells. For #29, equal-size masks with eight different cells require four losses and four gains. This establishes spatial redistribution but not organ formation.

## Priority experiment

1. Export native-resolution original and same-seed knockout occupancy masks for organisms **29, 31, and 24**, in exactly the same coordinates.
2. Record per-gene local guidance, cell fates, gradients, cell division/death, nutrient costs and region identity. Differentiate local tissue placement from global shape rearrangement.
3. Test whether intervention effects recur over multiple developmental and environmental seeds with paired controls. The current knockout is only within an individual/seed.
4. Add one minimal physical consequence to developed tissue (resource uptake or directed contractility), with explicit energy accounting, and compare survival-relevant output under gene knockout.
5. Keep function and ecological outcome independent of any decorative structural renderer.

**Verification status:** Values in this report were checked from user-supplied Unity CSVs, not a rerun of Unity in this environment. Additional audit script test execution on the Windows project remains pending.
