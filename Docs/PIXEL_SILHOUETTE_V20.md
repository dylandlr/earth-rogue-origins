# Pixel Facial Silhouette v2.0 — Shape-Aware Contours

## Implementation completed

This milestone targets the specific boxy head silhouettes identified in the
v1.9 visual study. It improves the authoritative Unity C# rasterizer, rather
than modifying only the separate Python preview.

- Four head styles now use **13 deliberately authored contour stations** per
  upper/lower outline, replacing the seven-station head forms.
- The pixel painter builds the exact interior mask first. An outline is then
  formed by one-pixel mask dilation, rather than painting a rectangle around
  every x-column.
- Shading is bounded by that silhouette, grouped into shadow, skin, dorsal
  lighting and isolated highlights.
- The existing recipe, hereditary controls, composable layers and four-frame
  walk/swim pipeline are retained. Artwork is newly generated, not extracted.

## Why the rendering change matters

Previous per-column code drew dark pixels outside the contour from y-low-2
to y-high+2, even at x positions beyond the actual silhouette. This can leave
blocky bars and squared corners around small faces. The new outline is based
on the actual mask neighborhood and cannot generate those rectangular bars
from the old logic.

## Acceptance and testing

New Unity Edit Mode regression tests verify the head contours never invert,
remain within the permitted normalized range, and leave transparent pixels
around the snout instead of producing completely filled rectangular corners.

**These tests are committed but have not yet run in Unity.** The last rendered
v1.9 Python preview is not representative of the updated v2.0 Unity rasterizer;
the next evidence should be a Unity C# contact sheet from:

```powershell
.\scripts\export-pixel-visual-audit.ps1 -UnityEditor "<path-to-Unity.exe>"
```

Compare at native 128×96 and scaled 32/64 screen pixels, and inspect the
MarshCrawler, ClayRunner, SlateNewt and PaleJuvenile against the approved
recreated sprite reference. Do not mark concept-art fidelity as passed without
reviewing that real output.

## What remains

Eye/cheek/jaw landmarks need to be checked against the new asymmetric
silhouettes, especially on small juveniles. A more organic neck connection,
pixel-clean shading clusters and a rigorous gameplay-scale quality gate remain
open. Fish and arthropods also require separate, visually approved profile
families before the style is complete.
