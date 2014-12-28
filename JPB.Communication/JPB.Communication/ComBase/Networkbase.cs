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
using System.Linq;
using JPB.Communication.ComBase.Messages;
using JPB.Communication.ComBase.Serializer;
using JPB.Communication.ComBase.Serializer.Contracts;

namespace JPB.Communication.ComBase
{
    /// <summary>
    /// Delegate for Incomming or Outging messages
    /// </summary>
    /// <param name="mess"></param>
    /// <param name="port"></param>
    public delegate void MessageDelegate(MessageBase mess, ushort port);

    /// <summary>
    /// Delegate for Incomming or Outging messages
    /// </summary>
    /// <param name="mess"></param>
    /// <param name="port"></param>
    public delegate void LargeMessageDelegate(LargeMessage mess, ushort port);

    /// <summary>
    /// The base class for Multible Network instances
    /// </summary>
    public abstract class Networkbase
    {
        /// <summary>
        /// 
        /// </summary>
        protected Networkbase()
        {
            Serlilizer = DefaultMessageSerializer;
        }

        /// <summary>
        /// Defines the Port the Instance is working on
        /// </summary>
        public abstract ushort Port { get; internal set; }

        /// <summary>
        /// When some Serlilizaion is requert this Interface will be used
        /// </summary>
        public IMessageSerializer Serlilizer { get; set; }

        /// <summary>
        /// The default Serializer
        /// </summary>
        public static readonly IMessageSerializer DefaultMessageSerializer = new DefaultMessageSerlilizer();

        /// <summary>
        /// A Standart Serializer that compress the message ( good for large MessageBase objects )
        /// </summary>
        public static readonly IMessageSerializer CompressedDefaultMessageSerializer = new BinaryCompressedMessageSerializer();

        /// <summary>
        /// A Standart JSON Serializer
        /// </summary>
        public static readonly IMessageSerializer JsonMessageSerializer = new MessageJsonSerlalizer();

        /// <summary>
        /// A Full XML Serializer
        /// </summary>
        public static readonly IMessageSerializer FullXmlSerializer = new FullXmlSerializer();

        public static event MessageDelegate OnNewItemLoadedSuccess;
        public static event EventHandler<string> OnNewItemLoadedFail;
        /// <summary>
        /// 
        /// </summary>
        public static event EventHandler<TcpMessage> OnIncommingMessage;
        public static event MessageDelegate OnMessageSend;
        public static event LargeMessageDelegate OnNewLargeItemLoadedSuccess;

        protected EventHandler<TcpMessage> IncommingMessage()
        {
            return OnIncommingMessage;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="metaData"></param>
        /// <param name="contendLoaded"></param>
        protected virtual LargeMessage RaiseNewLargeItemLoadedSuccess(MessageBase metaData, Func<Stream> contendLoaded)
        {
            try
            {
                var handler = OnNewLargeItemLoadedSuccess;
                var largeMessage = new LargeMessage(metaData as StreamMetaMessage, contendLoaded);
                if (handler != null)
                    handler(largeMessage, Port);
                return largeMessage;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return null;
            }
        }

        protected virtual void RaiseMessageSended(MessageBase message)
        {
            try
            {
                var handler = OnMessageSend;
                if (handler != null)
                    handler(message, Port);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        protected virtual void RaiseIncommingMessage(TcpMessage strReceived)
        {
            try
            {
                var handler = OnIncommingMessage;
                if (handler != null)
                    handler(this, strReceived);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        protected virtual void RaiseNewItemLoadedFail(string strReceived)
        {
            try
            {
                var handler = OnNewItemLoadedFail;
                if (handler != null)
                    handler(this, strReceived);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        protected virtual void RaiseNewItemLoadedSuccess(MessageBase loadMessageBaseFromBinary)
        {
            try
            {
                var handler = OnNewItemLoadedSuccess;
                if (handler != null)
                    handler(loadMessageBaseFromBinary, Port);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public TcpMessage DeSerialize(byte[] source)
        {
            try
            {
                return this.Serlilizer.DeSerializeMessage(source);
            }
            catch (Exception e)
            {
                return null;
            }
        }

        public byte[] Serialize(TcpMessage a)
        {
            try
            {
                return this.Serlilizer.SerializeMessage(a);
            }
            catch (Exception e)
            {
                return new byte[0];
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="A"></param>
        /// <returns></returns>
        public byte[] SaveMessageBaseAsContent(MessageBase A)
        {
            try
            {
                return this.Serlilizer.SerializeMessageContent(A);
            }
            catch (Exception e)
            {
                return new byte[0];
            }
        }

        public MessageBase LoadMessageBaseFromBinary(byte[] source)
        {
            return this.Serlilizer.DeSerializeMessageContent(source);
        }

        protected TcpMessage Wrap(MessageBase message)
        {
            var mess = new TcpMessage();
            var saveMessageBaseAsBinary = SaveMessageBaseAsContent(message);

            if (!saveMessageBaseAsBinary.Any())
                return null;

            mess.MessageBase = saveMessageBaseAsBinary;
            mess.Reciver = message.Reciver;
            mess.Sender = message.Sender;
            return mess;
        }
    }
}