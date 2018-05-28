using System;
using JPB.Communication.WinRT.combase;
using JPB.Communication.WinRT.Contracts.Intigration;

namespace JPB.Communication.WinRT.Combase.Generic.MessageHandlers
{
	internal interface IMessageHandler
	{
		ISocket Sock { get; set; }
		MessageMeta MessageMeta { get; set; }

		void OnBytesReceived(IAsyncResult result);
		void BeginRecive();
	}
}