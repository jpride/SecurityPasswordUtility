using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StringStorageUtility;

namespace SecurityPasswordUtility
{
    public class InheritTest : StringStore
    {
        public InheritTest() : base("/user/inheritTest", 8000, 1)
        {
            
        }
    }
}
