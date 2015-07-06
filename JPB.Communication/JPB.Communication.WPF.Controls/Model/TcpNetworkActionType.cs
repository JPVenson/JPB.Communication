namespace JPB.Communication.WPF.Model
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