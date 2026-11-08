import unittest
import numpy as np
from model import measure
from run import schedules
class Contracts(unittest.TestCase):
 def test_equal_total_and_cutoff(self):
  for s in schedules().values():self.assertAlmostEqual(s.sum(),100);self.assertTrue(np.all(s[180:]==0))
 def test_delayed_food_cannot_revive(self):
  r=measure({(0,0)},set(),0,1,steps=4,threshold=2,schedule=[0,0,100,0])
  np.testing.assert_allclose(r['final_alive'],[0,0]);np.testing.assert_allclose(r['inaccessible'],[100,100]);np.testing.assert_allclose(r['remaining'],[0,0])
 def test_fixed_source_allocation(self):
  r=measure({(0,0),(1,0)},set(),0,1,steps=4,threshold=2,schedule=[0,0,100,0])
  np.testing.assert_allclose(r['inaccessible'],[100,100])
 def test_one_cell_exact(self):
  r=measure({(0,0)},set(),0,1,demand=1,steps=3,schedule=[2,2,2])
  np.testing.assert_allclose(r['maintenance'],[3,3]);np.testing.assert_allclose(r['remaining'],[3,3])
 def test_invalid_schedule(self):
  for s in [[-1],[float('nan')],[1,2]]:
   with self.assertRaises(ValueError):measure({(0,0)},set(),0,1,steps=1,schedule=s)
if __name__=='__main__':unittest.main()
