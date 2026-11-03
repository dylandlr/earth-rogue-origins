# Earth: Rogue Origins

**A 2.5D prehistoric evolution, ecology, roguelike, and colony simulator built in Unity.**

The approved visual target is a hand-illustrated, top-down cartoon swamp with dark RimWorld-inspired management panels. Mechanically, this project combines individual organism simulation, heritable genetics, ecological competition, emergent selection, and long-term geologic change.

## Status

**Early Unity prototype. Not a finished game.** A Unity v0.8 project archive was generated in the development conversation, but its complete Unity source and binary art assets have **not yet been committed to this repository**. This repository is now writable and has the initial Unity configuration, build helper, and planning documents. Do not treat this initial commit as a complete runnable game.

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
