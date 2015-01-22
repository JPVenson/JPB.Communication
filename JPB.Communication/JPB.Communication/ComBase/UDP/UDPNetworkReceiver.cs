using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using JPB.Communication.ComBase.Messages;
using JPB.Communication.ComBase.TCP;

namespace JPB.Communication.ComBase.UDP
{
    public class UdpNetworkReceiver : NetworkReceiverBase
    {
        internal static UdpNetworkReceiver CreateReceiverInSharedState(ushort portInfo, Socket sock)
        {
            var inst = new UdpNetworkReceiver(portInfo, sock);
            inst.StartListener(sock);

            lock (NetworkFactory.Instance._mutex)
            {
                NetworkFactory.Instance._receiversUdp.Add(portInfo, inst);
                NetworkFactory.Instance.RaiseReceiverCreate(inst);
            }

            return inst;
        }

        private UdpNetworkReceiver(ushort port, Socket sock = null)
        {
            _workeritems = new ConcurrentQueue<Action>();

            OnNewItemLoadedSuccess += TcpConnectionOnOnNewItemLoadedSuccess;
            OnNewLargeItemLoadedSuccess += TcpConnectionOnOnNewItemLoadedSuccess;
            Port = port;
            _typeCallbacks.Add(typeof(RequstMessage), WorkOn_RequestMessage);
        }

        internal UdpNetworkReceiver(ushort port)
            : this(port, null)
        {
            _listenerSocket = new Socket(IPAddress.Any.AddressFamily,
                               SocketType.Dgram,
                               ProtocolType.Udp);

            _listenerSocket.SetIPProtectionLevel(IPProtectionLevel.Unrestricted);

            _listenerSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);

            // Bind the socket to the address and port.
            _listenerSocket.Bind(new IPEndPoint(NetworkInfoBase.IpAddress, Port));

            // Start listening.
            _listenerSocket.Listen(5000);

            // Set up the callback to be notified when somebody requests
            // a new connection.
            _listenerSocket.BeginAccept(OnConnectRequest, _listenerSocket);
        }

        /// <summary>
        /// FOR INTERNAL USE ONLY
        /// </summary>
        internal Dictionary<Type, Action<object>> _typeCallbacks;

        internal readonly Socket _listenerSocket;

        private readonly ConcurrentQueue<Action> _workeritems;
        private AutoResetEvent _autoResetEvent;

        private bool _isWorking;
        private bool _incommingMessage;

        /// <summary>
        /// If Enabled this Receiver can handle streams and messages
        /// 
        /// </summary>
        public bool LargeMessageSupport { get; set; }

        public event Func<UdpNetworkReceiver, Socket, bool> OnCheckConnectionInbound;
        /// <summary>
        /// Is raised when a message is inside the buffer but not fully parsed
        /// </summary>
        public event EventHandler OnIncommingMessage;

        /// <summary>
        /// Enables or Disable the Auto Respond for long working Requests
        /// </summary>
        public bool AutoRespond { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public bool IsDisposing { get; private set; }

        public override ushort Port { get; internal set; }

        /// <summary>
        /// If enabled a Incomming connection will be kept open and will be used for Outgoing and Incomming Trafic to that host
        /// </summary>
        public bool SharedConnection { get; set; }

        private void TcpConnectionOnOnNewItemLoadedSuccess(object mess, ushort port)
        {
            if (port == Port)
            {
                IncommingMessage = false;
                object messCopy = mess;
                _workeritems.Enqueue(() =>
                {
                    var type = messCopy.GetType();

                    var handler = _typeCallbacks.FirstOrDefault(s => s.Key == type);

                    if (!default(KeyValuePair<Type, Action<object>>).Equals(handler))
                    {
                        handler.Value(messCopy);
                    }
                });
                if (_isWorking)
                    return;

                _isWorking = true;
                var task = new Task(WorkOnItems);
                task.ContinueWith(s => { _isWorking = false; });
                task.Start();
            }
        }

        private void WorkOn_RequestMessage(object messCopy)
        {
            //message with return value inbound
            var requstInbound = messCopy as RequstMessage;
            if (requstInbound == null)
                return;

            var sender = NetworkFactory.Instance.GetSender(requstInbound.ExpectedResult);

            var firstOrDefault = _requestHandler.Where(pendingrequest => pendingrequest.Item2.Equals(requstInbound.InfoState)).ToArray();
            if (firstOrDefault.Any())
            {
                object result = null;

                foreach (var tuple in firstOrDefault)
                {
                    //Found a handler for that message and executed it
                    Thread waiter = null;

                    try
                    {
                        if (AutoRespond)
                        {
                            waiter = new Thread(() =>
                            {
                                while (result == null)
                                {
                                    //Fixed value because on the Sender side we are waiting 
                                    Thread.Sleep(TimeSpan.FromSeconds(10));

                                    if (result != null)
                                        return;

                                    sender.SendNeedMoreTimeBackAsync(new RequstMessage()
                                    {
                                        ResponseFor = requstInbound.Id
                                    }, requstInbound.Sender);
                                }
                            });
                            waiter.Start();
                        }

                        result = tuple.Item1(requstInbound);
                    }
                    catch (Exception)
                    {
                        result = null;
                    }
                    finally
                    {
                        if (waiter != null)
                            waiter.Abort();
                    }

                    if (result != null)
                        break;
                }

                if (result == null)
                    return;

                sender.SendMessageAsync(new RequstMessage()
                {
                    Message = result,
                    ResponseFor = requstInbound.Id
                }, requstInbound.Sender);
            }
            else
            {
                //This is an awnser
                var awnser = _pendingrequests.FirstOrDefault(pendingrequest => pendingrequest.Item2.Equals(requstInbound.ResponseFor));
                if (awnser != null)
                    awnser.Item1(requstInbound);
                _pendingrequests.Remove(awnser);
            }
        }

        /// <summary>
        /// True if we are Recieving a message
        /// </summary>
        public bool IncommingMessage
        {
            get { return _incommingMessage; }
            private set
            {
                _incommingMessage = value;
                RaiseIncommingMessage();
            }
        }

        private bool RaiseConnectionInbound(Socket sock)
        {
            var handler = OnCheckConnectionInbound;
            if (handler != null)
                return handler(this, sock);
            return true;
        }

        /// <summary>
        /// Is raised when a message is inside the buffer but not fully parsed
        /// </summary>
        private void RaiseIncommingMessage()
        {
            var handler = OnIncommingMessage;
            if (handler != null)
                handler(this, EventArgs.Empty);
        }



        private void WorkOnItems()
        {
            _autoResetEvent = new AutoResetEvent(false);
            while (_workeritems.Any())
            {
                if (IsDisposing)
                    break;

                Action action = null;
                if (!_workeritems.TryDequeue(out action))
                    return;
                action.BeginInvoke(s => { }, null);
            }
            _autoResetEvent.Set();
        }

        private void OnConnectRequest(IAsyncResult result)
        {
            IncommingMessage = true;
            var sock = ((Socket)result.AsyncState);

            var endAccept = sock.EndAccept(result);
            endAccept.NoDelay = true;

            StartListener(endAccept);
            // Tell the listener socket to start listening again.
            sock.BeginAccept(OnConnectRequest, sock);
        }

        internal void StartListener(Socket endAccept)
        {
            if (RaiseConnectionInbound(endAccept))
            {
                if (SharedConnection)
                {
                    var firstOrDefault = ConnectionPool.Instance.Connections.FirstOrDefault(s => endAccept == s.Socket);
                    if (firstOrDefault == null)
                    {
                        //TODO IMP
                        //ConnectionPool.Instance.AddConnection(endAccept, this);
                    }
                }

                TcpConnectionBase conn;

                if (!LargeMessageSupport)
                {
                    conn = new DefaultTcpConnection(endAccept)
                    {
                        Port = Port
                    };
                }
                else
                {
                    conn = new LargeTcpConnection(endAccept)
                    {
                        Port = Port
                    };
                }
                conn.Serlilizer = this.Serlilizer;
                conn.BeginReceive();
            }
        }


        /// <summary>
        /// Returns a Sender or null
        /// </summary>
        /// <param name="ipOrHost"></param>
        /// <returns></returns>
        public TCPNetworkSender GetFirstSharedSenderOrNull(string ipOrHost)
        {
            var firstOrDefault = ConnectionPool.Instance.Connections.FirstOrDefault(s => s.Ip == ipOrHost);
            if (firstOrDefault == null)
                return null;
            return firstOrDefault.TCPNetworkSender;
        }

        public TCPNetworkSender GetFirstSharedSenderOrNull(string ipOrHost, ushort port)
        {
            var firstOrDefault = ConnectionPool.Instance.Connections.FirstOrDefault(s => s.Ip == ipOrHost && s.TCPNetworkSender.Port == port);
            if (firstOrDefault == null)
                return null;
            return firstOrDefault.TCPNetworkSender;
        }

        #region Implementation of IDisposable

        public void Dispose()
        {
            if (IsDisposing)
                return;

            IsDisposing = true;
            if (_autoResetEvent != null)
                _autoResetEvent.WaitOne();
            if (_listenerSocket != null)
                _listenerSocket.Dispose();

            NetworkFactory.Instance._receivers.Remove(Port);
        }

        #endregion
    }
}
