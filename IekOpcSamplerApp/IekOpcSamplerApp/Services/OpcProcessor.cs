using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IekOpcSamplerApp.Services
{
    class OpcProcessor
    {
        private Queue<Models.OpcMessage> _MessageQ = new Queue<Models.OpcMessage>();

        public void AddIncomingMessage(string msgString)
        {
            
        }

    }
}
