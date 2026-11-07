# Earth: Rogue Origins — v2.5 Research-to-Implementation Contract

## Evidence boundary

The deep literature review supports testing local NCA updates (Mordvintsev et al., 2020), soft-cell mechanics and chemical sensing (Deshpande et al., 2025; JAX-Morph), conservative transport (Flow-Lenia), low-rank developmental perturbations (Hartl et al., 2026), and quality-diversity search (MAP-Elites). These are *candidate techniques*, not proof that our sim develops organs.

Our Unity data (seed 3452, 4 generations, population 8) show six causally active final-generation subtree knockouts. The #29 innovation-1 knockout changes 8 occupancy sites without a change in total occupied count (4 removals + 4 additions); no measured nutrient uptake, movement, reproduction, or independent tissue function exists yet. Structural PNG and tissue PNG do not share a coordinate frame.

### Important cautions when interpreting the literature

- The original Growing NCA is a trained-to-target image model, not spontaneously evolving biological tissue. Its simulation should be a baseline, not our default body generator.
- Flow-Lenia conserves its model's flowing activation/mass in the conservative update; that does not, by itself, provide biological nutrient conversion, growth-energy thermodynamics, or conserved reproduction.
- A 2D division preserving *area* uses daughter radius = parent radius / sqrt(2); in 3D, volume preservation uses radius / cbrt(2). Halving radius does not conserve either.
- The 2D explicit 5-point diffusion bound is D*dt/dx^2 <= 1/4 (not an unconditional bound on dt). Boundary conditions, nonlinear reactions, and mechanics require their own stability checks.
- An equal-mask-size spatial knockout difference establishes changed occupancy but **not physical cell migration**. Time-resolved lineage data are necessary.
- Claiming biological realism from an uncalibrated Morse potential or bioelectric switch would be premature. Their useful initial role is as experimental models.

## v2.5A — current code

`Assets/Scripts/Anatomy/OpenMorphMaterialLedger.cs` implements a deterministic **material** ledger, separate from `OpenMorphTissue.Grow`.

For each lattice site i, define nutrient N_i, internal reserve R_i, biomass B_i, and waste W_i (all nonnegative, consistent mass-equivalent units). External resources are absent in the first closed-system assay.

An uptake request u_i produces actual transfer q_i = min(u_i, N_i). Maintenance request m_i and growth request g_i transfer available reserve after earlier debits:

```text
q_i = min(u_i, N_i)
N_i <- N_i - q_i              R_i <- R_i + q_i
m_i* = min(m_i, R_i)
R_i <- R_i - m_i*             W_i <- W_i + m_i*
g_i* = min(g_i, R_i)
R_i <- R_i - g_i*             B_i <- B_i + g_i*
```

Thus M = sum_i(N_i + R_i + B_i + W_i) is invariant in a closed assay, up to floating-point roundoff.

Nutrient diffusion uses four-neighbor no-flux 2D transport:

```text
N_i(t+dt) = N_i(t) + alpha * sum_{j in neighbors(i)}(N_j(t)-N_i(t))
alpha = D * dt / dx^2, 0 <= alpha <= 1/4.
```

The implementation processes each undirected grid edge once, credits and debits both ends, and rejects unstable `alpha`.

**Interpretation:** These variables are material proxies. Waste formation models degradation of *available resource*; it does not assert thermodynamic energy conservation or chemical stoichiometry. Cellular ATP, heat, and environment inputs/outputs must be modeled with a physically explicit budget later. This assay does **not** change the old artificial energy mechanism.

NUnit tests check material conservation, no free growth in a depleted environment, positivity and conservation under diffusion, deterministic replay, and invalid parameter rejection.

## v2.5B — next implementation: coupling to evolving tissue

A conservative coupling must use a single authoritative resource world, not a second energy array that both systems can modify. Add a `ResourceWorld` owning nutrient/waste and a per-occupied-cell reserve/biomass ledger, then apply:

1. Freeze current tissue topology and environmental fields at tick n.
2. Diffuse extracellular nutrient once, subject to stability constraint.
3. Compute local uptake requests as functions of actual cell fates and nutrient exposure, **not** an automatic organizer bonus.
4. Debit maintenance and actuation; insufficient reserve triggers inactivity and eventually death.
5. Compute all daughter-cell proposals from frozen parent state. Require both vacant adjacent space and enough debitable parent material for daughter biomass.
6. Resolve simultaneous candidate births deterministically (fixed per-cell/tick random hash), debit accepted parental biomass/reserves, conserve system totals, record rejected births separately.
7. Apply cell death/pruning: move lost biomass and reserve to detritus or extracellular fields, never delete them silently.
8. Emit event receipts including `environmentSeed`, `genomeSeed`, `tick`, `cellIndex`, `transferKind`, `before`, `after`, and account balances.
9. Export a per-tick resource conservation residual and negative-value count. Experimental growth must fail if either exceeds tolerance.

The current `OpenMorphTissue.Grow` includes a provisional replenishment of `current.energy` from guide/neighbor signals. This code cannot be used for ecological survival claims until fully replaced or isolated as a **legacy baseline**.

## Candidate developmental AI models (paired, fixed-compute budget)

- **A: existing recurrent GRN** — preserve as the first interpretable baseline.
- **B: GRN + local chemical gradients** — test whether signaling produces reproducible differentiated spatial regions.
- **C: lightweight NCA** — 16-channel state, local perception and asynchronous updates; exclude target-image losses in the main evolutionary treatment.
- **D: low-rank GRN modules** — `W_G = W_0 + sum_k beta_k A_k B_k^T`, evaluated only after baseline is stable.
- **E: optional bioelectric memory** — later ablation, not currently needed for first ecological fitness result.

Controller parameters may be evolved by mutation and selection; differentiable surrogate/JAX models can be separate experimental optimizers, with held-out Unity rollouts guarding against model exploitation.

## Experimental test design

For a genome G, gene-subtree ablation G^{-g}, development random seed xi, and environment seed omega, define paired causal functional effect:

```text
delta_g(xi,omega) = J(Develop(G,xi),omega)
                   - J(Develop(G^{-g},xi),omega)
CFC_g = mean_{xi,omega} delta_g(xi,omega)
```

To isolate *functional consequences of the same morphology* from *stochastic developmental differences*, store both realized tissue states and all seeds. Gene ablation removes descendants: this measures the subtree effect, not direct gene action. Report distributions and confidence intervals, not only a best case.

Initial J can be net captured usable material after maintenance, explicitly distinct from fitness. Later J may include survival time and viable offspring under competition, with a predeclared evaluation protocol. Randomized environmental variation and held-out seeds prevent optimizing against one nutrient placement.

Track:
- conservation residual e_t = |M(t+1)-M(t)-M_in(t)+M_out(t)|;
- spatial branch replacement and mass changes (from occupancy masks);
- tissue compactness/connectivity/fate-localization;
- functional uptake minus maintenance;
- genotype and phenotype diversity, lineage survival and extinction;
- mutation effect robustness across environmental seeds and development seeds.

## Acceptance gates

**A (now):** standalone accounting and diffusion tests compile/pass in Unity EditMode. Not yet verified in this environment.

**B:** headless resource/cell coupling tests demonstrate no reproduction or growth from empty reserves, and no unaccounted biomass even with 100+ simultaneously competing cell birth proposals.

**C:** for organisms 29 and 31, paired ablation resource experiments show measured functional effect intervals across multiple environmental seeds, with structurally consistent masks.

**D:** multi-generation ecological trials, then MAP-Elites *as an analysis/search control*, not a secret reward term inside the natural population.

## Primary references

- Mordvintsev et al., Growing Neural Cellular Automata (2020): https://distill.pub/2020/growing-ca/
- Deshpande et al., Engineering Morphogenesis of Cell Clusters with Differentiable Programming (2025): https://arxiv.org/abs/2407.06295
- JAX-Morph: https://github.com/fmottes/jax-morph
- Flow-Lenia: https://arxiv.org/abs/2506.08569
- Karl Sims, Evolving Virtual Creatures (1994): https://www.karlsims.com/papers/siggraph94.pdf
- Mouret & Clune, MAP-Elites: https://arxiv.org/abs/1504.04909
- Hartl et al., Reusable Regulatory Handles (2026): https://arxiv.org/abs/2609.29755
- Cortes-Poza, Hybrid Framework for Morphogenesis and Regeneration (2026): https://doi.org/10.1007/s00285-026-02459-2

Peer-reviewed, preprint, and software evidence are distinct. These sources support *components and hypotheses*, not the unverified claim that this project has achieved organogenesis.
