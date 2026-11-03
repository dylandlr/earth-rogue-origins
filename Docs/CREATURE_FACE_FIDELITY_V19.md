# Creature Face Study v1.9 — Expressive Pixel Features

## Goal
Move original procedural creatures closer to the recreated Earth: Rogue Origins
reference sprites, emphasizing a recognizable expression at 32–96px display
size. Avoid extracting sprites from concept art.

## Actual committed Unity C# changes

- `PixelFaceGrammar.cs`: deterministic, dimension-aware eye, nostril, mouth,
  chin and cheek landmarks tied to the inherited head profile.
- `PixelCreatureRecipe.cs`: new `FaceDetails` composable layer for
  tetrapods; eye placement follows the head dimensions. Explicit draw order
  ensures jaw and cheek graphics appear in front of the head while the eye
  remains the topmost facial landmark.
- `PixelCreaturePainter.cs`: a subtle cheek, broken mouth/lip line, nostril,
  small black pupil, single eye glint and brow highlight replace the generic
  single circular eye and line-mouth treatment.
- `PixelCreaturePipelineTests.cs`: coverage for facial bounds and exact
  sprite/layer parity across marsh crawler, clay runner, juvenile and multiple
  head proportions.

## Independently rendered v1.9 preview

A standalone Python/Pillow art study generates five **original**
transparent 128x96 PNGs and a v1.8-v1.9 before/after board. The preview is
informed by the same landmarks, but uses a **different drawing backend**.
It is neither a Unity screenshot nor proof that the C# sprite looks identical.

## What the new image reveals

- Eye and mouth read as separate facial features.
- The juvenile shows a relatively larger expressive head.
- Broad-head creatures have a stronger cheek and nose read.
- Some snouts still look angular and the head-to-trunk outline is not yet as
  organic as the target reference.
- Animation remains a simple four-frame movement cycle, not a refined gait.

The quality goal is still **open**. The editor sprite audit script should be
run after a real Unity compilation to inspect the authoritative C# output.

## Recommended next milestone
1. Real Unity C# contact-sheet capture and automated Editor test results.
2. Replace head profile primitives with curated asymmetric masks, including
   explicit silhouette contours for jaw and brow.
3. Improve head-to-neck interpolation and reduce rectangular pixel clusters.
4. Compare species at actual target gameplay scale, including on swamp terrain.
5. Only then extend polished facial and body templates to fish and arthropods.

Do not declare this art milestone concept-matched without an actual review.
