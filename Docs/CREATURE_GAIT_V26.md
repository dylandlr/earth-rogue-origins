# Ground-Contact Gait v2.6 — Four Frame Phase Contract

## Objective

Reduce foot sliding and mechanically straight limb movement while respecting
the approved Earth: Rogue Origins creature style. This milestone addresses the
procedural walker directly; no visual resemblance is claimed without rendering.

## Committed runtime code

- `PixelPartDescriptor.footLift` carries a visual-only integer foot displacement.
  It defaults to zero, so old recipes retain their neutral pose.
- `PixelCreaturePoses.Layout` defines alternating foreground foreleg/hindleg
  swing phases and counterphased far-side limbs. Frame 0 and frame 2 are planted
  on the common stance plane. In frame 1 the foreground forefoot lifts; in frame
  3 the foreground hindfoot lifts. At every frame at least one foreground
  support foot is planted.
- `PixelLimbSilhouette.Build(...,footLift)` moves the ankle vertically while
  preserving the attachment root and allowing a small knee displacement.
- `PixelCreaturePainter` consumes `footLift` and renders the resulting
  anatomy. Swimming continues to use an independent offset.

## Tests and visual review

Two added Edit Mode regression tests assert:
1. At least one planted foreground foot in every walk frame, with consistent
   ankle contact height across stance phases.
2. A lifted foot moves vertically without changing the shoulder, hip or
   distal x coordinate.

Authoritative output is now available through the Unity menu command:

**Earth: Rogue Origins > Art > Pixel Creatures > Export Walking Gait Audit**

This writes:
- `ValidationResults/CreatureGait/actual_csharp_walk_strip.png` —
  four C# rendered 128x96 sprites in a single transparent-compatible strip
- `ValidationResults/CreatureGait/joint_positions.csv` —
  positions, lift and draw order for each limb on each frame

The standalone file `ERO_v26_Gait_Contact_Audit.png` is a technical geometry
replay, created using Pillow from committed leg equations. It is **not**
a Unity screenshot, not a complete character render, and not an AI-generated
approximation of the final game.

## Outstanding limitations

- No foot locking in world coordinates; stride cycles are still sprite poses
  driven by traveled distance, not a physics-based ground-contact solver.
- Body mass, vertical bob, toe bend, and actual ground-contact elevation
  are not integrated.
- The far-side and foreground toes need artistic cleanup after an actual Unity
  sprite sheet is captured.
- Unity 6 is unavailable in the current execution environment. C# compilation,
  Editor test execution and actual sprite review are not yet verified.

## Next acceptance

Run the editor export and check the actual C# walk strip against approved
low-slung amphibian sprites at native resolution and 32/64px viewing scales.
Verify no detached joint pixels, feet floating above ground or overlapping
body shading. Do not progress to a more sophisticated animation model until
those errors have concrete screenshot evidence.
