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

#region Jean-Pierre Bachmann

// Erstellt von Jean-Pierre Bachmann am 10:24

#endregion

using System;
using System.Runtime.Serialization;

namespace JPB.Communication.ComBase.Messages
{
    /// <summary>
    /// The Base object that can be transferred
    /// To ensure a valid Serliazion every inherited class should impliment its own ISerializable Implimentation
    /// </summary>
    [Serializable]
    public class MessageBase : ISerializable, ICloneable
    {
        private object _message;
        private object _infoState;

        public MessageBase()
        {
            Message = new object();
            Id = Guid.NewGuid();
        }

        public MessageBase(object mess)
            : this(Guid.NewGuid())
        {
            Message = mess ?? new object();
        }

        public MessageBase(Guid guid)
        {
            Id = guid;
        }

        internal MessageBase(SerializationInfo info,
            StreamingContext context)
        {
            Message = info.GetValue("Message", typeof(object));
            InfoState = info.GetValue("InfoState", typeof(object));
            Id = (Guid)info.GetValue("ID", typeof(Guid));
            RecievedAt = (DateTime)info.GetValue("RecievedAt", typeof(DateTime));

            Sender = (string)info.GetValue("Sender", typeof(string));
            Reciver = (string)info.GetValue("Reciver", typeof(string));
        }

        /// <summary>
        /// The Content we want to send
        /// </summary>
        public object Message
        {
            get { return _message; }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("value", @"Message can not be null");
                _message = value;
            }
        }

        /// <summary>
        /// The Contract to identify this message on the Distent PC
        /// </summary>
        public object InfoState
        {
            get { return _infoState; }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("value", @"InfoState can not be null");
                _infoState = value;
            }
        }

        /// <summary>
        /// The information about the Original Sender
        /// Is used to identify a Routed message
        /// Readonly
        /// </summary>
        public string Sender { get; internal set; }

        /// <summary>
        /// The information about the Original Sender
        /// Is used to identify a Routed message
        /// Readonly
        /// </summary>
        public string Reciver { get; internal set; }

        /// <summary>
        /// The ID of this message
        /// Is used to clearly identify this message over the network
        /// Readonly
        /// </summary>
        public Guid Id { get; internal set; }

        /// <summary>
        /// Readonly
        /// </summary>
        public DateTime RecievedAt { get; internal set; }

        /// <summary>
        /// Readonly
        /// </summary>
        public DateTime SendAt { get; internal set; }

        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (Message == null)
                Message = new object();
            if (InfoState == null)
                InfoState = new object();

            info.AddValue("Reciver", Reciver, Reciver.GetType());
            info.AddValue("Sender", Sender, Sender.GetType());

            info.AddValue("Message", Message, Message.GetType());
            info.AddValue("InfoState", InfoState, InfoState.GetType());
            info.AddValue("ID", Id, Id.GetType());
            info.AddValue("RecievedAt", RecievedAt, RecievedAt.GetType());
        }

        public object Clone()
        {
            var obje = this.MemberwiseClone() as MessageBase;
            if (obje != null)
            {
                obje.Id = Guid.NewGuid();
                return obje;
            }
            return new MessageBase(Message)
            {
                InfoState = InfoState
            };
        }
    }
}