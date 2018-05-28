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
using JPB.Communication.WinRT.Contracts.Intigration;

namespace JPB.Communication.WinRT.Combase.Generic
{
    /// <summary>
    /// </summary>
    public class ConnectionWrapper : IComparable<ConnectionWrapper>
    {
        /// <summary>
        /// </summary>
        /// <param name="genericNetworkSender"></param>
        /// <param name="genericNetworkReceiver"></param>
        /// <param name="ISocket"></param>
        /// <param name="ip"></param>
        public ConnectionWrapper(string ip, ISocket ISocket, GenericNetworkReceiver genericNetworkReceiver,
            GenericNetworkSender genericNetworkSender)
        {
            Ip = ip;
            Socket = ISocket;
            GenericNetworkReceiver = genericNetworkReceiver;
            GenericNetworkSender = genericNetworkSender;
        }

        /// <summary>
        /// </summary>
        public string Ip { get; private set; }

        /// <summary>
        /// </summary>
        public ISocket Socket { get; private set; }

        /// <summary>
        /// </summary>
        public GenericNetworkReceiver GenericNetworkReceiver { get; private set; }

        /// <summary>
        /// </summary>
        public GenericNetworkSender GenericNetworkSender { get; private set; }

        public int CompareTo(ConnectionWrapper other)
        {
            return String.Compare(Ip, other.Ip, StringComparison.Ordinal);
        }
    }
}