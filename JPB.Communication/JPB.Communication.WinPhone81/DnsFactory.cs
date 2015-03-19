using JPB.Communication.PCLIntigration.Contracts.Factorys;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using JPB.Communication.WinRT;

using Windows.Networking;
using Windows.Networking.Connectivity;
using Windows.Networking.Sockets; 

namespace JPB.Communication.WinRT.WinRT
{
    public class DnsFactory : IDNSFactory
    {
        public string GetHostName()
        {
          
            return NetworkInfomation.GetHostName();
        }

        public PCLIntigration.Contracts.IPHostEntry GetHostEntry(string p)
        {
            return Dns.GetHostEntry(p).AsGeneric();
        }

        public PCLIntigration.ComBase.IPAddress[] GetHostAddresses(string host)
        {
            return Dns.GetHostAddresses(host).Select(s => s.AsGeneric()).Where(s => s != null).ToArray();
        }
    }
}
