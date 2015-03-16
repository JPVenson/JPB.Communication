using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JPB.Communication.PCLIntigration.Contracts
{
    public static class DnsAdapter
    {        
        internal static string GetHostName()
        {
            return NetworkFactory.PlatformFactory.DnsFactory.GetHostName();
        }

        internal static IPHostEntry GetHostEntry(string p)
        {
            return NetworkFactory.PlatformFactory.DnsFactory.GetHostEntry(p);
        }

        internal static ComBase.IPAddress[] GetHostAddresses(string host)
        {
            return NetworkFactory.PlatformFactory.DnsFactory.GetHostAddresses(host);
        }
    }
}
