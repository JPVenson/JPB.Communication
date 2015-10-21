using System.IO;
using System.Runtime.Serialization;
using System.Text;
using JPB.Communication.ComBase.Messages;
using JPB.Communication.Contracts;

namespace JPB.Communication.WinRT.Local.Serilizer
{
    public class NetContractSerializer : IMessageSerializer
    {
        public bool IlMergeSupport { get; set; }

        public byte[] SerializeMessage(MessageBase a)
        {
            using (var memst = new MemoryStream())
            {
                NetDataContractSerializer formatter = GetSerializer<MessageBase>();
                formatter.Serialize(memst, a);
                return memst.ToArray();
            }
        }


        public MessageBase DeSerializeMessage(byte[] source)
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
            if (IlMergeSupport)
            {
                serilizer.Binder = new DefaultMessageSerlilizer.IlMergeBinder();
            }
            return serilizer;
        }
    }
}