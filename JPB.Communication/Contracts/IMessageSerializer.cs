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

using JPB.Communication.ComBase.Messages;

namespace JPB.Communication.Contracts
{
    /// <summary>
    /// </summary>
    public interface IMessageSerializer
    {
        /// <summary>
        ///     Is used to convert the Complete Message Object into a byte[] that will be transferted to the remote computer
        /// </summary>
        /// <param name="a"></param>
        /// <returns></returns>
        byte[] SerializeMessage(MessageBase a);

        /// <summary>
        ///     Converts the output from the TCP network adapter into a valid TCP message
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        MessageBase DeSerializeMessage(byte[] source);

        /// <summary>
        ///     tries to convert the Message to a string Representation
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        string ResolveStringContent(byte[] message);
    }
}