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
using System.Linq;
using JPB.Communication.ComBase.Messages;
using JPB.Communication.ComBase.Serializer;
using JPB.Communication.ComBase.Serializer.Contracts;

namespace JPB.Communication.ComBase
{
    public delegate void MessageDelegate(MessageBase mess, ushort port);

    public abstract class Networkbase
    {
        protected Networkbase()
        {
            Serlilizer = DefaultMessageSerializer;
        }

        public abstract ushort Port { get; internal set; }

        public IMessageSerializer Serlilizer { get; set; }

        public static readonly IMessageSerializer DefaultMessageSerializer = new DefaultMessageSerlilizer();
        public static readonly IMessageSerializer CompressedDefaultMessageSerializer = new BinaryCompressedMessageSerializer();
        public static readonly IMessageSerializer JsonMessageSerializer = new MessageJsonSerlalizer();

        public static event MessageDelegate OnNewItemLoadedSuccess;
        public static event EventHandler<string> OnNewItemLoadedFail;
        public static event EventHandler<TcpMessage> OnIncommingMessage;
        public static event MessageDelegate OnMessageSend;

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

        public byte[] SaveMessageBaseAsBinary(MessageBase A)
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
            var saveMessageBaseAsBinary = SaveMessageBaseAsBinary(message);

            if (!saveMessageBaseAsBinary.Any())
                return null;

            mess.MessageBase = saveMessageBaseAsBinary;
            mess.Reciver = message.Reciver;
            mess.Sender = message.Sender;
            return mess;
        }
    }
}