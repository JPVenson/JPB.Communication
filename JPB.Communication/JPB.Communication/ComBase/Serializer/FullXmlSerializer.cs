/*
 Created by Jean-Pierre Bachmann
 Visit my GitHub page at:
 
 https://github.com/JPVenson/

 Please respect the Code and Work of other Programers an Read the license carefully

 GNU AFFERO GENERAL PUBLIC LICENSE
                       Version 3, 19 November 2007

 Copyright (C) 2007 Free Software Foundation, Inc. <http://fsf.org/>
 Everyone is permitted to copy and distribute verbatim copies
 of this license document, but changing it is not allowed.

 READ THE FULL LICENSE AT:

 https://github.com/JPVenson/JPB.Communication/blob/master/LICENSE
 */

using System.IO;
using System.Text;
using System.Xml.Serialization;
using JPB.Communication.ComBase.Messages;
using JPB.Communication.ComBase.Serializer.Contracts;

namespace JPB.Communication.ComBase.Serializer
{
    public class FullXmlSerializer : IMessageSerializer
    {
        public bool IlMergeSupport { get; set; }

        public byte[] SerializeMessage(NetworkMessage a)
        {
            using (var memst = new MemoryStream())
            {
                XmlSerializer formatter = GetSerializer<NetworkMessage>();
                formatter.Serialize(memst, a);
                return memst.ToArray();
            }
        }

        public byte[] SerializeMessageContent(MessageBase mess)
        {
            using (var memst = new MemoryStream())
            {
                XmlSerializer formatter = GetSerializer<MessageBase>();
                formatter.Serialize(memst, mess);
                return memst.ToArray();
            }
        }

        public NetworkMessage DeSerializeMessage(byte[] source)
        {
            using (var memst = new MemoryStream(source))
            {
                XmlSerializer formatter = GetSerializer<NetworkMessage>();
                var deserialize = (NetworkMessage) formatter.Deserialize(memst);
                return deserialize;
            }
        }

        public MessageBase DeSerializeMessageContent(byte[] source)
        {
            using (var memst = new MemoryStream(source))
            {
                XmlSerializer formatter = GetSerializer<MessageBase>();
                var deserialize = (MessageBase) formatter.Deserialize(memst);
                return deserialize;
            }
        }

        public string ResolveStringContent(byte[] message)
        {
            return Encoding.ASCII.GetString(message);
        }

        private XmlSerializer GetSerializer<T>()
        {
            var serilizer = new XmlSerializer(typeof (T));
            if (IlMergeSupport)
            {
                //TODO IMPLIEMNT SerializationBinder
            }
            return serilizer;
        }
    }
}