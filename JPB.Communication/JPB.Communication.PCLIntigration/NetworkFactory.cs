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
using System.Runtime.InteropServices;
using JPB.Communication.ComBase.TCP;
using JPB.Communication.Contracts;
using JPB.Communication.Contracts.Factorys;

namespace JPB.Communication
{
    public class NetworkFactory : IDisposable
    {
        private static NetworkFactory _instance;
        public static IPlatformFactory PlatformFactory { get; private set; }

        static NetworkFactory()
        {

        }

        internal readonly Object _mutex;
        private TCPNetworkReceiver _commonReciever;
        private TCPNetworkSender _commonSender;
        internal Dictionary<ushort, TCPNetworkReceiver> _receivers;
        internal Dictionary<ushort, TCPNetworkSender> _senders;

        private NetworkFactory()
        {
            _receivers = new Dictionary<ushort, TCPNetworkReceiver>();
            _senders = new Dictionary<ushort, TCPNetworkSender>();
            _mutex = new object();
        }

        public bool ShouldRaiseEvents { get; set; }

        /// <summary>
        /// Must be called before everything else
        /// When no value is provied guess WinRT
        /// </summary>
        /// <param name="factory"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        public static NetworkFactory Create(IPlatformFactory factory)
        {
            if (_instance != null)
                throw new InvalidOperationException("You cannot call Create multible times, it should be used to set the program wide socket type");
            PlatformFactory = factory;
            _instance = new NetworkFactory();
            return _instance;
        }

        public static NetworkFactory Instance
        {
            get
            {
                if (_instance == null)
                    throw new InvalidOperationException("Must call create 1 time");

                return _instance;
            }
        }

        /// <summary>
        ///     The Easy-To-Access Receiver that is created or pulled by InitCommonSenderAndReciver
        /// </summary>
        public TCPNetworkReceiver Reciever
        {
            get
            {
                if (_commonReciever == null)
                    throw new ArgumentException("There is no port supplied. call InitCommonSenderAndReciver first");
                return _commonReciever;
            }
            private set { _commonReciever = value; }
        }

        /// <summary>
        ///     The Easy-To-Access Sender that is created or pulled by InitCommonSenderAndReciver
        /// </summary>
        public TCPNetworkSender Sender
        {
            get
            {
                if (_commonSender == null)
                    throw new ArgumentException("There is no port supplied. call InitCommonSenderAndReciver first");
                return _commonSender;
            }
            private set { _commonSender = value; }
        }

        /// <summary>
        ///     Object for sync access
        /// </summary>
        public object SyncRoot
        {
            get { return _mutex; }
        }

        public event EventHandler<TCPNetworkSender> OnSenderCreate;
        public event EventHandler<TCPNetworkReceiver> OnReceiverCreate;

        internal virtual void RaiseSenderCreate(TCPNetworkSender item)
        {
            if (!ShouldRaiseEvents)
                return;

            if (OnSenderCreate != null)
                OnSenderCreate(this, item);
        }

        internal virtual void RaiseReceiverCreate(TCPNetworkReceiver item)
        {
            if (!ShouldRaiseEvents)
                return;

            if (OnReceiverCreate != null)
                OnReceiverCreate(this, item);
        }

        /// <summary>
        ///     Returns a flat copy of all known tcp Receivers
        /// </summary>
        /// <returns></returns>
        public Dictionary<ushort, TCPNetworkReceiver> GetReceivers()
        {
            return _receivers.Select(s => s).ToDictionary(s => s.Key, s => s.Value);
        }

        /// <summary>
        ///     Returns a flat copy of all known tcp senders
        /// </summary>
        /// <returns></returns>
        public Dictionary<ushort, TCPNetworkSender> GetSenders()
        {
            return _senders.Select(s => s).ToDictionary(s => s.Key, s => s.Value);
        }

        /// <summary>
        ///     This will set the Sender and Reciever Property
        /// </summary>
        /// <param name="listeningPort"></param>
        /// <param name="sendingPort"></param>
        public void InitCommonSenderAndReciver(ushort listeningPort = 0, ushort sendingPort = 0)
        {
            if (listeningPort != 0)
            {
                Reciever = GetReceiver(listeningPort);
            }

            if (sendingPort != 0)
            {
                Sender = GetSender(sendingPort);
            }
        }

        /// <summary>
        ///     Gets or Creates a Network sender for a given port
        ///     Thread-Save
        /// </summary>
        /// <param name="port"></param>
        /// <returns></returns>
        public TCPNetworkSender GetSender(ushort port)
        {
            lock (_mutex)
            {
                KeyValuePair<ushort, TCPNetworkSender> element = _senders.FirstOrDefault(s => s.Key == port);

                if (!element.Equals(null) && element.Value != null)
                {
                    return element.Value;
                }

                return CreateSender(port);
            }
        }

        internal TCPNetworkSender CreateSender(ushort port)
        {
            var sender = new TCPNetworkSender(port, PlatformFactory.SocketFactory);
            _senders.Add(port, sender);
            RaiseSenderCreate(sender);
            return sender;
        }

        /// <summary>
        ///     Gets or Creats a network Reciever for a given port
        ///     Thread-Save
        /// </summary>
        /// <param name="port"></param>
        /// <returns></returns>
        public TCPNetworkReceiver GetReceiver(ushort port)
        {
            lock (_mutex)
            {
                KeyValuePair<ushort, TCPNetworkReceiver> element = _receivers.FirstOrDefault(s => s.Key == port);

                if (!element.Equals(null) && element.Value != null)
                {
                    return element.Value;
                }

                return CreateReceiver(port);
            }
        }

        internal TCPNetworkReceiver CreateReceiver(ushort port)
        {
            var receiver = new TCPNetworkReceiver(port, PlatformFactory.SocketFactory);
            _receivers.Add(port, receiver);
            RaiseReceiverCreate(receiver);
            return receiver;
        }

        public bool ContainsReceiver(ushort port)
        {
            KeyValuePair<ushort, TCPNetworkReceiver> element = _receivers.FirstOrDefault(s => s.Key == port);

            if (!element.Equals(null) && element.Value != null)
            {
                return true;
            }
            return false;
        }

        public void Dispose()
        {
            foreach (var item in this._receivers)
            {
                item.Value.Dispose();
            }

            foreach (var item in this._senders)
            {
                item.Value.Dispose();
            }
        }
    }
}