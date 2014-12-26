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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using JPB.Communication.ComBase.Messages;

namespace JPB.Communication.ComBase
{
    public sealed class TCPNetworkReceiver : Networkbase, IDisposable
    {
        internal static TCPNetworkReceiver CreateReceiverInSharedState(ushort portInfo, Socket sock)
        {
            var inst = new TCPNetworkReceiver(portInfo, sock);
            inst.StartListener(sock);

            lock (NetworkFactory.Instance._mutex)
            {
                NetworkFactory.Instance._receivers.Add(portInfo, inst);
            }

            return inst;
        }

        //internal static TCPNetworkReceiver CreateReceiverInSharedState(ushort portInfo)
        //{
        //    var inst = new TCPNetworkReceiver(portInfo);
        //    lock (NetworkFactory.Instance._mutex)
        //    {
        //        NetworkFactory.Instance._receivers.Add(portInfo, inst);
        //    }

        //    return inst;
        //}

        private TCPNetworkReceiver(ushort port, Socket sock = null)
        {
            _largeMessages = new List<Tuple<Action<LargeMessage>, object>>();
            _onetimeupdated = new List<Tuple<Action<MessageBase>, Guid>>();
            _pendingrequests = new List<Tuple<Action<RequstMessage>, Guid>>();
            _requestHandler = new List<Tuple<Func<RequstMessage, object>, object>>();
            _updated = new List<Tuple<Action<MessageBase>, object>>();
            _workeritems = new ConcurrentQueue<Action>();

            OnNewItemLoadedSuccess += TcpConnectionOnOnNewItemLoadedSuccess;
            OnNewLargeItemLoadedSuccess += TcpConnectionOnOnNewItemLoadedSuccess;
            Port = port;

            TypeCallbacks = new Dictionary<Type, Action<object>>();
            TypeCallbacks.Add(typeof(RequstMessage), WorkOn_RequestMessage);
            TypeCallbacks.Add(typeof(MessageBase), WorkOn_MessageBase);
            TypeCallbacks.Add(typeof(LargeMessage), WorkOn_LargeMessage);
        }

        internal TCPNetworkReceiver(ushort port)
            : this(port, null)
        {
            _listenerSocket = new Socket(IPAddress.Any.AddressFamily,
                               SocketType.Stream,
                               ProtocolType.Tcp);

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

        //private void AsyncReader()
        //{
        //    while (true)
        //    {
        //        IDefaultTcpConnection conn = null;
        //        conn = new DefaultTcpConnection(_listenerSocket.ReceiveBufferSize, _listenerSocket)
        //        {
        //            Port = Port
        //        };
        //        conn.Receive();
        //    }
        //}


        private Thread waiterThread;

        /// <summary>
        /// FOR INTERNAL USE ONLY
        /// </summary>
        internal Dictionary<Type, Action<object>> TypeCallbacks;

        private readonly List<Tuple<Action<LargeMessage>, object>> _largeMessages;
        private readonly List<Tuple<Action<MessageBase>, Guid>> _onetimeupdated;
        private readonly List<Tuple<Action<RequstMessage>, Guid>> _pendingrequests;
        private readonly List<Tuple<Func<RequstMessage, object>, object>> _requestHandler;
        internal readonly Socket _listenerSocket;
        
        private readonly List<Tuple<Action<MessageBase>, object>> _updated;
        private readonly ConcurrentQueue<Action> _workeritems;
        private AutoResetEvent _autoResetEvent;

        private bool _isWorking;
        private bool _incommingMessage;

        private void TcpConnectionOnOnNewItemLoadedSuccess(object mess, ushort port)
        {
            if (port == Port)
            {
                IncommingMessage = false;
                object messCopy = mess;
                _workeritems.Enqueue(() =>
                {
                    var type = messCopy.GetType();

                    var handler = TypeCallbacks.FirstOrDefault(s => s.Key == type);

                    if (!default(KeyValuePair<Type, Action<object>>).Equals(handler))
                    {
                        handler.Value(messCopy);
                    }
                });
                if (_isWorking)
                    return;

                _isWorking = true;
                var task = new Task(WorkOnItems);
                task.Start();
                task.ContinueWith(s => { _isWorking = false; });
            }
        }

        private void WorkOn_LargeMessage(object metaData)
        {
            var messCopy = metaData as LargeMessage;

            var updateCallbacks = _largeMessages.Where(action => messCopy != null && (action.Item2 == null || action.Item2.Equals(messCopy.MetaData.InfoState))).ToArray();
            foreach (var action in updateCallbacks)
            {
                action.Item1.BeginInvoke(messCopy, e => { }, null);
            }
        }

        private void WorkOn_MessageBase(object message)
        {
            var messCopy = message as MessageBase;

            var updateCallbacks = _updated.Where(action => messCopy != null && (action.Item2 == null || action.Item2.Equals(messCopy.InfoState))).ToArray();
            foreach (var action in updateCallbacks)
            {
                action.Item1.BeginInvoke(messCopy, e => { }, null);
            }

            //Go through all one time items and check for ID
            var oneTimeImtes = _onetimeupdated.Where(s => messCopy != null && messCopy.Id == s.Item2).ToArray();

            foreach (var action in oneTimeImtes)
            {
                action.Item1.BeginInvoke(messCopy, e => { }, null);
            }

            foreach (var useditem in oneTimeImtes)
                _onetimeupdated.Remove(useditem);
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
                        waiter = new Thread(() =>
                        {
                            while (true)
                            {
                                if (result != null)
                                    return;

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

                    if (result == null)
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

        /// <summary>
        /// If Enabled this Receiver can handle streams and messages
        /// 
        /// </summary>
        public bool LargeMessageSupport { get; set; }

        public event Func<TCPNetworkReceiver, Socket, bool> OnCheckConnectionInbound;

        protected bool RaiseConnectionInbound(Socket sock)
        {
            var handler = OnCheckConnectionInbound;
            if (handler != null)
                return handler(this, sock);
            return true;
        }


        /// <summary>
        /// Is raised when a message is inside the buffer but not fully parsed
        /// </summary>
        public event EventHandler OnIncommingMessage;

        /// <summary>
        /// Is raised when a message is inside the buffer but not fully parsed
        /// </summary>
        protected void RaiseIncommingMessage()
        {
            var handler = OnIncommingMessage;
            if (handler != null)
                handler(this, EventArgs.Empty);
        }

        #region Implementation of IDisposable

        public void Dispose()
        {
            if (IsDisposing)
                return;

            IsDisposing = true;
            if (_autoResetEvent != null)
                _autoResetEvent.WaitOne();
            _listenerSocket.Dispose();
        }

        #endregion

        /// <summary>
        /// 
        /// </summary>
        public bool IsDisposing { get; private set; }

        public void UnregisterChanged(Action<MessageBase> action, object state)
        {
            var enumerable = _updated.FirstOrDefault(s => s.Item1 == action && s.Item2 == state);
            if (enumerable != null)
            {
                _updated.Remove(enumerable);
            }
        }

        public void UnregisterChanged(Action<MessageBase> action)
        {
            var enumerable = _updated.FirstOrDefault(s => s.Item1 == action);
            if (enumerable != null)
            {
                _updated.Remove(enumerable);
            }
        }

        /// <summary>
        /// Register a Callback localy that will be used when a new message is inbound that has state in its InfoState
        /// </summary>
        /// <param name="action">Callback</param>
        /// <param name="state">Maybe an Enum?</param>
        public void RegisterMessageBaseInbound(Action<MessageBase> action, object state)
        {
            _updated.Add(new Tuple<Action<MessageBase>, object>(action, state));
        }

        /// <summary>
        /// Register a Callback localy that will be used when a new Large message is inbound that has state in its InfoState
        /// </summary>
        /// <param name="action">Callback</param>
        /// <param name="state">Maybe an Enum?</param>
        public void RegisterMessageBaseInbound(Action<LargeMessage> action, object state)
        {
            _largeMessages.Add(new Tuple<Action<LargeMessage>, object>(action, state));
        }

        /// <summary>
        /// Register a Callback localy that will be used when a message contains a given Guid
        /// </summary>
        /// <param name="action"></param>
        /// <param name="guid"></param>
        public void RegisterOneTimeMessage(Action<MessageBase> action, Guid guid)
        {
            _onetimeupdated.Add(new Tuple<Action<MessageBase>, Guid>(action, guid));
        }

        /// <summary>
        /// Register a Callback localy that will be used when a Requst is inbound that has state in its InfoState
        /// </summary>
        /// <param name="action"></param>
        /// <param name="state"></param>
        public void RegisterRequstHandler(Func<RequstMessage, object> action, object state)
        {
            _requestHandler.Add(new Tuple<Func<RequstMessage, object>, object>(action, state));
        }


        public void UnRegisterRequstHandler(Func<RequstMessage, object> action, object state)
        {
            var enumerable = _requestHandler.FirstOrDefault(s => s.Item1 == action && state == s.Item2);
            if (enumerable != null)
            {
                _requestHandler.Remove(enumerable);
            }
        }

        public void UnRegisterRequstHandler(Func<RequstMessage, object> action)
        {
            var enumerable = _requestHandler.FirstOrDefault(s => s.Item1 == action);
            if (enumerable != null)
            {
                _requestHandler.Remove(enumerable);
            }
        }

        internal void RegisterRequst(Action<RequstMessage> action, Guid guid)
        {
            _pendingrequests.Add(new Tuple<Action<RequstMessage>, Guid>(action, guid));
        }

        internal void UnRegisterRequst(Guid guid)
        {
            var firstOrDefault = _pendingrequests.FirstOrDefault(s => s.Item2 == guid);
            if (firstOrDefault != null)
            {
                _pendingrequests.Remove(firstOrDefault);
            }
        }

        internal void UnRegisterCallback(Guid guid)
        {
            _onetimeupdated.Remove(_onetimeupdated.FirstOrDefault(s => s.Item2 == guid));
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

        internal void OnConnectRequest(IAsyncResult result)
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
                    var firstOrDefault = SharedConnectionManager.Instance.Connections.FirstOrDefault(s => endAccept == s.Item2);
                    if (firstOrDefault == null)
                    {
                        SharedConnectionManager.Instance.AddConnection(endAccept, this);
                    }
                }

                IDefaultTcpConnection conn;

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
                conn.BeginReceive();
            }
        }

        public override ushort Port { get; internal set; }
        public bool SharedConnection { get; set; }

        public TCPNetworkSender GetSharedSender(string ipOrHost)
        {
            var firstOrDefault = SharedConnectionManager.Instance.Connections.FirstOrDefault(s => s.Item1 == ipOrHost);
            if (firstOrDefault == null)
                return null;
            return firstOrDefault.Item4;
        }
    }
}