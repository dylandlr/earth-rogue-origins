# Original composable pixel creatures — system design v1

## Target and provenance

The style target is the recreated hand-drawn 2D game assets: small, thick near-black outlines, limited earthy color ramps, a light upper-left highlight, shadowed undersides, dark eyes with single-pixel glints, and visible segmentation/pigmentation. **No pixels are extracted from concept-art images.** Starter shapes are constructed mathematically; all generated layers remain editable.

The system aims to generate many **coherent new organism variants**, not claim biologically unlimited body-plan evolution.

## Modules

```text
Authoring Workbench / JSON recipe / ecological genome
                    |
           PixelCreatureRecipe (v1)
                    |
     PixelCreatureDevelopment.Layout()
       validated body-plan grammar
                    |
             Ordered part descriptors
     Head | Trunk | Tail | Legs | Eyes | Fins | Armor | Pigment
                    |
         PixelCreaturePainter.RenderLayerPixels()
           original 128×96 transparent RGBA
                    |
          Layered PNGs + composed PNG
                    |
             Unity Sprite import
                    |
          Ecosystem / comparison galleries
```

- **Recipe**: versioned seed, body plan, 9 continuous controls, 3 optional module styles and editable palette.
- **Layout**: chooses valid parts, order and anchor positions by plan. Current plans are tetrapod, fish and segmented arthropod.
- **Painter**: generates opaque pixel shapes, near-black contour, highlight bands, camouflage, plates, fins and eyes. Builds each layer independently, then composites in display order.
- **Exporter**: makes JSON + single transparent assembled PNG + per-part layers + machine-readable layer CSV.
- **Workbench**: editor view with live preview and parameter controls.
- **Ecology bridge**: tetrapod genetic traits drive the recipe via `PixelRecipeAdapter`. This is visual projection; the authoritative physical phenotype is still defined in `Development.Express`.

## Generate starter assets

Unity 6 menu:
- **Earth: Rogue Origins > Art > Pixel Creatures > Generate Starter Library**
- **Earth: Rogue Origins > Art > Pixel Creatures > Open Composer**

Starter directory: `Assets/Art/PixelCreatures/`. Each organism has a JSON recipe and a PNG per anatomical layer. The contact sheet displays 12 initial organisms: 5 tetrapods, 4 fish, and 3 segmented arthropods.

In `LiveEcosystemPreview` set `usePixelCreatureSprites=true` to draw genome-derived pixel tetrapods. The existing articulated renderer remains selectable by switching this setting off. Fish and arthropod recipes are currently an **art system only**, not ecologically active simulation agents.

Regeneration never overwrites existing files unless the developer uses the **Rebuild Starter Library (Overwrite)** menu item. The Composer asks before overwriting an individual custom creature.

## Layer authoring contract

- Logical output: 128×96 RGBA at integer pixel positions. No antialiasing.
- Every exported part uses the **same canvas coordinates** so layers reassemble exactly.
- Parts are composited by draw order; each fully opaque pixel replaces the underlying pixel.
- Editable slots are `Tail,HindLeg,ForeLeg,Trunk,Head,Eye,Armor,Fin,Markings`.
- `anchor`, `extent`, `style`, and `drawOrder` are explicit per layer.
- Raster outline is dark (`#1A1D19`); body pigments are muted olive, clay, grey or cream.
- Pixel-art import: `FilterMode.Point`, no mipmaps, no texture compression, 64 pixels per unit.
- Appearance determinism: recipe version + seed + palette + body plan + controls → same output pixels.

## Improving morphology beyond v1

The first renderer still uses fixed symbolic primitives and many closely related tetrapods. To approach the authored reference, we need:
1. 2–4 curated head silhouettes per clade (round, wedge, long snout, wide jaw) and multiple eye placements;
2. A **silhouette grammar** with constrained appendage topology, not merely scaling;
3. Curvature-aware jointed limbs, alternative tail root profiles and more substantial fin/armor shapes;
4. Palette-indexed, authored pixel textures that are warped/masked onto these generated shapes;
5. 4- and 8-direction render passes, frame-based animation and generated pivots;
6. Layer compatibility tests (no floating parts), visible foreground/occlusion and fixed-resolution perceptual regression contact sheets.

Avoid creating combinations of anatomical parts that lack a viable connection to the body plan. Future body plans should define separate locomotion rules, not be introduced by assigning an organism a different rendered icon.

## Performance

Runtime tetrapods generate a 128×96 texture once at first render and destroy it on death/reset. Pixel memory is about 49 KB per organism before sprite overhead, but this still costs CPU and memory for large populations. Next step: key sprites by quantized visual recipe, pool them, and eventually atlas draw calls. Do not quantize ecological phenotype or selection metrics.

## Verification

The following Edit Mode tests are committed in `PixelCreaturePipelineTests.cs`: exact pixel reproducibility, nonempty output for all plans, valid anchors, layer compositing parity, bounded offspring mutation, cross-plan mating protection and invalid recipe handling.

The Python-made preview pack provided separately is an **inspectable companion demonstration**, not the exact output of the Unity rasterizer; the Unity generated PNGs are the authoritative game assets. Neither Unity compile nor the tests have run in this environment.

## Next priorities

1. Build/import the complete Unity starter atlas and inspect its render at actual game zoom.
2. Integrate a shared morphological sprite atlas/pool.
3. Replace basic polygon shapes with high-quality authored pixel brushes, contour masks and anatomical templates.
4. Introduce fish and arthropod locomotion/energy models, then connect them to real ecology and inheritance.
