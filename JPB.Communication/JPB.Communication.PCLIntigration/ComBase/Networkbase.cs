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
using System.Diagnostics;
using System.IO;
using System.Linq;
using JPB.Communication.ComBase.Messages;
using JPB.Communication.ComBase.Messages.Wrapper;
using JPB.Communication.ComBase.Serializer;
using JPB.Communication.ComBase.Serializer.Contracts;
using JPB.Communication.PCLIntigration.Shared.CrossPlatform;

namespace JPB.Communication.ComBase
{
    /// <summary>
    ///     Delegate for Incomming or Outging messages
    /// </summary>
    /// <param name="mess"></param>
    /// <param name="port"></param>
    public delegate void MessageDelegate(MessageBase mess, ushort port);

    /// <summary>
    ///     Delegate for Incomming or Outging messages
    /// </summary>
    /// <param name="mess"></param>
    /// <param name="port"></param>
    public delegate void LargeMessageDelegate(LargeMessage mess, ushort port);

    /// <summary>
    ///     The base class for Multible Network instances
    /// </summary>
    public abstract class Networkbase
    {
        public const string TraceCategory = "JPB.Communication";

        /// <summary>
        ///     The default Serializer
        /// </summary>
        public static readonly IMessageSerializer DefaultMessageSerializer;

        ///// <summary>
        /////     A Standart Serializer that compress the message ( good for large MessageBase objects )
        ///// </summary>
        //public static readonly IMessageSerializer CompressedDefaultMessageSerializer;

        ////public static readonly IMessageSerializer JsonMessageSerializer;
        ///// <summary>
        /////     A Standart JSON Serializer
        ///// </summary>
        ///// <summary>
        /////     A Standart Soap Serializer
        ///// </summary>
        //public static readonly IMessageSerializer SoapSerializer;

        ///// <summary>
        /////     A Standart NetData Serializer
        ///// </summary>
        //public static readonly IMessageSerializer NetDataSerializer;

        /// <summary>
        ///     A Full XML Serializer
        /// </summary>
        public static readonly IMessageSerializer FullXmlSerializer;

        private IMessageSerializer _serlilizer;

        static Networkbase()
        {
            FullXmlSerializer = new FullXmlSerializer();
            DefaultMessageSerializer = FullXmlSerializer;
            //DefaultMessageSerializer = new DefaultMessageSerlilizer();
            //CompressedDefaultMessageSerializer = new BinaryCompressedMessageSerializer();
            ////JsonMessageSerializer = new MessageJsonSerlalizer();
            //SoapSerializer = new SoapSerializer();
            //NetDataSerializer = new NetContractSerializer();
        }

        /// <summary>
        /// </summary>
        protected Networkbase()
        {
            Serlilizer = DefaultMessageSerializer;
        }

        /// <summary>
        ///     Defines the Port the Instance is working on
        /// </summary>
        public abstract ushort Port { get; internal set; }

        /// <summary>
        ///     When Serlilizaion is request this Interface will be used
        /// </summary>
        public IMessageSerializer Serlilizer
        {
            get { return _serlilizer; }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value", "Value cannot be null");
                }
                _serlilizer = value;
            }
        }

        public static event MessageDelegate OnNewItemLoadedSuccess;
        public static event Action<string> OnNewItemLoadedFail;
        public static event Action<NetworkMessage> OnIncommingMessage;
        public static event MessageDelegate OnMessageSend;
        public static event LargeMessageDelegate OnNewLargeItemLoadedSuccess;

        protected Action<NetworkMessage> IncommingMessageHandler()
        {
            return OnIncommingMessage;
        }

        /// <summary>
        /// </summary>
        /// <param name="metaData"></param>
        /// <param name="contendLoaded"></param>
        protected virtual LargeMessage RaiseNewLargeItemLoadedSuccess(MessageBase metaData, Func<Stream> contendLoaded)
        {
            try
            {
                LargeMessageDelegate handler = OnNewLargeItemLoadedSuccess;
                var largeMessage = new LargeMessage(metaData as StreamMetaMessage, contendLoaded);
                if (handler != null)
                    handler(largeMessage, Port);
                return largeMessage;
            }
            catch (Exception e)
            {
                PclTrace.WriteLine(string.Format("> Networkbase> RaiseNewLargeItemLoadedSuccess>{0}", e), TraceCategory);
                return null;
            }
        }

        protected virtual void RaiseMessageSended(MessageBase message)
        {
            try
            {
                MessageDelegate handler = OnMessageSend;
                if (handler != null)
                    handler(message, Port);
            }
            catch (Exception e)
            {
                PclTrace.WriteLine(string.Format("> Networkbase> RaiseMessageSended>{0}", e), TraceCategory);
            }
        }

        protected virtual void RaiseIncommingMessage(NetworkMessage strReceived)
        {
            try
            {
                var handler = OnIncommingMessage;
                if (handler != null)
                    handler(strReceived);
            }
            catch (Exception e)
            {
                PclTrace.WriteLine(string.Format("> Networkbase> RaiseIncommingMessage>{0}", e), TraceCategory);
            }
        }

        protected virtual void RaiseNewItemLoadedFail(string strReceived)
        {
            try
            {
                var handler = OnNewItemLoadedFail;
                if (handler != null)
                    handler(strReceived);
            }
            catch (Exception e)
            {
                PclTrace.WriteLine(string.Format("> Networkbase> RaiseNewItemLoadedFail>{0}", e), TraceCategory);
            }
        }

        protected virtual void RaiseNewItemLoadedSuccess(MessageBase loadMessageBaseFromBinary)
        {
            try
            {
                MessageDelegate handler = OnNewItemLoadedSuccess;
                if (handler != null)
                    handler(loadMessageBaseFromBinary, Port);
            }
            catch (Exception e)
            {
                PclTrace.WriteLine(string.Format("> Networkbase> RaiseNewItemLoadedSuccess>{0}", e), TraceCategory);
            }
        }

        public NetworkMessage DeSerialize(byte[] source)
        {
            try
            {
                return Serlilizer.DeSerializeMessage(source);
            }
            catch (Exception e)
            {
                return null;
            }
        }

        public byte[] Serialize(NetworkMessage a)
        {
            try
            {
                return Serlilizer.SerializeMessage(a);
            }
            catch (Exception e)
            {
                return new byte[0];
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="A"></param>
        /// <returns></returns>
        public byte[] SaveMessageBaseAsContent(MessageBase A)
        {
            try
            {
                return Serlilizer.SerializeMessageContent(A);
            }
            catch (Exception e)
            {
                return new byte[0];
            }
        }

        public MessageBase LoadMessageBaseFromBinary(byte[] source)
        {
            return Serlilizer.DeSerializeMessageContent(source);
        }

        protected NetworkMessage Wrap(MessageBase message)
        {
            var mess = new NetworkMessage();
            byte[] saveMessageBaseAsBinary = SaveMessageBaseAsContent(message);

            if (!saveMessageBaseAsBinary.Any())
                return null;

            mess.MessageBase = saveMessageBaseAsBinary;
            mess.Reciver = message.Reciver;
            mess.Sender = message.Sender;
            return mess;
        }
    }
}