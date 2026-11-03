# Earth: Rogue Origins — Pixel Art Bible v1.1

## Visual target
Stylized prehistoric creatures with confident dark contour, deliberately chunky head/snout anatomy, short planted legs, flowing tail root, olive/clay/slate/cream skin and understated stippled highlights. These references are a **style target**. All generated pixels come from independent authored procedural geometry; don't scrape or cut sprites from concept images.

## Locked palette ramps (darkest → lightest)
| Name | Ink | Shadow | Midtone | Light | Highlight | Marking |
|---|---|---|---|---|---|---|
| Swamp Olive | #1A1D19 | #4D5B3A | #7E8A59 | #A7B277 | #D8D7A9 | #586745 |
| Rust Clay | #211C1A | #68453D | #AD715D | #D19270 | #E8B394 | #794B43 |
| Slate | #222326 | #55575A | #85878B | #B5B3B0 | #D6D3C9 | #65676B |
| Pale Sand | #282620 | #83776A | #C6B8A2 | #DFD1B8 | #EFE5D1 | #988778 |
| Moss | #222A22 | #53684B | #859868 | #A5AD7C | #D2D0A0 | #596B4C |
| Marsh Teal | #1C2929 | #345A60 | #628F8E | #91B2A8 | #C7D4BA | #39666B |

Avoid random per-pixel RGB noise. Preserve the order ink → shadow → midtone → light → highlight. Prefer grouped, intentional colored shapes over dithering.

## Hero creature families
| Family | Palette | Silhouette | Marking |
|---|---|---|---|
| Marsh crawler | Swamp Olive | Broad rounded head, squat body, sturdy small legs | Mottled back |
| Clay runner | Rust Clay | Longer snout, narrow body, tall light limbs | Side freckles |
| Slate newt | Slate | Wedge head, elongated swimming tail, low trunk | Dorsal stripe |
| Pale juvenile | Pale Sand | Oversized head, shortened trunk, tiny legs | Sparse |
| Armored crawler | Moss | Deep body, plate development, strong legs | Tail bands |
| Paddle fish | Marsh Teal | Wedge head, long fin-bearing tail | Dorsal stripe |
| Marsh fish | Slate | Narrow long snout, paddle tail | Mottled |
| Segmented burrower | Rust Clay | Repeated trunk plates, crouched legs | Bands |

These are starting **visual morphotypes**, not automatically distinct biological species or a validated phylogeny.

## Layer contract

Use identical 128×96 logical canvases for compositing. Every layer is generated from an anatomical slot with fixed draw-order and an attachment point. All opaque pixels blend by replacement. Eyes and markings must remain inside plausible head/trunk skin. Armor plates must overlap a dorsal silhouette rather than float. Limb bases must overlap trunk material by at least two logical pixels at supported extremes.

### Quality acceptance gates
- Full shape reads at 32–48 rendered pixels wide.
- Head/nose/mouth visibly differentiate at least four template shapes.
- No disconnected pixels or floating plates after recombination.
- Palette always uses the approved six art ramps (unless custom art is intentionally authored).
- Markings are clipped to living body pixels; no dots in transparent areas or through the black contour.
- Reproduce exactly from recipe version, seed, body plan, palette, and constraints.
- Evaluate a 4×8 matrix at common seed, varying one trait at a time.
- Review actual Unity camera sprites alongside the original intended concept, without claiming equivalence until confirmed.

## Technical files

- `Assets/Scripts/Anatomy/PixelArtDirection.cs`: art palettes, archetypes, family constraints.
- `Assets/Scripts/Anatomy/PixelCreatureRecipe.cs`: serialized archetype, palette, pattern plus morphology.
- `Assets/Scripts/Anatomy/PixelCreaturePainter.cs`: final pixel painter, clipped markings and part compositor.
- `Assets/Editor/PixelCreatureWorkbench.cs`: artist-facing control.
- `Assets/Editor/PixelCreatureAssetBuilder.cs`: starter export.
- `Assets/Tests/Editor/PixelCreaturePipelineTests.cs`: regression tests.

## Known deficits
This is not yet the approved final pixel art. The current shapes remain primitive and template fidelity still needs hand-designed pixel control points, body-mask-aware deformation, stronger coherent mouth and belly contour, skin material clusters, true articulated poses, and a visual test run in Unity. Existing Unity compile and tests have not been verified in this environment.

Next: replace head ellipses with pixel-authored silhouette masks, add seam-covering shoulder and hip transitions, then bake 4-frame movement while retaining editable layers.
