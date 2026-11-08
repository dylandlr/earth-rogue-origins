import unittest
import numpy as np
from run import reachable,measure
class Contracts(unittest.TestCase):
 def test_island_unreachable(self):
  self.assertEqual(reachable({(0,0),(1,0),(3,0)},{(0,0)}),{(0,0),(1,0)})
 def test_diagonal_not_connected(self):
  self.assertEqual(reachable({(0,0),(1,1)},{(0,0)}),{(0,0)})
 def test_storage_does_not_change_rate_ceiling(self):
  for cap in [1,10]:
   r=measure({(0,0)},set(),0,1,steps=1,rate=.1,capacity=cap)
   np.testing.assert_allclose(r['acquired'],[.1,.1])
 def test_larger_cap_accepts_more_when_rate_allows(self):
  r=measure({(0,0)},set(),0,1,steps=1,rate=5,capacity=10)
  np.testing.assert_allclose(r['acquired'],[5,5])
if __name__=='__main__':unittest.main()
