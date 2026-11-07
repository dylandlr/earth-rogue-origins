import unittest
import numpy as np
from maintenance import measure
class Contracts(unittest.TestCase):
 def test_no_transport_identical(self):
  r=measure({(i,0) for i in range(8)},set(),0,1,.01)
  for v in r.values():self.assertAlmostEqual(v[0],v[1])
 def test_isolated_cell_exact_accounting(self):
  r=measure({(0,0)},{(0,0)},0,1,.2,steps=1,source_mass=1,upkeep=.1)
  np.testing.assert_allclose(r['remaining'],[.8,.7]);np.testing.assert_allclose(r['maintenance'],[.2,.2]);np.testing.assert_allclose(r['upkeep'],[0,.1])
 def test_disconnected_demand_unserved(self):
  r=measure({(0,0),(3,0)},set(),0,1,.01,steps=10)
  np.testing.assert_allclose(r['coverage'],[.5,.5]);np.testing.assert_allclose(r['full_fraction'],[.5,.5])
 def test_no_resource_no_service_or_upkeep(self):
  r=measure({(0,0),(1,0)},{(0,0)},0,1,.01,source_mass=0)
  for key in ['coverage','full_fraction','maintenance','upkeep','remaining']:np.testing.assert_allclose(r[key],[0,0])
 def test_mirror_invariance(self):
  body={(0,0),(1,0),(2,0),(2,1)};patch={(1,0)}
  a=measure(body,patch,0,1,.01);b=measure({(-x,y) for x,y in body},{(-x,y) for x,y in patch},0,-1,.01)
  for key in a:np.testing.assert_allclose(a[key],b[key],atol=1e-10)
if __name__=='__main__':unittest.main()
