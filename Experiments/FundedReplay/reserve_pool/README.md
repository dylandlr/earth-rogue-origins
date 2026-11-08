# Separate reserves: no survival improvement

Mobile nutrients retain unit diffusion volume and capacity. A separate initially empty immobile reserve holds up to9 units/cell. Transfers are bounded and conserve resources including transfer losses. This avoids automatic dilution from added storage.

With transport enabled, final survival at720 steps was14.867% without a pool,14.258% with a free pool, and13.603% with a costed pool. Ordinary transport results were13.637%,13.341%,12.671%, respectively. The chosen reserve policy does not improve mean survival. Acquired food increases, but this alone does not establish useful delivery or survival.

The free-pool result shows costs are not the only issue. Depositing mobile nutrient can alter delivery; the experiment does not isolate exactly why the fixed threshold policy underperforms. Do not promote this policy or describe it as evolved storage. These are reused seeds701-724 and uncalibrated assumptions. No fresh confirmation, reproduction or Unity integration.

Four tests passed: zero-pool baseline equivalence on a test tissue, explicit deposit loss, zero-capacity inactivity and full costed conservation. Every step of576 intervention evaluations checked capacity bounds and full resource accounting. Construction, maintenance, transfer losses and both nutrient pools are included. No post-result tuning occurred.

Requires Python3,numpy,pandas. Keep reserve_pool beside normalized/maps.json and storage/model.py. From reserve_pool run `python -m unittest -v test_pool.py` and `python run.py`. The archive contains exact dependencies and raw outputs. Reports include paired exploratory bootstrap intervals.

Next evaluate exchange policies against actual service interruptions rather than assuming storage should help under steady supply. Keep food budgets matched and specify interruption timing before testing. Policy selection and any fresh holdout must remain separate.
