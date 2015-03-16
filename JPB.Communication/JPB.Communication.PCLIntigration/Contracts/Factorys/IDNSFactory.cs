using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JPB.Communication.PCLIntigration.Contracts.Factorys
{
    public interface IDNSFactory
    {
        string GetHostName();
        IPHostEntry GetHostEntry(string p);
        ComBase.IPAddress[] GetHostAddresses(string host);
    }
}
