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

namespace JPB.Communication.ComBase
{
    public class NetworkMessage 
    {
        public NetworkMessage()
        {
            GUID = Guid.NewGuid();
        }

        //internal NetworkMessage(SerializationInfo info,
        //    StreamingContext context)
        //{
        //    GUID = (Guid) info.GetValue("GUID", typeof (Guid));
        //    MessageBase = (byte[]) info.GetValue("MessageBase", typeof (byte[]));
        //    Sender = (string) info.GetValue("Sender", typeof (string));
        //    Reciver = (string) info.GetValue("Reciver", typeof (string));
        //}

        public Guid GUID { get; set; }
        public byte[] MessageBase { get; set; }
        public string Sender { get; set; }
        public string Reciver { get; set; }

        //public void GetObjectData(SerializationInfo info, StreamingContext context)
        //{
        //    info.AddValue("Reciver", Reciver, Reciver.GetType());
        //    info.AddValue("Sender", Sender, Sender.GetType());
        //    info.AddValue("MessageBase", MessageBase, MessageBase.GetType());
        //    info.AddValue("GUID", GUID, GUID.GetType());
        //}
    }
}