# Branch placement search

Predeclared before generating new maps. Genome29, pinned model source 8524cb46a22b7fb5e7ee5e6a69af71fac3351d9a. Mono shim replay, no claim of Unity parity.

Exhaustive 135-candidate search: branch attachX and attachY offsets {-0.2,0,+0.2}, fateResponse {0.8,0.9,1.0}, capacity index {0,1,2,3,4}. Attachment values remain in [-1,1]. Same funded growth 64x48,36 ticks, cap360, nutrient/site4. Same mass-conserving fixed-tissue delivery assay,180 pulses, upkeep0.001.

Train seeds301-308, horizontal both signs. Advance top three by mean delivery (lexicographic ties). Choose best on validation309-312, both axes/signs. Freeze before generating test401-424, vertical both signs. Paired primary comparison against prior fixed candidate (offset0,0;response0.9;capacity4); secondary against selected morphology with capacity0. No test retuning. Seed is statistical unit; percentile bootstrap10000, RNG711. This changes body geometry and transport fate jointly, so improvement cannot establish location-specific causality. Delivery is a proxy, not organism fitness.
