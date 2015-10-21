using System.Linq;
using System.Net;
using JPB.Communication.Contracts.Factorys;
using IPAddress = JPB.Communication.Contracts.Intigration.IPAddress;
using IPHostEntry = JPB.Communication.Contracts.Intigration.IPHostEntry;

namespace JPB.Communication.WinRT.WinRT
{
    public class DnsFactory : IDNSFactory
    {
        public string GetHostName()
        {
            return Dns.GetHostName();
        }

        public IPHostEntry GetHostEntry(string p)
        {
            return Dns.GetHostEntry(p).AsGeneric();
        }

        public IPAddress[] GetHostAddresses(string host)
        {
            return Dns.GetHostAddresses(host).Select(s => s.AsGeneric()).Where(s => s != null).ToArray();
        }
    }
}
