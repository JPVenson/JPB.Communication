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
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using JPB.Communication.Contracts.Intigration;

namespace JPB.Communication.ComBase.Messages
{
    [DebuggerStepThrough]
    [DataContract()]
#if PCL    
    [Serializable]
#else
    [System.Serializable]
#endif
    [ClassInterfaceAttribute(ClassInterfaceType.None)]

    public class RequstMessage : MessageBase
    {
        public RequstMessage()
        {
            ResponseFor = Guid.NewGuid();
        }

        /// <summary>
        ///     The ID of an Requestmessage for this is an Awsner
        /// </summary>
        [DataMember]
        public Guid ResponseFor { get; set; }

        /// <summary>
        ///     If set, we defining a Port we expecting an awnser
        /// </summary>
        [DataMember]
        public ushort ExpectedResult { get; set; }

        /// <summary>
        ///     If set and this object is an Awnser to a Requst message,
        ///     The client will wait time specifiyed
        /// </summary>
        [DataMember]
        public long NeedMoreTime { get; set; }
    }
}