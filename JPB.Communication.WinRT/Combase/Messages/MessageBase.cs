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

#region Jean-Pierre Bachmann

// Erstellt von Jean-Pierre Bachmann am 10:24

#endregion

using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;

namespace JPB.Communication.WinRT.combase.Messages
{
	/// <summary>
    ///     The Base object that can be transferred
    ///     To ensure a valid Serliazion every inherited class should impliment its own ISerializable Implimentation
    /// </summary>
    [DebuggerStepThrough]
    [DataContract()]
#if PCL
    [Serializable]
#else
    [System.Serializable]
#endif
    [ClassInterface(ClassInterfaceType.None)]
    public class MessageBase :
#if !PCL
         ICloneable,
#endif
    IComparable, IComparable<MessageBase>
    {
        [IgnoreDataMember]
        private object _infoState;
        [IgnoreDataMember]
        private object _message;
        [IgnoreDataMember]
        private string _session;
        [IgnoreDataMember]
        private Guid _id;

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

        public MessageBase(object mess, object infoState)
                : this(Guid.NewGuid())
        {
            InfoState = infoState ?? new object();
            Message = mess ?? new object();
        }

        public MessageBase(Guid guid)
        {
            Id = guid;
        }

        /// <summary>
        ///     The Content we want to send
        /// </summary>
        [DataMember(Order=0, IsRequired = true)]
        public object Message
        {
            get { return _message; }
            set
            {
                if (value == null)
                {
	                throw new ArgumentNullException("value", @"Message can not be null");
                }

	            _message = value;
            }
        }

        /// <summary>
        ///     The Contract to identify this message on the Distent PC
        /// </summary>

        [DataMember(Order = 1, IsRequired = true)]
        public object InfoState
        {
            get { return _infoState; }
            set
            {
                if (value == null)
                {
	                throw new ArgumentNullException("value", @"InfoState can not be null");
                }

	            _infoState = value;
            }
        }

        [DataMember(Order = 2, IsRequired = true)]
        public string Session { get { return _session; } set { _session = value; } }

        /// <summary>
        ///     The ID of this message
        ///     Is used to clearly identify this message over the network
        ///     Readonly
        /// </summary>
        ///
        [DataMember(Order = 3, IsRequired = true)]
        public Guid Id { get { return _id; } set { _id = value; } }

        public object Clone()
        {
            var obje = MemberwiseClone() as MessageBase;
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

        public int CompareTo(object obj)
        {
            return ((IComparable)Id).CompareTo(obj);
        }

        public int CompareTo(MessageBase other)
        {
            return CompareTo(other);
        }
    }
}