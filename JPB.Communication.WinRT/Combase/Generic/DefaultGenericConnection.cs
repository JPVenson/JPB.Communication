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
using JPB.Communication.WinRT.Combase.Generic.MessageHandlers;
using JPB.Communication.WinRT.Contracts.Intigration;

namespace JPB.Communication.WinRT.Combase.Generic
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
			{
				throw new ArgumentException("No sock supplyed please call DefaultGenericConnection(ISocket sock)");
			}

			var cont = base.BeginReceive(checkCred);

			if (!cont)
			{
				return cont;
			}

			IMessageHandler handler = null;
			switch (MessageMeta.ProtocolType)
			{
				case 0:
					handler = new MessageHandler(this, _receiveBufferSize);
					break;
				case 1:
					handler = new StreamHandler(this, _receiveBufferSize);
					break;
			}

			if (handler != null)
			{
				handler.MessageMeta = MessageMeta;
				handler.Sock = Sock;
				handler.BeginRecive();
			}
			return handler != null;
		}

		#region Implementation of IDisposable

		public void Dispose()
		{
		}

		#endregion
	}
}