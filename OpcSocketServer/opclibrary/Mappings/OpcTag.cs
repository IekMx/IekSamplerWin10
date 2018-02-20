using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace opclibrary.Mappings
{
    public class OpcTag
    {
        public object Value { get; set; }
        public int Handle { get; set; }
        public int FormId { get; set; }
        public int Corr { get; set; }
        public string Name { get; set; }
        public DateTime Timestamp { get; set; }

        public OpcTag()
        {

        }
        
    }
}
