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
using System.Runtime.Serialization.Json;
using System.Text;
using JPB.Communication.ComBase.Messages;
using JPB.Communication.ComBase.Serializer.Contracts;

namespace JPB.Communication.ComBase.Serializer
{
    public class MessageJsonSerlalizer : IMessageSerializer
    {
        public byte[] SerializeMessage(NetworkMessage a)
        {
            using (var memstream = new MemoryStream())
            {
                var json = new DataContractJsonSerializer(typeof (NetworkMessage));
                json.WriteObject(memstream, a);
                return memstream.ToArray();
            }
        }

        public byte[] SerializeMessageContent(MessageBase mess)
        {
            mess.RecievedAt = DateTime.Now;
            using (var memstream = new MemoryStream())
            {
                var json = new DataContractJsonSerializer(typeof (MessageBase));
                json.WriteObject(memstream, mess);
                return memstream.ToArray();
            }
        }

        public NetworkMessage DeSerializeMessage(byte[] source)
        {
            using (var memstream = new MemoryStream(source))
            {
                var json = new DataContractJsonSerializer(typeof (NetworkMessage));
                return (NetworkMessage) json.ReadObject(memstream);
            }
        }

        public MessageBase DeSerializeMessageContent(byte[] source)
        {
            using (var memstream = new MemoryStream(source))
            {
                var json = new DataContractJsonSerializer(typeof (MessageBase));
                return (MessageBase) json.ReadObject(memstream);
            }
        }

        public string ResolveStringContent(byte[] message)
        {
            return Encoding.ASCII.GetString(message);
        }
    }
}