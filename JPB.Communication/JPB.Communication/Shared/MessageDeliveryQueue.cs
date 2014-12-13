using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JPB.Communication.ComBase;
using JPB.Communication.ComBase.Messages;
using JPB.Tasking.TaskManagement.Threading;

namespace JPB.Communication.Shared
{
    /// <summary>
    /// this queue will manage Multibe message Deliverys
    /// The message will be Send in the Order FIFO
    /// 
    /// </summary>
    public class MessageDeliveryQueue :
        Networkbase,
        ICollection, 
        IEnumerable
    {
        public MessageDeliveryQueue(ushort port)
        {
            Port = port;
            _sender = NetworkFactory.Instance.GetSender(port);
            Receivers = new SynchronizedCollection<string>();
            _internal = new SeriellTaskFactory();
        }

        public override ushort Port { get; internal set; }
        public ICollection<string> Receivers { get; private set; }

        private TCPNetworkSender _sender;
        private SeriellTaskFactory _internal;

        public new event Action<MessageDeliveryQueue, MessageBase, string[]> OnMessageSend;

        protected virtual void RaiseMessageSend(MessageBase mess, string[] unreacheble)
        {
            var handler = OnMessageSend;
            if (handler != null)
                handler(this, mess, unreacheble);
        }

        public void Enqueue(MessageBase mess)
        {
            _internal.Add(() => ForceSendMessage(mess));
        }

        public void ForceSendMessage(object message, object infoState)
        {
            if (message == null)
                throw new ArgumentNullException("message");
            if (infoState == null)
                throw new ArgumentNullException("infoState");

            this.ForceSendMessage(new MessageBase(message)
            {
                InfoState = infoState
            });
        }

        public void ForceSendMessage(MessageBase mess)
        {
            if (mess == null)
                throw new ArgumentNullException("mess");

            var unreachable = _sender.SendMultiMessage(mess, this.Receivers.ToArray()).ToArray();
            RaiseMessageSend(mess, unreachable);

            lock (SyncRoot)
            {
                foreach (var item in unreachable)
                {
                    Receivers.Remove(item);
                }
            }
        }

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
            get
            {
                return _internal.ConcurrentQueue.Count;
            }
        }

        public object SyncRoot
        {
            get
            {
                return (Receivers as ICollection).SyncRoot;
            }
        }

        public bool IsSynchronized
        {
            get
            {
                return (Receivers as ICollection).IsSynchronized;
            }
        }

    }
}
