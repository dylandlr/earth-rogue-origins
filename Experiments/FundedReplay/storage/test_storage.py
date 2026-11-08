import unittest
import numpy as np
from model import measure
class Contracts(unittest.TestCase):
 def test_explicit_costs(self):
  r=measure({(0,0)},set(),0,1,C=10,G=10,build=.1,storage_rate=.0001,steps=1)
  for key,val in [('construction',.9),('storage_cost',.0009),('basal',.01),('remaining',.3041),('external',13.485)]:np.testing.assert_allclose(r[key],[val,val])
 def test_gate_independent_from_storage(self):
  for c in [1,10]:
   r=measure({(0,0)},set(),0,1,C=c,G=1,ratio=1000,steps=1)
   np.testing.assert_allclose(r['acquired'],[.7,.7])
 def test_physical_capacity_bounds_uptake(self):
  r=measure({(0,0)},set(),0,1,C=1,G=10,ratio=1000,steps=1)
  np.testing.assert_allclose(r['remaining'],[.99,.99])
 def test_unaffordable_storage(self):
  with self.assertRaises(ValueError):measure({(0,0)},set(),0,1,C=10,build=100)
 def test_no_patch_equivalence(self):
  r=measure({(i,0) for i in range(8)},set(),0,1,C=10,G=10,build=.1,storage_rate=.0001)
  for v in r.values():np.testing.assert_allclose(v[0],v[1])
if __name__=='__main__':unittest.main()
