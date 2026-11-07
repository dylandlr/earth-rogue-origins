import unittest
import numpy as np
from survival import measure
class Contracts(unittest.TestCase):
 def test_starvation_timing(self):
  r=measure({(0,0)},set(),0,1,threshold=2,steps=4,source_mass=0)
  np.testing.assert_allclose(r['alive_time'],[.5,.5]);np.testing.assert_allclose(r['final_alive'],[0,0])
 def test_no_patch_equivalence(self):
  r=measure({(i,0) for i in range(10)},set(),0,1)
  for v in r.values():self.assertAlmostEqual(v[0],v[1])
 def test_disconnected_cell_dies(self):
  r=measure({(0,0),(3,0)},set(),0,1,threshold=2,steps=4)
  np.testing.assert_allclose(r['alive_time'],[.75,.75]);np.testing.assert_allclose(r['final_alive'],[.5,.5])
 def test_dead_resource_sealed(self):
  r=measure({(0,0)},{(0,0)},0,1,demand=1,threshold=.5,steps=5,source_mass=.4)
  np.testing.assert_allclose(r['alive_time'],[.2,.2]);np.testing.assert_allclose(r['remaining'],[0,0]);np.testing.assert_allclose(r['maintenance']+r['upkeep']+r['dead_material'],[.4,.4])
 def test_sufficient_food_no_deaths(self):
  r=measure({(0,0)},set(),0,1,demand=.01)
  np.testing.assert_allclose(r['alive_time'],[1,1]);np.testing.assert_allclose(r['final_alive'],[1,1])
 def test_mirror(self):
  body={(0,0),(1,0),(2,0),(2,1)};a=measure(body,{(1,0)},0,1);b=measure({(-x,y) for x,y in body},{(-1,0)},0,-1)
  for k in a:np.testing.assert_allclose(a[k],b[k],atol=1e-10)
if __name__=='__main__':unittest.main()
