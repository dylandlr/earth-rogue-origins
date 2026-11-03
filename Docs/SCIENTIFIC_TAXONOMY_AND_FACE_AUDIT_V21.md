# Simulated Scientific Taxonomy & Facial Contour Audit — v2.1

## Scientific naming contract

Scientific names are **plausible fictional Latin-style binomials** associated
with simulation species IDs, not individual animal nicknames or formally
published zoological taxa.

1. A founder is assigned a `speciesId` by the ecosystem.
2. The *first founding organism* for that ID provides the canonical baseline
   developmental genome and anatomical phenotype.
3. `PixelRecipeAdapter` maps this phenotype into a visual recipe including
   inherited armor, swimming/tail adaptations, leg proportions, and hue.
4. `ScientificNameGenerator` chooses a genus and species epithet from
   diagnostic traits: body-plan grammar, armor, tail, snout, limb elongation,
   pigmentation, and pattern density.
5. `EcosystemWorld` stores a single `ScientificTaxon` for that ID. All later
   founders and descendants belonging to the same species retain the name,
   even if individuals mutate or acquire different colors and proportions.
6. If two independently registered species yield the same binomial, the
   second receives a deterministic alphabetic suffix to prevent collisions.
7. Names are stable for the world run. A new species ID (through a future
   validated speciation event) creates a separate taxon; **minor mutation does
   not instantly create a new species**.

Example *possible* names derived from trait patterns, not claims about the
current exact seeded founder output:
- `Scutoderma armatum` — a heavily armored tetrapod.
- `Caudolimnus maculatus` — an elongated-tail, strongly marked tetrapod.
- `Rostrolimnus ferrugineus` — an elongated snout with rust pigmentation.
- `Pterolimnus longicaudus` — a long-tailed, fin-rich fish.
- `Scutarthron elongatum` — an armored, elongated arthropod.

Species records contain `speciesId`, `genus`, `epithet`, and
`diagnosticTraits`. The UI reads the live registry using
`EcosystemWorld.NamedSpecies` and displays population counts.

## Distinguish taxon vs individual vs genus

- **Individual:** immutable organism `id`, ancestry (`motherId`, `fatherId`),
  genotype, health, energy and appearance.
- **Species:** persistent `speciesId`, binomial, founding diagnostic
  description; births inherit the ID.
- **Genus:** morphological grouping represented by the first binomial term;
  this is currently heuristic, not a reconstructed evolutionary tree.

The current simulator does **not** yet model speciation decisions, lineages at
genus rank, reproductive isolation tests, fossil taxonomy, or taxonomic
reclassification. These require separate mechanisms and acceptance criteria
rather than automatic renaming based on changed hue.

## Art integration

- `PixelCreatureAssetBuilder` exports `species.json` beside each hero's
  sprite layers and adds `species_id,scientific_name` to the starter manifest.
  Visual variants of the same starter archetype reuse the same reference taxon.
- `PixelCreatureWorkbench` shows a **proposed** scientific name and writes
  `proposed_taxon.json`; that preview is not a registered world species.
- `LiveEcosystemPreview` lists registered binomials with living counts and
  gives organism GameObjects taxon-aware names for scene inspection.

## Face contour verification (after naming)

The existing v2.0 renderer has 13 authored profile stations per head style.
A fixed face coordinate could push an eye above the wedge forehead. The
updated `PixelFaceGrammar` now samples the head's actual upper/lower contour
at each x coordinate and positions eye, nostril, and both mouth endpoints
inside the silhouette.

Additional NUnit checks require each landmark to be contained within its
head for four head styles and three tested head sizes.

Run the authoritative Unity Editor visual check:

```powershell
.\scripts\verify-face-contours.ps1 -UnityEditor "C:\Program Files\Unity\Hub\Editor\6000.0.23f1\Editor\Unity.exe"
```

Expected outputs, generated only after a successful Unity run:
- `ValidationResults/FaceContour/csharp_face_contours.png` —
  12 actual C# rendered head/sprite morphs (4 styles × 3 sizes).
- `ValidationResults/FaceContour/face_landmarks.csv` —
  contour containment and eye visibility by morph.
- `ValidationResults/FaceContour/face-audit.log` — Editor log.

The check fails on missing outputs, invalid landmarks, or invisible eyes.
The local non-Unity geometry cross-check passed for sampled profile
combinations. **No Unity compilation, NUnit test result, or Unity C# render
was available in the current ChatGPT environment.**

## Next research and development requirements

- A biologically defensible speciation trigger based on heritable divergence
  and reproductive isolation/compatibility with persistence across generations.
- Family trees and phylogenetic groupings from actual pedigrees.
- Taxonomic descriptors from ecology (diet and habitat preference), not solely
  visual anatomy.
- Human-readable discovery logs, genus catalog, and species search/filter.
- Verified image-level contour and artistic comparison to the approved sprites
  at 32, 64, and 96 pixels.
