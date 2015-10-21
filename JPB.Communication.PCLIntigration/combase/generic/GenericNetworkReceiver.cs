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
using System.Threading;
using System.Threading.Tasks;
using JPB.Communication.ComBase.Messages;
using JPB.Communication.ComBase.Messages.Wrapper;
using JPB.Communication.ComBase.Security;
using JPB.Communication.Contracts;
using JPB.Communication.Contracts.Factorys;
using JPB.Communication.Contracts.Intigration;
using JPB.Communication.Shared.CrossPlatform;

namespace JPB.Communication.ComBase.Generic
{
    /// <summary>
    ///     A Manged wrapper with callback functions for a Socket
    ///     It will observe and serlilze the content of incomming data from the Socket
    /// </summary>
    public sealed class GenericNetworkReceiver : NetworkReceiverBase, IDisposable, INetworkReceiver
    {
        internal readonly ISocket _listenerISocket;
        
        private readonly Queue<Action> _workeritems;
        private AutoResetEvent _autoResetEvent;
        
        private bool _isWorking;
        GenericConnectionBase _assosciatedConnection;

        internal GenericNetworkReceiver(ushort port, ISocket sock) : base()
        {
            _listenerISocket = sock;
            _listenerISocket.Bind(new IPEndPoint() { Address = NetworkInfoBase.IpAddress, Port = port });
            _listenerISocket.Listen(5000);
            _listenerISocket.BeginAccept(OnConnectRequest, _listenerISocket);

            _workeritems = new Queue<Action>();
            //this is as far as i come, not generic
            _typeCallbacks.Add(typeof(RequstMessage), WorkOn_RequestMessage);

            OnNewItemLoadedSuccess += WrapSimpleItem;
            OnNewLargeItemLoadedSuccess += WrapLargeItem;
            Port = port;
        }

        internal GenericNetworkReceiver(ushort port, ISocketFactory factory)
            : this(port, factory.Create())
        {

        }

        [ThreadStatic]
        public ReceiverSession Session;

        public LoginMessage AuditInfos { get; set; }

        public bool CheckCredentials { get; set; }

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
        public event Func<GenericNetworkReceiver, ISocket, bool> OnCheckConnectionInbound;

        /// <summary>
        ///     Is raised when a message is inside the buffer but not fully parsed
        /// </summary>
        public new event EventHandler OnIncommingMessage;


        internal static GenericNetworkReceiver CreateReceiverInSharedState(ushort portInfo, ISocket basedOn)
        {
            var inst = new GenericNetworkReceiver(portInfo, NetworkFactory.PlatformFactory.SocketFactory.Create());
            inst.SharedConnection = true;
            inst.StartListener(basedOn);

            lock (NetworkFactory.Instance._mutex)
            {
                NetworkFactory.Instance._receivers.Add(portInfo, inst);
                NetworkFactory.Instance.RaiseReceiverCreate(inst);
            }

            return inst;
        }

        private void WrapSimpleItem(object mess, ushort port, MessageBaseInfo info)
        {
            OnNewItemLoadedSuccessHandler(mess, port, info);
        }

        private void WrapLargeItem(LargeMessage mess, ushort port)
        {
            OnNewItemLoadedSuccessHandler(mess, port, null);
        }

        private void OnNewItemLoadedSuccessHandler(object mess, ushort port, MessageBaseInfo info)
        {
            if (port == Port)
            {
                object messCopy = mess;
                _workeritems.Enqueue(() =>
                {
                    Session = new ReceiverSession()
                    {
                        NetworkReceiver = this,
                        Calle = NetworkAuthentificator.Instance.GetUser(_assosciatedConnection._assosciatedLogin),
                        Sock = this._listenerISocket,
                        PendingItems = _workeritems.Count,
                        Sender = _assosciatedConnection.Sock.RemoteEndPoint.Address.AddressContent,
                        Receiver = _assosciatedConnection.Sock.LocalEndPoint.Address.AddressContent,
                    };

                    Type type = messCopy.GetType();

                    //make type check if a type handler for the MessageBase object or one inhert object is defined use that
                    KeyValuePair<Type, Action<object>> handler = _typeCallbacks.FirstOrDefault(s => s.Key == type);

                    if (!default(KeyValuePair<Type, Action<object>>).Equals(handler))
                    {
                        handler.Value(messCopy);
                    }
                    else
                    {
                        //otherwise use the Unkown Message handler
                        foreach (var item in _unkownCallbacks)
                        {
                            if(item(messCopy))
                            {
                                break;
                            }
                        }
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


        private async void WorkOn_RequestMessage(object messCopy)
        {
            //message with return value inbound
            var requstInbound = messCopy as RequstMessage;
            if (requstInbound == null)
                return;

            var sender = NetworkFactory.Instance.GetSender(requstInbound.ExpectedResult);

            Tuple<Func<RequstMessage, object>, object>[] firstOrDefault =
                _requestHandler.Where(pendingrequest => pendingrequest.Item2.Equals(requstInbound.InfoState)).ToArray();
            var senderName = Session.Sender;

            if (firstOrDefault.Any())
            {
                object result = null;

                foreach (var tuple in firstOrDefault)
                {
                    //Found a handler for that message and executed it
                    PclTimer timer = null;

                    try
                    {
                        if (AutoRespond)
                        {
                            timer = new PclTimer(s =>
                            {
                                if (result != null)
                                    return;

                                sender.SendNeedMoreTimeBackAsync(new RequstMessage
                                {
                                    ResponseFor = requstInbound.Id
                                }, senderName);
                            }, null, 10000, 10000);
                        }

                        result = tuple.Item1(requstInbound);
                    }
                    catch (Exception)
                    {
                        result = new object();
                    }
                    finally
                    {
                        if (timer != null)
                        {
                            timer.Dispose();
                        }
                    }

                    if (result != null)
                        break;
                }

                if (result == null)
                    result = new object();

                await sender.SendMessageAsync(new RequstMessage
                {
                    Message = result,
                    ResponseFor = requstInbound.Id,
                    InfoState = requstInbound.InfoState
                }, senderName);
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
                    await sender.SendMessageAsync(new RequstMessage
                    {
                        Message = new object(),
                        ResponseFor = requstInbound.Id
                    }, senderName);
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

        private void WorkOnItems()
        {
            try
            {
                _autoResetEvent = new AutoResetEvent(false);
                while (_workeritems.Count > 0)
                {
                    if (IsDisposing)
                        break;

                    Action action = _workeritems.Dequeue();
                    if (action != null)
                        action.BeginInvoke(s => { }, null);
                }
                _autoResetEvent.Set();
            }
            catch
            {
                PclTrace.WriteLine("WorkOnItems has stop working");
            }
            finally
            {
                _isWorking = false;
            }
        }

        private void OnConnectRequest(IAsyncResult result)
        {
            IncommingMessage = true;
            var sock = ((ISocket)result.AsyncState);
            try
            {

                var acceptConnection = RaiseConnectionInbound(sock);
                if (!acceptConnection)
                {
                    sock.Dispose();
                    return;
                }

                var endAccept = sock.EndAccept(result);
                if (endAccept == null)
                {
                    //Fatal error clear connection
                    sock.Dispose();
                    return;
                }

                StartListener(endAccept);
                // Tell the listener Socket to start listening again.
                sock.BeginAccept(OnConnectRequest, sock);
            }
            catch (Exception)
            {
                IncommingMessage = false;
                //Fatal error clear connection
                sock.Dispose();
            }
        }

        internal void StartListener(ISocket endAccept)
        {
            if (!LargeMessageSupport)
            {
                _assosciatedConnection = new DefaultGenericConnection(endAccept)
                {
                    Port = Port,
                };
            }
            else
            {
                _assosciatedConnection = new LargeMessageHandler(endAccept)
                {
                    Port = Port
                };
            }

            if (SharedConnection)
            {
                ConnectionWrapper firstOrDefault =
                    ConnectionPool.Instance.Connections.FirstOrDefault(s => endAccept == s.Socket);
                if (firstOrDefault == null)
                {
                    var connection = ConnectionPool.Instance.AddConnection(endAccept, this);
                    RaiseSharedConnectionCreated(connection);
                }
            }

            _assosciatedConnection.Serlilizer = Serlilizer;
            _assosciatedConnection.IsSharedConnection = SharedConnection;
            _assosciatedConnection.EndReceiveInternal += (e, ef) => IncommingMessage = false;
            _assosciatedConnection.BeginReceive(CheckCredentials);
        }

        public event Action<ConnectionWrapper> OnSharedConnectionCreated;

        private void RaiseSharedConnectionCreated(ConnectionWrapper connection)
        {
            var hander = OnSharedConnectionCreated;
            if (hander != null)
                hander(connection);
        }


        /// <summary>
        ///     Returns a Sender or null
        /// </summary>
        /// <param name="ipOrHost"></param>
        /// <returns></returns>
        public GenericNetworkSender GetFirstSharedSenderOrNull(string ipOrHost)
        {
            ConnectionWrapper firstOrDefault = ConnectionPool.Instance.Connections.FirstOrDefault(s => s.Ip == ipOrHost);
            if (firstOrDefault == null)
                return null;
            return firstOrDefault.GenericNetworkSender;
        }

        public GenericNetworkSender GetFirstSharedSenderOrNull(string ipOrHost, ushort port)
        {
            ConnectionWrapper firstOrDefault =
                ConnectionPool.Instance.Connections.FirstOrDefault(
                    s => s.Ip == ipOrHost && s.GenericNetworkSender.Port == port);
            if (firstOrDefault == null)
                return null;
            return firstOrDefault.GenericNetworkSender;
        }

        private void CheckDisposedObjectOrClosedSocket()
        {
            if (IsDisposing)
            {
                string reason = "Unknown reason";
                throw new ObjectDisposedException("GenericNetworkReceiver", "This object is disposed you cannot access it anymore.")
                {
                    Source = reason
                };
            }
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

            if (SharedConnection)
            {
                var connec = ConnectionPool.Instance.Connections.FirstOrDefault(s => s.GenericNetworkReceiver == this);
                if (connec == null) return;

                connec.Socket.Close();
                connec.GenericNetworkSender.Dispose();
            }
        }

        #endregion
    }
}