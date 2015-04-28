using System;
using System.Threading.Tasks;

namespace JPB.Communication.Contracts.Intigration
{
    /// <summary>
    /// 
    /// </summary>
    public interface ISocket : IDisposable
    {
        bool Connected { get; }
        IPEndPoint LocalEndPoint { get; }
        IPEndPoint RemoteEndPoint { get; }
        int ReceiveBufferSize { get; }
        int ReceiveTimeout { get; set; }
        void Connect(string ipOrHost, ushort port);
        Task ConnectAsync(string ipOrHost, ushort port);

        void Send(byte content);
        int Send(byte[] content);
        int Send(byte[] content, int length, int start);
        void Receive(byte[] content);

        void Close();
        void BeginReceive(byte[] last, int i, int length, Action<IAsyncResult> onBytesReceived, object sender);
        int EndReceive(IAsyncResult result);
        void Bind(IPEndPoint ipEndPoint);
        void Listen(int i);
        void BeginAccept(Action<IAsyncResult> onConnectRequest, ISocket listenerISocket);
        ISocket EndAccept(IAsyncResult result);
    }
}