using System;
using System.IO;
using System.Linq;

namespace JPB.Communication.WinRT.combase
{
	public class MessageMeta
	{
		public MessageMeta()
		{
			
		}

		public MessageMeta(bool stream, int serializeLength, bool obtainRecivedRecipt)
		{
			ProtocolType = stream ? 1 : 0;
			ContentMessageLength = serializeLength;
			AwaitReciveBit = obtainRecivedRecipt;
		}

		public int ContentMessageLength { get; set; }
		public int ProtocolType { get; set; }
		public bool AwaitReciveBit { get; set; }

		public const int ReceiveBufferSize = 6;

		public byte[] ToHeader()
		{
			var memStream = new MemoryStream();
			//add message length plus header
			memStream.Write(BitConverter.GetBytes(ContentMessageLength), 0, 4);
			memStream.Write(new byte[] { (byte)ProtocolType }, 0, 1);
			memStream.Write(new byte[] { (byte) (AwaitReciveBit ? 1 : 0) }, 0, 1);
			return memStream.ToArray();
		}

		public static MessageMeta FromHeader(byte[] header)
		{
			var message = new MessageMeta();
			message.ContentMessageLength = BitConverter.ToInt32(header.Take(4).ToArray(), 0);
			message.ProtocolType = header.Skip(4).FirstOrDefault();
			message.AwaitReciveBit = header.Skip(5).FirstOrDefault() == 1;

			return message;
		}
	}
}