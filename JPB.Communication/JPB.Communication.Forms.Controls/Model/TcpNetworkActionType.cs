namespace JPB.Communication.WPF.Controls.Model
{
    public enum TcpNetworkActionType
    {
        Incomming,
        Send,
        LoadFail,
        LoadSuccess,
        LoadLargeSuccess,
        InitSender,
        InitReceiver,
        ConnectionClosed,
        ConnectionOpen
    }
}