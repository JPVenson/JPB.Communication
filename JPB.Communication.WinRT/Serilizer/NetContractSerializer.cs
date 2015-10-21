using System.IO;
using System.Runtime.Serialization;
using System.Text;
using JPB.Communication.ComBase.Messages;
using JPB.Communication.Contracts;
using System;

namespace JPB.Communication.WinRT.Serilizer
{
    public class NetContractSerializer : IMessageSerializer
    {

        public NetContractSerializer()
        {

        }

        public NetContractSerializer(Type[] expectedTypes)
        {
            foreach (var item in expectedTypes)
            {
                DefaultMessageSerlilizer.IlMergeBinder.GlobalAddOptimistic(item);
            }

            IlMergeSupport = true;
        }

        public bool IlMergeSupport { get; set; }

        public byte[] SerializeMessage(object a)
        {
            using (var memst = new MemoryStream())
            {
                NetDataContractSerializer formatter = GetSerializer();
                formatter.Serialize(memst, a);
                return memst.ToArray();
            }
        }


        public object DeSerializeMessage(byte[] source)
        {
            using (var memst = new MemoryStream(source))
            {
                NetDataContractSerializer formatter = GetSerializer();
                var deserialize = formatter.Deserialize(memst);
                return deserialize;
            }
        }
           

        public string ResolveStringContent(byte[] message)
        {
            return Encoding.ASCII.GetString(message);
        }

        private NetDataContractSerializer GetSerializer()
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