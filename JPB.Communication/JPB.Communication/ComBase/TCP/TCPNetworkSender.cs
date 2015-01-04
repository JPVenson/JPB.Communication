﻿/*
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
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using JPB.Communication.ComBase.Messages;

namespace JPB.Communication.ComBase.TCP
{
    /// <summary>
    /// A TCP sender
    /// </summary>
    public sealed class TCPNetworkSender : Networkbase, IDisposable
    {
        internal TCPNetworkSender(ushort port)
        {
            Port = port;
            Timeout = TimeSpan.FromSeconds(15);
        }

        /// <summary>
        /// The timeout for wating on Request messages callback
        /// Warning
        /// Setting this below 10 will maybe cause in timeout problems
        /// </summary>
        public TimeSpan Timeout { get; private set; }

        /// <summary>
        /// If set to true all future calls to a Remote host will be keept open and will be stored inside the ConnectionPool
        /// </summary>
        public bool SharedConnection { get; set; }

        /// <summary>
        /// Will be invoked to observe Exceptions
        /// </summary>
        public static event EventHandler<Exception> OnCriticalException;
        /// <summary>
        /// Thread static
        /// </summary>
        [ThreadStatic]
        public static Exception LastException;

        private static void SetException(object sender, Exception e)
        {
            LastException = e;
            RaiseCriticalException(sender, e);
        }

        private static void RaiseCriticalException(object sender, Exception e)
        {
            var handler = OnCriticalException;
            if (handler != null)
                handler(sender, e);
        }


        #region Message Methods

        /// <summary>
        /// Sends a message to a Given IP:Port and wait for Deliver
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
        /// Sends a message async to a IP:Port
        /// </summary>
        /// <param name="message">Instance of message</param>
        /// <param name="ip">Ip of sock pc</param>
        /// <param name="port">Port of sock pc</param>
        /// <returns></returns>
        /// <exception cref="TimeoutException"></exception>
        public static Task SendMessageAsync(MessageBase message, string ip, ushort port)
        {
            var sender = NetworkFactory.Instance.GetSender(port);
            return sender.SendMessageAsync(message, ip);
        }

        /// <summary>
        /// Sends a message to multible Hosts
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

                var runningMessages = ips.Select(ip => SendMessageAsync(message.Clone() as MessageBase, ip)).ToArray();

                for (var i = 0; i < runningMessages.Length; i++)
                {
                    try
                    {
                        var task = runningMessages[i];
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
        /// Sends a message to multible Hosts
        /// </summary>
        /// <returns>all non reached hosts</returns>
        public IEnumerable<string> SendMultiMessage(MessageBase message, params string[] ips)
        {
            var send = SendMultiMessageAsync(message, ips);
            send.Wait();
            return send.Result;
        }

        /// <summary>
        /// Sends one message to one sock and wait for deliver
        /// </summary>
        /// <param name="message">Message object or inherted object</param>
        /// <param name="ip">Ip of sock</param>
        /// <returns>frue if message was successful delivered otherwise false</returns>
        /// <exception cref="TimeoutException"></exception>
        public bool SendMessage(MessageBase message, string ip)
        {
            var sendMessageAsync = SendMessageAsync(message, ip);
            sendMessageAsync.Wait();
            var b = sendMessageAsync.Result;
            return b;
        }

        /// <summary>
        /// Sends one message to one sock async
        /// </summary>
        /// <param name="message">Message object or inherted object</param>
        /// <param name="ip">Ip of sock</param>
        /// <returns>frue if message was successful delivered otherwise false</returns>
        public Task<bool> SendMessageAsync(MessageBase message, string ip)
        {
            var task = new Task<bool>(() =>
            {
                var client = CreateClientSockAsync(ip);
                var tcpMessage = PrepareMessage(message, ip);
                client.Wait();
                var result = client.Result;
                if (result == null)
                    return false;
                SendBaseAsync(tcpMessage, result);
                RaiseMessageSended(message);
                return true;
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
        /// Sends a message an awaits a response on the same port from the other side
        /// </summary>
        /// <param name="mess">Message object or inherted object</param>
        /// <param name="ip">Ip of sock</param>
        /// <returns>Result from other side or default(T)</returns>
        /// <exception cref="TimeoutException"></exception>
        public Task<T> SendRequstMessageAsync<T>(RequstMessage mess, string ip)
        {
            var task = new Task<T>(() =>
            {
                if (mess.ExpectedResult == default(ushort))
                {
                    mess.ExpectedResult = Port;
                }

                var result = default(T);
                AutoResetEvent waitForResponsive;
                using (waitForResponsive = new AutoResetEvent(false))
                {
                    var reciever = NetworkFactory.Instance.GetReceiver(mess.ExpectedResult);

                    var moreTime = 0L;
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
                    var isSend = SendMessage(mess, ip);
                    if (isSend)
                    {
                        waitForResponsive.WaitOne(Timeout);
                        while (moreTime > 0)
                        {
                            var mT = moreTime;
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
        /// Sends a message an awaits a response on the same port from the other side
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
        /// Sends one message COPY to each ipOrHost and awaits from all a result or nothing
        /// </summary>
        /// <typeparam name="T">the result we await</typeparam>
        /// <param name="mess">Message object or inherted object</param>
        /// <param name="ips">Ips of sock</param>
        /// <returns>A Dictiornary that contains for each key ( IP ) the result we got</returns>
        public Dictionary<string, T> SendMultiRequestMessage<T>(RequstMessage mess, string[] ips)
        {
            var enumerable = ips.Distinct().ToArray();

            Task<T>[] pendingRequests =
                enumerable.Select(ip => this.SendRequstMessageAsync<T>(mess.Clone() as RequstMessage, ip)).ToArray();

            Task.WaitAll(pendingRequests);

            var results = new Dictionary<string, T>();

            for (int i = 0; i < enumerable.Length; i++)
            {
                results.Add(enumerable[i], pendingRequests[i].Result);
            }

            return results;
        }

        ///  <summary>
        /// This mehtod will send the content of the given stream to the given ip
        /// To Support this the Remote host must set the TCP Reciever property SupportLargeMessages 
        /// to true otherwise the message will be ignored
        ///  </summary>
        ///  <param name="stream"></param>
        ///  <param name="mess"></param>
        ///  <param name="ip"></param>
        /// <param name="disposeOnEnd">Close and Dispose the stream after work</param>
        /// <exception cref="NotImplementedException"></exception>
        public async void SendStreamDataAsync(Stream stream, StreamMetaMessage mess, string ip, bool disposeOnEnd = true)
        {
            var client = await CreateClientSockAsync(ip);
            if (client == null)
                return;

            mess.StreamSize = stream.Length;

            var prepairedMess = PrepareMessage(mess, ip);
            var serialize = this.Serialize(prepairedMess);

            using (var memstream = new MemoryStream(serialize))
            {
                var openNetwork = OpenAndSend(memstream, client);

                //wait for the Responce that the other side is waiting for the content
                //openNetwork.Write(new byte[] { 0x00 }, 0, 1);

                AwaitCallbackFromRemoteHost(client, true);

                SendOnStream(stream, client);

                AwaitCallbackFromRemoteHost(client, false);

                if (!SharedConnection)
                {
                    openNetwork.Send(new byte[0]);
                    openNetwork.LingerState = new LingerOption(true, 60);
                    openNetwork.Shutdown(SocketShutdown.Both);
                    openNetwork.Close();
                    openNetwork.Dispose();
                }
            }

            if (disposeOnEnd)
            {
                stream.Close();
                stream.Dispose();
                stream = null;
            }
        }

        #endregion

        #region Base Methods

        /// <summary>
        /// If set to true the external IP of this host will be used as Sender property
        /// </summary>
        public bool UseExternalIpAsSender { get; set; }
        
        /// <summary>
        /// Starts the injection of the given socket into the ConnectionPool
        /// </summary>
        /// <param name="ipOrHost"></param>
        /// <returns></returns>
        public async Task<TCPNetworkReceiver> InitSharedConnection(Socket ipOrHost)
        {
            if (!ipOrHost.Connected)
            {
                throw new ArgumentException("The socket must be connected");
            }

            return await ConnectionPool.Instance.InjectSocket(ipOrHost, this);
        }

        /// <summary>
        /// Will create a new Connection from this Pc to the IpOrHost pc
        /// When done and Successfull it will return a new Receiver instance that can be used to observe messages from the Remote host
        /// </summary>
        /// <param name="ipOrHost"></param>
        /// <returns></returns>
        public async Task<TCPNetworkReceiver> InitSharedConnection(string ipOrHost)
        {
            UseExternalIpAsSender = true;
            return await ConnectionPool.Instance.ConnectToHost(ipOrHost, Port);
        }

        internal async Task<Socket> _InitSharedConnection(string ip)
        {
            this.SharedConnection = true;
            return await CreateClientSockAsync(ip);
        }

        private NetworkMessage PrepareMessage(MessageBase message, string ip)
        {
            message.SendAt = DateTime.Now;
            if (string.IsNullOrEmpty(message.Sender) || !UseExternalIpAsSender)
                message.Sender = NetworkInfoBase.IpAddress.ToString();
            if (UseExternalIpAsSender)
            {
                message.Sender = NetworkInfoBase.IpAddressExternal.ToString();
            }
            message.Reciver = ip;
            return Wrap(message);
        }

        /// <summary>
        /// Prepaired mehtod call that uses the Connect mehtod with multible IP addresses
        /// 
        /// Behavior is not tested | Shared connections are not supported
        /// WIP
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        /// <returns></returns>
        private static async Task<TcpClient> CreateClientSockAsync(IEnumerable<string> ip, ushort port)
        {
            TcpClient client = null;
            try
            {
                client = new TcpClient();

                //resolve DNS entrys
                var ipAddresses =
                    ip
                    .Select(NetworkInfoBase.ResolveIp)
                    .AsParallel()
                    .ToArray();

                await client.ConnectAsync(ipAddresses, port);
                client.NoDelay = true;
                return client;
            }
            catch (Exception e)
            {
                SetException(client, e);
                return null;
            }
        }

        private async Task<Socket> CreateClientSockAsync(string ipOrHost)
        {
            if (this.SharedConnection)
            {
                var isConnected = ConnectionPool.Instance.GetSock(ipOrHost, Port);
                if (isConnected != null)
                {
                    if (!isConnected.Connected)
                    {
                        isConnected.Connect(ipOrHost, Port);
                    }
                    return isConnected;
                }
                else
                {
                    return await CreateClientSock(ipOrHost, Port);
                }
            }
            return await CreateClientSock(ipOrHost, Port);
        }

        private static async Task<Socket> CreateClientSock(string ipOrHost, ushort port)
        {
            var client = new TcpClient();
            try
            {
                client.NoDelay = true;
                await client.ConnectAsync(ipOrHost, port);
                return client.Client;
            }
            catch (Exception e)
            {
                SetException(client, e);
                return null;
            }
        }

        private void AwaitCallbackFromRemoteHost(Socket sock, bool wait)
        {
            sock.ReceiveTimeout = Timeout.Milliseconds;
            do
            {
                var tryCount = 0;
                var tryMax = 2;

                //ok due th fact that we are sometimes faster then the remote PC
                //we need to send maybe Multible times the zero byte message
                //normal messages are not effected

                tryCount++;
                try
                {
                    sock.Send(new byte[] { 0x00 });
                    //Nagles alg waits for 200 ms
                    Thread.Sleep(250);
                    sock.Send(new byte[] { 0x00 });
                    if (wait)
                        sock.Receive(new byte[] { 0x00 });
                }
                catch (Exception e)
                {
                    if (tryCount >= tryMax || !sock.Connected)
                    {
                        throw;
                    }
                    Debug.WriteLine(
                        string.Format("TCPSender> awaits callback from remote pc try {0} of {1}", tryCount, tryMax),
                        TraceCategory);

                    sock.Send(new byte[0]);
                    continue;
                }
                break;
            } while (true);
        }

        private void SendBaseAsync(NetworkMessage message, Socket openNetwork)
        {
            var serialize = Serialize(message);
            if (!serialize.Any())
                return;

            using (var memstream = new MemoryStream(serialize))
                OpenAndSend(memstream, openNetwork);

            AwaitCallbackFromRemoteHost(openNetwork, false);
            if (!SharedConnection)
            {
                openNetwork.Send(new byte[0]);
                openNetwork.Close();
            }
        }

        private Socket OpenAndSend(Stream stream, Socket client)
        {
            return SendOnStream(stream, client);
        }

        private Socket SendOnStream(Stream stream, Socket sock)
        {
            int bufSize = sock.ReceiveBufferSize;

            var buf = new byte[bufSize];
            var read = 0;
            int send = 0;
            while ((read = stream.Read(buf, 0, bufSize)) > 0)
            {
                send = sock.Send(buf, 0, read, SocketFlags.None);
                //target.Write(buf, 0, read);
            }
            return sock;
        }

        #endregion

        public override ushort Port { get; internal set; }

        /// <summary>
        /// Checks for an existing connection in the ConnectionPool
        /// </summary>
        /// <param name="ipOrHost"></param>
        /// <returns></returns>
        public bool ConnectionOpen(string ipOrHost)
        {
            if (!SharedConnection)
                return false;

            var sockForIpOrNull = ConnectionPool.Instance.GetSockForIpOrNull(ipOrHost);
            if (sockForIpOrNull == null)
                return false;

            return sockForIpOrNull.Connected;
        }

        public void Dispose()
        {
            NetworkFactory.Instance._senders.Remove(Port);
        }
    }
}