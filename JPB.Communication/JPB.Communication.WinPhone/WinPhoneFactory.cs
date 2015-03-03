using JPB.Communication.PCLIntigration.Contracts;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace JPB.Communication.WinPhone
{
    public class WinPhoneFactory : IPlatformFactory
    {
        public System.IO.Stream CreatePlatformFileStream()
        {
            return new MemoryStream();
        }

        public Contracts.ISocketFactory SocketFactory
        {
            get { throw new NotImplementedException(); }
        }

        public event Action<object, PCLIntigration.Shared.CrossPlatform.PclTraceWriteEventArgs> TraceMessage;

        public void RaiseTraceMessage(object sender, PCLIntigration.Shared.CrossPlatform.PclTraceWriteEventArgs arg)
        {
            if (TraceMessage != null)
                TraceMessage(sender, arg);
        }
    }
}
