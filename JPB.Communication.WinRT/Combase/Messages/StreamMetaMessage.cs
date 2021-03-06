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

using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;

namespace JPB.Communication.WinRT.combase.Messages
{
    [DebuggerStepThrough]
    [DataContract()]
#if PCL    
    [Serializable]
#else
    [System.Serializable]
#endif
    [ClassInterface(ClassInterfaceType.None)]

    public class StreamMetaMessage : MessageBase
    {
        public StreamMetaMessage()
        {
        }

        public StreamMetaMessage(object mess, object infoState)
            : base(mess, infoState)
        {
        }

        /// <summary>
        /// </summary>
        public long StreamSize { get; internal set; }
     
    }
}