# Authoritative Full-Creature Art Validation — v2.9

## Purpose

The preceding milestones introduced increasingly sophisticated head, trunk,
neck, limb, joint and gait geometry. They were committed without a Unity
Editor render, so a repeated technical drawing of isolated components
cannot establish whether assembled creatures match the approved concept art.

This milestone adds an **authoritative final-sprite visual audit**. It does
not change the appearance of the animals or pretend that sprites have been
rendered in Unity.

## Art style constraints

Compare against the approved Carboniferous sprite reference:
- One continuous low-slung body tapering into an organic tail
- Broad head with integrated cheek, brow, jaw and small readable eye
- Short planted limbs attached through a naturally curved underside
- Muted olive, rust, slate and cream ramps; restrained clustered markings
- No floating joint patches, blocky rectangular outlines or clipped pixels

## Implemented

- `PixelCreatureVisualMetrics.Analyze`: computes connected opaque components,
  largest-component ratio, nontransparent pixel bounds, number of opaque
  colors and horizontal/vertical extent from an actual final RGBA sprite.
- `CreatureStyleContactSheet.Run`: renders all eight curated hero recipes
  directly through `PixelCreaturePainter.RenderPixels` and exports each
  transparent PNG, a 4x2 contact sheet, a metrics CSV and a provenance file.
- `PixelVisualMetricsTests`: synthetic known-component test cases, empty
  arrays, invalid dimensions and deterministic results for all eight heroes.
- `scripts/export-actual-creature-style.ps1`: a Windows batch runner for
  the authoritative Unity 6 editor exporter.

## How to run

```powershell
.\scripts\export-actual-creature-style.ps1 -UnityEditor "C:\Program Files\Unity\Hub\Editor\6000.0.23f1\Editor\Unity.exe"
```

Review:
- `ValidationResults/CreatureStyle/actual_csharp_full_creatures.png`
- `ValidationResults/CreatureStyle/visual_metrics.csv`
- `ValidationResults/CreatureStyle/MarshCrawler.png`
- seven other individually rendered archetype images
- `ValidationResults/CreatureStyle/provenance.txt`

The exporter fails when a sprite is nearly blank, strongly disconnected
(dominant component below 80%), or clipped against the canvas border. These
are **initial heuristics**, not visual-fidelity acceptance. Fish fins or
decorative appendages may legitimately require reviewed exceptions.

## Verification status

C# source is committed. No Unity Editor or .NET compiler is available in the
current execution environment, and the new script has not been run here.
There is **no new v2.9 creature screenshot**. The last independent
standalone full-creature preview is v1.9 and cannot be relabeled as v2.9.

## Next decision gate

The next artistic adjustments should be chosen by inspecting the actual
authoritative full-sprite contact sheet. Rank defects in the marsh crawler's
head/body connection, foot-to-belly continuity, tail contour and shading.
Keep the reference style as the criterion rather than adding unsupported
detail or presenting image-generated approximations as evidence.
