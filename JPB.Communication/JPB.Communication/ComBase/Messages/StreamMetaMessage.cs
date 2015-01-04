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
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace JPB.Communication.ComBase.Messages
{
    [Serializable]
    public class StreamMetaMessage : MessageBase
    {
        public StreamMetaMessage()
        {

        }

        public StreamMetaMessage(object mess, object infoState)
            : base(mess, infoState)
        {

        }

        internal StreamMetaMessage(SerializationInfo info,
            StreamingContext context)
            : base(info, context)
        {
            StreamSize = (long)info.GetValue("StreamSize", typeof(long));
        }

        /// <summary>
        /// 
        /// </summary>
        public long StreamSize { get; internal set; }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("StreamSize", StreamSize, StreamSize.GetType());
        }
    }
}
