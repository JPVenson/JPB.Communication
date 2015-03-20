using JPB.Communication.Contracts;
using JPB.Communication.PCLIntigration.Contracts;
using System;
using System.IO;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace JPB.Communication.WinRT
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
           var taskCreate =  new Task<ISocket>(() =>
            {
                if (sock == null)
                    sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP);

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
            var endAccept = _sock.EndAccept(result);
            var sock = new WinRtSocket(endAccept);
            return sock;
        }
    }



}