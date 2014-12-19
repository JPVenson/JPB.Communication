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
using System.Runtime.Serialization;

namespace JPB.Communication.ComBase.Messages
{
    [Serializable]
    public class RequstMessage : MessageBase
    {
        public RequstMessage()
        {
            ResponseFor = Guid.NewGuid();
        }

        public RequstMessage(SerializationInfo info,
            StreamingContext context)
            : base(info, context)
        {
            ResponseFor = (Guid)info.GetValue("ResponseFor", typeof(Guid));
            ExpectedResult = (ushort)info.GetValue("ExpectedResult", typeof(ushort));
            NeedMoreTime = (long)info.GetValue("NeedMoreTime", typeof(long));
        }
        
        /// <summary>
        /// The ID of an Requestmessage for this is an Awsner
        /// </summary>
        public Guid ResponseFor { get; set; }

        /// <summary>
        /// If set, we defining a Port we expecting an awnser
        /// </summary>
        public ushort ExpectedResult { get; set; }

        /// <summary>
        /// If set and this object is an Awnser to a Requst message,
        /// The client will wait time specifiyed
        /// </summary>
        public long NeedMoreTime { get; set; }
        
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("ResponseFor", ResponseFor, ResponseFor.GetType());
            info.AddValue("ExpectedResult", ExpectedResult, ExpectedResult.GetType());
            info.AddValue("NeedMoreTime", NeedMoreTime, NeedMoreTime.GetType());
            base.GetObjectData(info, context);
        }
    }
}