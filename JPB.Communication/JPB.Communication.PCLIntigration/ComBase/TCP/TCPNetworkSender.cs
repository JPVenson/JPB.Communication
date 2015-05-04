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

#region Jean-Pierre Bachmann

// Erstellt von Jean-Pierre Bachmann am 10:11

#endregion

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JPB.Communication.ComBase.Messages;
using JPB.Communication.Contracts.Factorys;
using JPB.Communication.Contracts.Intigration;
using JPB.Communication.PCLIntigration.ComBase.Messages;
using JPB.Communication.Shared.CrossPlatform;

namespace JPB.Communication.ComBase.TCP
{
    /// <summary>
    ///     A TCP sender
    /// </summary>
    public sealed class TCPNetworkSender : Networkbase, IDisposable
    {
        /// <summary>
        ///     Thread static
        /// </summary>
        [ThreadStatic]
        public static Exception LastException;

        private readonly ISocketFactory _sockType;

        internal TCPNetworkSender(ushort port, ISocketFactory sockType)
        {
            _sockType = sockType;
            Port = port;
            Timeout = TimeSpan.FromSeconds(15);
        }

        /// <summary>
        ///     The timeout for wating on Request messages callback
        ///     Warning
        ///     Setting this below 10 will maybe cause in timeout problems
        /// </summary>
        public TimeSpan Timeout { get; private set; }

        /// <summary>
        ///     If set to true all future calls to a Remote host will be keept open and will be stored inside the ConnectionPool
        /// </summary>
        public bool SharedConnection { get; set; }

        public override ushort Port { get; internal set; }

        public bool UseNetworkCredentials { get; private set; }
        public byte[] PreCompiledLogin { get; private set; }

        public void ChangeNetworkCredentials(bool mode, LoginMessage mess)
        {
            UseNetworkCredentials = mode;
            if (mode)
            {
                PreCompiledLogin = SerializeLogin(mess);
            }
            else
            {
                PreCompiledLogin = null;
            }
        }

        /// <summary>
        ///     Will be invoked to observe Exceptions
        /// </summary>
        public static event Action<Exception> OnCriticalException;

        private static void SetException(object sender, Exception e)
        {
            LastException = e;
            RaiseCriticalException(sender, e);
        }

        private static void RaiseCriticalException(object sender, Exception e)
        {
            Action<Exception> handler = OnCriticalException;
            if (handler != null)
                handler(e);
        }

        /// <summary>
        ///     Checks for an existing connection in the ConnectionPool
        /// </summary>
        /// <param name="ipOrHost"></param>
        /// <returns></returns>
        public bool ConnectionOpen(string ipOrHost)
        {
            if (!SharedConnection)
                return false;

            ISocket sockForIpOrNull = ConnectionPool.Instance.GetSockForIpOrNull(ipOrHost);
            if (sockForIpOrNull == null)
                return false;

            return sockForIpOrNull.Connected;
        }

        #region Message Methods

        /// <summary>
        ///     Sends a message to a Given IP:Port and wait for Deliver
        /// </summary>
        /// <param name="message">Instance of message</param>
        /// <param name="ip">Ip of sock pc</param>
        /// <param name="port">Port of sock pc</param>
        /// <returns></returns>
        /// <exception cref="TimeoutException"></exception>
        public static async Task SendMessage(MessageBase message, string ip, ushort port)
        {
            await SendMessageAsync(message, ip, port);
        }

        /// <summary>
        ///     Sends a message async to a IP:Port
        /// </summary>
        /// <param name="message">Instance of message</param>
        /// <param name="ip">Ip of sock pc</param>
        /// <param name="port">Port of sock pc</param>
        /// <returns></returns>
        /// <exception cref="TimeoutException"></exception>
        public static Task SendMessageAsync(MessageBase message, string ip, ushort port)
        {
            TCPNetworkSender sender = NetworkFactory.Instance.GetSender(port);
            return sender.SendMessageAsync(message, ip);
        }

        /// <summary>
        ///     Sends a message to multible Hosts
        /// </summary>
        /// <returns>all non reached hosts</returns>
        public Task<IEnumerable<string>> SendMultiMessageAsync(MessageBase message, params string[] ips)
        {
            var gentask = new Task<IEnumerable<string>>(() =>
            {
                if (message == null)
                    throw new ArgumentNullException("message");
                if (ips == null)
                    throw new ArgumentNullException("ips");

                var failedMessages = new List<string>();

                Task<bool>[] runningMessages =
                    ips.Select(ip => SendMessageAsync(message.Clone() as MessageBase, ip)).ToArray();

                for (int i = 0; i < runningMessages.Length; i++)
                {
                    try
                    {
                        Task<bool> task = runningMessages[i];
                        task.Wait();
                        if (!task.Result)
                        {
                            failedMessages.Add(ips[i]);
                        }
                    }
                    catch (Exception)
                    {
                        failedMessages.Add(ips[i]);
                    }
                }

                return failedMessages;
            });
            gentask.Start();
            return gentask;
        }

        /// <summary>
        ///     Sends a message to multible Hosts
        /// </summary>
        /// <returns>all non reached hosts</returns>
        public IEnumerable<string> SendMultiMessage(MessageBase message, params string[] ips)
        {
            Task<IEnumerable<string>> send = SendMultiMessageAsync(message, ips);
            send.Wait();
            return send.Result;
        }

        /// <summary>
        ///     Sends one message to one sock and wait for deliver
        /// </summary>
        /// <param name="message">Message object or inherted object</param>
        /// <param name="ip">Ip of sock</param>
        /// <returns>frue if message was successful delivered otherwise false</returns>
        /// <exception cref="TimeoutException"></exception>
        public bool SendMessage(MessageBase message, string ip)
        {
            Task<bool> sendMessageAsync = SendMessageAsync(message, ip);
            sendMessageAsync.Wait();
            bool b = sendMessageAsync.Result;
            return b;
        }

        /// <summary>
        ///     Sends one message to one sock async
        /// </summary>
        /// <param name="message">Message object or inherted object</param>
        /// <param name="ip">Ip of sock</param>
        /// <returns>frue if message was successful delivered otherwise false</returns>
        public Task<bool> SendMessageAsync(MessageBase message, string ip)
        {
            //var callee = Thread.CurrentContext;
            var task = new Task<bool>(() =>
            {
                try
                {
                    Task<ISocket> client = CreateClientSockAsync(ip);
                    MessageBase tcpMessage = PrepareMessage(message, ip);
                    client.Wait();
                    var result = client.Result;
                    if (result == null)
                        return false;
                    SendBaseAsync(tcpMessage, result);
                    RaiseMessageSended(message);
                    return true;
                }
                catch (Exception e)
                {
                    //if (callee != null)
                    //    callee.DoCallBack(() =>
                    //    {
                    SetException(this, e);
                    //});
                    return false;
                }
            });
            task.Start();
            return task;
        }

        internal void SendNeedMoreTimeBackAsync(RequstMessage mess, string ip)
        {
            mess.NeedMoreTime = 20000;
            SendMessageAsync(mess, ip);
        }


        /// <summary>
        ///     Sends a message an awaits a response from the other side
        /// </summary>
        /// <param name="mess">Message object or inherted object</param>
        /// <param name="ip">Ip of sock</param>
        /// <returns>Result from other side or default(T)</returns>
        /// <exception cref="TimeoutException"></exception>
        public Task<T> SendRequstMessageAsync<T>(RequstMessage mess, string ip)
        {
            var task = new Task<T>(() =>
            {
                if (mess.ExpectedResult == default(ushort) || mess.ExpectedResult == 0)
                {
                    mess.ExpectedResult = Port;
                }

                T result = default(T);
                AutoResetEvent waitForResponsive;
                using (waitForResponsive = new AutoResetEvent(false))
                {
                    TCPNetworkReceiver reciever = NetworkFactory.Instance.GetReceiver(mess.ExpectedResult);

                    long moreTime = 0L;
                    //register a callback that is filtered by the Guid we send inside our requst
                    Action<RequstMessage> ack = null;
                    ack = s =>
                    {
                        if (s.NeedMoreTime > 0)
                        {
                            moreTime = s.NeedMoreTime;
                            //reregister request
                            reciever.RegisterRequst(ack, mess.Id);
                            return;
                        }

                        if (s.Message is T)
                            result = (T)s.Message;
                        if (waitForResponsive != null)
                            waitForResponsive.Set();
                    };

                    reciever.RegisterRequst(ack, mess.Id);
                    bool isSend = SendMessage(mess, ip);
                    if (isSend)
                    {
                        waitForResponsive.WaitOne(Timeout);
                        while (moreTime > 0)
                        {
                            long mT = moreTime;
                            moreTime = 0;
                            waitForResponsive.WaitOne(TimeSpan.FromMilliseconds(mT));
                        }

                        //are we still receiving?
                        while (reciever.IncommingMessage)
                        {
                            waitForResponsive.WaitOne(Timeout);
                        }
                    }
                    reciever.UnRegisterCallback(mess.Id);
                }
                waitForResponsive = null;
                return result;
            });
            task.Start();
            return task;
        }

        /// <summary>
        ///     Sends a message an awaits a response on the same port from the other side
        /// </summary>
        /// <param name="mess">Message object or inherted object</param>
        /// <param name="ip">Ip of sock</param>
        /// <returns>Result from other side or default(T)</returns>
        /// <exception cref="TimeoutException"></exception>
        public async Task<T> SendRequstMessage<T>(RequstMessage mess, string ip)
        {
            return await SendRequstMessageAsync<T>(mess, ip);
        }

        /// <summary>
        ///     Sends a message an awaits a response on the same port from the other side
        /// </summary>
        /// <param name="mess">Message object or inherted object</param>
        /// <param name="ip">Ip of sock</param>
        /// <returns>Result from other side or default(T)</returns>
        /// <exception cref="TimeoutException"></exception>
        public async Task<T> SendRequstMessage<T>(object mess, object infoState, string ip)
        {
            return await SendRequstMessageAsync<T>(new RequstMessage()
            {
                Message = mess,
                InfoState = infoState
            }, ip);
        }

        /// <summary>
        ///     Sends one message COPY to each ipOrHost and awaits from all a result or nothing
        /// </summary>
        /// <typeparam name="T">the result we await</typeparam>
        /// <param name="mess">Message object or inherted object</param>
        /// <param name="ips">Ips of sock</param>
        /// <returns>A Dictiornary that contains for each key ( IP ) the result we got</returns>
        public Dictionary<string, T> SendMultiRequestMessage<T>(RequstMessage mess, string[] ips)
        {
            string[] enumerable = ips.Distinct().ToArray();

            Task<T>[] pendingRequests =
                enumerable.Select(ip => SendRequstMessageAsync<T>(mess.Clone() as RequstMessage, ip)).ToArray();

            Task.WaitAll(pendingRequests);

            var results = new Dictionary<string, T>();

            for (int i = 0; i < enumerable.Length; i++)
            {
                results.Add(enumerable[i], pendingRequests[i].Result);
            }

            return results;
        }

        /// <summary>
        ///     This mehtod will send the content of the given stream to the given ip
        ///     To Support this the Remote host must set the TCP Reciever property SupportLargeMessages
        ///     to true otherwise the message will be ignored
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="mess"></param>
        /// <param name="ip"></param>
        /// <param name="disposeOnEnd">Close and Dispose the stream after work</param>
        /// <exception cref="NotImplementedException"></exception>
        public async void SendStreamDataAsync(Stream stream, StreamMetaMessage mess, string ip, bool disposeOnEnd = true)
        {
            var client = await CreateClientSockAsync(ip);
            if (client == null)
                return;

            mess.StreamSize = stream.Length;

            var prepairedMess = PrepareMessage(mess, ip);
            byte[] serialize = Serialize(prepairedMess);

            using (var memstream = new MemoryStream(serialize))
            {
                var openNetwork = SendOnStream(memstream, client);

                AwaitCallbackFromRemoteHost(client, true);

                SendOnStream(stream, client);

                AwaitCallbackFromRemoteHost(client, false);

                if (!SharedConnection)
                {
                    openNetwork.Send(new byte[0]);
                    openNetwork.Close();
                    openNetwork.Dispose();
                }
            }

            if (disposeOnEnd)
            {
                stream.Dispose();
                stream = null;
            }
        }

        #endregion

        #region Base Methods

        /// <summary>
        ///     If set to true the external IP of this host will be used as Sender property
        /// </summary>
        public bool UseExternalIpAsSender { get; set; }

        /// <summary>
        ///     Starts the injection of the given Socket into the ConnectionPool
        /// </summary>
        /// <param name="ipOrHost"></param>
        /// <returns></returns>
        public async Task<TCPNetworkReceiver> InitSharedConnection(ISocket ipOrHost)
        {
            if (!ipOrHost.Connected)
            {
                throw new ArgumentException("The Socket must be connected");
            }
            SharedConnection = true;
            return ConnectionPool.Instance.InjectISocket(ipOrHost, this);
        }

        /// <summary>
        ///     Will create a new Connection from this Pc to the IpOrHost pc
        ///     When done and Successfull it will return a new Receiver instance that can be used to observe messages from the
        ///     Remote host
        /// </summary>
        /// <param name="ipOrHost"></param>
        /// <returns></returns>
        public async Task<TCPNetworkReceiver> InitSharedConnection(string ipOrHost)
        {
            SharedConnection = true;
            return await ConnectionPool.Instance.ConnectToHost(ipOrHost, Port);
        }

        internal async Task<ISocket> _InitSharedConnection(string ip)
        {
            SharedConnection = true;
            return await CreateClientSockAsync(ip);
        }

        private MessageBase PrepareMessage(MessageBase message, string ip)
        {
            message.SendAt = DateTime.Now;
            if (string.IsNullOrEmpty(message.Sender) || !UseExternalIpAsSender)
                message.Sender = NetworkInfoBase.IpAddress.ToString();
            if (UseExternalIpAsSender)
            {
                message.Sender = NetworkInfoBase.IpAddressExternal.ToString();
            }
            return message;
        }

        private async Task<ISocket> CreateClientSockAsync(string ipOrHost)
        {
            try
            {
                ISocket sock;
                if (SharedConnection)
                {
                    sock = ConnectionPool.Instance.GetSock(ipOrHost, Port);
                    if (sock != null)
                    {
                        if (!sock.Connected)
                        {
                            sock.Connect(ipOrHost, Port);
                        }
                    }
                    else
                    {
                        sock = await _sockType.CreateAndConnectAsync(ipOrHost, Port);
                    }
                }
                else
                {
                    sock = await _sockType.CreateAndConnectAsync(ipOrHost, Port);
                }

                if (UseNetworkCredentials)
                {
                    SendLoginData(sock);
                    if (!sock.Connected)
                    {
                        sock.Close();
                        sock.Dispose();
                    }
                }

                return sock;
            }
            catch (Exception)
            {
                return null;
            }
        }

        private void AwaitCallbackFromRemoteHost(ISocket sock, bool wait)
        {
            sock.ReceiveTimeout = Timeout.Milliseconds;
            do
            {
                int tryCount = 0;
                int tryMax = 2;

                //ok due th fact that we are sometimes faster then the remote PC
                //we need to send maybe Multible times the zero byte message
                //normal messages are not effected

                tryCount++;
                try
                {
                    sock.Send(0x01);
                    //Thread.Sleep(150);
                    //Thread.Yield();
                    if (wait)
                    {
                        //sock.Receive(new byte[] { 0x00 });
                    }
                    return;
                }
                catch
                {
                    if (tryCount >= tryMax || !sock.Connected)
                    {
                        throw;
                    }
                    PclTrace.WriteLine(string.Format("TCPSender> awaits callback from remote pc try {0} of {1}", tryCount, tryMax), Networkbase.TraceCategoryCriticalSerilization);

                    sock.Send(new byte[0]);
                    continue;
                }
            } while (true);
        }

        private void SendBaseAsync(MessageBase message, ISocket openNetwork)
        {
            byte[] serialize = Serialize(message);
            if (!serialize.Any())
                return;

            lock (this)
            {
                using (var memstream = new MemoryStream(serialize))
                    SendOnStream(memstream, openNetwork);
            }

            AwaitCallbackFromRemoteHost(openNetwork, false);
            if (!SharedConnection)
            {
                openNetwork.Send(new byte[0]);
                openNetwork.Close();
                openNetwork.Dispose();
            }
        }

        private void SendLoginData(ISocket sock)
        {
            //int bufSize = sock.ReceiveBufferSize;
            //var buf = new byte[bufSize];
            //PreCompiledLogin.CopyTo(buf, 0);
            sock.Send(PreCompiledLogin, 0, PreCompiledLogin.Length);
        }

        private ISocket SendOnStream(Stream stream, ISocket sock)
        {
            int bufSize = sock.ReceiveBufferSize;
            var buf = new byte[bufSize];
            int read = 0;
            long send = 0;
            int lastOverZeroRead = 0;
            do
            {
                read = stream.Read(buf, 0, bufSize);
                send = sock.Send(buf, 0, read);
                if (read > 0)
                    lastOverZeroRead = read;
            } while (read > 0);

            //if the last send would be a Full packet
            //the remote maschine would still guess that there are data open
            //check the last read to be a full packet if so send 0x01 to open a new packet
            if (SharedConnection)
            {
                if (lastOverZeroRead == bufSize)
                {
                    sock.Send(0x01);
                }
                else
                {
                    var remaining = bufSize - lastOverZeroRead - 1;

                    var buff = new byte[remaining];
                    for (int i = 0; i < remaining; i++)
                    {
                        buff[i] = 0x00;
                    }
                    sock.Send(buff, 0, remaining);
                }
            }

            return sock;
        }

        #endregion

        public void Dispose()
        {
            NetworkFactory.Instance._senders.Remove(Port);

            //remove shared Collection and close

            if (SharedConnection)
            {
                var connec = ConnectionPool.Instance.Connections.FirstOrDefault(s => s.TCPNetworkSender == this);
                if (connec == null) return;

                connec.Socket.Close();
                connec.TCPNetworkReceiver.Dispose();
            }
        }
    }
}