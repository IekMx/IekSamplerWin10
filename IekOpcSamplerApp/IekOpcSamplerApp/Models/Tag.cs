using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IekOpcSamplerApp.Models
{
    public class Tag : IComparable
    {
        public int Handle { get; set; }
        public string Name { get; set; }
        public string Value { get; set; }
        public string Server { get; set; }

        public int CompareTo(object obj)
        {
            return ((IComparable)Handle).CompareTo(((Tag)obj).Handle);
        }
    }
}
