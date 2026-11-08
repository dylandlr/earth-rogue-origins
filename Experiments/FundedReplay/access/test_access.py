import unittest
import numpy as np
from model import measure
class Contracts(unittest.TestCase):
 def test_reserve_budget(self):
  r=measure({(0,0),(1,0)},set(),0,1,steps=1,rate=0,reserve=.3,source_mass=1,demand=.01)
  np.testing.assert_allclose(r['external'],[.4,.4]);np.testing.assert_allclose(r['remaining'],[.58,.58]);np.testing.assert_allclose(r['maintenance'],[.02,.02])
 def test_equal_initial_rate(self):
  body={(i,0) for i in range(4)}
  for uniform in [False,True]:
   r=measure(body,set(),0,1,steps=1,rate=.1,uniform=uniform)
   np.testing.assert_allclose(r['acquired'],[.1,.1])
 def test_uniform_symmetry(self):
  r=measure({(0,0),(1,0)},set(),0,1,steps=10,uniform=True)
  for v in r.values():np.testing.assert_allclose(v[0],v[1])
 def test_invalid_reserve(self):
  for reserve in [-1,2]:
   with self.assertRaises(ValueError):measure({(0,0)},set(),0,1,source_mass=1,reserve=reserve)
if __name__=='__main__':unittest.main()
