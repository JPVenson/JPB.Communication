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
using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml.Serialization;
using JPB.Communication.ComBase.Messages;

namespace JPB.Communication.ComBase.Serializer.Contracts
{
    public class FullXmlSerializer : IMessageSerializer
    {
        public byte[] SerializeMessage(NetworkMessage a)
        {
            using (var memst = new MemoryStream())
            {
                var formatter = new XmlSerializer(typeof(NetworkMessage));
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

        public NetworkMessage DeSerializeMessage(byte[] source)
        {
            using (var memst = new MemoryStream(source))
            {
                var formatter = new XmlSerializer(typeof(NetworkMessage));
                var deserialize = (NetworkMessage)formatter.Deserialize(memst);
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