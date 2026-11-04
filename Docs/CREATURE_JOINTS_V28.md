# Creature Joint Integration v2.8 — Directional Skin Folds

## Scope

The approved Earth: Rogue Origins reference uses broad amphibian torsos
with limbs emerging naturally through folds of pigmented skin. Previous
procedural versions used conspicuous elliptical patches over the shoulder
and hip joints that appeared to be separate pieces.

This pass changes the actual `PixelCreaturePainter` renderer, not only the
visual mockup.

## Implementation

- New `PixelJointTransition.cs` builds original, asymmetric 5-vertex outer
  skin-fold contours and slightly inset color patches.
- The shoulder fold slopes toward the head; the hip fold slopes toward the
  tail. Both follow the **inherited** limb root already computed by the
  morphological layout.
- PixelCreaturePainter replaces the oval shoulder and hip covers with
  shadow, base-skin and highlight polygon layers. Neck, head, eye,
  limb and tail layering remains composable.
- Edit Mode tests cover 3 inherited torso lengths × 3 depths × 4 walking
  frames, root alignment, valid canvas bounds and distinct hip/shoulder
  geometry.

## Evidence

A local technical preview `ERO_v28_Joint_Fold_Geometry.png` was made using
a standalone Pillow replay of the exact C# polygon formulas. It includes
simplified limb context lines to make attachment points readable; it is **not**
a sprite from Unity, a real gameplay screenshot, a complete creature render,
or an AI-generated concept-art creature. The purpose is solely to visualize
the actual new fold contours across inherited dimensions.

## Visual acceptance remains open

This replacement should avoid the isolated circle/coin visual language.
However, only a genuine Unity render of complete creatures can establish
whether the art matches the reference. The current silhouette still needs
hand-curated pixel cleanup for skin clusters, underside gradients, and
variation between fore- and hind-leg junctions.

To export actual Unity C# pixel output:

1. Open Unity 6 with the repository project.
2. Run **Earth: Rogue Origins > Art > Pixel Creatures > Export Full Creature
   Morphology Audit** and **Export Walking Gait Audit**.
3. Run Edit Mode tests and inspect `ValidationResults/CreatureMorphology/`
   and `ValidationResults/CreatureGait/` against the approved sprite sheet.
4. Reject any result with floating skin folds, pixel seams or face coverage.

**Validation:** GitHub files have been committed and fetched for structural
checks. Unity compilation and rendering remain unverified in this session.
