# Composable Pixel Creatures — Seam and Motion Pass v1.3

## Goal

Make the creature resemble one authored organism rather than a stack of visibly
detached sprites. This is a new visual-implementation milestone, **not** a
verified match to the approved concept art.

## Changes

- Tetrapod leg attachment positions moved from high on the torso to its lower
  shoulder/hip line, producing a more grounded silhouette.
- Three composable attachment layers were introduced:
  `NeckBlend`, `ShoulderBlend`, `HipBlend`.
  They paint a controlled mid/shadow overlay in draw order, covering the exposed
  joint border without changing underlying anatomy or physiology.
- Original authored seven-sample head and torso contours remain the basis of
  the silhouette.
- The new `PixelCreaturePoses` transforms only the ordered part descriptors
  through four integer-pixel walking and swimming frames.
- `PixelCreaturePainter.Render(recipe, frame, swimming)` and
  `RenderLayerPixels(recipe, partIndex, frame, swimming)` expose deterministic
  posed image and layer output.
- `PixelCreatureAssetBuilder` exports 8 frame PNGs per starter creature under
  `Animation/walk_00..03.png` and `Animation/swim_00..03.png`, preserving existing
  files unless explicitly requested to overwrite.
- `LiveEcosystemPreview` now selects frame by *actual simulated distance*
  moved and terrain (water versus land). It reuses each organism's baked frames
  until death/reset, when the textures and sprites are destroyed.

## Test coverage added

1. Deterministic four-frame raster output and visible pose differences.
2. Exact per-frame layer compositing parity.
3. Dominant connected pixel component in each tetrapod walk frame, to flag
   unexpectedly floating limbs, plates or heads.

Existing tests continue to cover palettes, head silhouettes, genetic variation,
and deterministic asset generation.

These tests have been **committed but not executed in the Unity Editor**.

## Limitations / quality gates

- The blend caps are procedural pigment/shadow patches, not yet hand-authored
  shoulder folds, neck skin or tail-base anatomy.
- Pixel limb poses are small stepped shifts, not a full bone/IK gait; swimming
  currently changes the tail and limb offsets, not all water-contact dynamics.
- Each unique organism can allocate 8 raw 128×96 RGBA textures (~384 KiB before
  Unity object overhead). Pooling/atlasing and visual-key-based reuse are the
  next scale priorities; otherwise this will become expensive as populations grow.
- New pixels must be compared to the **recreated concept reference** at 32, 64,
  96 and 128 screen pixels. Review connectedness, snout and torso proportions,
  shaded joint transitions, color grouping and feet contacting terrain.
- Fish and arthropod artwork still lacks authoritative gameplay physiology.

## How to inspect

1. Open the Unity 6 project.
2. Choose `Earth: Rogue Origins > Art > Pixel Creatures > Open Composer`.
3. Choose `MarshCrawler` and export layers. Inspect the three blend layers and
   assembled sprite for visible joints.
4. Choose `Pixel Creatures > Rebuild Starter Library (Overwrite)` if you
   intentionally want to regenerate the starter PNGs and eight-frame animations.
5. Run Edit Mode tests; then start Ecosystem Lab and watch multiple organisms
   walk, stop and enter water.
6. Capture real Unity screenshots before accepting visual fidelity.

## Next design priorities

- Distinct painted masks for neck folds, joint occlusion and tail root instead
  of oval patches.
- Art-directed foot shapes and shadow at terrain contact.
- True skeletal/interpolated motion for close-up organisms; static/batched
  sprite LOD for distant organisms.
- Morphology sprite cache shared across visually equivalent genotypes.
- Real game screenshots side by side with approved concept art, then targeted
  redrawing based on visible issues.
