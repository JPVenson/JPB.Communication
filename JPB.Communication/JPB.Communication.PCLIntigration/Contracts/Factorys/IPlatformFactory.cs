using System;
using System.IO;
using JPB.Communication.Shared.CrossPlatform;

namespace JPB.Communication.Contracts.Factorys
{
    public interface IPlatformFactory
    {
        Stream CreatePlatformFileStream();

        ISocketFactory SocketFactory { get; }
        IDNSFactory DnsFactory { get; }
        //IIPaddressFactory IpAddressFactory { get; }

        event Action<object, PclTraceWriteEventArgs> TraceMessage;
        void RaiseTraceMessage(object sender, PclTraceWriteEventArgs arg);
    }   
}
