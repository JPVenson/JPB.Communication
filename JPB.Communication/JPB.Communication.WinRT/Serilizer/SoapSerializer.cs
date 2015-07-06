//using System.IO;
//using System.Runtime.Serialization.Formatters.Soap;
//using System.Text;
//using JPB.Communication.ComBase.Messages;
//using JPB.Communication.ComBase.Serializer.Contracts;

//namespace JPB.Communication.ComBase.Serializer
//{
//    public class SoapSerializer : IMessageSerializer
//    {
//        public bool IlMergeSupport { get; set; }

//        public byte[] SerializeMessage(NetworkMessage a)
//        {
//            using (var memst = new MemoryStream())
//            {
//                SoapFormatter formatter = GetSerializer<NetworkMessage>();
//                formatter.Serialize(memst, a);
//                return memst.ToArray();
//            }
//        }

//        public byte[] SerializeMessageContent(MessageBase mess)
//        {
//            using (var memst = new MemoryStream())
//            {
//                SoapFormatter formatter = GetSerializer<MessageBase>();
//                formatter.Serialize(memst, mess);
//                return memst.ToArray();
//            }
//        }

//        public NetworkMessage DeSerializeMessage(byte[] source)
//        {
//            using (var memst = new MemoryStream(source))
//            {
//                SoapFormatter formatter = GetSerializer<NetworkMessage>();
//                var deserialize = (NetworkMessage) formatter.Deserialize(memst);
//                return deserialize;
//            }
//        }

//        public MessageBase DeSerializeMessageContent(byte[] source)
//        {
//            using (var memst = new MemoryStream(source))
//            {
//                SoapFormatter formatter = GetSerializer<MessageBase>();
//                var deserialize = (MessageBase) formatter.Deserialize(memst);
//                return deserialize;
//            }
//        }

//        public string ResolveStringContent(byte[] message)
//        {
//            return Encoding.ASCII.GetString(message);
//        }

//        private SoapFormatter GetSerializer<T>()
//        {
//            var serilizer = new SoapFormatter();
//            if (IlMergeSupport)
//            {
//                serilizer.Binder = new DefaultMessageSerlilizer.IlMergeBinder();
//            }
//            return serilizer;
//        }
//    }
//}