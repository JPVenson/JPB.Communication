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
using System.IO.Compression;
using System.Text;
using JPB.Communication.ComBase.Messages;
using JPB.Communication.ComBase.Serializer.Contracts;

namespace JPB.Communication.ComBase.Serializer
{
    /// <summary>
    ///     Uses the Default Serlilizer but compresses the output by using the GZipStream
    /// </summary>
    public class BinaryCompressedMessageSerializer : IMessageSerializer
    {
        private bool _ilMergeSupport;

        static BinaryCompressedMessageSerializer()
        {
            DefaultMessageSerlilizer = new DefaultMessageSerlilizer();
        }

        public bool IlMergeSupport
        {
            get { return DefaultMessageSerlilizer.IlMergeSupport; }
            set { DefaultMessageSerlilizer.IlMergeSupport = value; }
        }

        public static DefaultMessageSerlilizer DefaultMessageSerlilizer { get; set; }

        public byte[] SerializeMessage(NetworkMessage a)
        {
            byte[] mess = DefaultMessageSerlilizer.SerializeMessage(a);
            return Compress(mess);
        }

        public byte[] SerializeMessageContent(MessageBase mess)
        {
            return DefaultMessageSerlilizer.SerializeMessageContent(mess);
        }

        public NetworkMessage DeSerializeMessage(byte[] source)
        {
            source = DeCompress(source);
            return DefaultMessageSerlilizer.DeSerializeMessage(source);
        }

        public MessageBase DeSerializeMessageContent(byte[] source)
        {
            return DefaultMessageSerlilizer.DeSerializeMessageContent(source);
        }

        public string ResolveStringContent(byte[] message)
        {
            return Encoding.ASCII.GetString(message);
        }

        /// <summary>
        ///     Compresses byte array to new byte array.
        /// </summary>
        public static byte[] Compress(byte[] raw)
        {
            using (var memory = new MemoryStream())
            {
                using (var gzip = new GZipStream(memory, CompressionMode.Compress, true))
                {
                    gzip.Write(raw, 0, raw.Length);
                }
                return memory.ToArray();
            }
        }

        /// <summary>
        ///     UnCompresses byte array to new byte array.
        /// </summary>
        public static byte[] DeCompress(byte[] raw)
        {
            using (var stream = new GZipStream(new MemoryStream(raw), CompressionMode.Decompress))
            {
                const int size = 4096;
                var buffer = new byte[size];
                using (var memory = new MemoryStream())
                {
                    int count = 0;
                    do
                    {
                        count = stream.Read(buffer, 0, size);
                        if (count > 0)
                        {
                            memory.Write(buffer, 0, count);
                        }
                    } while (count > 0);
                    return memory.ToArray();
                }
            }
        }
    }
}