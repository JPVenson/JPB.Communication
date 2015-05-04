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
using System.Linq;
using System.Threading;
using JPB.Communication.ComBase.Messages.Wrapper;
using JPB.Communication.Contracts.Intigration;

namespace JPB.Communication.ComBase.TCP
{
    internal class LargeTcpConnection : TcpConnectionBase, IDisposable
    {
        private readonly StreamBuffer _streamData;
        private LargeMessage _metaMessage;

        internal LargeTcpConnection(ISocket s) : base(s)
        {
            _streamData = new StreamBuffer();
            _datarec.Add(new byte[Sock.ReceiveBufferSize], 0);
        }

        // Call this method to set this connection's Socket up to receive data.

        public bool MetaDataReached { get; set; }

        public bool LastCallWasMeta { get; set; }
        public override ushort Port { get; internal set; }

        public override bool BeginReceive(bool checkCred)
        {
            var cont = base.BeginReceive(checkCred);

            if (!cont)
                return cont;

            byte[] last = _datarec.Last;
            Sock.BeginReceive(
                last, 0,
                last.Length,
                OnBytesReceived,
                this);
            return true;
        }

        // This is the method that is called whenever the Socket receives
        // incoming bytes.
        protected void OnBytesReceived(IAsyncResult result)
        {
            // End the data receiving that the Socket has done and get
            // the number of bytes read.
            int rec;
            try
            {
                rec = Sock.EndReceive(result);
            }
            catch (Exception)
            {
                if (_metaMessage != null)
                    _metaMessage.RaiseLoadCompleted();
                return;
            }

            if (rec > 1)
            {
                if (!MetaDataReached)
                {
                    if (rec < _receiveBufferSize)
                    {
                        MetaDataReached = true;
                        //Thread.Sleep(150);
                        //Thread.Yield();
                        Sock.Send(0x01);
                    }
                    else
                    {
                        var newbuff = new byte[Sock.ReceiveBufferSize];
                        _datarec.Add(newbuff, rec);
                        Sock.BeginReceive(
                            newbuff, 0,
                            newbuff.Length,
                            OnBytesReceived,
                            this);
                        return;
                    }
                }

                if (MetaDataReached)
                {
                    if (_metaMessage == null)
                    {
                        _metaMessage = ParseLargeObject(concatBytes(_datarec, rec), () => _streamData.UnderlyingStream);
                    }

                    var bytes = new byte[Sock.ReceiveBufferSize];
                    _streamData.Flush(rec);
                    _streamData.Write(bytes);

                    if (_metaMessage.StreamSize >= _streamData.Length)
                    {
                        Sock.BeginReceive(
                            bytes, 0,
                            bytes.Length,
                            OnBytesReceived,
                            this);
                    }
                    else
                    {
                        _metaMessage.RaiseLoadCompleted();
                    }
                }
                else
                {
                    LastCallWasMeta = false;
                    var newbuff = new byte[Sock.ReceiveBufferSize];
                    _datarec.Add(newbuff, rec);
                    Sock.BeginReceive(
                        newbuff, 0,
                        newbuff.Length,
                        OnBytesReceived,
                        this);
                    return;
                }
            }

            if (rec == 0)
            {
                _streamData.Wait();
                if (_metaMessage != null)
                {
                    _metaMessage.RaiseLoadCompleted();
                }

                //was there no Magic byte in the last message?
                if (!LastCallWasMeta)
                {
                    Parse(concatBytes(_datarec, rec));
                }
            }
        }

        private byte[] concatBytes(InternalMemoryHolder rec, int adjust)
        {
            byte[] buff = rec.Get(adjust);
            int count = buff.Count();
            var compltearray = new byte[count];
            for (int i = 0; i < count; i++)
                compltearray.SetValue(buff[i], i);
            return compltearray;
        }

        #region Implementation of IDisposable

        public void Dispose()
        {
            _datarec.Dispose();
        }

        #endregion
    }
}