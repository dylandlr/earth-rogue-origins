# Joint selection pilot
Source simulation pinned to 8524cb46a22b7fb5e7ee5e6a69af71fac3351d9a.
Run with Mono 6.12 and the included Unity math shim. Python requires numpy and pandas.
Training seeds 101–116 use horizontal sources; validation 117–124 uses all directions.
The frozen winner is response .9 and capacity index 4 (double conductance).
Final testing used seeds 201–224 and vertical sources only after frozen.json was written.
All 20 selection repeats selected this candidate, matching exhaustive training scores.
Development has 36 ticks, 64x48 sites, cap 360 and nutrient 4 per site.
Delivery has 180 pulses, source mass 100, upkeep .001 charged locally.
Mutation chooses one axis per child and moves -1/0/+1 with probabilities .15/.7/.15.
Population 32, 20 generations, one elite, tournament size 3.

Rebuild using the supplied C# sources and run.sh with Mono packages extracted under root/.
Run run.sh 101 124, python select.py select, run.sh 201 224, python select.py test.
The bundle contains all generated maps and scores for auditing.
The source lineage uses experimental artificial nutrient delivery as the objective.
Paired comparisons include morphology effects versus ancestor and capacity effects on
the selected morphology. Matched placement controls remain pending.
Unity runtime parity, reproduction and survival selection remain unverified.
All final candidate scores are retained; no retuning followed final-test inspection.
