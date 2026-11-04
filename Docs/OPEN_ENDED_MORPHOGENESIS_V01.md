# Open-Ended Morphogenesis — Architecture v0.1 (November 2026)

## Design correction

Earth: Rogue Origins must not limit organisms to Tetrapod, Fish or
Arthropod, nor to hand-authored MarshCrawler/ClayRunner body plans.
Those eight templates are a temporary art-validation and compatibility
path, not the evolutionary search space.

The primary objective is **open-ended morphology under simulation rules**:
individuals should be able to discover new connectivity, symmetry,
branching, locomotor appendages, sensory arrangements, body proportions
and niches through heritable developmental variation. No finite
computer model can literally generate every form that biological
evolution could produce; our explicit target is an extensible space
that does not enumerate species or require predefined anatomical roles.

## v0.1 — delivered, experimental

- `OpenMorphGenome`: variable-length gene graph, innovation identifiers,
  parent links, attachment coordinates, angle, dimensions, stiffness,
  pigment, symmetry, repetition and bend. Mutation can add/delete branches,
  tune parameters and change repetition/symmetry. Genes describe growth,
  **not** limb/head/tail categories.
- `OpenMorphDevelopment`: deterministic decoding into connected segments
  using parent-local coordinates, reflecting and repeating subtrees.
  Expansion is capped at 256 expressed segments and depth 12 to prevent
  infinite or excessive phenotype development.
- `OpenMorphRasterizer`: exact C# diagnostic pixel raster of segment
  envelopes. This is intentionally rudimentary: visualizing a graph is not
  equivalent to growing muscles, organs or an organism.
- `OpenMorphContactSheet`: Unity menu exporter generating eight separate
  random-walk evolutionary experiments as one 4×2 PNG and CSV.
- `OpenMorphDevelopmentTests`: correctness and integrity tests.

The legacy `PixelCreaturePainter` and its eight preset archetypes are still
present and unmodified. They remain important for comparing art quality,
but must not define the possibilities of evolution.

## v0.2 — continuous developmental fields

Move from only geometric capsules to a discrete/continuous tissue field
with developmental channels and localized gene expression:

- Cell fate: structural tissue, contractile tissue, sensory tissue, transport
  tissue, storage tissue, metabolic tissue and reproductive tissue
- Local signaling: morphogen gradients, diffusion, adhesion, polarity,
  growth rate, programmed death and compartmental boundaries
- Genome encodes regulatory expression functions that read cell position,
  neighborhood chemistry, developmental time and inherited signals
- Topology changes can emerge from local growth and cell differentiation,
  including holes, branching, fusion, segmentation and asymmetry
- Use a bounded grid / sparse voxels initially, with deterministic CPU
  simulator; fit Unity mesh/sprite visualization to generated tissue

## v0.3 — functional organization

Physiological networks must be derived from the generated tissues:

- Nutrient and oxygen transport with real spatial costs
- Mass, buoyancy, structural stress and motion constraints
- Actuator/sensor placement generated together with morphogenesis
- Shared energy budget for maintenance, growth, motion and reproduction
- Evaluation of viable connected tissue networks instead of aesthetic
  similarities to familiar animal classes

## v0.4 — controller co-evolution

Use neural policies that read the sensors the body actually possesses,
output to its actual actuators, and adapt across changing morphology.
No inherited LLM taxonomy knowledge. Assess viability, locomotion,
feeding, survival and ecological interaction over multiple environments.
Do not bake locomotion or known species templates into the fitness score.

## v0.5 — evolving ecosystems and real novelty

Test lineages over many generations with environmental variation,
competition, cooperation and reproduction. Keep mutation/selection logs,
genome/phenotype hashes, novelty metrics and extinction reasons.
Open-endedness is an empirical property to test, not a guarantee.

## Validation gates

1. Graph mutation yields new valid topologies with deterministic replay
2. Distinct topology yields distinct real C# raster output
3. A non-template body plan can arise without mapping it to existing categories
4. Organisms survive only if their generated structure and physiology can
   actually support survival
5. Multiple divergent, viable lineages persist and acquire novel morphology

## How to inspect v0.1 in Unity 6

Menu: **Earth: Rogue Origins > Art > Pixel Creatures >
Export Open Morphology Experiments**

Or invoke Unity batch mode (from the project root):

```powershell
& "C:\Program Files\Unity\Hub\Editor\6000.6.5f1\Editor\Unity.exe" -batchmode -nographics -quit -projectPath (Get-Location).Path -logFile "ValidationResults\open-morph-unity.log" -executeMethod EarthRogueOrigins.Editor.OpenMorphContactSheet.Run
```

Look in `ValidationResults/OpenMorph/run-*/actual_open_morphologies.png`.
The gallery will be **actual developmental C# output**, but it is only a
procedural geometry prototype, not evidence of biological evolution or
independent viable organisms.

## Reference direction

Karl Sims (1994), *Evolving Virtual Creatures*; Lai et al. (2021),
*Virtual Creature Morphology — A Review*; developmental/indirect
encodings and recent soft-robot morphological co-design research.
