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
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using JPB.Communication.ComBase.TCP;

namespace JPB.Communication.ComBase
{
    /// <summary>
    /// Stores open Connections
    /// </summary>
    public class ConnectionPool
    {
        private ConnectionPool()
        {
            Connections = new List<ConnectionWrapper>();
            _stateTimer = new Timer();
            _stateTimer.AutoReset = false;
            _stateTimer.Elapsed += socketStateCheck;
            _stateTimer.Interval = 5000;
            _stateTimer.Start();
        }

        private void socketStateCheck(object state, ElapsedEventArgs elapsedEventArgs)
        {
            try
            {
                _stateTimer.Stop();
                foreach (var connectionWrapper in Connections.ToArray())
                {
                    if (!connectionWrapper.Socket.Connected)
                    {
                        Connections.Remove(connectionWrapper);
                        this.RaiseConnectionClosed(connectionWrapper);
                    }
                }
            }
            catch (Exception)
            {
                Trace.WriteLine(">ConnectionPool >socketStateCheck >Error due Timer check", Networkbase.TraceCategory);
            }
            finally
            {
                _stateTimer.Start();
            }
        }

        private readonly Timer _stateTimer;

        private static ConnectionPool _instance;

        public static ConnectionPool Instance
        {
            get { return _instance ?? (_instance = new ConnectionPool()); }
        }
        /// <summary>
        /// Is invoked when a connection is created
        /// this can caused by an incomming or a Shared connection from this side
        /// </summary>
        public event EventHandler<ConnectionWrapper> OnConnectionCreated;

        protected virtual void RaiseConnectionCreated(ConnectionWrapper item)
        {
            var handler = OnConnectionCreated;
            if (handler != null)
                handler(this, item);
        }

        /// <summary>
        /// Is invoked when a connection is closed from this or Remote side
        /// </summary>
        public event EventHandler<ConnectionWrapper> OnConnectionClosed;

        protected virtual void RaiseConnectionClosed(ConnectionWrapper item)
        {
            var handler = OnConnectionClosed;
            if (handler != null)
                handler(this, item);
        }

        /// <summary>
        /// 
        /// </summary>
        internal List<ConnectionWrapper> Connections { get; private set; }

        /// <summary>
        /// Returns a Flat copy of all Connections that are existing
        /// </summary>
        /// <returns></returns>
        public ILookup<string, ConnectionWrapper> GetConnections()
        {
            return Connections.ToLookup(s => s.Ip);
        }

        internal Socket GetSockForIpOrNull(string hostOrIp)
        {
            IPAddress ipAddress;
            if (!IPAddress.TryParse(hostOrIp, out ipAddress))
            {
                ipAddress = NetworkInfoBase.ResolveIp(hostOrIp);
            }

            var fod = Connections.FirstOrDefault(s => s.Ip == ipAddress.ToString());

            if (fod == null)
            {
                return null;
            }
            return fod.Socket;
        }

        internal async Task<TCPNetworkReceiver> ConnectToHost(string hostOrIp, ushort port)
        {
            var ip = NetworkInfoBase.ResolveIp(hostOrIp).ToString();
            var fod = Connections.FirstOrDefault(s => s.Ip == ip);
            if (fod != null)
            {
                if (fod.TCPNetworkSender.ConnectionOpen(hostOrIp))
                {
                    return fod.TCPNetworkReceiver;
                }
                fod.TCPNetworkReceiver.Dispose();
                fod.TCPNetworkSender.Dispose();
                Connections.Remove(fod);
                this.RaiseConnectionClosed(fod);
            }

            var sender = NetworkFactory.Instance.GetSender(port);
            var socket = await sender._InitSharedConnection(ip);
            if (socket == null)
            {
                ThrowSockedNotAvailbileHelper();
            }
            var tcpNetworkReceiver = await InjectSocket(socket, sender);
            return tcpNetworkReceiver;
        }

        internal async Task<TCPNetworkReceiver> InjectSocket(Socket sock, TCPNetworkSender sender)
        {
            var localIp = sock.LocalEndPoint as IPEndPoint;
            var remoteIp = sock.RemoteEndPoint as IPEndPoint;
            var port1 = (ushort)localIp.Port;
            var receiver = TCPNetworkReceiver.CreateReceiverInSharedState(port1, sock);
            AddConnection(new ConnectionWrapper(remoteIp.Address.ToString(), sock, receiver, sender));
            return receiver;
        }

        private void AddConnection(ConnectionWrapper connectionWrapper)
        {
            this.Connections.Add(connectionWrapper);
            this.RaiseConnectionCreated(connectionWrapper);
        }

        [Obsolete]
        private void AddConnection(string ip, Socket socket, TCPNetworkReceiver receiver, TCPNetworkSender sender)
        {
            var connectionWrapper = new ConnectionWrapper(ip, socket, receiver, sender);
            AddConnection(connectionWrapper);
        }

        private void ThrowSockedNotAvailbileHelper()
        {
            var networkInformationException = new NetworkInformationException()
            {
                Source = typeof(TCPNetworkSender).FullName
            };
            networkInformationException.Data.Add("Description", "The creation of the Socked was not successfull");
            throw networkInformationException;
        }

        internal Socket GetSock(string ipOrHost, ushort port)
        {
            string ip = NetworkInfoBase.ResolveIp(ipOrHost).ToString();
            var fod = Connections.FirstOrDefault(s => s.Ip == ip && s.TCPNetworkSender.Port == port);
            if (fod == null)
                return null;
            return fod.Socket;
        }

        internal void AddConnection(Socket endAccept, TCPNetworkReceiver tcpNetworkReceiver)
        {
            var ipEndPoint = endAccept.RemoteEndPoint as IPEndPoint;
            var senderForRemotePort = NetworkFactory.Instance.GetSender((ushort)ipEndPoint.Port);

            AddConnection(
                new ConnectionWrapper(ipEndPoint.Address.ToString(), endAccept,
                    tcpNetworkReceiver, senderForRemotePort));
        }
    }
}
