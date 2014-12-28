using System.Net.Sockets;

namespace JPB.Communication.ComBase
{
    internal abstract class TcpConnectionBase : ConnectionBase
    {
        public abstract void BeginReceive();
    }
}