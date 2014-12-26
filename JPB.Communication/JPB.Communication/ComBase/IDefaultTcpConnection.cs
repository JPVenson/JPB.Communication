using System.Net.Sockets;

namespace JPB.Communication.ComBase
{
    internal interface IDefaultTcpConnection
    {
        void BeginReceive();
        //void Receive();

        NetworkStream Stream { get; }
    }
}