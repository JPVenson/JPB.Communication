namespace JPB.Communication.WPF.Controls.Model
{
    public class TcpNetworkAction
    {
        public TcpNetworkAction(TcpNetworkActionType tcpNetworkActionType, object content)
        {
            TcpNetworkActionType = tcpNetworkActionType;
            Content = content;
        }

        public TcpNetworkActionType TcpNetworkActionType { get; private set; }
        public object Content { get; private set; }
    }
}