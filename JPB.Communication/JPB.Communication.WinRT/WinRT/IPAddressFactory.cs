using System;
using JPB.Communication.Contracts.Factorys;
using JPB.Communication.Contracts.Intigration;

namespace JPB.Communication.NativeWin.WinRT
{
    class IPAddressFactory : IIPaddressFactory
    {
        public bool TryParse(string hostOrIp, out IPAddress ipAddress)
        {
            throw new NotImplementedException();
        }

        public IPAddress Parse(string p)
        {
            throw new NotImplementedException();
        }
    }
}
