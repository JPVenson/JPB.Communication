using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using JPB.Communication.ComBase;
using JPB.Communication.ComBase.Messages;
using JPB.Communication.ComBase.Serializer.Contracts;
using JPB.Communication.NativeWin.Serilizer;

namespace JPB.Communication.WinRT.Serilizer
{
    public class FullXmlSerilizer : IMessageSerializer
    {
        private DefaultMessageSerlilizer.IlMergeBinder Binder;

        public FullXmlSerilizer(params Type[] extraInfo)
        { 
            Binder = new DefaultMessageSerlilizer.IlMergeBinder();
            foreach (var item in extraInfo)
            {
                Binder.AddOptimistic(item);
            }
        }

        public bool IlMergeSupport { get; set; }

        public byte[] SerializeMessage(NetworkMessage a)
        {
            using (var memst = new MemoryStream())
            {
                var formatter = GetSerializer<NetworkMessage>();
                formatter.Serialize(memst, a);
                return memst.ToArray();
            }
        }
        

        private void Formatter_UnknownAttribute(object sender, XmlAttributeEventArgs e)
        {
        }

        private void Formatter_UnreferencedObject(object sender, UnreferencedObjectEventArgs e)
        {
        }

        private void Formatter_UnknownNode(object sender, XmlNodeEventArgs e)
        {
        }

        private void Formatter_UnknownElement(object sender, XmlElementEventArgs e)
        {
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

        public string ResolveStringContent(byte[] message)
        {
            return Encoding.ASCII.GetString(message);
        }

        private XmlSerializer GetSerializer<T>()
        {
            var formatter = new XmlSerializer(typeof(T), DefaultMessageSerlilizer.IlMergeBinder.GetOptimistics().Select(s => s.Value).ToArray());
            formatter.UnknownElement += Formatter_UnknownElement;
            formatter.UnreferencedObject += Formatter_UnreferencedObject;
            formatter.UnknownNode += Formatter_UnknownNode;
            formatter.UnknownAttribute += Formatter_UnknownAttribute;
            return formatter;
        }
    }
}
