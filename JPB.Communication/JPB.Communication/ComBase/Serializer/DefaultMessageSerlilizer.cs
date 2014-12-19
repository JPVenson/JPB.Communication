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
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Xml.Serialization;
using JPB.Communication.ComBase.Messages;
using JPB.Communication.ComBase.Serializer.Contracts;

namespace JPB.Communication.ComBase.Serializer
{
    /// <summary>
    /// Contains a Mixed Message Serlilizer that Converts the Message as XML and the Content to Binary
    /// </summary>
    public class DefaultMessageSerlilizer : IMessageSerializer
    {

        private Stream getStream(object validator)
        {
            var target = string.Empty;
            Stream stream = null;

            if (validator is Array && (validator as Array).LongLength > (InternalMemoryHolder.MaximumStoreageInMemory * 4))
            {
                target = Path.GetTempFileName();
                stream = new FileStream(target, FileMode.Open);
            }
            else
            {
                stream = new MemoryStream();
            }
            return stream;
        }

        private byte[] getInfo(Stream stream)
        {
            var array = new byte[stream.Length];
            stream.Seek(0, SeekOrigin.Begin);
            stream.Read(array, 0, array.Length);
            return array;
        }

        internal static Encoding Encoding = System.Text.Encoding.UTF8;

        public byte[] SerializeMessage(TcpMessage a)
        {
            using (var stream = getStream(a.MessageBase))
            {
                var serializer = new XmlSerializer(a.GetType());
                serializer.Serialize(stream, a);
                return getInfo(stream);
            }
        }

        public byte[] SerializeMessageContent(MessageBase mess)
        {
            //support for large objects

            using (var stream = getStream(mess.Message))
            {
                var formatter = new BinaryFormatter();
                formatter.FilterLevel = TypeFilterLevel.Full;
                formatter.Serialize(stream, mess);
                return getInfo(stream);
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