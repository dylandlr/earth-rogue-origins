# Articulated Creature Renderer — Prototype v0.4

## Purpose
Replace the per-organism eight-full-frame sprite approach with separately articulated anatomical parts controlled by the same developmental phenotype used by the ecosystem.

This **does not yet equal the approved hand-painted concept-art fidelity**. It is an intermediate rig and rendering interface that can receive painted sprite/mesh layers later without changing anatomy or simulation.

## Files
- `Assets/Scripts/Anatomy/ArticulatedCreatureView.cs`: builds the visual hierarchy; limbs, head, tail, dorsal armor have independent pivots.
- `Assets/Scripts/Ecology/LiveEcosystemPreview.cs`: defaults to `useArticulatedRig=true`, creates one rig per organism and updates it from authoritative positions and terrain.
- `Assets/Tests/Editor/ArticulatedCreatureViewTests.cs`: anchoring, swim tail, armor geometry and allocation-regression tests.
- `PhenotypeSpriteCache.cs` remains available for comparisons with the earlier frame-baked fallback.

## Behavior
- Organism position comes from `EcosystemWorld`; the renderer does **not** move organisms.
- Gait phase advances only when simulated movement occurs.
- Diagonal limbs alternate in phase.
- Swimming produces a different stroke and greater tail swing.
- Head movement is subtle and synchronized.
- Orientation follows horizontal movement.
- Armor pieces attach along the phenotype's computed dorsal surface.
- Morphology remains inherited, not selected by the art renderer.

## Run
1. Open Unity 6.
2. Choose **Earth: Rogue Origins > World > Create Ecosystem Lab Scene**.
3. Select `Live Ecosystem World` and ensure `Use Articulated Rig` is enabled.
4. Press Play; use WASD, wheel, Space and 1/2/3.
5. To compare old and new approaches, disable `Use Articulated Rig` and enable `Use Anatomical Sprites`.
6. Run Edit Mode tests including `ArticulatedCreatureViewTests`.

## Known limitations
- Meshes are generated for multiple small body parts and use draw calls per subpart; **not yet production-optimized** for large populations.
- The shape renderer uses flat vertex-color materials rather than the approved painted cartoon textures. It needs painterly body material atlases, outline treatments and shading masks.
- Gait is an approximate pivot animation, not IK foot planting. It does not model contact with the ground, terrain slope, or physical joint limits.
- Adult phenotype is calculated once at birth; age-driven growth, damage, scars, sexual dimorphism and life-stage transitions are not represented visually.
- Current 2D side-on rigs are a transitional rendering approach; final 2.5D art needs directional orientation and depth sorting.
- Existing scene, gameplay and editor tests have not been compiled or playtested in Unity in this environment.

## Next acceptance gates
1. Run Unity compilation and editor tests.
2. Record a 60-second playtest with 60 organisms; verify no missing meshes/parts and correct gait mode transitions.
3. Capture identical genotype at multiple animation phases and zoom levels, with overlay of attachment anchors.
4. Profile draw calls, CPU, texture memory, allocations and frame times.
5. Replace mesh-colored surfaces with a **shared hand-painted parts atlas** (body textures, limb segments, faces, fins, plates). Preserve the rig's pivot contract.
6. Introduce an organism rendering LOD: full articulated nearby; batched sprites/instancing in distant regions.
