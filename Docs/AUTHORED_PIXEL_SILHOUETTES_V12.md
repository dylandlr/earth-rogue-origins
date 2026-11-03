# Authored Silhouette Pass — v1.2

The pixel-creature system now has **original authored contour profiles** for the head and trunk, which can be smoothly resized by inherited traits while retaining deliberate silhouette landmarks. This replaces the first-generation primitive elliptical body/head renderer.

## Drawing contract

- Authored top/bottom contour samples (7 each) define the torso and head silhouette rather than arbitrary sprite extraction.
- The renderer samples those profiles at each integer x-column; no resampling of screenshots or imported art occurs.
- Shape profiles: round head, broad/robust head, wedge head, long snout; standard, stocky and slender trunks.
- Rendered pixel material: 2px palette-specific contour, shadow underside, midtone body and deliberately placed top-left highlights.
- Every profile remains in the 128×96 transparent canvas.
- Independent tail, limb, dorsal plate, eye and markings layers still use the existing composable contract.
- Genome-driven dimensions remain continuous. Profile style selection is discrete and may be inherited.

## Gold-standard initial creature

**MarshCrawler / SwampOlive** is the benchmark. Target:
1. Wide, heavy head without a detached circle appearance
2. Broad torso transitioning into a tapering tail
3. Short but distinctly planted limbs
4. Small dark eye with one glint and readable mouth edge
5. Deep olive underside, lighter dorsal shoulder, restrained mottled markings
6. Distinct contour at 32, 64 and 96 screen pixels

Other archetypes are compared against this benchmark, not simply differentiated by color.

## Unity verification

Open **Earth: Rogue Origins > Art > Pixel Creatures > Open Composer**; choose the `MarshCrawler` hero template. Change `Head module` between `Rounded`, `Robust`, `Wedge` and `LongSnout`, then vary `Trunk length` and `Head size`. Rendered silhouette should meaningfully change without modifying the procedural generation method.

Regenerate the 12 starter PNG sets with **Rebuild Starter Library (Overwrite)** only when deliberately replacing previously generated assets. The default Generate command preserves any custom painted layers.

Tests added: distinct head contours, valid extremes, archetype head-to-body envelope overlap, and custom pigment changes. As before, these are committed but **not executed in Unity in the current runtime**.

## Known remaining gaps

- Silhouette shapes are more deliberately specified but still need artist review and pixel-level clean-up for actual concept-art fidelity.
- Tail root, shoulders and jaw should next use mask-based transition sprites to avoid strong joint lines.
- Pixel poses remain static; skeletal motion is a separate renderer and the pixel mode still lacks directional 4-frame animation.
- There is no captured Unity contact sheet verifying the real output, no visual difference threshold against the approved references, and no verified Unity build.
