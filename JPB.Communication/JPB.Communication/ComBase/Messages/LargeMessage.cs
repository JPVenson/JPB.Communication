using System;
using System.IO;

namespace JPB.Communication.ComBase.Messages
{
    /// <summary>
    /// Support for very Large Messages that can not be hold inside an Array
    /// This class indicates that the Content should be written to the HDD as soon as availbile
    /// </summary>
    public class LargeMessage
    {
        public LargeMessage(MessageBase metaData, Func<Stream> infoLoaded)
        {
            InfoLoaded = infoLoaded;
            MetaData = metaData;
        }

        /// <summary>
        /// Raised when the stream is complted
        /// </summary>
        public event EventHandler OnLoadCompleted;

        /// <summary>
        /// 
        /// </summary>
        protected internal virtual void RaiseLoadCompleted()
        {
            var handler = OnLoadCompleted;
            if (handler != null)
                handler(this, EventArgs.Empty);

            DataComplete = true;
        }


        /// <summary>
        /// Provieds you a maybe only partial exisiting stream to the Large data
        /// </summary>
        public Func<Stream> InfoLoaded { get; private set; }

        /// <summary>
        /// Provieds a Full instance of your MetaData
        /// </summary>
        public MessageBase MetaData { get; private set; }

        public bool DataComplete { get; set; }
    }
}