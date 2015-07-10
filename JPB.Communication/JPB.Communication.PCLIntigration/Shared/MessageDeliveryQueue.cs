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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JPB.Communication.ComBase;
using JPB.Communication.ComBase.Messages;
using JPB.Communication.ComBase.Generic;

namespace JPB.Communication.Shared
{
    /// <summary>
    ///     this queue will manage Multibe message Deliverys
    ///     The message will be Send in the Order FIFO
    /// </summary>
    public class MessageDeliveryQueue :
        Networkbase,
        ICollection,
        IEnumerable
    {
        private readonly SeriellTaskFactory _internal;
        private readonly GenericNetworkSender _sender;

        public MessageDeliveryQueue(ushort port)
        {
            Port = port;
            _sender = NetworkFactory.Instance.GetSender(port);
            Receivers = new SynchronizedCollection<string>();
            _internal = new SeriellTaskFactory();
        }

        public override sealed ushort Port { get; internal set; }
        public ICollection<string> Receivers { get; private set; }

        public IEnumerator GetEnumerator()
        {
            return _internal.ConcurrentQueue.GetEnumerator();
        }

        public void CopyTo(Array array, int index)
        {
            _internal.ConcurrentQueue.ToArray().CopyTo(array, index);
        }

        public int Count
        {
            get { return _internal.ConcurrentQueue.Count; }
        }

        public object SyncRoot
        {
            get { return (Receivers as ICollection).SyncRoot; }
        }

        public bool IsSynchronized
        {
            get { return (Receivers as ICollection).IsSynchronized; }
        }

        public new event Action<MessageDeliveryQueue, NetworkMessage, string[]> OnMessageSend;

        protected virtual void RaiseMessageSend(NetworkMessage mess, string[] unreacheble)
        {
            Action<MessageDeliveryQueue, NetworkMessage, string[]> handler = OnMessageSend;
            if (handler != null)
                handler(this, mess, unreacheble);
        }

        public void Enqueue(NetworkMessage mess)
        {
            _internal.Add(() => ForceSendMessage(mess));
        }

        public void ForceSendMessage(object message, object infoState)
        {
            if (message == null)
                throw new ArgumentNullException("message");
            if (infoState == null)
                throw new ArgumentNullException("infoState");

            ForceSendMessage(new NetworkMessage(message)
            {
                InfoState = infoState
            });
        }

        public void ForceSendMessage(NetworkMessage mess)
        {
            if (mess == null)
                throw new ArgumentNullException("mess");

            string[] unreachable = _sender.SendMultiMessage(mess, Receivers.ToArray()).ToArray();
            RaiseMessageSend(mess, unreachable);

            lock (SyncRoot)
            {
                foreach (string item in unreachable)
                {
                    Receivers.Remove(item);
                }
            }
        }
    }
}