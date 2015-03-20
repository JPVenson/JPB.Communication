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
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JPB.Communication.ComBase;
using JPB.Communication.ComBase.Messages;
using JPB.Communication.ComBase.TCP;
using System.Collections.Specialized;

namespace JPB.Communication.Shared
{
    /// <summary>
    ///     This class holds and Updates unsorted values that will be Synced over the Network
    ///     On a Pc, only one Network Value bag with the given guid can exists
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class NetworkValueBag<T> :
        Networkbase,
        IList<T>,
        IList,
        INotifyCollectionChanged,
        IDisposable
    {
        protected readonly TCPNetworkReceiver TCPNetworkReceiver;
        protected readonly TCPNetworkSender TcpNetworkSernder;

        private volatile ICollection<T> _localValues;

        /// <summary>
        /// </summary>
        /// <param name="port"></param>
        /// <param name="guid"></param>
        protected NetworkValueBag(ushort port, string guid)
        {
            RegisterCollecion(guid);

            //objects that Impliments or Contains a Serializable Attribute are supported
            //if (!typeof(T).IsValueType && !typeof(T).IsPrimitive)
            //{
            //    throw new TypeLoadException("Typeof T must be a Value type ... please use the NonValue collection");
            //}

            CollectionRecievers = new ObservableCollection<string>();

            Port = port;
            Guid = guid;
            LocalValues = new ObservableCollection<T>();
            SyncRoot = new object();

            TCPNetworkReceiver = NetworkFactory.Instance.GetReceiver(port);
            TcpNetworkSernder = NetworkFactory.Instance.GetSender(port);
            RegisterCallbacks();
        }

        /// <summary>
        ///     if this is not the first Host that Maintains the target Collection we are initly connectet
        /// </summary>
        public string ConnectedToHost { get; protected set; }

        /// <summary>
        ///     All other PC's that Contains a NetworkValueBag with the desiered GUID.
        ///     When Add,Remove,Clear is invoked this PC's will be notifyed to do the same
        /// </summary>
        public ObservableCollection<string> CollectionRecievers
        {
            get;
            protected set;
        }

        /// <summary>
        ///     The Internal collection that contains the values
        /// </summary>
        protected ICollection<T> LocalValues
        {
            get { return _localValues; }
            set { _localValues = value; }
        }

        public override ushort Port { get; internal set; }
        public string Guid { get; protected set; }

        public bool IsDisposing { get; protected set; }

        public bool Registerd { get; protected set; }

        public void Dispose()
        {
            if (IsDisposing)
                return;

            IsDisposing = true;

            PushUnRegisterMessage();
            UnRegisterCallbacks();

            NetworkListControler.Guids.Remove(Guid);
            LocalValues = null;
            CollectionRecievers = null;
        }

        public bool IsFixedSize
        {
            get { return false; }
        }

        public int Add(object value)
        {
            lock (SyncRoot)
            {
                Add((T)value);
                return Count;
            }
        }

        /// <summary>
        ///     Not thread save
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool Contains(object value)
        {
            return Contains((T)value);
        }

        /// <summary>
        ///     Not thread save
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public int IndexOf(object value)
        {
            return IndexOf((T)value);
        }

        /// <summary>
        ///     Not Implemented
        /// </summary>
        /// <param name="index"></param>
        /// <param name="value"></param>
        public void Insert(int index, object value)
        {
            throw new NotImplementedException();
        }

        public void Remove(object value)
        {
            Remove((T)value);
        }

        object IList.this[int index]
        {
            get { return this[index]; }
            set { this[index] = (T)value; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        /// <summary>
        /// Will add the value to the local Collection and then trigger a Push notification to all receivers async
        /// </summary>
        /// <param name="item"></param>
        public virtual void Add(T item)
        {
            lock (SyncRoot)
            {
                PushAddMessage(item);
                LocalValues.Add(item);
                TriggerAdd(item);
            }
        }

        public void Clear()
        {
            lock (SyncRoot)
            {
                PushClearMessage();
                LocalValues.Clear();
                TriggerReset();
            }
        }

        public int IndexOf(T item)
        {
            int result = -1;
            for (int i = 0; i < LocalValues.Count; i++)
            {
                T localValue = this[i];

                if (localValue.Equals(item))
                {
                    result = i;
                    break;
                }
            }
            return result;
        }

        /// <summary>
        ///     To be Supported
        ///     throw new NotImplementedException();
        /// </summary>
        /// <param name="index"></param>
        /// <param name="item"></param>
        public void Insert(int index, T item)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        ///     To be Supported
        /// </summary>
        /// <param name="index"></param>
        public void RemoveAt(int index)
        {
            Remove(this[index]);
        }

        public T this[int index]
        {
            get { return LocalValues.ElementAt(index); }
            set { Insert(index, value); }
        }

        public bool Remove(T item)
        {
            lock (SyncRoot)
            {
                PushRemoveMessage(item);
                bool remove = LocalValues.Remove(item);
                if (remove)
                    TriggerRemove(item, IndexOf(item));
                return remove;
            }
        }

        public bool Contains(T item)
        {
            lock (SyncRoot)
            {
                return LocalValues.Contains(item);
            }
        }

        public event NotifyCollectionChangedEventHandler CollectionChanged;

        public int Count
        {
            get { return LocalValues.Count; }
        }

        public object SyncRoot { get; protected set; }

        public bool IsSynchronized
        {
            get { return !Monitor.IsEntered(SyncRoot); }
        }

        public IEnumerator<T> GetEnumerator()
        {
            T[] array;
            lock (SyncRoot)
            {
                array = ToArray();
            }
            return array.Cast<T>().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            lock (SyncRoot)
            {
                LocalValues.CopyTo(array, arrayIndex);
            }
        }

        public bool TryAdd(T item)
        {
            lock (SyncRoot)
            {
                Add(item);
                return Contains(item);
            }
        }

        public bool TryTake(out T item)
        {
            lock (SyncRoot)
            {
                item = this[Count];
            }
            return true;
        }

        public T[] ToArray()
        {
            lock (SyncRoot)
            {
                return LocalValues.ToArray();
            }
        }

        public void CopyTo(Array array, int index)
        {
            lock (SyncRoot)
            {
                for (int i = index; i < LocalValues.Count; i++)
                {
                    T localValue = LocalValues.ElementAt(i);
                    array.SetValue(localValue, i);
                }
            }
        }

        /// <summary>
        ///     Creates a new Instance of the NetworkValueBag
        /// </summary>
        /// <param name="port"></param>
        /// <param name="guid"></param>
        /// <returns></returns>
        public static NetworkValueBag<T> CreateNetworkValueCollection(ushort port, string guid)
        {
            return new NetworkValueBag<T>(port, guid);
        }

        /// <summary>
        ///     Must be called to ensure a Single Usage of an GUID
        /// </summary>
        /// <param name="guid"></param>
        /// <exception cref="ArgumentException"></exception>
        protected static void RegisterCollecion(string guid)
        {
            if (NetworkListControler.Guids.Contains(guid))
            {
                throw new ArgumentException(@"This guid is in use. Please use a global _Uniq_ Identifier", "guid");
            }
            NetworkListControler.Guids.Add(guid);
        }

        /// <summary>
        ///     Gets a non tracking version of all items that are stored on the server
        /// </summary>
        /// <param name="host"></param>
        /// <param name="port"></param>
        /// <param name="guid"></param>
        /// <returns></returns>
        public static async Task<ICollection<T>> GetCollection(string host, ushort port, string guid)
        {
            T[] sender = await NetworkFactory.Instance.GetSender(port).SendRequstMessage<T[]>(new RequstMessage
            {
                InfoState = NetworkCollectionProtocol.CollectionGetCollection,
                Message = guid
            }, host);

            return sender;
        }

        private void RegisterCallbacks()
        {
            TCPNetworkReceiver.RegisterMessageBaseInbound(pPullAddMessage, NetworkCollectionProtocol.CollectionAdd);
            TCPNetworkReceiver.RegisterMessageBaseInbound(pPullClearMessage, NetworkCollectionProtocol.CollectionReset);
            TCPNetworkReceiver.RegisterMessageBaseInbound(pPullRemoveMessage, NetworkCollectionProtocol.CollectionRemove);
            TCPNetworkReceiver.RegisterMessageBaseInbound(PullRegisterMessage,
                NetworkCollectionProtocol.CollectionRegisterUser);
            TCPNetworkReceiver.RegisterMessageBaseInbound(PullUnRegisterMessage,
                NetworkCollectionProtocol.CollectionUnRegisterUser);
            TCPNetworkReceiver.RegisterRequstHandler(PullGetCollectionMessage,
                NetworkCollectionProtocol.CollectionGetCollection);
            TCPNetworkReceiver.RegisterRequstHandler(PullConnectMessage, NetworkCollectionProtocol.CollectionGetUsers);
        }

        private void UnRegisterCallbacks()
        {
            TCPNetworkReceiver.UnregisterChanged(pPullAddMessage, NetworkCollectionProtocol.CollectionAdd);
            TCPNetworkReceiver.UnregisterChanged(pPullClearMessage, NetworkCollectionProtocol.CollectionReset);
            TCPNetworkReceiver.UnregisterChanged(pPullRemoveMessage, NetworkCollectionProtocol.CollectionRemove);
            TCPNetworkReceiver.UnregisterChanged(PullRegisterMessage, NetworkCollectionProtocol.CollectionRegisterUser);
            TCPNetworkReceiver.UnregisterChanged(PullUnRegisterMessage,
                NetworkCollectionProtocol.CollectionUnRegisterUser);
            TCPNetworkReceiver.UnRegisterRequstHandler(PullGetCollectionMessage,
                NetworkCollectionProtocol.CollectionGetCollection);
            TCPNetworkReceiver.UnRegisterRequstHandler(PullConnectMessage, NetworkCollectionProtocol.CollectionGetUsers);
        }

        /// <summary>
        ///     Callback for CollectionGetUsers
        /// </summary>
        /// <param name="arg"></param>
        /// <returns></returns>
        protected object PullConnectMessage(RequstMessage arg)
        {
            if (arg.Message is string && Guid.Equals(arg.Message as string))
                lock (SyncRoot)
                {
                    if (!CollectionRecievers.Contains(arg.Sender))
                    {
                        CollectionRecievers.Add(arg.Sender);
                    }
                    //if (!CollectionRecievers.Contains(NetworkInfoBase.IpAddress.ToString()))
                    //{
                    //    CollectionRecievers.Add(NetworkInfoBase.IpAddress.ToString());
                    //}
                    return CollectionRecievers.ToArray();
                }
            return null;
        }

        /// <summary>
        ///     Connects this Collection to one Host inside the NetworkCollection
        ///     After the Connect this instance will get:
        ///     All Other existing Users
        ///     A Copy of the Current Network List
        /// </summary>
        /// <param name="host"></param>
        /// <returns></returns>
        public virtual async Task<bool> Connect(string host)
        {
            Task<ICollection<T>> collection = GetCollection(host, Port, Guid);

            ConnectedToHost = host;
            string[] sendRequstMessage = await TcpNetworkSernder.SendRequstMessage<string[]>(
                new RequstMessage { InfoState = NetworkCollectionProtocol.CollectionGetUsers, Message = Guid }, host);

            string[] users = sendRequstMessage;

            if (users == null)
            {
                return false;
            }

            CollectionRecievers = new ObservableCollection<string>(users);
            CollectionRecievers.Add(host);

#pragma warning disable 4014
            TcpNetworkSernder.SendMultiMessageAsync(
#pragma warning restore 4014
new MessageBase { InfoState = NetworkCollectionProtocol.CollectionRegisterUser }, users);
            Registerd = true;

            foreach (T item in await collection)
            {
                LocalValues.Add(item);
                TriggerAdd(item);
            }
            return true;
        }

        /// <summary>
        ///     Callback for CollectionGetCollection
        /// </summary>
        /// <param name="arg"></param>
        /// <returns></returns>
        protected object PullGetCollectionMessage(RequstMessage arg)
        {
            if (!Guid.Equals(arg.Message as string))
                return null;

            lock (SyncRoot)
            {
                return LocalValues.ToArray();
            }
        }

        protected void PullRegisterMessage(MessageBase obj)
        {
            lock (SyncRoot)
            {
                if (!CollectionRecievers.Contains(obj.Sender))
                    CollectionRecievers.Add(obj.Sender);
            }
        }

        protected void PullUnRegisterMessage(MessageBase obj)
        {
            lock (SyncRoot)
            {
                if (obj.Message is IEnumerable<string>)
                {
                    var diabled = obj.Message as IEnumerable<string>;
                    var toRemove = CollectionRecievers.Where(s => diabled.Contains(s));

                    foreach (var item in toRemove)
                    {
                        CollectionRecievers.Remove(item);
                    }
                }
                else
                {
                    CollectionRecievers.Remove(obj.Sender);
                }
            }
        }

        protected void PushUnRegisterMessage()
        {
            lock (SyncRoot)
            {
                Registerd = false;
                TcpNetworkSernder.SendMultiMessageAsync(new MessageBase(new object()), CollectionRecievers.ToArray());
            }
        }

        private void pPullAddMessage(MessageBase obj)
        {
            PullAddMessage(obj);
        }

        private void pPullClearMessage(MessageBase obj)
        {
            PullClearMessage(obj);
        }

        private void pPullRemoveMessage(MessageBase obj)
        {
            PullRemoveMessage(obj);
        }

        protected virtual void PullAddMessage(MessageBase obj)
        {
            if (obj.Message is NetworkCollectionMessage)
            {
                var mess = obj.Message as NetworkCollectionMessage;
                if (mess.Guid != null && Guid.Equals(mess.Guid) && mess.Value is T)
                {
                    lock (SyncRoot)
                    {
                        LocalValues.Add((T)mess.Value);
                        TriggerAdd((T)mess.Value);
                    }
                }
            }
        }

        protected virtual void PullClearMessage(MessageBase obj)
        {
            if (obj.Message is NetworkCollectionMessage)
            {
                var mess = obj.Message as NetworkCollectionMessage;
                if (mess.Guid != null && Guid.Equals(mess.Guid))
                {
                    lock (SyncRoot)
                    {
                        LocalValues.Clear();
                        TriggerReset();
                    }
                }
            }
        }

        protected virtual void PullRemoveMessage(MessageBase obj)
        {
            if (obj.Message is NetworkCollectionMessage)
            {
                var mess = obj.Message as NetworkCollectionMessage;
                if (mess.Guid != null && Guid.Equals(mess.Guid) && mess.Value is T)
                {
                    lock (SyncRoot)
                    {
                        var value = (T)mess.Value;
                        int indexOf = IndexOf(value);
                        bool remove = LocalValues.Remove(value);
                        if (remove)
                            TriggerRemove(value, indexOf);
                    }
                }
            }
        }

        protected async Task SendPessage(string id, object value)
        {
            var mess = new MessageBase(new NetworkCollectionMessage(value)
            {
                Guid = Guid
            })
            {
                InfoState = id
            };

            string[] ips;
            lock (SyncRoot)
            {
                ips = CollectionRecievers.Where(s => !s.Equals(NetworkInfoBase.IpAddress.ToString())).ToArray();
            }

            //Possible long term work
            IEnumerable<string> sendMultiMessage = await TcpNetworkSernder.SendMultiMessageAsync(mess, ips);

            lock (SyncRoot)
            {
                foreach (var item in sendMultiMessage)
                {
                    CollectionRecievers.Remove(item);
                }
            }
        }

        protected async Task PushAddMessage(T item)
        {
            await SendPessage(NetworkCollectionProtocol.CollectionAdd, item);
        }

        protected async Task PushClearMessage()
        {
            await SendPessage(NetworkCollectionProtocol.CollectionReset, default(T));
        }

        protected async Task PushRemoveMessage(T item)
        {
            await SendPessage(NetworkCollectionProtocol.CollectionRemove, item);
        }

        protected virtual void TriggerAdd(T added)
        {
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, added));
        }

        protected virtual void TriggerRemove(T removed, int result)
        {
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, removed,
                result));
        }

        protected virtual void TriggerReset()
        {
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        protected virtual void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            NotifyCollectionChangedEventHandler handler = CollectionChanged;
            if (handler != null)
                handler(this, e);
        }

        public virtual async Task<bool> Reload()
        {
            ICollection<T> collection = await GetCollection(ConnectedToHost, 1337, Guid);
            lock (SyncRoot)
            {
                LocalValues.Clear();

                if (collection == null)
                    return false;

                foreach (T item in collection)
                {
                    LocalValues.Add(item);
                }

                return true;
            }
        }

        ~NetworkValueBag()
        {
            Dispose();
        }
    }
}