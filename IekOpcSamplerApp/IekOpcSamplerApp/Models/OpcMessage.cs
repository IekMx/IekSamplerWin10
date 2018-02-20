using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IekOpcSamplerApp.Models
{
    class OpcMessage
    {
        public Tag Tag { get; set; }
        public DateTime Timestamp { get; set; }
        public string Content { get; set; }


    }
}
