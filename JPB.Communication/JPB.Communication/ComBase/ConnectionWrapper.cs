using System;
using System.Net.Sockets;

namespace JPB.Communication.ComBase
{
    /// <summary>
    /// 
    /// </summary>
    public class ConnectionWrapper : IComparable<ConnectionWrapper>
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="tcpNetworkSender"></param>
        /// <param name="tcpNetworkReceiver"></param>
        /// <param name="socket"></param>
        /// <param name="ip"></param>
        public ConnectionWrapper(string ip, Socket socket, TCPNetworkReceiver tcpNetworkReceiver, TCPNetworkSender tcpNetworkSender)
        {
            Ip = ip;
            Socket = socket;
            TCPNetworkReceiver = tcpNetworkReceiver;
            TCPNetworkSender = tcpNetworkSender;
        }

        /// <summary>
        /// 
        /// </summary>
        public string Ip { get; private set; }
        /// <summary>
        /// 
        /// </summary>
        public Socket Socket { get; private set; }
        /// <summary>
        /// 
        /// </summary>
        public TCPNetworkReceiver TCPNetworkReceiver { get; private set; }
        /// <summary>
        /// 
        /// </summary>
        public TCPNetworkSender TCPNetworkSender { get; private set; }

        public int CompareTo(ConnectionWrapper other)
        {
            return System.String.Compare(Ip, other.Ip, System.StringComparison.Ordinal);
        }
    }
}