using JPB.Communication.PCLIntigration.Contracts.Factorys;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JPB.Communication.WinRT.WinRT
{
    class IPAddressFactory : IIPaddressFactory
    {
        public bool TryParse(string hostOrIp, out PCLIntigration.ComBase.IPAddress ipAddress)
        {
            throw new NotImplementedException();
        }

        public PCLIntigration.ComBase.IPAddress Parse(string p)
        {
            throw new NotImplementedException();
        }
    }
}
