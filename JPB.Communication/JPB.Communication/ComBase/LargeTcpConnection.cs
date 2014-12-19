using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using JPB.Communication.ComBase.Messages;

namespace JPB.Communication.ComBase
{
    internal class LargeTcpConnection : ConnectionBase, IDisposable
    {
        private readonly Socket _sock;

        /// <summary>
        /// Is used for the Message Content
        /// </summary>
        private readonly InternalMemoryHolder _datarec;
        private readonly InternalMemoryHolder _streamData;

        internal LargeTcpConnection(Socket s)
        {
            _datarec = new InternalMemoryHolder();
            _streamData = new InternalMemoryHolder
            {
                ForceSharedMem = true
            };
            _sock = s;
            _datarec.Add(new byte[_sock.ReceiveBufferSize]);
        }

        // Call this method to set this connection's socket up to receive data.
        internal void BeginReceive()
        {
            var last = _datarec.Last;
            _sock.BeginReceive(
                last, 0,
                last.Length,
                SocketFlags.None,
                OnBytesReceived,
                this);
        }

        public bool LastDataReached { get; set; }
        public bool MessageOut { get; set; }

        LargeMessage metaMessage;

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
                if (metaMessage != null)
                    metaMessage.RaiseLoadCompleted();
                return;
            }

            //to incomming data left
            //try to concat the message

            //rec is empty means we are at the end of both message
            if (rec == 0 || !_sock.Connected)
            {
                if (metaMessage != null)
                {
                    metaMessage.RaiseLoadCompleted();
                }
                else
                {
                    var buff = NullRemover(_datarec.Get());
                    int count = buff.Count();
                    var compltearray = new byte[count];
                    for (int i = 0; i < count; i++)
                        compltearray.SetValue(buff[i], i);
                    //No folloring data ... push the message as is
                    Parse(compltearray);
                }
                LastDataReached = true;
            }
            else if (LastDataReached)
            {
                if (!MessageOut)
                {
                    //no message was out ... push Normal message or message will Content callback

                    //we are at the end of the MetaData in anyway
                    var buff = NullRemover(_datarec.Get());
                    int count = buff.Count();
                    var compltearray = new byte[count];
                    for (int i = 0; i < count; i++)
                        compltearray.SetValue(buff[i], i);

                    if (rec == 0)
                    {
                        //No folloring data ... push the message as is
                        Parse(compltearray);
                    }
                    else
                    {
                        //data availbile. Push Large message
                        metaMessage = ParseLargeObject(compltearray, () =>
                        {
                            return this._streamData.GetStream();
                        });
                        MessageOut = true;
                    }
                }

                //We got the Meta Infos, store all other data inside the FS
                var bytes = new byte[_sock.ReceiveBufferSize];
                _streamData.Add(bytes);

                _sock.BeginReceive(
                    bytes, 0,
                    bytes.Length,
                    SocketFlags.None,
                    OnBytesReceived,
                    this);
            }
            //Part with only one byte indicates end of Part
            else if (rec == 1)
            {
                //this indicates that we are at the end of the first message.
                LastDataReached = true;

                // message was complete ... send now more if availbile
                _sock.Send(new byte[] { 0x01 });

                //We got the Meta Infos, store all other data inside the FS
                //wait ... are there other data?
                if (_sock.Connected)
                {
                    var bytes = new byte[_sock.ReceiveBufferSize];
                    _streamData.Add(bytes);
                    _sock.BeginReceive(
                         bytes, 0,
                         bytes.Length,
                         SocketFlags.None,
                         OnBytesReceived,
                         this);
                }
            }
            else
            {
                //this is Not the end, my only friend the end
                //allocate new memory and add the mem to the Memory holder
                var newbuff = new byte[_sock.ReceiveBufferSize];
                _datarec.Add(newbuff);

                _sock.BeginReceive(
                    newbuff, 0,
                    newbuff.Length,
                    SocketFlags.None,
                    OnBytesReceived,
                    this);
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
            _datarec.Dispose();
        }

        #endregion

        public override ushort Port { get; internal set; }
    }
}