"""Synthetic accounting fixtures for scripts/audit-causal-branches.py."""
import csv
import importlib.util
import tempfile
import unittest
from pathlib import Path

SCRIPT = Path(__file__).resolve().parents[1] / "audit-causal-branches.py"
spec = importlib.util.spec_from_file_location("causal_audit", SCRIPT)
module = importlib.util.module_from_spec(spec)
spec.loader.exec_module(module)


def write_csv(path, rows):
    with path.open("w", newline="", encoding="utf-8") as handle:
        writer = csv.DictWriter(handle, fieldnames=list(rows[0]))
        writer.writeheader()
        writer.writerows(rows)


class CausalAuditTests(unittest.TestCase):
    def setUp(self):
        self.temp = tempfile.TemporaryDirectory()
        self.addCleanup(self.temp.cleanup)
        self.folder = Path(self.temp.name)
        write_csv(self.folder / "population.csv", [
            dict(generation=0, id=0, occupied=10, fitness=.4, novelty=.6,
                 causal_branches=0, strongest_branch_effect=0),
            dict(generation=1, id=1, occupied=10, fitness=.8, novelty=.3,
                 causal_branches=1, strongest_branch_effect=.5)
        ])

    def branches(self, changed=5, iou=.5, effect=.5, active="true",
                 original=10, ablated=10):
        write_csv(self.folder / "causal_branch_ablation.csv", [
            dict(generation=1, id=1, innovation=3, removed_genes=1,
                 original_cells=original, ablated_cells=ablated,
                 changed_cells=changed, occupancy_iou=iou,
                 changed_fraction=effect, morphologically_active=active)
        ])

    def test_detects_mass_balanced_rearrangement(self):
        # Two masks of size 10 with five changed cells is parity-impossible:
        # use 4 changes (2 removed + 2 added), union=12 and effect=1/3.
        write_csv(self.folder / "population.csv", [
            dict(generation=1, id=1, occupied=10, fitness=.8, novelty=.3,
                 causal_branches=1, strongest_branch_effect=1/3)
        ])
        self.branches(changed=4, iou=2/3, effect=1/3)
        report, rows = module.audit(self.folder)
        self.assertEqual([], report["errors"])
        self.assertEqual(4, rows[0]["redistribution_cells"])
        self.assertEqual("mixed_gain_loss_requires_spatial_review",
                         rows[0]["interpretation"])

    def test_rejects_wrong_effect_accounting(self):
        self.branches(changed=4, iou=.5, effect=.5)
        report, _ = module.audit(self.folder)
        self.assertTrue(any("changed_fraction inconsistent" in e for e in report["errors"]))

    def test_extent_only_knockout(self):
        write_csv(self.folder / "population.csv", [
            dict(generation=1, id=1, occupied=10, fitness=.8, novelty=.3,
                 causal_branches=1, strongest_branch_effect=.2)
        ])
        # 10 cells versus 8 cells, changed=2 and union=10.
        self.branches(changed=2, iou=.8, effect=.2, original=10, ablated=8)
        report, rows = module.audit(self.folder)
        self.assertEqual([], report["errors"])
        self.assertEqual(0, rows[0]["redistribution_cells"])
        self.assertEqual("pure_extent_compatible", rows[0]["interpretation"])


if __name__ == "__main__":
    unittest.main()
