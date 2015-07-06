using JPB.Communication.NativeWin.ViewModel;
using System.Windows;
using System.Windows.Controls;

namespace JPB.Communication.WPF.Controls.View
{
    /// <summary>
    /// Interaction logic for NetworkEventLogger.xaml
    /// </summary>
    public partial class NetworkEventLogger : UserControl
    {
        public NetworkEventLogger()
        {
            InitializeComponent();
            NetworkLogViewModel = new NetworkLogViewModel { ShowDetailMode = Visibility.Visible };
            ConnectionPoolViewModel = new ConnectionPoolViewModel {ShowDetailMode = Visibility.Visible};
            NetworkFactoryViewModel = new NetworkFactoryViewModel();

            //IPAddress ex = null;
            //IPAddress inter = null;
            //var resolvIps = new Task(() =>
            //{
            //    ex = NetworkInfoBase.IpAddressExternal;
            //    inter = NetworkInfoBase.IpAddress;
            //});
            //resolvIps.ContinueWith(s =>
            //{
            //    base.Dispatcher.Invoke(() =>
            //    {
            //        ExternalIp = ex;
            //        InternalIp = inter;
            //    });
            //});
        }

        public static readonly DependencyProperty ConnectionPoolViewModelProperty = DependencyProperty.Register(
            "ConnectionPoolViewModel", typeof(ConnectionPoolViewModel), typeof(NetworkEventLogger), new PropertyMetadata(default(ConnectionPoolViewModel)));

        public ConnectionPoolViewModel ConnectionPoolViewModel
        {
            get { return (ConnectionPoolViewModel) GetValue(ConnectionPoolViewModelProperty); }
            private set { SetValue(ConnectionPoolViewModelProperty, value); }
        }

        public static readonly DependencyProperty NetworkLogViewModelProperty = DependencyProperty.Register(
            "NetworkLogViewModel", typeof (NetworkLogViewModel), typeof (NetworkEventLogger), new PropertyMetadata(default(NetworkLogViewModel)));

        public NetworkLogViewModel NetworkLogViewModel
        {
            get { return (NetworkLogViewModel) GetValue(NetworkLogViewModelProperty); }
            set { SetValue(NetworkLogViewModelProperty, value); }
        }

        public static readonly DependencyProperty NetworkFactoryViewModelProperty = DependencyProperty.Register(
            "NetworkFactoryViewModel", typeof (NetworkFactoryViewModel), typeof (NetworkEventLogger), new PropertyMetadata(default(NetworkFactoryViewModel)));

        public NetworkFactoryViewModel NetworkFactoryViewModel
        {
            get { return (NetworkFactoryViewModel) GetValue(NetworkFactoryViewModelProperty); }
            set { SetValue(NetworkFactoryViewModelProperty, value); }
        }
    }
}
