using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JPB.Communication.ComBase.TCP;
using JPB.Communication.Contracts.Intigration;

namespace JPB.Communication.PCLIntigration.ComBase.Security
{
    public class ReceiverSession
    {
        public TCPNetworkReceiver Receiver { get; set; }
        public LoginMessageEx Calle { get; set; }
        public ISocket Sock { get; internal set; }
        public int PendingItems { get; internal set; }
    }
}
