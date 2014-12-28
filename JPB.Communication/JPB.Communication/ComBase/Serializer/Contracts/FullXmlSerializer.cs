using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml.Serialization;
using JPB.Communication.ComBase.Messages;

namespace JPB.Communication.ComBase.Serializer.Contracts
{
    public class FullXmlSerializer : IMessageSerializer
    {
        public byte[] SerializeMessage(TcpMessage a)
        {
            using (var memst = new MemoryStream())
            {
                var formatter = new XmlSerializer(typeof(TcpMessage));
                formatter.Serialize(memst, a);
                return memst.ToArray();
            }
        }

        public byte[] SerializeMessageContent(MessageBase mess)
        {
            using (var memst = new MemoryStream())
            {
                var formatter = new XmlSerializer(typeof(MessageBase), new[]
                {
                    mess.GetType()
                });
                formatter.Serialize(memst, mess);
                return memst.ToArray();
            }
        }

        public TcpMessage DeSerializeMessage(byte[] source)
        {
            using (var memst = new MemoryStream(source))
            {
                var formatter = new XmlSerializer(typeof(TcpMessage));
                var deserialize = (TcpMessage)formatter.Deserialize(memst);
                return deserialize;
            }
        }

        public MessageBase DeSerializeMessageContent(byte[] source)
        {
            using (var memst = new MemoryStream(source))
            {
                var formatter = new XmlSerializer(typeof(MessageBase));
                var deserialize = (MessageBase)formatter.Deserialize(memst);
                return deserialize;
            }
        }

        public string ResolveStringContent(byte[] message)
        {
            throw new System.NotImplementedException();
        }
    }
}