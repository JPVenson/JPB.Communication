﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using JPB.Communication.WinRT.combase.Messages;
using JPB.Communication.WinRT.Contracts;

namespace JPB.Communication.WinRT.Serilizer
{
    public class FullXmlSerilizer : IMessageSerializer
    {
        public FullXmlSerilizer()
        {
        }

        public bool IlMergeSupport { get; set; }

        public byte[] SerializeMessage(object a)
        {
            using (var memst = new MemoryStream())
            {
                var formatter = GetSerializer(a.GetType());
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

        public object DeSerializeMessage(byte[] source)
        {
            using (var memst = new MemoryStream(source))
            {
                var formatter = GetSerializer(typeof(MessageBase));
                var deserialize = formatter.Deserialize(memst);
                return deserialize;
            }
        }   

        public string ResolveStringContent(byte[] message)
        {
            return Encoding.ASCII.GetString(message);
        }

        private XmlSerializer GetSerializer(Type main)
        {
            var types = DefaultMessageSerlilizer.IlMergeBinder.GetOptimistics().Select(s => s.Value).ToArray();

            var formatter = new XmlSerializer(null, types);
            formatter.UnknownElement += Formatter_UnknownElement;
            formatter.UnreferencedObject += Formatter_UnreferencedObject;
            formatter.UnknownNode += Formatter_UnknownNode;
            formatter.UnknownAttribute += Formatter_UnknownAttribute;
            return formatter;
        }
    }
}
