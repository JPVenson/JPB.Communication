using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using JPB.Communication.ComBase;
using JPB.Communication.ComBase.Messages;
using JPB.Communication.WPF.Controls.ViewModel;

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
