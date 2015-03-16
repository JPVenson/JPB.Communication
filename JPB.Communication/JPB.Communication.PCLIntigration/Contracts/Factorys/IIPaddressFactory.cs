using JPB.Communication.PCLIntigration.ComBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JPB.Communication.PCLIntigration.Contracts.Factorys
{
    public interface IIPaddressFactory
    {
        bool TryParse(string hostOrIp, out IPAddress ipAddress);
        IPAddress Parse(string p);
    }
}
