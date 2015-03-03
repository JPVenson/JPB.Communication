using JPB.Communication.Contracts;
using JPB.Communication.PCLIntigration.ComBase;
using JPB.Communication.PCLIntigration.Shared.CrossPlatform;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JPB.Communication.PCLIntigration.Contracts
{
    public interface IPlatformFactory
    {
        Stream CreatePlatformFileStream();
        ISocketFactory SocketFactory { get; }
        event Action<object, PclTraceWriteEventArgs> TraceMessage;
        void RaiseTraceMessage(object sender, PclTraceWriteEventArgs arg);
    }   
}
