/*
 Created by Jean-Pierre Bachmann
 Visit my GitHub page at:
 
 https://github.com/JPVenson/

 Please respect the Code and Work of other Programers an Read the license carefully

 GNU AFFERO GENERAL PUBLIC LICENSE
                       Version 3, 19 November 2007

 Copyright (C) 2007 Free Software Foundation, Inc. <http://fsf.org/>
 Everyone is permitted to copy and distribute verbatim copies
 of this license document, but changing it is not allowed.

 READ THE FULL LICENSE AT:

 https://github.com/JPVenson/JPB.Communication/blob/master/LICENSE
 */

using System;
using JPB.Communication.Contracts;

namespace JPB.Communication.ComBase.TCP
{
    /// <summary>
    /// </summary>
    public class ConnectionWrapper : IComparable<ConnectionWrapper>
    {
        /// <summary>
        /// </summary>
        /// <param name="tcpNetworkSender"></param>
        /// <param name="tcpNetworkReceiver"></param>
        /// <param name="ISocket"></param>
        /// <param name="ip"></param>
        public ConnectionWrapper(string ip, ISocket ISocket, TCPNetworkReceiver tcpNetworkReceiver,
            TCPNetworkSender tcpNetworkSender)
        {
            Ip = ip;
            Socket = ISocket;
            TCPNetworkReceiver = tcpNetworkReceiver;
            TCPNetworkSender = tcpNetworkSender;
        }

        /// <summary>
        /// </summary>
        public string Ip { get; private set; }

        /// <summary>
        /// </summary>
        public ISocket Socket { get; private set; }

        /// <summary>
        /// </summary>
        public TCPNetworkReceiver TCPNetworkReceiver { get; private set; }

        /// <summary>
        /// </summary>
        public TCPNetworkSender TCPNetworkSender { get; private set; }

        public int CompareTo(ConnectionWrapper other)
        {
            return String.Compare(Ip, other.Ip, StringComparison.Ordinal);
        }
    }
}