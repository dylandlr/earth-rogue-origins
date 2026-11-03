# Creature Silhouette & Attachment Integrity — v2.4

## Goal
Enforce the approved concept art's low-slung, asymmetrically curved amphibian body
language in the authoritative Unity C# creature renderer.

## Changes committed
- Tetrapod legs now attach based on each animal's inherited torso length and depth,
  instead of using fixed horizontal/vertical anchor positions.
- The tail root and tail module track the actual inherited rump position.
- A dedicated neck bridge sits between head and torso for all extreme head/trunk
  widths; it is drawn behind the head and facial features.
- Rounded, robust and slender body styles now have 13 upper/lower contour
  stations (previously seven). The upper contour rises from the shoulder into
  the back and tapers toward the rump; the belly is deliberately asymmetrical.
- Added Edit Mode tests for attachment continuity, profile integrity and
  facial draw-order safety.
- Added `CreatureMorphologyAudit.Run()` to export a real **C#-rendered**
  3x3 grid of complete marsh crawlers varying trunk length and depth, plus
  CSV anatomy metrics. This code must run inside an installed Unity Editor.

## Evidence / provenance
A local image named `EarthRogueOrigins_v24_TorsoShape_Audit.png` is a
**standalone replay of the committed 13-station numeric body profiles**. It
renders only the mathematical torso mask, not an entire animal. It is neither
a screenshot from Unity nor an AI-generated full-creature image. It confirms
the intended contour shape but does **not** verify the actual game art quality.

To obtain the authoritative full creature render, open Unity and choose:
**Earth: Rogue Origins > Art > Pixel Creatures > Export Full Creature Morphology Audit**

This writes:
- `ValidationResults/CreatureMorphology/actual_csharp_morphology.png`
- `ValidationResults/CreatureMorphology/morphology_metrics.csv`

Also run the previously added face and compositing tests.

## Known limitations
- Unity compilation, test execution and pixel output still need verification.
- Simple shadows and limb rendering remain too geometric relative to the
  approved reference image.
- The body contours, despite being closer to the intended silhouette language,
  are not an accepted pixel-perfect copy of the approved creature style.
- The existing walking frames use discrete distal shifts and still need
  actual support-foot contact and joint motion validation.

## Next
Inspect a genuine Unity full-creature contact sheet and prioritize the largest
visual failures before making more speculative art changes. Afterwards, build
the same morph audit for armored crawlers and juveniles.
