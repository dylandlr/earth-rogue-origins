# Earth: Rogue Origins

**A 2.5D prehistoric evolution, ecology, roguelike, and colony simulator built in Unity.**

The approved visual target is a hand-illustrated, top-down cartoon swamp with dark RimWorld-inspired management panels. Mechanically, this project combines individual organism simulation, heritable genetics, ecological competition, emergent selection, and long-term geologic change.

## Status

**Early Unity simulation prototype. Not a finished game.** A Unity v0.8 project archive was generated in the development conversation, but its complete Unity source and binary art assets have **not yet been committed to this repository**. This repository is now writable and has the initial Unity configuration, build helper, and planning documents. Do not treat this initial commit as a complete runnable game.

## New: Procedural Ecosystem Lab

This repository now contains a **self-contained prototype world model** (separate from the older v0.8 illustrated scene). Its organism sprites and terrain PNGs are generated inside Unity.

1. Open this repository using **Unity 6**.
2. From the top menu, choose **Earth: Rogue Origins → World → Create Ecosystem Lab Scene**.
3. The tool generates **24 organism variants and 40 terrain tiles** and saves `Assets/Scenes/EcosystemLab.unity`.
4. Press **Play**. The scene starts with a spatial ecosystem of grazers, omnivores, detritivores and predators.
5. Space pauses; 1/2/3 adjusts speed; WASD pans; mouse wheel zooms; P/O changes temperature, C changes CO2, and R resets the seed.

Use **Earth: Rogue Origins → Experiments → Run Ecosystem Seed Sweep** to export a 12-seed paired run of predator/no-predator ecosystems to `ExperimentResults/ecosystem_seed_sweep.csv`. Unity Edit Mode tests are in `Assets/Tests/Editor`.

The graphics are **generated prototype assets**, not a 1:1 reproduction of the concept-art quality. The scene has not yet been compiled or playtested in Unity here.

For architecture, limitations and research methods, see [Docs/WORLD_AND_EMERGENCE.md](Docs/WORLD_AND_EMERGENCE.md).

## Verify the Unity project on Windows

Run `./scripts/validate-unity.ps1 -UnityEditor "<path-to-Unity.exe>"` to generate the ecosystem scene, run Edit Mode tests, and attempt a Windows build. See [Docs/UNITY_VALIDATION.md](Docs/UNITY_VALIDATION.md). Actual compilation and playtest are **not yet verified** by the project maintainers through this pipeline.

When regenerating the scene, hand-painted PNG replacements and manually assigned sprites in the CreatureArtLibrary are preserved by default. The explicit `Art > Regenerate Starter Parts (Overwrite PNGs)` command intentionally overwrites these generated parts.

## Target engine

Unity **6000.0.23f1**, Windows 64-bit.

## Setup

1. Install Unity 6 through Unity Hub, matching `ProjectSettings/ProjectVersion.txt`.
2. Clone or download this repository.
3. Add the folder as a Unity project.
4. Import the prototype's `Assets/Scenes`, `Assets/Scripts`, and `Assets/Resources/Art` from the existing Unity v0.8 archive, keeping their paths intact.
5. Open `Assets/Scenes/EmberDelta.unity` and run the scene.
6. After resolving compile/runtime issues, use **Earth: Rogue Origins → Build Windows Game**.

## Core design

- **Living organisms:** named individuals with needs, movement, predation, growth, aging, and parentage.
- **Evolution:** diploid inheritance, mutation, recombination, variation, natural selection, and eventually speciation.
- **Colonies:** selectable organisms, contextual orders, nesting, migration, and emergent social structures.
- **Planet:** changing environments and major geological events in a continuous, persistent world—not a fixed four-level campaign.
- **Roguelike:** extinction and survival create different branches of evolutionary history.
- **Art:** preserve the approved illustrated 2.5D aesthetic; prototype sprites are temporary until a modular animated art pipeline is complete.

## Immediate priorities

1. Upload all Unity source, scene, and art assets to GitHub.
2. Validate Unity scene compilation and actual Windows play mode.
3. Replace concept-art backdrop with layered tiles, independent organism sprites and animation.
4. Verify genetics, energy ecology, and selection through reproducible tests.
5. Add lineage/speciation inspection, colony AI, and long-run simulation profiling.

See [ROADMAP.md](ROADMAP.md) for development phases.
