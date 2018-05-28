using System.Collections.Generic;
using JPB.Communication.WinRT.Combase.Generic;
using JPB.Communication.WinRT.combase.Messages;

namespace JPB.Communication.WinRT.Shared
{
    public class Broadcaster
    {
        GenericNetworkReceiver _receiver;

        public const string RegisterReceiver = "B90444A0-9CD3-458E-AB66-CB3217484F29";
        public const string BroadcastMessage = "58BD08D7-53DB-415C-B4E2-8DA7FD847E60";

        public Broadcaster(ushort listen) 
            : this(NetworkFactory.Instance.CreateReceiver(listen))
        {

        }

        public Broadcaster(GenericNetworkReceiver receiver)
        {
            _receiver = receiver;
            _receiver.SharedConnection = true;
            _receiver.OnSharedConnectionCreated += _receiver_OnSharedConnectionCreated;
            _receiver.RegisterMessageBaseInbound(OnBroadcastMessage, BroadcastMessage);
        }

        private List<ConnectionWrapper> _connections = new List<ConnectionWrapper>();

        private void _receiver_OnSharedConnectionCreated(ConnectionWrapper obj)
        {
            _connections.Add(obj);
        }

        protected virtual bool _receiver_OnCheckConnectionInbound(GenericNetworkReceiver arg1, Contracts.Intigration.ISocket arg2)
        {
            return true;
        }

        protected virtual void OnBroadcastMessage(MessageBase obj)
        {
            foreach (var item in _connections)
            {
                item.GenericNetworkSender.SendMessageAsync(obj, true, item.Socket.RemoteEndPoint.Address.AddressContent);
            }
        }
    }
}
