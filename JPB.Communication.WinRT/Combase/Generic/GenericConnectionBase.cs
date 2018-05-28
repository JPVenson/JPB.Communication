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
using JPB.Communication.WinRT.combase;
using JPB.Communication.WinRT.combase.Messages;
using JPB.Communication.WinRT.combase.Security;
using JPB.Communication.WinRT.Contracts.Intigration;

namespace JPB.Communication.WinRT.Combase.Generic
{
    internal abstract class GenericConnectionBase : ConnectionBase
    {
        public GenericConnectionBase(ISocket sock)
        {
            this._sock = sock;
            _receiveBufferSize = sock.ReceiveBufferSize;
        }

	    public MessageMeta MessageMeta { get; set; }

        private readonly ISocket _sock;
        public event EventHandler EndReceiveInternal;
        protected int _receiveBufferSize;
        public bool IsSharedConnection { get; set; }

        public ISocket Sock
        {
            get
            {
                return _sock;
            }
        }

		protected void ClearBuffer()
	    {
		    MessageParsed = false;
		    MessageMeta = null;
	    }

        public virtual bool BeginReceive(bool CheckCredentials)
		{
			MessageMeta = null;
			ClearBuffer();
			if (CheckCredentials)
            {
                try
                {
                    var credMessage = ReciveCredentials();
                    if (credMessage == null)
                    {
                        Sock.Send(0x00);
                        Sock.Close();
                        Sock.Dispose();
                        return false;
                    }
                    var isAudit = NetworkAuthentificator.Instance.CheckCredentials(credMessage, Sock.RemoteEndPoint.Address.AddressContent, Sock.RemoteEndPoint.Port);
                    if (!isAudit)
                    {
                        Sock.Send(0x00);
                        Sock.Close();
                        Sock.Dispose();
                        return false;
                    }
                    else
                    {
                        Sock.Send(0x01);
                    }
                }
                catch (Exception)
                {
                    Sock.Send(0x00);
                    RaiseEndReceiveInternal();
                    return false;
                }
            }

			MessageMeta = ReciveMetaMessage();
			return MessageMeta != null;
        }

        protected void RaiseEndReceiveInternal()
        {
            var handler = EndReceiveInternal;

            if (handler != null)
            {
	            handler(this, new EventArgs());
            }
        }

        internal LoginMessage ReciveCredentials()
        {
            byte[] maybeLoginMessage = new byte[NetworkAuthentificator.ReceiveBufferSize];
            Sock.Receive(maybeLoginMessage);
            if (maybeLoginMessage[0] == 0x00)
            {
	            return null;
            }

	        return DeSerializeLogin(maybeLoginMessage);
        }

	    internal MessageMeta ReciveMetaMessage()
        {
            byte[] maybeMetaMessage = new byte[MessageMeta.ReceiveBufferSize];
            Sock.Receive(maybeMetaMessage);
            if (maybeMetaMessage.All(e => e == 0x00))
            {
	            return null;
            }
	        return MessageMeta = MessageMeta.FromHeader(maybeMetaMessage);
        }
    }
}