using System;
using JPB.Communication.WinRT.combase;
using JPB.Communication.WinRT.Contracts.Intigration;

namespace JPB.Communication.WinRT.Combase.Generic.MessageHandlers
{
	internal abstract class MessageHandlerBase : IMessageHandler
	{
		protected readonly ConnectionBase _publisher;

		public MessageHandlerBase(ConnectionBase publisher, int receiveBufferSize)
		{
			_publisher = publisher;
			MemoryBuffer = new InternalMemoryHolder();
			MemoryBuffer.SetBuffer(receiveBufferSize);
		}

		public ISocket Sock { get; set; }

		public MessageMeta MessageMeta { get; set; }

		public InternalMemoryHolder MemoryBuffer { get; set; }

		public void BeginRecive()
		{
			MemoryBuffer.Clear();
			Sock.BeginReceive(MemoryBuffer.WriteBuffer, 0,
			MemoryBuffer.WriteBuffer.Length,
			OnBytesReceived, this);
		}

		public void SendAckIfRequested()
		{
			if (MessageMeta.AwaitReciveBit)
			{
				Sock.Send(0x01);
			}
		}

		public abstract void OnBytesReceived(IAsyncResult obj);
	}

	internal class MessageHandler : MessageHandlerBase
	{
		/// <inheritdoc />
		public MessageHandler(ConnectionBase publisher, int receiveBufferSize) : base(publisher, receiveBufferSize)
		{
		}

		public override void OnBytesReceived(IAsyncResult result)
		{
			// End the data receiving that the Socket has done and get
			// the number of bytes read.
			int reveived;
			try
			{
				reveived = Sock.EndReceive(result);
			}
			catch (Exception)
			{
				reveived = -1;
			}

			if (reveived == 0)
			{
				MemoryBuffer.Dispose();
				return;
			}

			MemoryBuffer.Add(reveived);

			if (MemoryBuffer.DataWritten >= MessageMeta.ContentMessageLength)
			{
				var splice = MemoryBuffer.Splice(MessageMeta.ContentMessageLength);
				_publisher.ParseAndPublish(splice, Sock);
				SendAckIfRequested();
			}

			Sock.BeginReceive(MemoryBuffer.WriteBuffer, 0,
			MemoryBuffer.WriteBuffer.Length,
			OnBytesReceived, this);
		}
	}
}