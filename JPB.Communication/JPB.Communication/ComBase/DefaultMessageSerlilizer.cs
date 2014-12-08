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
using System.Runtime.Serialization.Formatters;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Xml.Serialization;
using JPB.Communication.ComBase.Messages;

namespace JPB.Communication.ComBase
{
    /// <summary>
    /// Contains a Mixed Message Serlilizer that Converts the Message as XML and the Content to Binary
    /// </summary>
    public class DefaultMessageSerlilizer : IMessageSerializer
    {
        internal static Encoding Encoding = System.Text.Encoding.UTF8;

        public byte[] SerializeMessage(TcpMessage a)
        {
            using (var stream = new MemoryStream())
            {
                var serializer = new XmlSerializer(a.GetType());
                serializer.Serialize(stream, a);
                return stream.ToArray();
            }
        }

        public byte[] SerializeMessageContent(MessageBase A)
        {
            using (var fs = new MemoryStream())
            {
                var formatter = new BinaryFormatter();
                formatter.FilterLevel = TypeFilterLevel.Full;
                formatter.Serialize(fs, A);
                var array = fs.ToArray();
                return array;
            }
        }

        public TcpMessage DeSerializeMessage(byte[] source)
        {
            try
            {
                using (var textReader = new StringReader(ResolveStringContent(source)))
                {
                    var deserializer = new XmlSerializer(typeof(TcpMessage));
                    var tcpMessage = (TcpMessage)deserializer.Deserialize(textReader);
                    return tcpMessage;
                }
            }
            catch (Exception e)
            {
                return null;
            }
        }

        public MessageBase DeSerializeMessageContent(byte[] source)
        {
            using (var memst = new MemoryStream(source))
            {
                var formatter = new BinaryFormatter();
                var deserialize = (MessageBase)formatter.Deserialize(memst);
                return deserialize;
            }
        }

        public string ResolveStringContent(byte[] message)
        {
            return (Encoding.GetString(message, 0, message.Length));
        }
    }
}