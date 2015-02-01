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
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using System.Timers;
using JPB.Communication.ComBase.TCP;
using JPB.Communication.Contracts;

namespace JPB.Communication.ComBase
{
    /// <summary>
    ///     Stores open Connections
    /// </summary>
    public class ConnectionPool
    {
        private static ConnectionPool _instance;
        private readonly Timer _stateTimer;

        private ConnectionPool()
        {
            Connections = new List<ConnectionWrapper>();
            _stateTimer = new Timer();
            _stateTimer.AutoReset = false;
            _stateTimer.Elapsed += ISocketStateCheck;
            _stateTimer.Interval = 5000;
            _stateTimer.Start();
        }

        public static ConnectionPool Instance
        {
            get { return _instance ?? (_instance = new ConnectionPool()); }
        }

        /// <summary>
        /// </summary>
        internal List<ConnectionWrapper> Connections { get; private set; }

        private void ISocketStateCheck(object state, ElapsedEventArgs elapsedEventArgs)
        {
            try
            {
                _stateTimer.Stop();
                foreach (ConnectionWrapper connectionWrapper in Connections.ToArray())
                {
                    if (!connectionWrapper.Socket.Connected)
                    {
                        Connections.Remove(connectionWrapper);
                        RaiseConnectionClosed(connectionWrapper);
                    }
                }
            }
            catch (Exception)
            {
                Trace.WriteLine(">ConnectionPool >ISocketStateCheck >Error due Timer check", Networkbase.TraceCategory);
            }
            finally
            {
                _stateTimer.Start();
            }
        }

        /// <summary>
        ///     Is invoked when a connection is created
        ///     this can caused by an incomming or a Shared connection from this side
        /// </summary>
        public event EventHandler<ConnectionWrapper> OnConnectionCreated;

        protected virtual void RaiseConnectionCreated(ConnectionWrapper item)
        {
            EventHandler<ConnectionWrapper> handler = OnConnectionCreated;
            if (handler != null)
                handler(this, item);
        }

        /// <summary>
        ///     Is invoked when a connection is closed from this or Remote side
        /// </summary>
        public event EventHandler<ConnectionWrapper> OnConnectionClosed;

        protected virtual void RaiseConnectionClosed(ConnectionWrapper item)
        {
            EventHandler<ConnectionWrapper> handler = OnConnectionClosed;
            if (handler != null)
                handler(this, item);
        }

        /// <summary>
        ///     Returns a Flat copy of all Connections that are existing
        /// </summary>
        /// <returns></returns>
        public ILookup<string, ConnectionWrapper> GetConnections()
        {
            return Connections.ToLookup(s => s.Ip);
        }

        internal ISocket GetSockForIpOrNull(string hostOrIp)
        {
            IPAddress ipAddress;
            if (!IPAddress.TryParse(hostOrIp, out ipAddress))
            {
                ipAddress = NetworkInfoBase.ResolveIp(hostOrIp);
            }

            ConnectionWrapper fod = Connections.FirstOrDefault(s => s.Ip == ipAddress.ToString());

            if (fod == null)
            {
                return null;
            }
            return fod.Socket;
        }

        internal async Task<TCPNetworkReceiver> ConnectToHost(string hostOrIp, ushort port)
        {
            string ip = NetworkInfoBase.ResolveIp(hostOrIp).ToString();
            ConnectionWrapper fod = Connections.FirstOrDefault(s => s.Ip == ip);
            if (fod != null)
            {
                if (fod.TCPNetworkSender.ConnectionOpen(hostOrIp))
                {
                    return fod.TCPNetworkReceiver;
                }
                fod.TCPNetworkReceiver.Dispose();
                fod.TCPNetworkSender.Dispose();
                Connections.Remove(fod);
                RaiseConnectionClosed(fod);
            }

            TCPNetworkSender sender = NetworkFactory.Instance.GetSender(port);
            ISocket ISocket = await sender._InitSharedConnection(ip);
            if (ISocket == null)
            {
                ThrowSockedNotAvailbileHelper();
            }
            TCPNetworkReceiver tcpNetworkReceiver = await InjectISocket(ISocket, sender);
            return tcpNetworkReceiver;
        }

        internal async Task<TCPNetworkReceiver> InjectISocket(ISocket sock, TCPNetworkSender sender)
        {
            IPEndPoint localIp = sock.LocalEndPoint;
            IPEndPoint remoteIp = sock.RemoteEndPoint;
            var port1 = (ushort) localIp.Port;
            TCPNetworkReceiver receiver = TCPNetworkReceiver.CreateReceiverInSharedState(port1, sock);
            AddConnection(new ConnectionWrapper(remoteIp.Address.ToString(), sock, receiver, sender));
            return receiver;
        }

        private void AddConnection(ConnectionWrapper connectionWrapper)
        {
            Connections.Add(connectionWrapper);
            RaiseConnectionCreated(connectionWrapper);
        }

        [Obsolete]
        private void AddConnection(string ip, ISocket ISocket, TCPNetworkReceiver receiver, TCPNetworkSender sender)
        {
            var connectionWrapper = new ConnectionWrapper(ip, ISocket, receiver, sender);
            AddConnection(connectionWrapper);
        }

        private void ThrowSockedNotAvailbileHelper()
        {
            var networkInformationException = new NetworkInformationException
            {
                Source = typeof (TCPNetworkSender).FullName
            };
            networkInformationException.Data.Add("Description", "The creation of the Socked was not successfull");
            throw networkInformationException;
        }

        internal ISocket GetSock(string ipOrHost, ushort port)
        {
            string ip = NetworkInfoBase.ResolveIp(ipOrHost).ToString();
            ConnectionWrapper fod = Connections.FirstOrDefault(s => s.Ip == ip && s.TCPNetworkSender.Port == port);
            if (fod == null)
                return null;
            return fod.Socket;
        }

        internal void AddConnection(ISocket endAccept, TCPNetworkReceiver tcpNetworkReceiver)
        {
            var ipEndPoint = endAccept.RemoteEndPoint as IPEndPoint;
            TCPNetworkSender senderForRemotePort = NetworkFactory.Instance.GetSender((ushort) ipEndPoint.Port);

            AddConnection(
                new ConnectionWrapper(ipEndPoint.Address.ToString(), endAccept,
                    tcpNetworkReceiver, senderForRemotePort));
        }
    }
}