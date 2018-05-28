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
using JPB.Communication.WinRT;
using JPB.Communication.WinRT.combase;
using JPB.Communication.WinRT.combase.Messages;
using JPB.Communication.WinRT.Serilizer;

namespace JPB.Communication.Example.Chat
{
    public class SimpleChat
    {
        public SimpleChat()
        {
            SimpleChat.Main2();
        }

        public static void Main2()
        {
            Networkbase.DefaultMessageSerializer = new NetContractSerializer();
            //NetworkFactory.Create(new WinRTFactory());

            //Define a Contract that is not used by others
            const string messageContract = "CC1AAD01-501C-46F6-A885-1C93946C79F8";

            //the port we are using
            const ushort port = 1337;

            //Register a callback for this Contract
            NetworkFactory.Instance.GetReceiver(port).RegisterMessageBaseInbound(s =>
            {
                Console.WriteLine("> {0}", s.Message);
            }, messageContract);

            var input = "";
            var sender = NetworkFactory.Instance.GetSender(port);
            while (true)
            {
                input = Console.ReadLine();
                sender.SendMessage(new MessageBase(input)
                {
		                InfoState = messageContract
                }, true, NetworkInfoBase.IpAddress.ToString());
            }
        }
    }
}
