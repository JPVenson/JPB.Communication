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
using JPB.Communication.ComBase.Messages.Wrapper;
using JPB.Communication.Contracts;

namespace JPB.Communication.ComBase.TCP
{
    internal class LargeTcpConnection : TcpConnectionBase, IDisposable
    {
        /// <summary>
        ///     Is used for the Message Content
        /// </summary>
        private readonly InternalMemoryHolder _datarec;

        private readonly ISocket _sock;

        private readonly StreamBuffer _streamData;
        private LargeMessage _metaMessage;

        internal LargeTcpConnection(ISocket s)
        {
            _datarec = new InternalMemoryHolder();
            _streamData = new StreamBuffer();
            _sock = s;
            _datarec.Add(new byte[_sock.ReceiveBufferSize], 0);
        }

        // Call this method to set this connection's Socket up to receive data.

        public bool MetaDataReached { get; set; }

        public bool LastCallWasMeta { get; set; }
        public override ushort Port { get; internal set; }

        public override void BeginReceive()
        {
            byte[] last = _datarec.Last;
            _sock.BeginReceive(
                last, 0,
                last.Length,
                OnBytesReceived,
                this);
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
                rec = _sock.EndReceive(result);
            }
            catch (Exception)
            {
                if (_metaMessage != null)
                    _metaMessage.RaiseLoadCompleted();
                return;
            }

            if (rec == 1)
            {
                //meta data in buffer set MetaDataReached
                MetaDataReached = true;
                LastCallWasMeta = true;
                _sock.Send(new byte[] {0x00});

                //start writing into content Stream
                var bytes = new byte[_sock.ReceiveBufferSize];
                _streamData.Write(bytes);

                if (_sock.Connected)
                {
                    try
                    {
                        _sock.BeginReceive(
                            bytes, 0,
                            bytes.Length,
                            OnBytesReceived,
                            this);
                    }
                    catch (Exception)
                    {
                        //no more data?!
                        Parse(concatBytes(_datarec));
                    }
                }
                else
                {
                    //no more data?!
                    Parse(concatBytes(_datarec));
                }

                return;
            }

            if (rec > 1)
            {
                if (MetaDataReached)
                {
                    if (_metaMessage == null)
                    {
                        _metaMessage = ParseLargeObject(concatBytes(_datarec), () => _streamData.UnderlyingStream);
                    }

                    var bytes = new byte[_sock.ReceiveBufferSize];
                    _streamData.Flush(rec);
                    _streamData.Write(bytes);

                    if (_metaMessage.StreamSize >= _streamData.Length)
                    {
                        _sock.BeginReceive(
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
                    var newbuff = new byte[_sock.ReceiveBufferSize];
                    _datarec.Add(newbuff, rec);
                    _sock.BeginReceive(
                        newbuff, 0,
                        newbuff.Length,
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
                }
            }
        }

        private byte[] concatBytes(InternalMemoryHolder rec)
        {
            byte[] buff = NullRemover(rec.Get());
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
            for (i = 0; i < dataStream.Count(); i++)
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
    }
}