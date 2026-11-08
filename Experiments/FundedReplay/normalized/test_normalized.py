import unittest
import numpy as np
from run import parameters
from model import measure
class Contracts(unittest.TestCase):
 def test_matched_rate(self):
  body={(i,0) for i in range(8)}
  for axis in [0,1]:
   for uniform in [False,True]:
    kw=parameters(body,axis,1,1.5);kw['steps']=1
    r=measure(body,set(),axis,1,uniform=uniform,**kw)
    np.testing.assert_allclose(r['acquired'],[.12,.12])
 def test_uniform_surplus_survives(self):
  body={(i,0) for i in range(8)};kw=parameters(body,0,1,1.5)
  r=measure(body,body,0,1,uniform=True,**kw)
  np.testing.assert_allclose(r['final_alive'],[1,1])
 def test_budget_and_reserve(self):
  body={(i,0) for i in range(8)};kw=parameters(body,0,1,1)
  kw.update(steps=1,rate=0);r=measure(body,set(),0,1,**kw)
  np.testing.assert_allclose(r['external'],[115.2,115.2]);np.testing.assert_allclose(r['remaining'],[2.32,2.32])
if __name__=='__main__':unittest.main()
