using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace JPB.Communication.ComBase
{
    internal class SharedConnectionManager
    {
        private SharedConnectionManager()
        {
            Connections = new List<Tuple<string, Socket, TCPNetworkReceiver, TCPNetworkSender>>();
        }

        private static SharedConnectionManager _instance;

        public static SharedConnectionManager Instance
        {
            get { return _instance ?? (_instance = new SharedConnectionManager()); }
        }

        public static IPAddress ResolveIp(string host)
        {
            return NetworkInfoBase.RaiseResolveDistantIp(Dns.GetHostAddresses(host), host);
        }

        /// <summary>
        /// 
        /// </summary>
        public List<Tuple<string, Socket, TCPNetworkReceiver, TCPNetworkSender>> Connections { get; set; }

        public Socket GetSockForIpOrNull(string hostOrIp)
        {
            IPAddress ipAddress;
            if (!IPAddress.TryParse(hostOrIp, out ipAddress))
            {
                ipAddress = ResolveIp(hostOrIp);
            }

            var fod = Connections.FirstOrDefault(s => s.Item1 == ipAddress.ToString());

            if (fod == null)
            {
                return null;
            }
            return fod.Item2;
        }

        public async Task<TCPNetworkReceiver> ConnectToHost(string hostOrIp, ushort port)
        {
            var ip = ResolveIp(hostOrIp).ToString();
            var fod = Connections.FirstOrDefault(s => s.Item1 == ip);
            if (fod == null)
            {
                var sender = NetworkFactory.Instance.GetSender(port);
                var socket = await sender._InitSharedConnection(ip);
                var ipAddress = socket.LocalEndPoint as IPEndPoint;
                var port1 = (ushort)ipAddress.Port;
                var receiver = TCPNetworkReceiver.CreateReceiverInSharedState(port1, socket);
                Connections.Add(new Tuple<string, Socket, TCPNetworkReceiver, TCPNetworkSender>(ip, socket, receiver, sender));
                return receiver;
            }
            return null;
        }

        public Socket GetSock(string ipOrHost)
        {
            string ip = ResolveIp(ipOrHost).ToString();
            var fod = Connections.FirstOrDefault(s => s.Item1 == ip);
            if (fod == null)
                return null;
            return fod.Item2;
        }

        public void AddConnection(Socket endAccept, TCPNetworkReceiver tcpNetworkReceiver)
        {
            var ipEndPoint = endAccept.RemoteEndPoint as IPEndPoint;
            var senderForRemotePort = NetworkFactory.Instance.GetSender((ushort)ipEndPoint.Port);

            Connections.Add(
                new Tuple<string, Socket, TCPNetworkReceiver, TCPNetworkSender>(ipEndPoint.Address.ToString(), endAccept,
                    tcpNetworkReceiver, senderForRemotePort));
        }
    }
}
