using JPB.Communication.PCLIntigration.ComBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JPB.Communication.PCLIntigration.Contracts
{
    public class IPEndPoint
    {
        public ushort Port { get; set; }
        public IPAddress Address { get; set; }
    }
}
