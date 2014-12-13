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
using JPB.Communication.ComBase;

namespace JPB.Communication
{
    [ComVisible(true)]
    [Guid("C6A14174-092C-40E5-BB12-207F3BC77F38")]
    public class NetworkFactory
    {
        private static NetworkFactory _instance = new NetworkFactory();
        private Dictionary<ushort, TCPNetworkReceiver> _receivers;
        private Dictionary<ushort, TCPNetworkSender> _senders;
        private TCPNetworkReceiver _commonReciever;
        private TCPNetworkSender _commonSender;
        private readonly Object _mutex;

        public bool ShouldRaiseEvents { get; set; }

        public event EventHandler OnSenderCreate;
        public event EventHandler OnReceiverCreate;

        protected virtual void RaiseSenderCreate()
        {
            if(!ShouldRaiseEvents)
                return;

            var handler = OnSenderCreate;
            if (handler != null)
                handler(this, EventArgs.Empty);
        }

        protected virtual void RaiseReceiverCreate()
        {
            if (!ShouldRaiseEvents)
                return;

            var handler = OnReceiverCreate;
            if (handler != null)
                handler(this, EventArgs.Empty);
        }

        private NetworkFactory()
        {
            _receivers = new Dictionary<ushort, TCPNetworkReceiver>();
            _senders = new Dictionary<ushort, TCPNetworkSender>();
            _mutex = new object();
        }

        public static NetworkFactory Instance
        {
            get { return _instance; }
        }

        public Dictionary<ushort, TCPNetworkReceiver>.Enumerator GetReceivers()
        {
            return _receivers.GetEnumerator();
        }

        public Dictionary<ushort, TCPNetworkSender>.Enumerator GetSenders()
        {
            return _senders.GetEnumerator();
        }

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
        /// This will set the Sender and Reciever Property
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
        /// Gets or Creates a Network sender for a given port
        /// Thread-Save
        /// </summary>
        /// <param name="port"></param>
        /// <returns></returns>
        public TCPNetworkSender GetSender(ushort port)
        {
            lock (_mutex)
            {
                var element = _senders.FirstOrDefault(s => s.Key == port);

                if (!element.Equals(null) && element.Value != null)
                {
                    return element.Value;
                }

                return CreateSender(port);
            }
        }

        private TCPNetworkSender CreateSender(ushort port)
        {
            var sender = new TCPNetworkSender(port);
            _senders.Add(port, sender);
            RaiseSenderCreate();
            return sender;
        }

        /// <summary>
        /// Gets or Creats a network Reciever for a given port
        /// Thread-Save
        /// </summary>
        /// <param name="port"></param>
        /// <returns></returns>
        public TCPNetworkReceiver GetReceiver(ushort port)
        {
            lock (_mutex)
            {
                var element = _receivers.FirstOrDefault(s => s.Key == port);

                if (!element.Equals(null) && element.Value != null)
                {
                    return element.Value;
                }

                return CreateReceiver(port);
            }
        }

        private TCPNetworkReceiver CreateReceiver(ushort port)
        {
            var receiver = new TCPNetworkReceiver(port);
            _receivers.Add(port, receiver);
            RaiseReceiverCreate();
            return receiver;
        }
    }
}