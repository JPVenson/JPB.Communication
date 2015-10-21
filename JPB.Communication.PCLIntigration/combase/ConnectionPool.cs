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
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JPB.Communication.ComBase.Generic;
using JPB.Communication.Contracts.Intigration;
using IPAddress = JPB.Communication.Contracts.Intigration.IPAddress;
using IPEndPoint = JPB.Communication.Contracts.Intigration.IPEndPoint;

namespace JPB.Communication.ComBase
{
    /// <summary>
    ///     Stores open Connections
    /// </summary>
    public class ConnectionPool
    {
        private static ConnectionPool _instance;

        private ConnectionPool()
        {
            Connections = new List<ConnectionWrapper>();
        }

        public static ConnectionPool Instance
        {
            get { return _instance ?? (_instance = new ConnectionPool()); }
        }

        /// <summary>
        /// </summary>
        internal List<ConnectionWrapper> Connections { get; private set; }
        
        /// <summary>
        ///     Is invoked when a connection is created
        ///     this can caused by an incomming or a Shared connection from this side
        /// </summary>
        public event EventHandler<ConnectionWrapper> OnConnectionCreated;

        protected virtual void RaiseConnectionCreated(ConnectionWrapper item)
        {
            var handler = OnConnectionCreated;
            if (handler != null)
                handler(this, item);
        }

        /// <summary>
        ///     Is invoked when a connection is closed from this or Remote side
        /// </summary>
        public event EventHandler<ConnectionWrapper> OnConnectionClosed;

        protected virtual void RaiseConnectionClosed(ConnectionWrapper item)
        {
            var handler = OnConnectionClosed;
            if (handler != null)
                handler(this, item);
        }

        private volatile bool _stateCheckInProgress;

        private void CheckSockStates()
        {
            if(_stateCheckInProgress)
                return;
            _stateCheckInProgress = true;
            try
            {
                var toRemove = Connections.Where(connectionWrapper => !connectionWrapper.Socket.Connected).ToList();
                foreach (var connectionWrapper in toRemove)
                {
                    Connections.Remove(connectionWrapper);
                    RaiseConnectionClosed(connectionWrapper);
                }
            }
            finally
            {
                _stateCheckInProgress = false;
            }
          
        }

        /// <summary>
        ///     Returns a Flat copy of all Connections that are existing
        /// </summary>
        /// <returns></returns>
        public IEnumerable<ConnectionWrapper> GetConnections()
        {
            CheckSockStates();
            return Connections.Select(s => s);
        }

        internal ISocket GetSockForIpOrNull(string hostOrIp)
        {
            CheckSockStates();
            IPAddress ipAddress;
            if (!IPAddress.TryParse(hostOrIp, out ipAddress) || ipAddress == null)
            {
                ipAddress = NetworkInfoBase.ResolveIp(hostOrIp);
            }

            if (ipAddress == null)
                return null;

            var fod = Connections.FirstOrDefault(s => s.Ip == ipAddress.ToString() && s.Socket.Connected);

            if (fod == null)
            {
                return null;
            }
            return fod.Socket;
        }

        internal async Task<GenericNetworkReceiver> ConnectToHost(string hostOrIp, ushort port)
        {
            CheckSockStates();
            string ip = NetworkInfoBase.ResolveIp(hostOrIp).ToString();
            var fod = Connections.FirstOrDefault(s => s.Ip == ip);
            if (fod != null)
            {
                if (fod.GenericNetworkSender.ConnectionOpen(hostOrIp))
                {
                    return fod.GenericNetworkReceiver;
                }
                fod.GenericNetworkReceiver.Dispose();
                fod.GenericNetworkSender.Dispose();
                Connections.Remove(fod);
                RaiseConnectionClosed(fod);
            }

            var sender = NetworkFactory.Instance.GetSender(port);
            var senderSock = await sender._InitSharedConnection(ip);
            if (senderSock == null)
            {
                ThrowSockedNotAvailbileHelper();
            }
            var GenericNetworkReceiver = InjectISocket(senderSock, sender);
            return GenericNetworkReceiver;
        }

        internal GenericNetworkReceiver InjectISocket(ISocket sock, GenericNetworkSender sender)
        {
            IPEndPoint localIp = sock.LocalEndPoint;
            IPEndPoint remoteIp = sock.RemoteEndPoint;
            var port1 = (ushort)localIp.Port;
            sender.SharedConnection = true;
            var receiver = GenericNetworkReceiver.CreateReceiverInSharedState(port1, sock);
            AddConnection(new ConnectionWrapper(remoteIp.Address.ToString(), sock, receiver, sender));
            return receiver;
        }

        private ConnectionWrapper AddConnection(ConnectionWrapper connectionWrapper)
        {
            CheckSockStates();
            Connections.Add(connectionWrapper);
            RaiseConnectionCreated(connectionWrapper);
            return connectionWrapper;
        }

        private void ThrowSockedNotAvailbileHelper()
        {
            var networkInformationException = new Exception
            {
            };
            networkInformationException.Data.Add("Description", "The creation of the Socked was not successful");
            throw networkInformationException;
        }

        internal ISocket GetSock(string ipOrHost, ushort port)
        {
            CheckSockStates();
            string ip = NetworkInfoBase.ResolveIp(ipOrHost).ToString();
            ConnectionWrapper fod = Connections.FirstOrDefault(s => s.Ip == ip && s.GenericNetworkSender.Port == port);
            if (fod == null)
                return null;
            return fod.Socket;
        }

        internal ConnectionWrapper AddConnection(ISocket endAccept, GenericNetworkReceiver GenericNetworkReceiver)
        {
            var ipEndPoint = endAccept.RemoteEndPoint as IPEndPoint;
            GenericNetworkSender senderForRemotePort = NetworkFactory.Instance.GetSender((ushort)ipEndPoint.Port);
            senderForRemotePort.SharedConnection = true;

            return AddConnection(
                new ConnectionWrapper(ipEndPoint.Address.ToString(), endAccept,
                    GenericNetworkReceiver, senderForRemotePort));
        }
    }
}