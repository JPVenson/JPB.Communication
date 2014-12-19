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
using System.Linq;
using System.Net.Sockets;

namespace JPB.Communication.ComBase
{
    internal class DefaultTcpConnection : ConnectionBase, IDisposable
    {
        private readonly Socket sock;
        private InternalMemoryHolder datarec;

        internal DefaultTcpConnection(Socket s)
        {
            datarec = new InternalMemoryHolder();
            sock = s;
            datarec.Add(new byte[sock.ReceiveBufferSize]);
        }

        // Call this method to set this connection's socket up to receive data.
        internal void BeginReceive()
        {
            var last = datarec.Last;
            sock.BeginReceive(
                last, 0,
                last.Length,
                SocketFlags.None,
                OnBytesReceived,
                this);
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
                rec = sock.EndReceive(result);
            }
            catch (Exception)
            {
                Dispose();
                return;
            }

            //to incomming data left
            //try to concat the message
            if (rec == 0)
            {
                var buff = NullRemover(datarec.Get());
                int count = buff.Count();
                var compltearray = new byte[count];
                for (int i = 0; i < count; i++)
                    compltearray.SetValue(buff[i], i);

                Parse(compltearray);
                return;
            }

            //this is Not the end, my only friend the end
            //allocate new memory and add the mem to the Memory holder
            var newbuff = new byte[sock.ReceiveBufferSize];
            datarec.Add(newbuff);

            sock.BeginReceive(
                newbuff, 0,
                newbuff.Length,
                SocketFlags.None,
                OnBytesReceived,
                this);

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
    }
}