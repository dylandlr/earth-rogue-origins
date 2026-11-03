# Procedural locomotion and phenotype cache — v0.3

## Changes

- Four discrete walking poses and four swimming poses per visual phenotype.
- Walking moves limbs in alternating phases; swimming adds tail undulation and reduced limb excursions.
- Pose phase is chosen from **actual simulated travel distance**, not an independent movement clock. Idle creatures hold the neutral frame.
- The world chooses a swimming pose by actual terrain type.
- Cache keys come from quantized anatomical phenotype values. Quantization affects **visual fidelity only**: exact genotype and derived physical phenotype continue to determine fitness, movement and energy.
- Reference-counted sprite entries are reclaimed when organisms die or when the lab scene resets.
- Cache eviction operates only on entries no longer referenced. If all entries are in use, cache may temporarily exceed capacity rather than removing a living creature's texture.

## Verification

Run `Window > General > Test Runner > EditMode` and select `AnimatedAnatomyTests`.
Run `Earth: Rogue Origins > World > Create Ecosystem Lab Scene` to regenerate the scene.

Review at 64px, 96px and 128px organism screen sizes. Check:
1. Movement causes a visible but restrained gait, including alternate feet.
2. Swimming changes the tail and limbs without independent game-state motion.
3. Spine plate roots remain anchored across gait phases.
4. Repeat genotype gives repeat silhouette and spots.
5. Memory allocation stabilizes after repeated organism death/birth; cache does not remove referenced sprites.
6. Resetting the scene destroys all generated frames.

## Performance notes and limitations

- The current cache bakes **eight 384x224 RGBA32 sprites** per unique quantized morphology. Approximate uncompressed pixel memory is `384*224*4*8 = 2.75 MB` per entry, exclusive of overhead. Sixty distinct morphs may use around 165 MB for raw pixels.
- Cache quantization is high-dimensional, so many unrelated organisms will not share an entry. Next optimization is to separate **shared deformable anatomical parts** from per-creature pigments, switch to fewer atlased frames, compress stored art and use pooled sprite renderers.
- The gait uses rasterized posing, not full IK/foot planting. It is a working animation prototype, not polished concept-art animation.
- Sprite textures are generated on the Unity main thread. Batch creation, pre-baking and throttling are required for large populations.
- This module has not been compiled or tested in a live Unity Editor in the current environment.

## Next visual milestone

Adopt a skeleton of deformable sprite parts (head / trunk / forelegs / hindlegs / tail) with painted material masks. Generate variation through mesh deformation and palette overlays; use procedural graphics only as an anatomical fallback. Add silhouette snapshots and screenshot regressions against the approved reference.
