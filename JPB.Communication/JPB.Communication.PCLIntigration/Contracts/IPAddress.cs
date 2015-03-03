using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JPB.Communication.PCLIntigration.ComBase
{
    public class IPAddress
    {
        public string Address { get; set; }
        public static bool TryParse(string hostOrIp, out IPAddress ipAddress) { ipAddress = null; return true; }
        public static IPAddress Parse(string p) { return null; }
    }
}
