# v3.3 — Belly contour based on first real Unity creature renders

## Visual evidence

The reviewed v3.2 `actual_csharp_full_creatures.png` shows all eight
archetypes. Its MarshCrawler, PaleJuvenile and ArmoredCrawler have a
visually straight or barrel-like ventral span between their legs.
Our earlier freehand technical geometry replays are not substitutes for
this actual C# output.

## Implementation

`PixelSilhouetteTemplates.TetrapodBellyLower(style,u)` samples the
authored lower torso profile and adds a smooth sin-squared positive-Y
ventral tuck between normalized body x positions .29 and .73. Maximal
lifting at u=.51 is .24 profile units for robust, .19 for rounded, and
.13 for slender trunks. This raises the mid-belly by only 1–3 native
pixels depending on body depth. The contour remains unchanged at both
limb attachment regions.

`PixelCreaturePainter.PaintProfile` now accepts a tetrapod-only belly
flag. The mask and low-side shading use the same contour, preventing
the dark horizontal bar remaining visible after a geometric change.
Fish and other creature plans retain the existing lower profile.
All artwork is original raster geometry; the genotype and simulation
are unaffected.

## Regression checks

- Contour outside the center span remains unchanged for all three torso
  styles; the midsection rises but remains within the dorsal boundary.
- Five curated tetrapods × three body-depth settings remain nonempty,
  connected, and unclipped in renderer-generated pixels.

## Verification

`ERO_v33_Belly_Contour_Technical_Comparison.png` is a separate
Python/Pillow **silhouette-only formula replay**, not a Unity render.
The change has been committed but has **not yet been compiled, tested
or visually approved** in Unity. The user will generate the next real
full-creature output via `scripts/export-actual-creature-style.ps1`,
which retains the last successful renders in unique run directories.

## Next art priorities

Review the actual new MarshCrawler versus the baseline at its 128x96
pixel size and at 4x zoom. Specifically judge whether central ventral
curvature connects naturally to shoulders and hips rather than forming
a suspended arc. Then assess leg weight, head profile and underside
pixel clusters before expanding procedural variation.
