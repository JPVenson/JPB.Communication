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
using System.Diagnostics;
using System.Runtime;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;

namespace JPB.Communication.ComBase.Messages
{
    /// <summary>
    ///     The Base object that can be transferred
    ///     To ensure a valid Serliazion every inherited class should impliment its own ISerializable Implimentation
    /// </summary>
    [DebuggerStepThrough]
#if PCL    
    [System.Runtime.Serializable]
    [DataContract]
#else
    [System.Serializable]
#endif
    [ClassInterfaceAttribute(ClassInterfaceType.None)]
    public class NetworkMessage
    {
        private object _infoState;
        private object _message;
        
        public NetworkMessage()
        {
            Message = new object();
            Id = Guid.NewGuid();
        }

        public NetworkMessage(object mess)
            : this(Guid.NewGuid())
        {
            Message = mess ?? new object();
        }

        public NetworkMessage(object mess, object infoState)
            : this(Guid.NewGuid())
        {
            InfoState = infoState ?? new object();
            Message = mess ?? new object();
        }

        public NetworkMessage(Guid guid)
        {
            Id = guid;
        }      

        /// <summary>
        ///     The Content we want to send
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
        ///     The Contract to identify this message on the Distent PC
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
        ///     The information about the Original Sender
        ///     Is used to identify a Routed message
        ///     Readonly
        /// </summary>
        public string Sender { get; set; }

        public string Session { get; set; }

        /// <summary>
        ///     The ID of this message
        ///     Is used to clearly identify this message over the network
        ///     Readonly
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        ///     Readonly
        /// </summary>
        public DateTime RecievedAt { get; set; }

        /// <summary>
        ///     Readonly
        /// </summary>
        public DateTime SendAt { get; set; }

        public object Clone()
        {
            var obje = MemberwiseClone() as NetworkMessage;
            if (obje != null)
            {
                obje.Id = Guid.NewGuid();
                return obje;
            }
            return new NetworkMessage(Message)
            {
                InfoState = InfoState
            };
        }     
    }
}