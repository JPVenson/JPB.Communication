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

namespace JPB.Communication.ComBase.Messages.Wrapper
{
    /// <summary>
    ///     Support for very Large Messages that can not be hold inside an Array
    ///     This class indicates that the Content should be written to the HDD as soon as availbile
    /// </summary>
    public class LargeMessage
    {
        public LargeMessage(StreamMetaMessage metaData, Func<Stream> infoLoaded)
        {
            StreamSize = metaData.StreamSize;
            InfoLoaded = infoLoaded;
            MetaData = metaData;
        }

        /// <summary>
        /// </summary>
        public long StreamSize { get; private set; }


        /// <summary>
        ///     Provieds you a maybe only partial exisiting stream to the Large data
        /// </summary>
        public Func<Stream> InfoLoaded { get; private set; }

        /// <summary>
        ///     Provieds a Full instance of your MetaData
        /// </summary>
        public MessageBase MetaData { get; private set; }

        public bool DataComplete { get; set; }

        /// <summary>
        ///     Raised when the stream is complted
        /// </summary>
        public event Action<LargeMessage> OnLoadCompleted;

        /// <summary>
        ///     Raises the OnLoadComplete event and Seek 0
        /// </summary>
        protected internal virtual void RaiseLoadCompleted()
        {
            Action<LargeMessage> handler = OnLoadCompleted;
            if (handler != null)
                handler(this);

            DataComplete = true;
        }
    }
}