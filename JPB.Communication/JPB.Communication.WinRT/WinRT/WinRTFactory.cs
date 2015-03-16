using JPB.Communication.Contracts;
using JPB.Communication.PCLIntigration.Contracts;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JPB.Communication.WinRT.WinRT
{
    public class WinRTFactory : IPlatformFactory
    {
        public System.IO.Stream CreatePlatformFileStream()
        {
            return new FileStream(Path.GetTempFileName(), FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite);
        }

        public ISocketFactory SocketFactory
        {
            get { return new WinRtSocketFactory(); }
        }

        public event Action<object, PCLIntigration.Shared.CrossPlatform.PclTraceWriteEventArgs> TraceMessage;

        public void RaiseTraceMessage(object sender, PCLIntigration.Shared.CrossPlatform.PclTraceWriteEventArgs arg)
        {
            if (TraceMessage != null)
                TraceMessage(sender, arg);
        }


        public PCLIntigration.Contracts.Factorys.IDNSFactory DnsFactory
        {
            get { return new DnsFactory(); }
        }
    }
}
