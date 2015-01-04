using System.IO;
using System.Runtime.Serialization;
using System.Text;
using JPB.Communication.ComBase.Messages;
using JPB.Communication.ComBase.Serializer.Contracts;

namespace JPB.Communication.ComBase.Serializer
{
    class NetContractSerializer : IMessageSerializer
    {
        public bool IlMergeSupport { get; set; }

        private NetDataContractSerializer GetSerializer<T>()
        {
            var serilizer = new NetDataContractSerializer();
            if (IlMergeSupport)
            {
                serilizer.Binder = new DefaultMessageSerlilizer.IlMergeBinder();
            }
            return serilizer;
        }

        public byte[] SerializeMessage(NetworkMessage a)
        {
            using (var memst = new MemoryStream())
            {
                var formatter = GetSerializer<NetworkMessage>();
                formatter.Serialize(memst, a);
                return memst.ToArray();
            }
        }

        public byte[] SerializeMessageContent(MessageBase mess)
        {
            using (var memst = new MemoryStream())
            {
                var formatter = GetSerializer<MessageBase>();
                formatter.Serialize(memst, mess);
                return memst.ToArray();
            }
        }

        public NetworkMessage DeSerializeMessage(byte[] source)
        {
            using (var memst = new MemoryStream(source))
            {
                var formatter = GetSerializer<NetworkMessage>();
                var deserialize = (NetworkMessage)formatter.Deserialize(memst);
                return deserialize;
            }
        }

        public MessageBase DeSerializeMessageContent(byte[] source)
        {
            using (var memst = new MemoryStream(source))
            {
                var formatter = GetSerializer<MessageBase>();
                var deserialize = (MessageBase)formatter.Deserialize(memst);
                return deserialize;
            }
        }

        public string ResolveStringContent(byte[] message)
        {
            return Encoding.ASCII.GetString(message);
        }
    }
}