using System.Linq;
using System.Windows;
using JPB.WPFBase.MVVM.DelegateCommand;
using JPB.WPFBase.MVVM.ViewModel;
using JPB.Communication.WPF.Model;
using JPB.Communication.ComBase;
using JPB.Communication.ComBase.TCP;
using JPB.Communication.ComBase.Messages.Wrapper;
using JPB.Communication.ComBase.Messages;

namespace JPB.Communication.NativeWin.ViewModel
{
    public class NetworkLogViewModel : AsyncViewModelBase
    {
        public NetworkLogViewModel()
        {
            TcpNetworkActionLog = new ThreadSaveObservableCollection<TcpNetworkAction>();

            Networkbase.OnIncommingMessage += Networkbase_OnIncommingMessage;
            Networkbase.OnMessageSend += Networkbase_OnMessageSend;
            Networkbase.OnNewItemLoadedFail += Networkbase_OnNewItemLoadedFail;
            Networkbase.OnNewItemLoadedSuccess += Networkbase_OnNewItemLoadedSuccess;
            Networkbase.OnNewLargeItemLoadedSuccess += Networkbase_OnNewLargeItemLoadedSuccess;

            NetworkFactory.Instance.OnReceiverCreate += Instance_OnReceiverCreate;
            NetworkFactory.Instance.OnSenderCreate += InstanceOnOnSenderCreate;

            ConnectionPool.Instance.OnConnectionCreated += Instance_OnConnectionCreated;
            ConnectionPool.Instance.OnConnectionClosed += InstanceOnOnConnectionClosed;

            ShowDetailMode = Visibility.Collapsed;
            ClearGridCommand = new DelegateCommand(ExecuteClearGrid, CanExecuteClearGrid);
        }

        public DelegateCommand ClearGridCommand { get; private set; }

        public void ExecuteClearGrid(object sender)
        {
            TcpNetworkActionLog.Clear();
        }

        public bool CanExecuteClearGrid(object sender)
        {
            return TcpNetworkActionLog.Any();
        }

        public ObjectViewModelHierarchy Explorer
        {
            get { return _explorer; }
            private set
            {
                _explorer = value;
                SendPropertyChanged(() => Explorer);
            }
        }

        private Visibility _showDetailMode;

        public Visibility ShowDetailMode
        {
            get { return _showDetailMode; }
            set
            {
                _showDetailMode = value;
                SendPropertyChanged(() => ShowDetailMode);
            }
        }

        private ThreadSaveObservableCollection<TcpNetworkAction> _tcpNetworkActionLog;

        public ThreadSaveObservableCollection<TcpNetworkAction> TcpNetworkActionLog
        {
            get { return _tcpNetworkActionLog; }
            set
            {
                _tcpNetworkActionLog = value;
                SendPropertyChanged(() => TcpNetworkActionLog);
            }
        }

        private TcpNetworkAction _selectedNetworkAction;
        private ObjectViewModelHierarchy _explorer;

        public TcpNetworkAction SelectedNetworkAction
        {
            get { return _selectedNetworkAction; }
            set
            {
                _selectedNetworkAction = value;
                Explorer = new ObjectViewModelHierarchy(value);
                SendPropertyChanged(() => SelectedNetworkAction);
            }
        }

        private void InstanceOnOnConnectionClosed(object sender, ConnectionWrapper connectionWrapper)
        {
            TcpNetworkActionLog.Add(new TcpNetworkAction(TcpNetworkActionType.ConnectionClosed, new { Sender = sender, Source = connectionWrapper }));
        }

        void Instance_OnConnectionCreated(object sender, ConnectionWrapper connectionWrapper)
        {
            TcpNetworkActionLog.Add(new TcpNetworkAction(TcpNetworkActionType.ConnectionOpen, new { Sender = sender, Source = connectionWrapper }));
        }

        private void InstanceOnOnSenderCreate(object sender, TCPNetworkSender tcpNetworkSender)
        {
            TcpNetworkActionLog.Add(new TcpNetworkAction(TcpNetworkActionType.InitSender, new { Sender = sender, Source = tcpNetworkSender }));
        }

        void Instance_OnReceiverCreate(object sender, TCPNetworkReceiver tcpNetworkReceiver)
        {
            TcpNetworkActionLog.Add(new TcpNetworkAction(TcpNetworkActionType.InitReceiver, new { Sender = sender, Source = tcpNetworkReceiver }));
        }

        void Networkbase_OnNewLargeItemLoadedSuccess(LargeMessage mess, ushort port)
        {
            TcpNetworkActionLog.Add(new TcpNetworkAction(TcpNetworkActionType.LoadLargeSuccess, new { Port = port, Message = mess }));
        }

        void Networkbase_OnNewItemLoadedSuccess(MessageBase mess, ushort port)
        {
            TcpNetworkActionLog.Add(new TcpNetworkAction(TcpNetworkActionType.LoadSuccess, new { Port = port, Message = mess }));
        }

        void Networkbase_OnNewItemLoadedFail(object sender, string e)
        {
            TcpNetworkActionLog.Add(new TcpNetworkAction(TcpNetworkActionType.LoadFail, new { (sender as Networkbase).Port, Message = e }));
        }

        void Networkbase_OnMessageSend(MessageBase mess, ushort port)
        {
            TcpNetworkActionLog.Add(new TcpNetworkAction(TcpNetworkActionType.Send, new { Port = port, Message = mess }));
        }

        void Networkbase_OnIncommingMessage(object sender, NetworkMessage e)
        {
            TcpNetworkActionLog.Add(new TcpNetworkAction(TcpNetworkActionType.Incomming, new { (sender as Networkbase).Port, Message = e }));
        }
    }
}
