using System.Windows;
using JPB.WPFBase.MVVM.ViewModel;
using JPB.Communication.ComBase.Generic;
using JPB.Communication.ComBase;

namespace JPB.Communication.NativeWin.ViewModel
{
    public class ConnectionPoolViewModel : AsyncViewModelBase
    {
        public ConnectionPoolViewModel()
        {
            //not thread save
            OpenConnections = new ThreadSaveObservableCollection<ConnectionWrapper>(ConnectionPool.Instance.GetConnections());
            ConnectionPool.Instance.OnConnectionCreated += Instance_OnConnectionCreated;
            ConnectionPool.Instance.OnConnectionClosed += InstanceOnOnConnectionClosed;
            ShowDetailMode = Visibility.Collapsed;
        }

        private void InstanceOnOnConnectionClosed(object sender, ConnectionWrapper connectionWrapper)
        {
            OpenConnections.Remove(connectionWrapper);
        }

        void Instance_OnConnectionCreated(object sender, ConnectionWrapper connectionWrapper)
        {
            OpenConnections.Add(connectionWrapper);
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

        private ConnectionWrapper _selectedConnection;

        public ConnectionWrapper SelectedConnection
        {
            get { return _selectedConnection; }
            set
            {
                _selectedConnection = value;
                SendPropertyChanged(() => SelectedConnection);
            }
        }

        private ThreadSaveObservableCollection<ConnectionWrapper> _openConnections;

        public ThreadSaveObservableCollection<ConnectionWrapper> OpenConnections
        {
            get { return _openConnections; }
            set
            {
                _openConnections = value;
                SendPropertyChanged(() => OpenConnections);
            }
        }
    }
}
