using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using JPB.Communication.ComBase.Messages;

namespace JPB.Communication.ComBase
{
    internal class LargeTcpConnection : ConnectionBase, IDisposable, IDefaultTcpConnection
    {
        private readonly Socket _sock;

        /// <summary>
        /// Is used for the Message Content
        /// </summary>
        private readonly InternalMemoryHolder _datarec;
        private readonly StreamBuffer _streamData;

        internal LargeTcpConnection(Socket s)
        {
            _datarec = new InternalMemoryHolder();
            _streamData = new StreamBuffer();
            _sock = s;
            _datarec.Add(new byte[_sock.ReceiveBufferSize], 0);
        }

        internal LargeTcpConnection(NetworkStream s)
        {
            throw new NotImplementedException();
        }

        // Call this method to set this connection's socket up to receive data.
        public void BeginReceive()
        {
            var last = _datarec.Last;
            _sock.BeginReceive(
                last, 0,
                last.Length,
                SocketFlags.None,
                OnBytesReceived,
                this);
        }

        public void Receive()
        {
            throw new NotImplementedException();
        }

        public NetworkStream Stream { get; private set; }

        public bool MetaDataReached { get; set; }

        public bool LastCallWasMeta { get; set; }

        LargeMessage _metaMessage;

        // This is the method that is called whenever the socket receives
        // incoming bytes.
        protected void OnBytesReceived(IAsyncResult result)
        {
            // End the data receiving that the socket has done and get
            // the number of bytes read.
            int rec;
            try
            {
                rec = _sock.EndReceive(result);
            }
            catch (Exception)
            {
                Dispose();
                if (_metaMessage != null)
                    _metaMessage.RaiseLoadCompleted();
                return;
            }

            if (rec == 1)
            {
                //meta data in buffer set MetaDataReached
                MetaDataReached = true;
                LastCallWasMeta = true;
                _sock.Send(new byte[] { 0x00 });

                //start writing into content Stream
                var bytes = new byte[_sock.ReceiveBufferSize];
                _streamData.Write(bytes);
                _sock.BeginReceive(
                    bytes, 0,
                    bytes.Length,
                    SocketFlags.None,
                    OnBytesReceived,
                    this);

                return;
            }

            if (rec > 1)
            {
                if (MetaDataReached)
                {
                    if (_metaMessage == null)
                    {
                        _metaMessage = ParseLargeObject(concatBytes(_datarec), () => this._streamData.UnderlyingStream);
                    }

                    var bytes = new byte[_sock.ReceiveBufferSize];
                    _streamData.Flush(rec);
                    _streamData.Write(bytes);
                    _sock.BeginReceive(
                        bytes, 0,
                        bytes.Length,
                        SocketFlags.None,
                        OnBytesReceived,
                        this);
                }
                else
                {
                    LastCallWasMeta = false;
                    var newbuff = new byte[_sock.ReceiveBufferSize];
                    _datarec.Add(newbuff, rec);
                    _sock.BeginReceive(
                        newbuff, 0,
                        newbuff.Length,
                        SocketFlags.None,
                        OnBytesReceived,
                        this);
                }
            }

            if (rec == 0)
            {
                if (_metaMessage != null)
                {
                    _metaMessage.RaiseLoadCompleted();
                }

                //was there no Magic byte in the last message?
                if (!LastCallWasMeta)
                {
                    Parse(concatBytes(_datarec));
                    return;
                }
            }
        }

        private byte[] concatBytes(InternalMemoryHolder rec)
        {
            var buff = NullRemover(rec.Get());
            int count = buff.Count();
            var compltearray = new byte[count];
            for (int i = 0; i < count; i++)
                compltearray.SetValue(buff[i], i);
            return compltearray;
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
            _datarec.Dispose();
        }

        #endregion

        public override ushort Port { get; internal set; }
    }
}