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
using JPB.Communication.Contracts.Intigration;

namespace JPB.Communication.ComBase.Generic
{
    internal class DefaultGenericConnection : GenericConnectionBase, IDisposable
    {
        public DefaultGenericConnection(ISocket sock) : base(sock)
        {

        }
        public override ushort Port { get; internal set; }

        // Call this method to set this connection's Socket up to receive data.
        public override bool BeginReceive(bool checkCred)
        {
            if (Sock == null)
                throw new ArgumentException("No sock supplyed please call DefaultGenericConnection(ISocket sock)");

            var cont = base.BeginReceive(checkCred);

            if (!cont)
                return cont;

            _datarec.Add(new byte[_receiveBufferSize], 0);
            Sock.BeginReceive(_datarec.Last, 0,
                _datarec.Last.Length,
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
                rec = -1;
            }

            try
            {
                var dataMode = HandleRec(rec);
                if (dataMode == HandeldMode.NoDateAvailble)
                    return;

                if (dataMode == HandeldMode.MaybeMoreData && dataMode != HandeldMode.Exception)
                {
                    //this is Not the end, my only friend the end
                    //allocate new memory and add the mem to the Memory holder
                    _datarec.Add(new byte[_receiveBufferSize], rec);
                    Sock.BeginReceive(_datarec.Last, 0,
                        _datarec.Last.Length,
                        OnBytesReceived,
                        this);
                }
                else
                {
                    _datarec.Clear();
                    _datarec.Add(new byte[_receiveBufferSize], 0);
                    if (Sock.Connected)
                    {
                        try
                        {
                            Sock.BeginReceive(_datarec.Last, 0,
                                _datarec.Last.Length,
                                OnBytesReceived,
                                this);
                        }
                        catch (Exception)
                        {
                            Dispose();
                        }
                    }
                }
            }
            catch (Exception)
            {
                if (!Sock.Connected)
                {
                    Dispose();
                }

                _datarec.Clear();
                _datarec.Add(new byte[_receiveBufferSize], 0);
                try
                {
                    Sock.BeginReceive(_datarec.Last, 0,
                        _datarec.Last.Length,
                        OnBytesReceived,
                        this);
                }
                catch (Exception)
                {
                    Dispose();
                }
            }
        }

        #region Implementation of IDisposable

        public void Dispose()
        {
            _datarec.Dispose();
        }

        #endregion
    }
}