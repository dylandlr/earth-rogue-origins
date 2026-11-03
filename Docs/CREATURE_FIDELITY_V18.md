# Creature Fidelity v1.8 — silhouette-first refinement

## Target

The recreated reference sheet shows squat amphibian-like animals: short limbs,
broad continuous trunk, enlarged expressive head, small shiny eyes, rounded
tapering hindquarters, clear mouth line, muted limited-color ramps and
hand-grouped markings. It does **not** show long-legged walking salamanders.

## Source changes committed

- `PixelDetailGrammar.cs`: controlled groups of dorsal markings and anatomically placed toe endpoints, both deterministic by recipe seed.
- `PixelCreaturePainter.cs`: uses those anatomical details and replaces the
  old pointed tetrapod tail wedge with a larger rounded, tapering silhouette.
- `PixelCreaturePipelineTests.cs`: added raster bounds and determinism tests
  for markings, feet, and tail poses.

## Genuine preview experiment

The independent Python/Pillow prototype `ero_pixel_fidelity_v18.py` draws
five original 128×96 RGBA creatures from explicit profiles and pixel primitives.
This is a **standalone procedural renderer**, not Unity, and no artwork is
sampled or extracted from the concept art.

The generated `EarthRogueOrigins_Procedural_Visual_v18.png` compares v1.5
sprites on the left with v1.8 sprites on the right for five clades.

Generated files in the portable study ZIP:
- five transparent, standalone v1.8 creature PNGs;
- a four-frame marsh crawler walking strip;
- source Python script;
- before/after visual board;
- `ERO_v18_metrics.json` from pixel-level comparisons.

The v1.8 experiments have measurable changes in 2,450–2,771 logical pixels
compared with v1.5 per character, but **pixel differences are not a measure of
aesthetic fidelity**.

## Acceptance checklist

- [x] New art study with genuine regenerated procedural pixels
- [x] Shorter legs / lower silhouette in standalone prototype
- [x] Original dedicated markup for face, dorsal markings, toe outlines
- [x] More rounded hindquarters in Python study and Unity C# geometry
- [ ] Reproduce study output in actual Unity C# engine and inspect
- [ ] Match reference portrait silhouettes at 32/64/96 px in blind review
- [ ] Verify natural stance across all motion frames
- [ ] Compare true gameplay screenshots with reference habitat
- [ ] Complete high-quality fish/arthropod variants

**Status:** Not yet visually approved. Unity compilation and Edit Mode tests
have not been run in this environment.
