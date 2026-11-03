# Illustrated Creature Skin System — v0.5

## Goal
Bridge the emergent developmental genome and the approved Carboniferous hand-painted 2.5D artwork using shared anatomical layers rather than full-frame, per-organism PNG generation.

## Implementation

- `CreatureArtLibrary`: Unity ScriptableObject referencing transparent PNG/sprite components (torso, highlight, head, eye, fore/hind limbs, foot, tail, fin, armor, shadow).
- `IllustratedCreatureSkin`: projects those parts onto the existing articulated anatomical joints, scales them to inherited proportions, and applies consistent layering.
- `ArticulatedCreatureView`: exposes joint and armor attachment transforms and hides procedural meshes when illustration layers are complete.
- `IllustratedPartsGenerator`: Editor command generating a coherent, editable starter library of source PNGs plus `CarboniferousTetrapod.asset`; replaces generic shapes as the default rendering surface in Ecosystem Lab.
- `EcosystemSceneBuilder`: generates and assigns the part library during scene creation.
- `IllustratedCreatureSkinTests`: tests skeletal attachments, missing-asset fallback and repeated-skin replacement.

## Unity workflow

1. Open Unity 6 project and allow scripts to import.
2. From the menu choose **Earth: Rogue Origins > World > Create Ecosystem Lab Scene**.
3. Open `Assets/Scenes/EcosystemLab.unity` and press Play.
4. Inspect organism GameObjects in Hierarchy: `ArticulatedCreatureView`, `IllustratedCreatureSkin`, and child anatomical pivots.
5. Open `Assets/Art/IllustratedParts/CarboniferousTetrapod.asset` to replace placeholder PNGs with hand-painted artwork. Use consistent centered sprite pivots.
6. Run Edit Mode tests from **Window > General > Test Runner**.

Generated placeholder PNGs deliberately use shared hues, opaque edges, soft highlights and subtle stippling. **They are not a finished 1:1 rendering of the approved concept art.**

## Painting and import specification

Each sprite must have a transparent background, centered sprite pivot, contour width equivalent to 2–3px at 96px display size, warm upper-left lighting, muted olive/terracotta/turquoise palette, and no baked global shadow except in the dedicated `groundShadow` layer.

Part sizing is derived exclusively from the inherited anatomical phenotype. A head component expands with head radius, armor follows the back curve, tail fin expands with fin trait, and limbs follow the animated joints.

Do not encode body mass, defensive value or movement ability inside artistic sprites. Those remain authoritative genome-derived physical properties. Illustration layers must be cosmetic only.

## Performance and QA

- A shared `CreatureArtLibrary` avoids creating unique textures for every individual.
- Each organism still creates multiple SpriteRenderers; later batch with SpriteAtlas and renderer LOD.
- Run both an isolated portrait test and a dense colony test at 64px, 96px and 128px visual scales.
- Verify no dislocated joints while moving, no duplicated components on art replacement, stable depth sorting, and acceptable frame timings.

## Known limitations

- The generator creates stylized prototype paintings using a CPU rasterizer; these are not human-authored concept-quality art.
- There is no directional turn animation or IK foot planting.
- A single base library means multiple taxa need distinct archetype-specific libraries before the game contains a credible Carboniferous biodiversity.
- This work is committed but compilation, tests and scene rendering have not been run in Unity in the current environment.
