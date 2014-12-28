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
