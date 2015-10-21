using JPB.Communication.Contracts.Intigration;

namespace JPB.Communication.Contracts
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

        internal static IPAddress[] GetHostAddresses(string host)
        {
            return NetworkFactory.PlatformFactory.DnsFactory.GetHostAddresses(host);
        }
    }
}
