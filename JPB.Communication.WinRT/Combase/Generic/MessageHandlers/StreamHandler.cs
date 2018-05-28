using System;
using JPB.Communication.WinRT.combase;
using JPB.Communication.WinRT.combase.Messages.Wrapper;
using JPB.Communication.WinRT.Contracts.Intigration;

namespace JPB.Communication.WinRT.Combase.Generic.MessageHandlers
{
	internal class StreamHandler : MessageHandlerBase
	{
		private LargeMessage _largeMessage;

		/// <inheritdoc />
		public StreamHandler(ConnectionBase publisher, int receiveBufferSize) : base(publisher, receiveBufferSize)
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
				if (_largeMessage != null)
				{
					_largeMessage.RaiseLoadCompleted();
				}

				return;
			}

			//does the stream meta data object is submitted and is everything that follows now part of the Stream
			if (_largeMessage != null)
			{
				_largeMessage.InfoLoaded.Write(MemoryBuffer.WriteBuffer, 0, reveived);
			}
			else
			{
				MemoryBuffer.Add(reveived);

				if (MemoryBuffer.DataWritten > MessageMeta.ContentMessageLength)
				{
					var splice = MemoryBuffer.Splice(MessageMeta.ContentMessageLength);
					_largeMessage = _publisher.ParseLargeObject(splice);
					MemoryBuffer.Clear();
					SendAckIfRequested();
					if (_largeMessage == null)
					{
						return;
					}
				}
			}

			Sock.BeginReceive(MemoryBuffer.WriteBuffer, 0,
			MemoryBuffer.WriteBuffer.Length,
			OnBytesReceived, this);
		}
	}
}