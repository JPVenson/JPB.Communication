using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using JPB.Communication.ComBase.TCP;

namespace JPB.Communication.Forms.Controls
{
    public partial class NetworkFactoryControl : UserControl
    {
        public NetworkFactoryControl()
        {
            InitializeComponent();

            lock (NetworkFactory.Instance.SyncRoot)
            {
                foreach (var receiver in NetworkFactory.Instance.GetReceivers())
                {
                    Receiver.Rows.Add(receiver.Value);
                }

                foreach (var sender in NetworkFactory.Instance.GetSenders())
                {
                    Sender.Rows.Add(sender.Value);
                }

                NetworkFactory.Instance.ShouldRaiseEvents = true;
                NetworkFactory.Instance.OnReceiverCreate += Instance_OnReceiverCreate;
                NetworkFactory.Instance.OnSenderCreate += Instance_OnSenderCreate;
            }
        }

        void Instance_OnSenderCreate(object sender, GenericNetworkSender e)
        {
            Sender.Rows.Add(e);
        }

        void Instance_OnReceiverCreate(object sender, GenericNetworkReceiver e)
        {
            Receiver.Rows.Add(e);
        }
    }
}
