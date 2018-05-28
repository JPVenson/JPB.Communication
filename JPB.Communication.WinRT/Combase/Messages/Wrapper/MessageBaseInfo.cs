using System;

namespace JPB.Communication.WinRT.combase.Messages.Wrapper
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
