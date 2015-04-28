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
using JPB.Communication.ComBase.Messages.Wrapper;
using JPB.Communication.ComBase.Serializer.Contracts;
using JPB.Communication.Shared.CrossPlatform;
using JPB.Communication.PCLIntigration.Contracts.Security;
using JPB.Communication.PCLIntigration.ComBase.Messages;
using JPB.Communication.PCLIntigration.ComBase;
using System.Text;

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
        public const string TraceCategoryCriticalSerilization = "JPB.Communication.Critical.Serilization";
        public const string TraceCategoryLowSerilization = "JPB.Communication.Low.Serilization";

        /// <summary>
        ///     The default Serializer
        /// </summary>
        public static IMessageSerializer DefaultMessageSerializer;
        
        private IMessageSerializer _serlilizer;

        static Networkbase()
        {

        }

        /// <summary>
        /// </summary>
        protected Networkbase()
        {
            Serlilizer = DefaultMessageSerializer;
        }

        protected LoginMessage _calle;

        /// <summary>
        ///     Defines the Port the Instance is working on
        /// </summary>
        public abstract ushort Port { get; internal set; }

        public ISecureMessageProvider Security { get; set; }

        /// <summary>
        ///     When Serlilizaion is request this Interface will be used
        /// </summary>
        public IMessageSerializer Serlilizer
        {
            get
            {
                if(_serlilizer == null)
                    throw new Exception("Please define an Default Serializer");
                return _serlilizer; 
            }
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
        public static event EventHandler<string> OnNewItemLoadedFail;
        public static event EventHandler<NetworkMessage> OnIncommingMessage;
        public static event MessageDelegate OnMessageSend;
        public static event LargeMessageDelegate OnNewLargeItemLoadedSuccess;

        protected EventHandler<NetworkMessage> IncommingMessageHandler()
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
                PclTrace.WriteLine(string.Format("> Networkbase> RaiseNewLargeItemLoadedSuccess>{0}", e), TraceCategoryCriticalSerilization);
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
                PclTrace.WriteLine(string.Format("> Networkbase> RaiseMessageSended>{0}", e), TraceCategoryCriticalSerilization);
            }
        }

        protected virtual void RaiseIncommingMessage(NetworkMessage strReceived)
        {
            try
            {
                var handler = OnIncommingMessage;
                if (handler != null)
                    handler(this, strReceived);
            }
            catch (Exception e)
            {
                PclTrace.WriteLine(string.Format("> Networkbase> RaiseIncommingMessage>{0}", e), TraceCategoryCriticalSerilization);
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
                PclTrace.WriteLine(string.Format("> Networkbase> RaiseNewItemLoadedFail>{0}", e), TraceCategoryCriticalSerilization);
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
                PclTrace.WriteLine(string.Format("> Networkbase> RaiseNewItemLoadedSuccess>{0}", e), TraceCategoryCriticalSerilization);
            }
        }

        public MessageBase LoadMessageBaseFromBinary(byte[] source)
        {
            return Serlilizer.DeSerializeMessageContent(source);
        }

        public NetworkMessage DeSerialize(byte[] source)
        {
            try
            {
                var sor = source;
                if (Security != null)
                {
                    sor = Security.Decrypt(sor);
                }
                return Serlilizer.DeSerializeMessage(sor);
            }
            catch (Exception e)
            {
                PclTrace.WriteLine(e.ToString(), TraceCategoryLowSerilization);
                return null;
            }
        }

        public byte[] Serialize(NetworkMessage networkMessage)
        {
            try
            {
                var sor =  Serlilizer.SerializeMessage(networkMessage);
                if (Security != null)
                {
                    sor = Security.Encrypt(sor);
                }
                return sor;
            }
            catch (Exception e)
            {
                PclTrace.WriteLine(e.ToString(), TraceCategoryLowSerilization);
                return new byte[0];
            }
        }

        internal LoginMessage DeSerializeLogin(byte[] maybeLoginMessage)
        {
            var message = maybeLoginMessage;
            if (Security != null)
            {
                message = Security.Decrypt(maybeLoginMessage);
            }

            var passwordEncoded = message
                .Take(NetworkAuthentificator.PasswordBufferSize)
                .Where(s => s != 0x00)
                .ToArray();
            var usernameEncoded = message
                .Skip(NetworkAuthentificator.PasswordBufferSize)
                .Where(s => s != 0x00)
                .ToArray();

            var passwordPlain = Encoding.Unicode.GetString(passwordEncoded, 0, passwordEncoded.Length);
            var usernamePlain = Encoding.Unicode.GetString(usernameEncoded, 0, usernameEncoded.Length);

            return _calle = new LoginMessage()
            {
                Password = passwordPlain,
                Username = usernamePlain
            };
        }

        internal byte[] SerializeLogin(LoginMessage mess)
        {
            var passwordSize = NetworkAuthentificator.PasswordBufferSize;
            var nameSize = NetworkAuthentificator.CredBufferSize - NetworkAuthentificator.PasswordBufferSize;
            
            var passwordPlain = Encoding.Unicode.GetBytes(mess.Password);
            var usernamePlain = Encoding.Unicode.GetBytes(mess.Username);

            if (passwordPlain.Length > passwordSize)
                throw new ArgumentException("The password must be smaller then the maximum Password buffer size", "mess.Password");
            if (usernamePlain.Length > passwordSize)
                throw new ArgumentException("The Username must be smaller then the maximum Username buffer size", "mess.Username");

            var passwordEncoded = new byte[NetworkAuthentificator.PasswordBufferSize];
            var usernameEncoded = new byte[NetworkAuthentificator.CredBufferSize - NetworkAuthentificator.PasswordBufferSize];

            passwordPlain.CopyTo(passwordEncoded, 0);
            usernameEncoded.CopyTo(usernamePlain, 0);
            
            var credData = passwordEncoded
                .Concat(usernamePlain)
                .ToArray();

            if (Security != null)
                credData = Security.Encrypt(credData);

            return credData;
        }

        /// <summary>
        /// </summary>
        /// <param name="A"></param>
        /// <returns></returns>
        public byte[] SaveMessageBaseAsContent(MessageBase A)
        {
            try
            {
                var sor = Serlilizer.SerializeMessageContent(A);
                return sor;
            }
            catch (Exception e)
            {
                PclTrace.WriteLine(e.ToString(), TraceCategoryLowSerilization);
                return new byte[0];
            }
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