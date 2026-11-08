import unittest,importlib.util
import numpy as np
from pathlib import Path
from model import measure
class Tests(unittest.TestCase):
 def test_baseline_equivalence(self):
  spec=importlib.util.spec_from_file_location('prior',Path('../storage/model.py'));m=importlib.util.module_from_spec(spec);spec.loader.exec_module(m)
  body={(i,0) for i in range(8)};a=measure(body,{(0,0)},0,1);b=m.measure(body,{(0,0)},0,1)
  for k in b:np.testing.assert_allclose(a[k],b[k],atol=1e-12)
 def test_deposit_cost(self):
  r=measure({(0,0)},set(),0,1,steps=1,ratio=100,reserve_capacity=9,exchange_cost=.1)
  np.testing.assert_allclose(r['stored'],[.05,.05]);np.testing.assert_allclose(r['exchange_cost'],[.005,.005])
 def test_zero_pool(self):
  r=measure({(0,0)},set(),0,1,steps=20,ratio=100)
  np.testing.assert_allclose(r['stored'],[0,0]);np.testing.assert_allclose(r['exchange_cost'],[0,0])
 def test_costed_conservation(self):
  r=measure({(i,0) for i in range(8)},{(0,0)},0,1,reserve_capacity=9,build=.1,storage_rate=.0001,exchange_cost=.01)
  total=sum(r[k] for k in ['external','remaining','stored','basal','storage_cost','transport_cost','construction','exchange_cost','dead_material'])
  np.testing.assert_allclose(total,[117.6,117.6])
if __name__=='__main__':unittest.main()
