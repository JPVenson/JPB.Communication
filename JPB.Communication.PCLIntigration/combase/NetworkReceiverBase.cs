using System;
using System.Collections.Generic;
using System.Linq;
using JPB.Communication.ComBase.Messages;
using JPB.Communication.ComBase.Messages.Wrapper;
using JPB.Communication.Contracts;

namespace JPB.Communication.ComBase
{
    public delegate bool UnkownMessageDelegate(object message);
    /// <summary>
    /// base class for Receiver logic
    /// </summary>
    public abstract class NetworkReceiverBase : Networkbase, INetworkReceiver
    {
        protected NetworkReceiverBase()
        {
            _largeMessages = new List<Tuple<Action<LargeMessage>, object>>();
            _onetimeupdated = new List<Tuple<Action<MessageBase>, Guid>>();
            _pendingrequests = new List<Tuple<Action<RequstMessage>, Guid>>();
            _requestHandler = new List<Tuple<Func<RequstMessage, object>, object>>();
            _messageBaseCallbacks = new List<Tuple<Action<MessageBase>, object>>();
            _unkownCallbacks = new List<UnkownMessageDelegate>();
            _typeCallbacks = new Dictionary<Type, Action<object>>();
            _typeCallbacks.Add(typeof(MessageBase), WorkOn_MessageBase);
            _typeCallbacks.Add(typeof(LargeMessage), WorkOn_LargeMessage);
        }

        private void WorkOn_LargeMessage(object metaData)
        {
            var messCopy = metaData as LargeMessage;

            var updateCallbacks = _largeMessages.Where(action => messCopy != null && (action.Item2 == null || action.Item2.Equals(messCopy.MetaData.InfoState))).ToArray();
            foreach (var action in updateCallbacks)
            {
                action.Item1.BeginInvoke(messCopy, e => { }, null);
            }
        }

        private void WorkOn_MessageBase(object message)
        {
            var messCopy = message as MessageBase;

            var updateCallbacks = _messageBaseCallbacks.Where(action => messCopy != null && (action.Item2 == null || action.Item2.Equals(messCopy.InfoState))).ToArray();
            foreach (var action in updateCallbacks)
            {
                action.Item1.BeginInvoke(messCopy, e => { }, null);
            }

            //Go through all one time items and check for ID
            var oneTimeImtes = _onetimeupdated.Where(s => messCopy != null && messCopy.Id == s.Item2).ToArray();

            foreach (var action in oneTimeImtes)
            {
                action.Item1.BeginInvoke(messCopy, e => { }, null);
            }

            foreach (var useditem in oneTimeImtes)
                _onetimeupdated.Remove(useditem);
        }

        public bool IncommingMessage { get; protected set; }

        /// <summary>
        /// Removes a delegate from the Handler list
        /// </summary>
        /// <param name="action"></param>
        public void UnregisterChanged(Action<MessageBase> action, object state)
        {
            var enumerable = _messageBaseCallbacks.FirstOrDefault(s => s.Item1 == action && s.Item2 == state);
            if (enumerable != null)
            {
                _messageBaseCallbacks.Remove(enumerable);
            }
        }

        /// <summary>
        /// Removes a delegate from the Handler list
        /// </summary>
        /// <param name="action"></param>
        public void UnregisterChanged(Action<MessageBase> action)
        {
            var enumerable = _messageBaseCallbacks.FirstOrDefault(s => s.Item1 == action);
            if (enumerable != null)
            {
                _messageBaseCallbacks.Remove(enumerable);
            }
        }

        /// <summary>
        /// Register a Callback localy that will be used when a new message is inbound that has state in its InfoState
        /// </summary>
        /// <param name="action">Callback</param>
        /// <param name="state">Maybe an Enum?</param>
        public void RegisterMessageBaseInbound(Action<MessageBase> action, object state)
        {
            _messageBaseCallbacks.Add(new Tuple<Action<MessageBase>, object>(action, state));
        }

        /// <summary>
        /// Registers a Native type Callback that will be used when no PreDefined type callback fit ( example MessageBase, RequestMessage, LoginMessage, LargeMessage )
        /// </summary>
        public void AddNativeTypeCallback(Type targetType, Action<object> callback)
        {
            _typeCallbacks.Add(targetType, callback);
        }

        /// <summary>
        /// Registers a Native type Callback that will be used when no PreDefined type callback fit ( example MessageBase, RequestMessage, LoginMessage, LargeMessage )
        /// This callback will not filter for message States and will not be optimised
        /// </summary>
        public void AddNativeTypeCallback<T>(Action<T> callback)
        {
            _typeCallbacks.Add(typeof(T), s => callback((T)s));
        }

        /// <summary>
        /// Register a Callback localy that will be used when no match was found in _typeCallbacks
        /// This callback will not filter for message States and will not be optimised
        /// </summary>
        /// <param name="action">Callback</param>
        public void RegisterMessageBaseInbound(UnkownMessageDelegate action)
        {
            _unkownCallbacks.Add(action);
        }

        /// <summary>
        /// Register a Callback localy that will be used when a new Large message is inbound that has state in its InfoState
        /// </summary>
        /// <param name="action">Callback</param>
        /// <param name="state">Maybe an Enum?</param>
        public void RegisterMessageBaseInbound(Action<LargeMessage> action, object state)
        {
            _largeMessages.Add(new Tuple<Action<LargeMessage>, object>(action, state));
        }

        /// <summary>
        /// Register a Callback localy that will be used when a message contains a given Guid
        /// </summary>
        /// <param name="action"></param>
        /// <param name="guid"></param>
        public void RegisterOneTimeMessage(Action<MessageBase> action, Guid guid)
        {
            _onetimeupdated.Add(new Tuple<Action<MessageBase>, Guid>(action, guid));
        }

        /// <summary>
        /// Register a Callback localy that will be used when a Requst is inbound that has state in its InfoState
        /// </summary>
        /// <param name="action"></param>
        /// <param name="state"></param>
        public void RegisterRequstHandler(Func<RequstMessage, object> action, object state)
        {
            _requestHandler.Add(new Tuple<Func<RequstMessage, object>, object>(action, state));
        }

        /// <summary>
        /// Removes a delegate from the Handler list
        /// </summary>
        /// <param name="action"></param>
        public void UnRegisterRequstHandler(Func<RequstMessage, object> action, object state)
        {
            var enumerable = _requestHandler.FirstOrDefault(s => s.Item1 == action && state == s.Item2);
            if (enumerable != null)
            {
                _requestHandler.Remove(enumerable);
            }
        }

        /// <summary>
        /// Removes a delegate from the Handler list
        /// </summary>
        /// <param name="action"></param>
        public void UnRegisterRequstHandler(Func<RequstMessage, object> action)
        {
            var enumerable = _requestHandler.FirstOrDefault(s => s.Item1 == action);
            if (enumerable != null)
            {
                _requestHandler.Remove(enumerable);
            }
        }


        internal void RegisterRequst(Action<RequstMessage> action, Guid guid)
        {
            _pendingrequests.Add(new Tuple<Action<RequstMessage>, Guid>(action, guid));
        }

        internal void UnRegisterRequst(Guid guid)
        {
            var firstOrDefault = _pendingrequests.FirstOrDefault(s => s.Item2 == guid);
            if (firstOrDefault != null)
            {
                _pendingrequests.Remove(firstOrDefault);
            }
        }

        internal void UnRegisterCallback(Guid guid)
        {
            _onetimeupdated.Remove(_onetimeupdated.FirstOrDefault(s => s.Item2 == guid));
        }


        internal Dictionary<Type, Action<object>> _typeCallbacks;
        protected readonly List<Tuple<Action<LargeMessage>, object>> _largeMessages;
        protected readonly List<Tuple<Action<MessageBase>, Guid>> _onetimeupdated;
        protected readonly List<Tuple<Action<RequstMessage>, Guid>> _pendingrequests;
        protected readonly List<Tuple<Func<RequstMessage, object>, object>> _requestHandler;
        protected readonly List<Tuple<Action<MessageBase>, object>> _messageBaseCallbacks;
        protected readonly List<UnkownMessageDelegate> _unkownCallbacks;
    }
}
