using System.IO;
using System.Runtime.Serialization;
using System.Text;
using JPB.Communication.ComBase.Serializer.Contracts;
using JPB.Communication.ComBase.Messages;
using JPB.Communication.ComBase;

namespace JPB.Communication.NativeWin.Serilizer
{
    public class NetContractSerializer : IMessageSerializer
    {
        public bool IlMergeSupport { get; set; }

        public byte[] SerializeMessage(NetworkMessage a)
        {
            using (var memst = new MemoryStream())
            {
                NetDataContractSerializer formatter = GetSerializer<NetworkMessage>();
                formatter.Serialize(memst, a);
                return memst.ToArray();
            }
        }

        public byte[] SerializeMessageContent(MessageBase mess)
        {
            using (var memst = new MemoryStream())
            {
                NetDataContractSerializer formatter = GetSerializer<MessageBase>();
                formatter.Serialize(memst, mess);
                return memst.ToArray();
            }
        }

        public NetworkMessage DeSerializeMessage(byte[] source)
        {
            using (var memst = new MemoryStream(source))
            {
                NetDataContractSerializer formatter = GetSerializer<NetworkMessage>();
                var deserialize = (NetworkMessage)formatter.Deserialize(memst);
                return deserialize;
            }
        }

        public MessageBase DeSerializeMessageContent(byte[] source)
        {
            using (var memst = new MemoryStream(source))
            {
                NetDataContractSerializer formatter = GetSerializer<MessageBase>();
                var deserialize = (MessageBase)formatter.Deserialize(memst);
                return deserialize;
            }
        }

        public string ResolveStringContent(byte[] message)
        {
            return Encoding.ASCII.GetString(message);
        }

        private NetDataContractSerializer GetSerializer<T>()
        {
            var serilizer = new NetDataContractSerializer();
            //if (IlMergeSupport)
            //{
            //    serilizer.Binder = new DefaultMessageSerlilizer.IlMergeBinder();
            //}
            return serilizer;
        }
    }
}