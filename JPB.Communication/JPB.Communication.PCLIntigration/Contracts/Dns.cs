using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JPB.Communication.PCLIntigration.Contracts
{
    public class Dns
    {
        internal static object GetHostName()
        {
            throw new NotImplementedException();
        }

        internal static IPHostEntry GetHostEntry(object p)
        {
            throw new NotImplementedException();
        }

        internal static ComBase.IPAddress[] GetHostAddresses(string host)
        {
            throw new NotImplementedException();
        }
    }
}
