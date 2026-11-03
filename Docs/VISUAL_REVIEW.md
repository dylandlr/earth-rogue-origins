# Morphology visual review — v0.7

## Why this exists
Procedural evolution must be tested at **actual gameplay scale**, not judged only from a full-resolution character portrait or an attractive AI-generated concept. This harness renders 20 phenotype-driven, articulated, illustrated-part creatures using Unity's real camera/graphics pipeline.

## Output
Run the menu command **Earth: Rogue Origins > Art > Create Morphology Review Board**, or from Windows PowerShell:

```powershell
.\scripts\capture-visual-review.ps1 -UnityEditor "C:\Program Files\Unity\Hub\Editor\6000.0.23f1\Editor\Unity.exe"
```

Produces:
- `ValidationResults/VisualReview/morphology_board.png`: actual 1600×1000 Unity render, 4 rows × 5 variants
- `ValidationResults/VisualReview/morphology_metrics.csv`: genotype-derived visual and physical measurements
- `ValidationResults/VisualReview/capture.log`: Unity run diagnostics when using PowerShell

The exporter creates a temporary preview scene, not a saved Unity scene. It doesn't modify the ecosystem's scene and does not claim that the rendered blockout art equals the approved concept. A foreground-pixel check fails on blank output. Because it uses `Camera.Render()`, **a real graphics device is required**; do not add `-nographics`.

## Review matrix
- Row 1: defensive armor (plate height, armor coverage, body depth)
- Row 2: semi-aquatic adaptation (tail, fin, foot webbing, reduced limbs)
- Row 3: terrestrial movement (limbs, muscle, trunk)
- Row 4: body/head proportions and pigment density

Each row includes five adaptation intensities, 0.0 to 1.0. Color is organized by row so the silhouette comparison is not confounded by arbitrary palettes within a row.

## Pass/fail questions
1. Do anatomy changes materially change silhouettes, not just colors?
2. Are fins connected to the tail, plates attached to curved dorsal surfaces, and legs attached to correct joints?
3. Do armor, aquatic, leg, and head adaptations agree with the metrics CSV?
4. Is each organism identifiable at 64–96 pixels wide without relying on text?
5. Are painted layer outlines, shading, contact shadows, and sort order consistent?
6. Does variation preserve a plausible vertebrate body plan rather than clipping or producing dislocated structures?
7. Is the captured render non-blank, and does the same seed give repeatable visual morphology?

## Workflow safeguards
Both the illustrated-parts generator and legacy terrain/creature generator now preserve existing PNGs by default. Explicit overwrite menu items regenerate them. Scene creation must not erase curated artwork or reassigned part sprites.

## Important limitations
- This is a morphology verification board, not a full gameplay screenshot.
- No Unity execution has been performed in the ChatGPT tool runtime; results are not yet verified.
- The initial illustrated body-part PNGs are procedurally shaded placeholders; replacing them with final curated assets is still necessary.
- The review tests variation under the same tetrapod developmental grammar. More body plans are not yet implemented.
