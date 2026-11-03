# Evidence-Based Visual Review & Speciation Cohort Integrity — v2.3

## Verified image provenance

The artifact `ERO_ActualAsset_Style_Audit.png` was assembled locally from
the existing approved sprite-reference sheet and four existing **v1.9
standalone procedural PNGs**. There is no AI-generated creature in that
comparison. It is an art QA comparison, **not** a v2.3 Unity screenshot.

The v2.2 speciation milestone has **no new pixel-rendering or appearance
changes**, and must not be represented with new fictional creatures. Real
Unity output of the v2.0 silhouette renderer remains unverified until the
editor can run.

Reference comparison shows meaningful art gaps:
- Reference tetrapods have a long cohesive sloping dorsal line, with limbs
  hanging naturally from an integrated belly; generated legs remain stiff
  and visually outlined like blocks.
- Heads in the reference show continuous snout, eye mound and jaw structure;
  procedural heads still resemble separately mounted simple shapes.
- The reference tail tapers as a continuation of the torso; the last
  rendered procedural PNGs end in a flatter geometric wedge.
- Approved skin shading uses subtle, low-contrast clusters that suggest
  volume; generator shading is organized as broad horizontal color bands.

## v2.3 code fix

The original allopatric-divergence detector required spatial centroid
continuity across consecutive windows but did not check whether the
organisms in the separated cohort were actually the same population.
A completely replaced cohort with identical coordinates could inherit the
previous cohort's apparent isolation streak and cause a false split.

The detector now calculates set overlap using **organism IDs**, requiring
at least 60% retention of the smaller previous/current population to
continue a streak. A newly substituted cohort starts over; modest
birth/death turnover is allowed. Regression tests exercise both outcomes.

This makes allopatric evidence more trustworthy, but it **does not**
prove reproductive isolation or calibrate a biological speciation model.

## Visual standard for every future update

1. For **code that changes pixels**, provide the **actual C# Unity-generated
   PNG from the committed renderer** if Unity is available. If it isn't,
   explicitly state that verification is pending; any Python study must
   be labeled as a separate prototype and never described as game output.
2. For **simulation-only changes**, use actual species data/logs/metrics
   and don't invent a new creature render.
3. Put the current result next to the approved visual reference at native
   and gameplay resolutions, with no misleading claims about matching.
4. Record precisely which git commit generated the images, recipe and seed,
   and include an exported contact sheet and tests.
5. Don't claim concept-art fidelity until a genuine Unity output is
   compared and approved.

## Pending

- Unity batchmode test execution and the authoritative C# pixel audit PNG
- Morphological connectivity, face contour and foot-contact QA against
  approved reference
- 2D habitat-component separation, gene flow and mating compatibility
- Generated provenance manifest for each art revision
