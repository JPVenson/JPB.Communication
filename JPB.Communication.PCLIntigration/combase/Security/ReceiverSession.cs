using JPB.Communication.ComBase.Generic;
using JPB.Communication.Contracts.Intigration;

namespace JPB.Communication.ComBase.Security
{
    public class ReceiverSession
    {
        public GenericNetworkReceiver NetworkReceiver { get; set; }
        public LoginMessageEx Calle { get; set; }
        public ISocket Sock { get; internal set; }
        public int PendingItems { get; internal set; }
        public string Sender { get; internal set; }
        public string Receiver { get; internal set; }
    }
}
