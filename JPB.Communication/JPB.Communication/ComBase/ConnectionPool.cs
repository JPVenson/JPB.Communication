using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

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
        }

        private static ConnectionPool _instance;

        public static ConnectionPool Instance
        {
            get { return _instance ?? (_instance = new ConnectionPool()); }
        }

        public static IPAddress ResolveIp(string host)
        {
            return NetworkInfoBase.RaiseResolveDistantIp(Dns.GetHostAddresses(host), host);
        }

        public event EventHandler<ConnectionWrapper> OnConnectionCreated;

        protected virtual void RaiseConnectionCreated(ConnectionWrapper item)
        {
            var handler = OnConnectionCreated;
            if (handler != null)
                handler(this, item);
        }

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

        public ILookup<string, ConnectionWrapper> GetConnections()
        {
            return Connections.ToLookup(s => s.Ip);
        }

        internal Socket GetSockForIpOrNull(string hostOrIp)
        {
            IPAddress ipAddress;
            if (!IPAddress.TryParse(hostOrIp, out ipAddress))
            {
                ipAddress = ResolveIp(hostOrIp);
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
            var ip = ResolveIp(hostOrIp).ToString();
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
            networkInformationException.Data.Add("Description", "");
            throw networkInformationException;
        }

        internal Socket GetSock(string ipOrHost, ushort port)
        {
            string ip = ResolveIp(ipOrHost).ToString();
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
