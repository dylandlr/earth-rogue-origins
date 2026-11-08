import unittest
import numpy as np
from model import measure
class Contracts(unittest.TestCase):
 def test_single_cell_rate(self):
  r=measure({(0,0)},set(),0,1,demand=.01,steps=2,rate=.1,source_mass=1)
  np.testing.assert_allclose(r['acquired'],[.2,.2]);np.testing.assert_allclose(r['remaining'],[.18,.18]);np.testing.assert_allclose(r['external'],[.8,.8])
 def test_budget_exhaustion(self):
  r=measure({(0,0)},set(),0,1,steps=3,rate=1,source_mass=.1)
  np.testing.assert_allclose(r['acquired'],[.1,.1]);np.testing.assert_allclose(r['external'],[0,0])
 def test_capacity_saturation(self):
  r=measure({(0,0)},set(),0,1,steps=2,rate=2,capacity=1,source_mass=10,demand=.01)
  np.testing.assert_allclose(r['acquired'],[1.01,1.01]);np.testing.assert_allclose(r['remaining'],[.99,.99])
 def test_dead_stops_acquisition(self):
  r=measure({(0,0)},set(),0,1,steps=4,rate=.01,source_mass=10,demand=1,threshold=1)
  np.testing.assert_allclose(r['acquired'],[.02,.02]);np.testing.assert_allclose(r['final_alive'],[0,0])
 def test_zero_rate(self):
  r=measure({(0,0)},set(),0,1,steps=4,rate=0,threshold=2)
  np.testing.assert_allclose(r['acquired'],[0,0]);np.testing.assert_allclose(r['external'],[100,100]);np.testing.assert_allclose(r['alive_time'],[.5,.5])
 def test_no_patch_equivalence(self):
  r=measure({(i,0) for i in range(5)},set(),0,1)
  for v in r.values():np.testing.assert_allclose(v[0],v[1])
if __name__=='__main__':unittest.main()
