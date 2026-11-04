# Grounded Whole-Body Stance — v2.7

## Implemented in the authoritative Unity C# renderer

1. The midpoint of the four-frame tetrapod walking sequence now compresses the body downward by **one 128×96 logical pixel**.
2. Each leg shortens by the same pixel at that phase, preserving the ankle's world-relative y-coordinate instead of pushing the foot through the floor.
3. Existing head, eye, armor, marking, torso, tail and attachment layers move coherently with the body's pose.
4. The independent contact shadow is now vertically aligned from the near-side foot geometry rather than a hard-coded canvas row.
5. Two new Unity Edit Mode checks require contact-height conservation across body length/depth extremes and shadow center tracking across inherited leg heights.

These are changes to sprite output; Unity compilation and the in-game effect **remain unverified** because Unity Editor is not installed here.

## Visual provenance

`ERO_v27_Stance_Geometry_Review.png` is a standalone **Pillow geometry replay** using the committed v2.4 numerical torso stations and v2.7 compression/foot support formulas. Its body has no head or tail, and its raster backend is not the Unity C# painter. This visual is intended to inspect contact-plane alignment only. It is **not** a Unity gameplay screenshot or a fully generated creature.

## Observations against the approved concept sheet

The grounding model is more consistent, but the replay shows legs with straight/sharp geometric transitions and a flat belly. These differ from the concept creatures' low-slung, broad organic bodies. The next art pass should prioritize internal belly-to-leg occlusion and contact-aware leg silhouettes rather than unrelated procedural ornamentation.

## Reproducible Unity review

Within Unity, run the existing menu commands:
- **Earth: Rogue Origins > Art > Pixel Creatures > Export Walking Gait Audit**
- **Earth: Rogue Origins > Art > Pixel Creatures > Export Full Creature Morphology Audit**

Capture `ValidationResults/CreatureGait/actual_csharp_walk_strip.png` and `ValidationResults/CreatureMorphology/actual_csharp_morphology.png`, run Edit Mode tests and inspect frame 0 against frame 2 at 4x zoom. Require the lowest foreground planted toe height to remain unchanged.

The subsequent quality gate is a real C# whole-creature render with an approved concept-side comparison at gameplay resolution.
