# Endpoint reversal diagnostic

Reuse inspected seeds601-624 and fixed morphology, demand0.01/threshold30. This is an explanatory follow-up, not fresh confirmation. Four interventions: ordinary (no upgrade, no upkeep); paid upgrade (+0.1 conductance,0.001 upkeep); free upgrade (+0.1,zero upkeep); cost-only (zero conductance upgrade,0.001 upkeep). All other rules exactly match survival.py. Free upgrade also removes the solvency gate: cannot distinguish cost removal from gate removal. Cost-only charges the same rate but its changed state can change total charges.

Record per-step living fraction, maintenance consumed, upkeep, remaining and dead material. Average four directions within each seed before paired bootstrap10000,RNG711. Compare each intervention with ordinary and paid with free. Reproduce prior paid/ordinary results before interpretation. No claim of additive attribution: dynamics and death alter subsequent flow. No new tuning or candidate promotion.
