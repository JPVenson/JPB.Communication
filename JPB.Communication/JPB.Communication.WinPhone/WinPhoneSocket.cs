﻿using JPB.Communication.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.NetworkInformation;

namespace JPB.Communication.WinPhone
{
    /// <summary>
    /// 
    /// </summary>
    public class WinPhoneSocket : ISocket
    {
        public WinPhoneSocket()
            : this(new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP))
        {
        }

        public WinPhoneSocket(Socket sock)
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
            _sock.Bind(new System.Net.IPEndPoint(System.Net.IPAddress.Parse(ipEndPoint.Address.Address), ipEndPoint.Port));
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
            var sock = new WinPhoneSocket(endAccept);
            return sock;
        }
    }
}