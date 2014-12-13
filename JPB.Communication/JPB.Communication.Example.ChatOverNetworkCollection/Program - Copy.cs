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
using System.Text;
using System.Threading.Tasks;
using JPB.Communication.ComBase;
using JPB.Communication.ComBase.Messages;
using JPB.Communication.Shared;

namespace JPB.Communication.Example.ChatOverNetworkCollection
{
    public class Program2
    {
        static void Main2(string[] args)
        {
            //Define a Contract that is not used by others
            var messageContract = "CC1AAD01-501C-46F6-A885-1C93946C79F8";

            //the port we are using
            ushort port = 1337;

            //Register a callback for this Contract
            NetworkFactory.Instance.GetReceiver(port).RegisterChanged(s =>
            {
                Console.WriteLine("> {0}", s.Message);
            }, messageContract);

            var input = "";
            var sender = NetworkFactory.Instance.GetSender(port);
            while (true)
            {
                input = Console.ReadLine();
                sender.SendMessage(new MessageBase(input) { InfoState = messageContract }, NetworkInfoBase.IpAddress.ToString());
            }
        }
    }
}
