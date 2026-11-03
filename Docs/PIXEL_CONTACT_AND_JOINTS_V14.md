# Concept-aligned Pixel Anatomy v1.4 — Contact and Joint Fidelity

## Committed changes

- Added a distinct `TailRootBlend` module to tetrapod anatomy, composited between trunk paint and surface markings to conceal tail seams.
- Replaced simple foot caps with broad, planted toe clusters; robust, slender and webbed feet generate different shapes.
- Added separate transparent `RenderGroundShadowPixels` output. Ground shadow size responds to inherited trunk length and depth, does not appear for fish, and renders beneath walking organisms in `LiveEcosystemPreview`. The shadow is hidden in water.
- Both the starter asset exporter and individual creature Composer now write `contact_shadow.png` alongside the assembled PNG and independent anatomical layers.
- Corrected walking and swimming poses: preserve leg/shoulder and tail-root anchors; change distal feet and tail-tip positions rather than translating whole parts. This limits attachment tearing between frames.
- Added Unity Edit Mode regression tests for seam layer parity, responsive and deterministic shadows, and separate transparent outputs.

## Review targets and standards

1. **Single connected silhouette:** all torso, limb, neck and tail parts should visually connect at normal gameplay scale. Small separate toe accents are acceptable if intentionally drawn, but isolated limb pieces are not.
2. **Foot grounding:** every walking frame should show at least one planted foot near the organism's contact plane. Check extreme limb lengths, webbing and stride phases.
3. **Natural tail base:** no black line should bisect the skin where the tail meets the torso.
4. **Consistent paint:** body, tail and seam caps share palette ramps and contour treatment. Avoid oversized solid patches that hide anatomical landmarks.
5. **Proportional shadows:** shadows should be under the animal, partially transparent, not a baked component of any body sprite, and should disappear in swimming mode.
6. **Art direction:** compare at 32, 64 and 96 screen pixels against the recreated Earth: Rogue Origins concept-art characters. Use natural gameplay backgrounds, not just blank editor canvases.

## Known limitations

The rendering still uses simple geometry for numerous appendages. There are no painted sprite atlases, directional animation sets, contact-aware IK or camera-verified screenshots. Eight 128×96 frame textures plus one shadow per organism are baked at runtime; asset cache/atlasing is needed to scale efficiently. Fish/arthropod body plans still only have art-generation support.

### Verification status

All changes and NUnit tests are **committed**, and a source-level presence/brace check was performed through the repository connector. **Unity compilation, actual test execution, game render capture and visual parity remain unverified** in this environment.

## Next practical milestone

Run Unity’s `scripts/validate-unity.ps1`, capture a real morphology review image, then iterate directly against visibly incorrect foot/neck/tail shapes. After the reference match is close, add painted pixel component atlases and a quantized shared animation cache.
