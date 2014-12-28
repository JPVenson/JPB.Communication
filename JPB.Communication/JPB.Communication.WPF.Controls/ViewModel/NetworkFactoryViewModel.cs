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
            Receiver = new ThreadSaveObservableCollection<IDisposable>();
            Sender = new ThreadSaveObservableCollection<IDisposable>();

            NetworkFactory.Instance.OnReceiverCreate += Instance_OnReceiverCreate;
            NetworkFactory.Instance.OnSenderCreate += Instance_OnSenderCreate;
        }

        void Instance_OnSenderCreate(object sender, TCPNetworkSender e)
        {
            Sender.Add(e);
        }

        void Instance_OnReceiverCreate(object sender, TCPNetworkReceiver e)
        {
            Receiver.Add(e);
        }

        private ThreadSaveObservableCollection<IDisposable> _receiver;

        public ThreadSaveObservableCollection<IDisposable> Receiver
        {
            get { return _receiver; }
            set
            {
                _receiver = value;
                SendPropertyChanged(() => Receiver);
            }
        }

        private ThreadSaveObservableCollection<IDisposable> _sender;

        public ThreadSaveObservableCollection<IDisposable> Sender
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
