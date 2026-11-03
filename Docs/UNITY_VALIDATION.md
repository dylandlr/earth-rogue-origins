# Unity Validation & Visual Acceptance Gate

This repository currently has **no verified Unity Editor compilation/playtest record**. Code commits are not evidence of successful gameplay. The scene builder and runtime must pass this checklist before a milestone is declared playable.

## One-command local validation (Windows)

Run PowerShell from the repository root, using your actual installed Unity editor path:

```powershell
.\scripts\validate-unity.ps1 -UnityEditor "C:\Program Files\Unity\Hub\Editor\6000.0.23f1\Editor\Unity.exe"
```

The runner:
1. Opens the project in Unity batch mode, generates the procedural art, creates `Assets/Scenes/EcosystemLab.unity`, and runs reference checks.
2. Executes Unity Edit Mode tests, saving `ValidationResults/editmode-results.xml`.
3. Builds `Builds/Windows/EarthRogueOrigins.exe` (omit this with `-SkipBuild`).
4. Saves operation logs in `ValidationResults/` and fails if an operation exits nonzero.

Unity Hub and an active valid Unity editor license are needed locally. Never run two Unity editor instances against the same project path simultaneously. The checked-in project is a prototype and may expose additional defects on its first import.

## Manual visual acceptance
- [ ] No magenta/pink missing-shader materials or invisible body parts
- [ ] Generated illustrated parts actually appear on rig anchors
- [ ] Head and tail remain attached throughout walking/swimming
- [ ] Armor follows dorsal curvature after different morphologies are generated
- [ ] Creature is legible at target gameplay size (64–96 screen px)
- [ ] Different gene changes produce distinct silhouettes rather than only tint shifts
- [ ] Artists can substitute torso/head/tail/armor PNGs and re-create scene without losing edits
- [ ] Observe 60+ organisms over 10 minutes, then reset without excessive memory growth
- [ ] Capture screenshots alongside approved art reference and record differences explicitly

## Known limitations
The generator's art remains schematic. The target is the previously approved 2.5D illustrated swamp; do not treat these generated PNGs as a visual match. Also, the original Unity v0.8 gameplay controller and binary approved concept art are not all present in GitHub. The generated ecosystem lab is the stand-alone testbed.

## Current highest-priority next work
After the first successful Unity execution: resolve any compiler errors; generate real 64px, 96px, and 128px screenshots; inspect layering and depth, then replace blockouts with a curated illustrated atlas. Only then build wider morphological families.
