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
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.ServiceModel.Channels;
using System.Threading;
using System.Threading.Tasks;
using JPB.Communication.ComBase.Messages;

namespace JPB.Communication.ComBase
{
    public sealed class TCPNetworkSender : Networkbase
    {
        internal TCPNetworkSender(ushort port)
        {
            Port = port;
            Timeout = TimeSpan.FromSeconds(15);
        }

        public TimeSpan Timeout { get; set; }

        #region Message Methods

        /// <summary>
        /// Sends a message to a Given IP:Port and wait for Deliver
        /// </summary>
        /// <param name="message">Instance of message</param>
        /// <param name="ip">Ip of client pc</param>
        /// <param name="port">Port of client pc</param>
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
        /// <param name="ip">Ip of client pc</param>
        /// <param name="port">Port of client pc</param>
        /// <returns></returns>
        /// <exception cref="TimeoutException"></exception>
        public static Task SendMessageAsync(MessageBase message, string ip, ushort port)
        {
            var sender = NetworkFactory.Instance.GetSender(port);
            return sender.SendMessageAsync(message, ip);
        }

        /// <summary>
        /// Sends a message to multible Hosts
        /// 
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
        /// Sends one message to one client and wait for deliver
        /// </summary>
        /// <param name="message">Message object or inherted object</param>
        /// <param name="ip">Ip of client</param>
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
        /// Sends one message to one client async
        /// </summary>
        /// <param name="message">Message object or inherted object</param>
        /// <param name="ip">Ip of client</param>
        /// <returns>frue if message was successful delivered otherwise false</returns>
        public Task<bool> SendMessageAsync(MessageBase message, string ip)
        {
            var task = new Task<bool>(() =>
            {
                var client = CreateClientSockAsync(ip, Port);
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

        #endregion

        internal void SendNeedMoreTimeBackAsync(RequstMessage mess, string ip)
        {
            mess.NeedMoreTime = 20000;
            SendMessageAsync(mess, ip);
        }

        /// <summary>
        /// Sends a message an awaits a response on the same port from the other side
        /// </summary>
        /// <param name="mess">Message object or inherted object</param>
        /// <param name="ip">Ip of client</param>
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
        /// <param name="ip">Ip of client</param>
        /// <returns>Result from other side or default(T)</returns>
        /// <exception cref="TimeoutException"></exception>
        public async Task<T> SendRequstMessage<T>(RequstMessage mess, string ip)
        {
            return await SendRequstMessageAsync<T>(mess, ip);
        }

        /// <summary>
        /// Sends one message COPY to each ip and awaits from all a result or nothing
        /// </summary>
        /// <typeparam name="T">the result we await</typeparam>
        /// <param name="mess">Message object or inherted object</param>
        /// <param name="ips">Ips of client</param>
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
        /// WIP
        ///  </summary>
        ///  <param name="stream"></param>
        ///  <param name="mess"></param>
        ///  <param name="ip"></param>
        /// <param name="disposeOnEnd">Close and Dispose the stream after work</param>
        /// <exception cref="NotImplementedException"></exception>
        public async void SendStreamDataAsync(Stream stream, MessageBase mess, string ip, bool disposeOnEnd = true)
        {
            var client = await CreateClientSockAsync(ip, Port);
            if (client == null)
                return;

            var prepairedMess = PrepareMessage(mess, ip);
            var serialize = this.Serialize(prepairedMess);

            using (var memstream = new MemoryStream(serialize))
            using (var openNetwork = OpenAndSend(memstream, client))
            {
                //wait for the Responce that the other side is waiting for the content
                openNetwork.ReadByte();
                SendOnStream(openNetwork, stream, client);
            }

            if (disposeOnEnd)
            {
                stream.Close();
                stream.Dispose();
                stream = null;
            }
        }

        #region Base Methods

        private TcpMessage PrepareMessage(MessageBase message, string ip)
        {
            message.SendAt = DateTime.Now;
            message.Sender = NetworkInfoBase.IpAddress.ToString();
            message.Reciver = ip;
            return Wrap(message);
        }

        /// <summary>
        /// Prepaired mehtod call that uses the Connect mehtod with multible IP addresses
        /// 
        /// Behavior is not tested
        /// WIP
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        /// <returns></returns>
        private static async Task<TcpClient> CreateClientSockAsync(IEnumerable<string> ip, int port)
        {
            try
            {
                var client = new TcpClient();

                //resolve DNS entrys
                var ipAddresses =
                    ip.Select(s =>
                    {
                        var hostAddresses = Dns.GetHostAddresses(s);
                        return new Tuple<string, IPAddress[]>(s, hostAddresses);
                    })
                    .Select(s => NetworkInfoBase.RaiseResolveDistantIp(s.Item2, s.Item1))
                    .AsParallel()
                    .ToArray();

                await client.ConnectAsync(ipAddresses, port);
                client.NoDelay = true;
                return client;
            }
            catch (Exception e)
            {
                return null;
            }
        }

        private static Task<TcpClient> CreateClientSockAsync(string ip, int port)
        {
            return CreateClientSockAsync(new[] { ip }, port);
        }

        private void SendBaseAsync(TcpMessage message, TcpClient client)
        {
            var serialize = Serialize(message);
            if (!serialize.Any())
                return;

            using (var memstream = new MemoryStream(serialize))
            using (OpenAndSend(memstream, client)) { }
        }

        private Stream OpenAndSend(Stream stream, TcpClient client)
        {
            Stream networkStream = client.GetStream();
            return SendOnStream(networkStream, stream, client);
        }

        private Stream SendOnStream(Stream networkStream, Stream stream, TcpClient client)
        {
            if (!stream.CanRead)
                return networkStream;

            int bufSize = client.ReceiveBufferSize;

            var buf = new byte[bufSize];

            int bytesRead;
            while ((bytesRead = stream.Read(buf, 0, bufSize)) > 0)
            {
                networkStream.Write(buf, 0, bytesRead);
            }
            
            //write an empty Chunck to indicate the end of this Part
            networkStream.Write(new byte[] { 0x00 }, 0, 1);

            return networkStream;
        }

        #endregion

        public override ushort Port { get; internal set; }
    }
}