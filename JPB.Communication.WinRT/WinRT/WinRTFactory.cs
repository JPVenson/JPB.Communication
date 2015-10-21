using System;
using System.IO;
using JPB.Communication.Contracts.Factorys;
using JPB.Communication.Shared.CrossPlatform;

namespace JPB.Communication.WinRT.WinRT
{
    public class WinRTFactory : IPlatformFactory
    {
        public Stream CreatePlatformFileStream()
        {
            return new FileStream(Path.GetTempFileName(), FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite);
        }

        public ISocketFactory SocketFactory
        {
            get { return new WinRtSocketFactory(); }
        }

        public event Action<object, PclTraceWriteEventArgs> TraceMessage;

        public void RaiseTraceMessage(object sender, PclTraceWriteEventArgs arg)
        {
            if (TraceMessage != null)
                TraceMessage(sender, arg);
        }


        public IDNSFactory DnsFactory
        {
            get { return new DnsFactory(); }
        }
    }
}
