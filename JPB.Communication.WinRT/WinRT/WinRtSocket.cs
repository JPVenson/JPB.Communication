using System;
using System.Net.Sockets;
using System.Threading.Tasks;
using JPB.Communication.Contracts.Factorys;
using JPB.Communication.Contracts.Intigration;

namespace JPB.Communication.WinRT.WinRT
{
    /// <summary>
    /// 
    /// </summary>
    public class WinRtSocket : ISocket
    {
        private WinRtSocket(Socket sock)
        {
            _sock = sock;
        }

        public static Task<ISocket> Create(Socket sock = null)
        {
            var taskCreate = new Task<ISocket>(() =>
            {
                if (sock == null)
                    sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                sock.NoDelay = false;
                sock.LingerState = new LingerOption(true, 30);
                sock.SetIPProtectionLevel(IPProtectionLevel.Unrestricted);
                var rtSock = new WinRtSocket(sock);
                return rtSock;
            });
            taskCreate.Start();
            return taskCreate;
        }

        private readonly Socket _sock;

        public void Dispose()
        {
            _sock.Dispose();
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
            get
            {
                return _sock.LocalEndPoint.AsGeneric();
            }
        }

        public IPEndPoint RemoteEndPoint
        {
            get { return _sock.RemoteEndPoint.AsGeneric(); }
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

        public SharedStateSupport SupportsSharedState
        {
            get
            {
                return SharedStateSupport.Full;
            }
        }

        public async void Connect(string ipOrHost, ushort port)
        {
            await ConnectAsync(ipOrHost, port);
        }

        public Task ConnectAsync(string ipOrHost, ushort port)
        {
            var task = new Task(() =>
            {
                try
                {
                    _sock.Connect(ipOrHost, port);
                }
                catch (Exception)
                {

                }
            });
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

        public int Send(byte[] content, int offset, int length)
        {
            return this.SendInternal(content, offset, length, SocketFlags.None);
        }

        public int SendPartial(byte[] content, int offset, int length)
        {
            return this.SendInternal(content, offset, length, SocketFlags.None);
        }

        public int SendInternal(byte[] content, int offset, int length, SocketFlags flags)
        {
            return _sock.Send(content, offset, length, flags);
        }

        public void Receive(byte[] content)
        {
            ReceiveInternal(content, SocketFlags.None);
        }

        public void ReceivePartial(byte[] content)
        {
            ReceiveInternal(content, SocketFlags.None);
        }

        public void ReceiveInternal(byte[] content, SocketFlags flags)
        {
            _sock.Receive(content, 0, content.Length, SocketFlags.None);
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
            _sock.Bind(new System.Net.IPEndPoint(new System.Net.IPAddress(ipEndPoint.Address.Address), ipEndPoint.Port));
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
            try
            {
                var endAccept = _sock.EndAccept(result);
                var sock = new WinRtSocket(endAccept);
                return sock;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public bool CheckSharedStateSupport()
        {
            return true;
        }
    }
}