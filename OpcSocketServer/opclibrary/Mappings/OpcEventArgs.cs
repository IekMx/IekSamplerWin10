using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace opclibrary.Mappings
{
    public class OpcEventArgs
    {
        public int ItemHandle { get; set; }
        public object ItemValue { get; set; }
        public bool IsFault { get; set; }
    }
}
