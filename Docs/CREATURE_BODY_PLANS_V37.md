# Creature Art v3.7 — Heritable tetrapod body-plan split

## Grounded visual finding
The user's actual Unity v3.6 eight-creature contact sheet showed only minor
silhouette improvement from isolated limb edits: squat animals still resembled
generic quadrupeds. This milestone changes the coordinated proportions of
the entire tetrapod, rather than adding more markings or micro-contours.

## Design
- `PixelTetrapodMass.Resolve` expresses a continuous squat value from
  inherited `trunkStyle` and `trunkDepth`. Robust styles are strongly
  squat; rounded styles intermediate; slender styles retain a running body.
  This is **not** a fixed, per-species sprite override.
- Body mass increases rendered torso depth by up to four pixels, integrates
  the head 0–2 pixels toward the torso and expands skull width 0–3 pixels.
- Fore- and hind-leg roots spread outward relative to torso length and rise
  into the trunk interior. The limbs shorten with squat mass, while
  shoulder and hip skin folds expand to cover the joints.
- Head landmarks, cheek and eyes continue deriving from adjusted head bounds
  rather than remaining fixed on old coordinates.
- Curated SlateNewt now uses a rounded, intermediate amphibian torso and
  limbs; ClayRunner retains the slender cursorial family. Genetics-to-art
  conversion uses existing inherited phenotypes and styles.
- The fish and arthropod branches bypass `PixelTetrapodMass` completely.

## Visual goals, not yet verified
- MarshCrawler: one continuous low-slung amphibian mass, with planted short
  legs rather than separate posts.
- PaleJuvenile and SlateNewt: visibly intermediate squat shapes, not exact
  copies of the robust crawler.
- ClayRunner: narrower torso, longer legs, and more open space under its body.
- Neither clipping, detached parts nor collapsed eyes.

## Regression checks
The new Edit Mode tests compare genotype-matched robust and slender
body plans across three trunk depths and three leg lengths. They also
check five hero tetrapods across three inherited depths for coherent
joint/head attachments, nonempty full sprites and valid canvas bounds.

## Provenance and execution
The only currently available v3.7 visual baseline is the **previous v3.6
real Unity render uploaded by the user**. No v3.7 Unity render is yet
available; actual compiler and test-run results must not be claimed.

On the user's Windows system, run:

```powershell
git pull origin main
.\scripts\export-actual-creature-style.ps1 -UnityEditor "C:\Program Files\Unity\Hub\Editor\6000.6.5f1\Editor\Unity.exe"
```

Open the new run-specific `actual_csharp_full_creatures.png`. Compare with
the v3.6 contact sheet at native 128x96 pixels and nearest-neighbor
enlargement, then inspect the full-creature QA metrics.

Do not keep layering micro-fixes if the silhouette remains generic:
consider redesigning full body profile masks and occlusion ordering.
