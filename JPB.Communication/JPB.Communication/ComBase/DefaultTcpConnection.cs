/*
 Created by Jean-Pierre Bachmann
 Visit my GitHub page at:
 
 https://github.com/JPVenson/

 Please respect the Code and Work of other Programers an Read the license carefully

 GNU AFFERO GENERAL PUBLIC LICENSE
                       Version 3, 19 November 2007

 Copyright (C) 2007 Free Software Foundation, Inc. <http://fsf.org/>
 Everyone is permitted to copy and distribute verbatim copies
 of this license document, but changing it is not allowed.

 READ THE FULL LICENSE AT:

 https://github.com/JPVenson/JPB.Communication/blob/master/LICENSE
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Threading;

namespace JPB.Communication.ComBase
{
    internal class DefaultTcpConnection : ConnectionBase, IDisposable, IDefaultTcpConnection
    {
        private readonly NetworkStream _stream;
        private readonly Socket sock;
        private InternalMemoryHolder datarec;
        readonly int _receiveBufferSize;

        internal DefaultTcpConnection(Socket s)
        {
            datarec = new InternalMemoryHolder();
            sock = s;
            _receiveBufferSize = sock.ReceiveBufferSize;
            _stream = new NetworkStream(sock, FileAccess.ReadWrite, true);
        }

        //internal DefaultTcpConnection(int receiveBufferSize, NetworkStream stream)
        //{
        //    datarec = new InternalMemoryHolder();
        //    _receiveBufferSize = receiveBufferSize;
        //    datarec.Add(new byte[receiveBufferSize]);
        //    _stream = stream;
        //}

        // Call this method to set this connection's socket up to receive data.
        public void BeginReceive()
        {
            if (sock == null)
                throw new ArgumentException("No sock supplyed please call DefaultTcpConnection(NetworkStream stream)");
            datarec.Add(new byte[_receiveBufferSize]);
            sock.BeginReceive(datarec.Last, 0,
                datarec.Last.Length,
                SocketFlags.None,
                OnBytesReceived,
                this);


            //sock.BeginReceive(
            //    last, 0,
            //    last.Length,
            //    SocketFlags.None,
            //    OnBytesReceived,
            //    this);
        }

        private bool HandleRec(int rec)
        {
            //0 is: No more Data
            //1 is: Part Complted
            //<1 is: Content


            //to incomming data left
            //try to concat the message
            if (rec == 0 || rec == 1)
            {
                sock.Send(new byte[] { });
                //Stream.Write(new byte[] { 0x00 }, 0, 1);
                var buff = NullRemover(datarec.Get());
                int count = buff.Count();
                var compltearray = new byte[count];
                for (int i = 0; i < count; i++)
                    compltearray.SetValue(buff[i], i);
                Parse(compltearray);
                return true;
            }
            return false;
        }

        // This is the method that is called whenever the socket receives
        // incoming bytes.
        protected void OnBytesReceived(IAsyncResult result)
        {
            // End the data receiving that the socket has done and get
            // the number of bytes read.
            int rec;
            try
            {
                SocketError errorCode;
                rec = sock.EndReceive(result, out errorCode);
            }
            catch (Exception)
            {
                rec = -1;
            }

            try
            {
                if (!HandleRec(rec) && rec > -1)
                {
                    //this is Not the end, my only friend the end
                    //allocate new memory and add the mem to the Memory holder


                    datarec.Add(new byte[_receiveBufferSize]);
                    sock.BeginReceive(datarec.Last, 0,
                        datarec.Last.Length, SocketFlags.None,
                        OnBytesReceived,
                        this);

                    //datarec.Add(new byte[_receiveBufferSize]);
                    //Stream.BeginRead(datarec.Last, 0,
                    //    datarec.Last.Length,
                    //    OnBytesReceived,
                    //    this);
                }
                else
                {
                    datarec.Clear();
                    datarec.Add(new byte[_receiveBufferSize]);
                    if (sock.Connected)
                    {
                        try
                        {
                            sock.BeginReceive(datarec.Last, 0,
                                datarec.Last.Length, SocketFlags.None,
                                OnBytesReceived,
                                this);
                        }
                        catch (Exception)
                        {
                            Dispose();
                        }
                    }

                    //Stream.BeginRead(datarec.Last, 0,
                    //    datarec.Last.Length,
                    //    OnBytesReceived,
                    //    this);
                }
            }
            catch (Exception)
            {
                if (!sock.Connected)
                {
                    Dispose();
                }

                datarec.Clear();
                datarec.Add(new byte[_receiveBufferSize]);
                try
                {
                    sock.BeginReceive(datarec.Last, 0,
                        datarec.Last.Length, SocketFlags.None,
                        OnBytesReceived,
                        this);
                }
                catch (Exception)
                {
                    Dispose();
                }
            }
        }

        private byte[] NullRemover(byte[] dataStream)
        {
            int i;
            var temp = new List<byte>();
            for (i = 0; i < dataStream.Count() - 1; i++)
            {
                if (dataStream[i] == 0x00) continue;
                temp.Add(dataStream[i]);
            }
            return temp.ToArray();
        }

        #region Implementation of IDisposable

        public void Dispose()
        {
            datarec.Dispose();
        }

        #endregion

        public override ushort Port { get; internal set; }

        public NetworkStream Stream
        {
            get { return _stream; }
        }
    }
}