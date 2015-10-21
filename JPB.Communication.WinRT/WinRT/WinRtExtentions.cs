using System.Linq;
using JPB.Communication.Contracts.Intigration;

namespace JPB.Communication.WinRT.WinRT
{
    public static class WinRTExtentions
    {
        public static IPEndPoint AsGeneric(this System.Net.EndPoint endpoint)
        {
            var add = endpoint as System.Net.IPEndPoint;
            return new IPEndPoint()
            {
                Address = add.Address.AsGeneric(),
                Port = (ushort)add.Port
            };
        }

        public static IPAddress AsGeneric(this System.Net.IPAddress address)
        {
            if (address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                return new IPAddress(address.Address);
            return null;
        }

        public static IPHostEntry AsGeneric(this System.Net.IPHostEntry entry)
        {
            return new IPHostEntry() { AddressList = entry.AddressList.Select(s => s.AsGeneric()).Where(s => s != null).ToArray() };
        }

    }
}
