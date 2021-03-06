﻿/*
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
using System.Runtime.Serialization.Formatters.Binary;
using JPB.Communication.WinRT.Contracts;

namespace JPB.Communication.WinRT.Serilizer
{
    /// <summary>
    ///     Uses the Default Serlilizer but compresses the output by using the GZipStream
    /// </summary>
    public class BinaryCompressedMessageSerializer : IMessageSerializer
    {
        private bool _ilMergeSupport;

        public BinaryCompressedMessageSerializer()
        {
        }             

        public byte[] SerializeMessage(object a)
        {
            var serilizer = new BinaryFormatter();
            serilizer.Binder = new DefaultMessageSerlilizer.IlMergeBinder();
            using (var memstream = new MemoryStream())
            {
                serilizer.Serialize(memstream, a);
                return Compress(memstream.ToArray());
            }
        }

        public object DeSerializeMessage(byte[] source)
        {
            source = DeCompress(source);
            var serilizer = new BinaryFormatter();
            serilizer.Binder = new DefaultMessageSerlilizer.IlMergeBinder();
            using (var memstream = new MemoryStream(source))
            {
                return serilizer.Deserialize(memstream);
            }
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
            using (var stream = new GZipStream(new MemoryStream(raw), CompressionMode.Decompress, false))
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
                    var arr = memory.ToArray();
                    if (arr.Length == 0)
                        return raw;
                    return arr;
                }
            }
        }
    }
}