using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JPB.Communication.ComBase;
using JPB.WPFBase.MVVM.ViewModel;

namespace JPB.Communication.WPF.Controls.ViewModel
{
    public class NetworkFactoryViewModel : AsyncViewModelBase
    {
        public NetworkFactoryViewModel()
        {
            Receiver = new ThreadSaveObservableCollection<object>();
            Sender = new ThreadSaveObservableCollection<object>();

            lock (NetworkFactory.Instance.SyncRoot)
            {
                foreach (var receiver in NetworkFactory.Instance.GetReceivers())
                {
                    Receiver.Add(receiver.Value);
                }

                foreach (var sender in NetworkFactory.Instance.GetSenders())
                {
                    Sender.Add(sender.Value);
                }

                NetworkFactory.Instance.ShouldRaiseEvents = true;
                NetworkFactory.Instance.OnReceiverCreate += Instance_OnReceiverCreate;
                NetworkFactory.Instance.OnSenderCreate += Instance_OnSenderCreate;
            }
        }

        void Instance_OnSenderCreate(object sender, TCPNetworkSender e)
        {
            Sender.Add(e);
        }

        void Instance_OnReceiverCreate(object sender, TCPNetworkReceiver e)
        {
            Receiver.Add(e);
        }

        private ThreadSaveObservableCollection<object> _receiver;

        public ThreadSaveObservableCollection<object> Receiver
        {
            get { return _receiver; }
            set
            {
                _receiver = value;
                SendPropertyChanged(() => Receiver);
            }
        }

        private ThreadSaveObservableCollection<object> _sender;

        public ThreadSaveObservableCollection<object> Sender
        {
            get { return _sender; }
            set
            {
                _sender = value;
                SendPropertyChanged(() => Sender);
            }
        }
    }
}
