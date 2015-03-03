using JPB.Communication.PCLIntigration.ComBase;
using JPB.Communication.PCLIntigration.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JPB.Communication.WinRT
{
    public static class WinRtExtentions
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
            return new IPAddress() { Address = address.ToString() };
        }
    }
}
