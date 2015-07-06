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
using JPB.Communication.ComBase.Messages;
using JPB.Communication.ComBase.Messages.Wrapper;

namespace JPB.Communication.Contracts
{
    public interface INetworkReceiver
    {
        ushort Port { get; }

        /// <summary>
        ///     True if we are Recieving a message
        /// </summary>
        bool IncommingMessage { get; }

        /// <summary>
        ///     Removes a delegate from the Handler list
        /// </summary>
        /// <param name="action"></param>
        void UnregisterChanged(Action<MessageBase> action, object state);

        /// <summary>
        ///     Removes a delegate from the Handler list
        /// </summary>
        /// <param name="action"></param>
        void UnregisterChanged(Action<MessageBase> action);

        /// <summary>
        ///     Register a Callback localy that will be used when a new message is inbound that has state in its InfoState
        /// </summary>
        /// <param name="action">Callback</param>
        /// <param name="state">Maybe an Enum?</param>
        void RegisterMessageBaseInbound(Action<MessageBase> action, object state);

        /// <summary>
        ///     Register a Callback localy that will be used when a new Large message is inbound that has state in its InfoState
        /// </summary>
        /// <param name="action">Callback</param>
        /// <param name="state">Maybe an Enum?</param>
        void RegisterMessageBaseInbound(Action<LargeMessage> action, object state);

        /// <summary>
        ///     Register a Callback localy that will be used when a message contains a given Guid
        /// </summary>
        /// <param name="action"></param>
        /// <param name="guid"></param>
        void RegisterOneTimeMessage(Action<MessageBase> action, Guid guid);

        /// <summary>
        ///     Register a Callback localy that will be used when a Requst is inbound that has state in its InfoState
        /// </summary>
        /// <param name="action"></param>
        /// <param name="state"></param>
        void RegisterRequstHandler(Func<RequstMessage, object> action, object state);

        /// <summary>
        ///     Removes a delegate from the Handler list
        /// </summary>
        /// <param name="action"></param>
        void UnRegisterRequstHandler(Func<RequstMessage, object> action, object state);

        /// <summary>
        ///     Removes a delegate from the Handler list
        /// </summary>
        /// <param name="action"></param>
        void UnRegisterRequstHandler(Func<RequstMessage, object> action);        
    }
}