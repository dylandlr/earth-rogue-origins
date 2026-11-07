import unittest
import numpy as np
from capacity import measure, evolve

class Contracts(unittest.TestCase):
    def setUp(self):
        self.body={(x,y) for x in range(8) for y in range(5)}
        self.patch={(3,y) for y in range(5)}

    def test_no_patch_matches_ordinary_at_all_levels(self):
        delivered, spent=measure(self.body,set(),0,1,.001)
        np.testing.assert_allclose(delivered,delivered[0],rtol=0,atol=1e-12)
        np.testing.assert_array_equal(spent,0)

    def test_unaffordable_upgrade_is_inactive(self):
        ordinary,_=measure(self.body,set(),0,1,.001)
        delivered,spent=measure(self.body,self.patch,0,1,1000)
        np.testing.assert_allclose(delivered,ordinary,rtol=0,atol=1e-12)
        np.testing.assert_array_equal(spent,0)

    def test_disconnected_source_cannot_feed_sink(self):
        body=self.body-{(4,y) for y in range(5)}
        delivered,_=measure(body,self.patch,0,1,.001)
        np.testing.assert_array_equal(delivered,0)

    def test_geometric_reflection_preserves_results(self):
        a=measure(self.body,self.patch,0,1,.001)
        b=measure({(-x,y) for x,y in self.body},{(-x,y) for x,y in self.patch},0,-1,.001)
        for left,right in zip(a,b):
            np.testing.assert_allclose(left,right,rtol=0,atol=1e-10)

    def test_selection_retains_zero_when_upgrade_is_harmful(self):
        scores=np.array([5,4,3,2,1])
        self.assertTrue(all(evolve(scores,s)==0 for s in range(20)))

if __name__=='__main__':
    unittest.main()
