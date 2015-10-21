using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JPB.Communication.ComBase.Messages.Wrapper
{
    public class MessageBaseInfo
    {
        /// <summary>
        ///     Readonly
        /// </summary>
        public string Sender { get; set; }

        /// <summary>
        ///     Readonly
        /// </summary>
        public DateTime RecievedAt { get; set; }

        /// <summary>
        ///     Readonly
        /// </summary>
        public DateTime SendAt { get; set; }
    }
}
