using System;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.ServiceModel.Channels;
using System.Threading.Tasks;

namespace JPB.Communication.Contracts
{
    /// <summary>
    /// 
    /// </summary>
    public class WinRtSocket : ISocket
    {
        public WinRtSocket()
            : this(new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP))
        {
        }

        public WinRtSocket(Socket sock)
        {
            _sock = sock;
        }

        private readonly Socket _sock;

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public bool Connected
        {
            get
            {
                return _sock.Connected;
            }
        }

        public IPEndPoint LocalEndPoint
        {
            get { return _sock.LocalEndPoint as IPEndPoint; }
        }

        public IPEndPoint RemoteEndPoint
        {
            get { return _sock.RemoteEndPoint as IPEndPoint; }
        }

        public int ReceiveBufferSize
        {
            get { return _sock.ReceiveBufferSize; }
        }

        public int ReceiveTimeout
        {
            get { return _sock.ReceiveTimeout; }
            set { _sock.ReceiveTimeout = value; }
        }

        public async void Connect(string ipOrHost, ushort port)
        {
            await ConnectAsync(ipOrHost, port);
        }

        public Task ConnectAsync(string ipOrHost, ushort port)
        {
            var task = new Task(() => _sock.Connect(ipOrHost, port));
            task.Start();
            return task;
        }

        public void Send(byte content)
        {
            this.Send(new[] { content });
        }

        public int Send(byte[] content)
        {
            return this.Send(content, content.Length, 0);
        }

        public int Send(byte[] content, int length, int start)
        {
            return _sock.Send(content, length, start, SocketFlags.None);
        }

        public void Receive(byte[] content)
        {
            _sock.Receive(content);
        }

        public void Close()
        {
            _sock.Close();
        }

        public void BeginReceive(byte[] last, int i, int length, Action<IAsyncResult> onBytesReceived, object sender)
        {
            _sock.BeginReceive(last, i, length, SocketFlags.None, new AsyncCallback(onBytesReceived), sender);
        }

        public int EndReceive(IAsyncResult result)
        {
            return _sock.EndReceive(result);
        }

        public void Bind(IPEndPoint ipEndPoint)
        {
            _sock.Bind(ipEndPoint);
        }

        public void Listen(int i)
        {
            _sock.Listen(i);
        }

        public void BeginAccept(Action<IAsyncResult> onConnectRequest, ISocket listenerISocket)
        {
            _sock.BeginAccept(new AsyncCallback(onConnectRequest), listenerISocket);
        }

        public ISocket EndAccept(IAsyncResult result)
        {
            var endAccept = _sock.EndAccept(result);
            var sock = new WinRtSocket(endAccept);
            return sock;
        }
    }

    public class WinRtSocketFactory : ISocketFactory
    {
        public ISocket CreateAndConnect(string ipOrHost, ushort port)
        {
            var sock = new WinRtSocket();
            sock.Connect(ipOrHost, port);
            return sock;
        }

        public async Task<ISocket> CreateAndConnectAsync(string ipOrHost, ushort port)
        {
            var sock = new WinRtSocket();
            sock.ConnectAsync(ipOrHost, port);
            return sock;
        }

        public ISocket Create()
        {
            throw new NotImplementedException();
        }

        public Task<ISocket> CreateAsync()
        {
            throw new NotImplementedException();
        }
    }
}