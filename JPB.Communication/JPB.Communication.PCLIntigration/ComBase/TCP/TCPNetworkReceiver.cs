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
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using JPB.Communication.ComBase.Messages;
using JPB.Communication.ComBase.Messages.Wrapper;
using JPB.Communication.Contracts;
using JPB.Communication.PCLIntigration.Contracts;

namespace JPB.Communication.ComBase.TCP
{
    /// <summary>
    ///     A Manged wrapper with callback functions for a Socket
    ///     It will observe and serlilze the content of incomming data from the Socket
    /// </summary>
    public sealed class TCPNetworkReceiver : Networkbase, IDisposable, INetworkReceiver
    {
        private readonly List<Tuple<Action<LargeMessage>, object>> _largeMessages;
        internal readonly ISocket _listenerISocket;
        private readonly List<Tuple<Action<MessageBase>, Guid>> _onetimeupdated;
        private readonly List<Tuple<Action<RequstMessage>, Guid>> _pendingrequests;

        private readonly List<Tuple<Func<RequstMessage, object>, object>> _requestHandler;

        private readonly List<Tuple<Action<MessageBase>, object>> _updated;
        private readonly Queue<Action> _workeritems;
        private AutoResetEvent _autoResetEvent;

        private bool _incommingMessage;
        private bool _isWorking;

        /// <summary>
        ///     FOR INTERNAL USE ONLY
        /// </summary>
        internal Dictionary<Type, Action<object>> _typeCallbacks;

        internal TCPNetworkReceiver(ushort port, ISocket sock)
        {
            _listenerISocket = sock;
            _listenerISocket.Bind(new IPEndPoint() { Address = NetworkInfoBase.IpAddress, Port = port });
            _listenerISocket.Listen(5000);
            _listenerISocket.BeginAccept(OnConnectRequest, _listenerISocket);
        }

        internal TCPNetworkReceiver(ushort port, ISocketFactory factory)
            : this(port, factory.Create())
        {
            _largeMessages = new List<Tuple<Action<LargeMessage>, object>>();
            _onetimeupdated = new List<Tuple<Action<MessageBase>, Guid>>();
            _pendingrequests = new List<Tuple<Action<RequstMessage>, Guid>>();
            _requestHandler = new List<Tuple<Func<RequstMessage, object>, object>>();
            _updated = new List<Tuple<Action<MessageBase>, object>>();
            _workeritems = new Queue<Action>();

            OnNewItemLoadedSuccess += TcpConnectionOnOnNewItemLoadedSuccess;
            OnNewLargeItemLoadedSuccess += TcpConnectionOnOnNewItemLoadedSuccess;
            Port = port;

            _typeCallbacks = new Dictionary<Type, Action<object>>();
            _typeCallbacks.Add(typeof(RequstMessage), WorkOn_RequestMessage);
            _typeCallbacks.Add(typeof(MessageBase), WorkOn_MessageBase);
            _typeCallbacks.Add(typeof(LargeMessage), WorkOn_LargeMessage);

        }

        /// <summary>
        ///     If Enabled this Receiver can handle streams and messages
        /// </summary>
        public bool LargeMessageSupport { get; set; }

        /// <summary>
        ///     Enables or Disable the Auto Respond for long working Requests
        /// </summary>
        public bool AutoRespond { get; set; }

        /// <summary>
        /// </summary>
        public bool IsDisposing { get; private set; }

        /// <summary>
        ///     If enabled a Incomming connection will be kept open and will be used for Outgoing and Incomming Trafic to that host
        /// </summary>
        public bool SharedConnection { get; set; }

        public override ushort Port { get; internal set; }

        /// <summary>
        ///     True if we are Recieving a message
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
        ///     Removes a delegate from the Handler list
        /// </summary>
        /// <param name="action"></param>
        public void UnregisterChanged(Action<MessageBase> action, object state)
        {
            Tuple<Action<MessageBase>, object> enumerable =
                _updated.FirstOrDefault(s => s.Item1 == action && s.Item2 == state);
            if (enumerable != null)
            {
                _updated.Remove(enumerable);
            }
        }

        /// <summary>
        ///     Removes a delegate from the Handler list
        /// </summary>
        /// <param name="action"></param>
        public void UnregisterChanged(Action<MessageBase> action)
        {
            Tuple<Action<MessageBase>, object> enumerable = _updated.FirstOrDefault(s => s.Item1 == action);
            if (enumerable != null)
            {
                _updated.Remove(enumerable);
            }
        }

        /// <summary>
        ///     Register a Callback localy that will be used when a new message is inbound that has state in its InfoState
        /// </summary>
        /// <param name="action">Callback</param>
        /// <param name="state">Maybe an Enum?</param>
        public void RegisterMessageBaseInbound(Action<MessageBase> action, object state)
        {
            _updated.Add(new Tuple<Action<MessageBase>, object>(action, state));
        }

        /// <summary>
        ///     Register a Callback localy that will be used when a new Large message is inbound that has state in its InfoState
        /// </summary>
        /// <param name="action">Callback</param>
        /// <param name="state">Maybe an Enum?</param>
        public void RegisterMessageBaseInbound(Action<LargeMessage> action, object state)
        {
            _largeMessages.Add(new Tuple<Action<LargeMessage>, object>(action, state));
        }

        /// <summary>
        ///     Register a Callback localy that will be used when a message contains a given Guid
        /// </summary>
        /// <param name="action"></param>
        /// <param name="guid"></param>
        public void RegisterOneTimeMessage(Action<MessageBase> action, Guid guid)
        {
            _onetimeupdated.Add(new Tuple<Action<MessageBase>, Guid>(action, guid));
        }

        /// <summary>
        ///     Register a Callback localy that will be used when a Requst is inbound that has state in its InfoState
        /// </summary>
        /// <param name="action"></param>
        /// <param name="state"></param>
        public void RegisterRequstHandler(Func<RequstMessage, object> action, object state)
        {
            _requestHandler.Add(new Tuple<Func<RequstMessage, object>, object>(action, state));
        }

        /// <summary>
        ///     Removes a delegate from the Handler list
        /// </summary>
        /// <param name="action"></param>
        public void UnRegisterRequstHandler(Func<RequstMessage, object> action, object state)
        {
            Tuple<Func<RequstMessage, object>, object> enumerable =
                _requestHandler.FirstOrDefault(s => s.Item1 == action && state == s.Item2);
            if (enumerable != null)
            {
                _requestHandler.Remove(enumerable);
            }
        }

        /// <summary>
        ///     Removes a delegate from the Handler list
        /// </summary>
        /// <param name="action"></param>
        public void UnRegisterRequstHandler(Func<RequstMessage, object> action)
        {
            Tuple<Func<RequstMessage, object>, object> enumerable =
                _requestHandler.FirstOrDefault(s => s.Item1 == action);
            if (enumerable != null)
            {
                _requestHandler.Remove(enumerable);
            }
        }

        internal static TCPNetworkReceiver CreateReceiverInSharedState(ushort portInfo, ISocket sock)
        {
            var inst = new TCPNetworkReceiver(portInfo, sock);
            inst.StartListener(sock);

            lock (NetworkFactory.Instance._mutex)
            {
                NetworkFactory.Instance._receivers.Add(portInfo, inst);
                NetworkFactory.Instance.RaiseReceiverCreate(inst);
            }

            return inst;
        }

        public event Func<TCPNetworkReceiver, ISocket, bool> OnCheckConnectionInbound;

        /// <summary>
        ///     Is raised when a message is inside the buffer but not fully parsed
        /// </summary>
        public event EventHandler OnIncommingMessage;

        private void TcpConnectionOnOnNewItemLoadedSuccess(object mess, ushort port)
        {
            if (port == Port)
            {
                IncommingMessage = false;
                object messCopy = mess;
                _workeritems.Enqueue(() =>
                {
                    Type type = messCopy.GetType();

                    KeyValuePair<Type, Action<object>> handler = _typeCallbacks.FirstOrDefault(s => s.Key == type);

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

        private void WorkOn_LargeMessage(object metaData)
        {
            var messCopy = metaData as LargeMessage;

            Tuple<Action<LargeMessage>, object>[] updateCallbacks =
                _largeMessages.Where(
                    action =>
                        messCopy != null && (action.Item2 == null || action.Item2.Equals(messCopy.MetaData.InfoState)))
                    .ToArray();
            foreach (var action in updateCallbacks)
            {
                action.Item1.BeginInvoke(messCopy, e => { }, null);
            }
        }

        private void WorkOn_MessageBase(object message)
        {
            var messCopy = message as MessageBase;

            Tuple<Action<MessageBase>, object>[] updateCallbacks =
                _updated.Where(
                    action => messCopy != null && (action.Item2 == null || action.Item2.Equals(messCopy.InfoState)))
                    .ToArray();
            foreach (var action in updateCallbacks)
            {
                action.Item1.BeginInvoke(messCopy, e => { }, null);
            }

            //Go through all one time items and check for ID
            Tuple<Action<MessageBase>, Guid>[] oneTimeImtes =
                _onetimeupdated.Where(s => messCopy != null && messCopy.Id == s.Item2).ToArray();

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

            TCPNetworkSender sender = NetworkFactory.Instance.GetSender(requstInbound.ExpectedResult);

            Tuple<Func<RequstMessage, object>, object>[] firstOrDefault =
                _requestHandler.Where(pendingrequest => pendingrequest.Item2.Equals(requstInbound.InfoState)).ToArray();
            if (firstOrDefault.Any())
            {
                object result = null;

                foreach (var tuple in firstOrDefault)
                {
                    //Found a handler for that message and executed it
                    Task waiter = null;
                    //Timer timer = null;

                    try
                    {
                        if (AutoRespond)
                        {
                            //timer = new Timer((s) =>
                            //{
                            //    if (result != null)
                            //        return;

                            //    sender.SendNeedMoreTimeBackAsync(new RequstMessage
                            //    {
                            //        ResponseFor = requstInbound.Id
                            //    }, requstInbound.Sender);
                            //}, result, 10000, 10000);
                        }

                        result = tuple.Item1(requstInbound);
                    }
                    catch (Exception)
                    {
                        result = null;
                    }
                    finally
                    {
                        //if (waiter != null)
                        //{
                        //    timer.Dispose();
                        //}
                    }

                    if (result != null)
                        break;
                }

                if (result == null)
                    return;

                sender.SendMessageAsync(new RequstMessage
                {
                    Message = result,
                    ResponseFor = requstInbound.Id
                }, requstInbound.Sender);
            }
            else
            {
                Tuple<Action<RequstMessage>, Guid> awnser =
                    _pendingrequests.FirstOrDefault(
                        pendingrequest => pendingrequest.Item2.Equals(requstInbound.ResponseFor));
                if (awnser != null)
                {
                    //This is an awnser
                    awnser.Item1(requstInbound);
                    _pendingrequests.Remove(awnser);
                }
                else
                {
                    //No ones listening ... send null as Defauld
                    sender.SendMessageAsync(new RequstMessage
                    {
                        Message = null,
                        ResponseFor = requstInbound.Id
                    }, requstInbound.Sender);
                }
            }
        }

        private bool RaiseConnectionInbound(ISocket sock)
        {
            var handler = OnCheckConnectionInbound;
            if (handler != null)
                return handler(this, sock);
            return true;
        }

        /// <summary>
        ///     Is raised when a message is inside the buffer but not fully parsed
        /// </summary>
        private void RaiseIncommingMessage()
        {
            EventHandler handler = OnIncommingMessage;
            if (handler != null)
                handler(this, EventArgs.Empty);
        }

        internal void RegisterRequst(Action<RequstMessage> action, Guid guid)
        {
            _pendingrequests.Add(new Tuple<Action<RequstMessage>, Guid>(action, guid));
        }

        internal void UnRegisterRequst(Guid guid)
        {
            Tuple<Action<RequstMessage>, Guid> firstOrDefault = _pendingrequests.FirstOrDefault(s => s.Item2 == guid);
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

                Action action = _workeritems.Dequeue();
                action.BeginInvoke(s => { }, null);
            }
            _autoResetEvent.Set();
        }

        private void OnConnectRequest(IAsyncResult result)
        {
            IncommingMessage = true;
            var sock = ((ISocket)result.AsyncState);

            var endAccept = sock.EndAccept(result);

            StartListener(endAccept);
            // Tell the listener Socket to start listening again.
            sock.BeginAccept(OnConnectRequest, sock);
        }

        internal void StartListener(ISocket endAccept)
        {
            if (RaiseConnectionInbound(endAccept))
            {
                if (SharedConnection)
                {
                    ConnectionWrapper firstOrDefault =
                        ConnectionPool.Instance.Connections.FirstOrDefault(s => endAccept == s.Socket);
                    if (firstOrDefault == null)
                    {
                        ConnectionPool.Instance.AddConnection(endAccept, this);
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
                conn.Serlilizer = Serlilizer;
                conn.BeginReceive();
            }
        }


        /// <summary>
        ///     Returns a Sender or null
        /// </summary>
        /// <param name="ipOrHost"></param>
        /// <returns></returns>
        public TCPNetworkSender GetFirstSharedSenderOrNull(string ipOrHost)
        {
            ConnectionWrapper firstOrDefault = ConnectionPool.Instance.Connections.FirstOrDefault(s => s.Ip == ipOrHost);
            if (firstOrDefault == null)
                return null;
            return firstOrDefault.TCPNetworkSender;
        }

        public TCPNetworkSender GetFirstSharedSenderOrNull(string ipOrHost, ushort port)
        {
            ConnectionWrapper firstOrDefault =
                ConnectionPool.Instance.Connections.FirstOrDefault(
                    s => s.Ip == ipOrHost && s.TCPNetworkSender.Port == port);
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
            if (_listenerISocket != null)
                _listenerISocket.Dispose();

            NetworkFactory.Instance._receivers.Remove(Port);
        }

        #endregion
    }
}