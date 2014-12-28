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
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters;
using System.Runtime.Serialization.Formatters.Binary;
using System.ServiceModel.Channels;
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
        //IL MERGE OR RENAMING SUPPORT

        private sealed class IlMergeBinder : SerializationBinder
        {
            static IlMergeBinder()
            {
                TypnameToType = new Dictionary<string, Type>();
            }

            public static Dictionary<string, Type> TypnameToType { get; set; }
            public readonly string MessageBaseTypeName = typeof(MessageBase).FullName;
            public static readonly List<AssemblyName> AdditionalLookups = new List<AssemblyName>(); 

            public override Type BindToType(string assemblyName, string typeName)
            {
                //Native support for MessageBase
                if (MessageBaseTypeName == typeName)
                {
                    return typeof(MessageBase);
                }

                if (TypnameToType.ContainsKey(typeName))
                {
                    return TypnameToType[typeName];
                }
                var callingAssembly = Assembly.GetEntryAssembly();
                foreach (var referencedAssembly in callingAssembly.GetReferencedAssemblies().Concat(AdditionalLookups))
                {
                    var assembly = Assembly.Load(referencedAssembly);
                    var type = assembly.GetType(typeName);
                    if (type != null)
                    {
                        TypnameToType.Add(typeName, type);
                        return type;
                    }
                }

                return null;
            }

            public void AddOptimistic(Type type)
            {
                var fullName = type.FullName;
                if (!TypnameToType.ContainsKey(fullName) && !fullName.Contains("System") && !fullName.Contains("mscorlib"))
                {
                    TypnameToType.Add(type.FullName, type);
                }
            }
        }

        public bool IlMergeSupport { get; set; }
        IlMergeBinder Binder { get; set; }

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

        private BinaryFormatter CreateFormatter()
        {
            var binaryFormatter = new BinaryFormatter()
            {
                AssemblyFormat = FormatterAssemblyStyle.Simple,
                FilterLevel = TypeFilterLevel.Low,
                TypeFormat = FormatterTypeStyle.TypesWhenNeeded
            };

            return binaryFormatter;
        }

        public byte[] SerializeMessageContent(MessageBase mess)
        {
            if (IlMergeSupport)
            {
                //what goes out maybe comes again in
                Binder.AddOptimistic(mess.Message.GetType());
                Binder.AddOptimistic(mess.InfoState.GetType());
            }

            //support for large objects
            using (var stream = getStream(mess.Message))
            {
                CreateFormatter().Serialize(stream, mess);
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
                var binaryFormatter = CreateFormatter();
                if (IlMergeSupport)
                {
                    binaryFormatter.Binder = new IlMergeBinder();
                }
                var deserialize = (MessageBase)binaryFormatter.Deserialize(memst);
                return deserialize;
            }
        }

        public string ResolveStringContent(byte[] message)
        {
            return (Encoding.GetString(message, 0, message.Length));
        }
    }
}