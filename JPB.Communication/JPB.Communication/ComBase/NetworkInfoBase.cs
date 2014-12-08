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

#region Jean-Pierre Bachmann

// Erstellt von Jean-Pierre Bachmann am 10:03

#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;

namespace JPB.Communication.ComBase
{
    public class NetworkInfoBase
    {
        private static IPAddress _ip;

        public static IPAddress IpAddress
        {
            get
            {
                if (_ip != null)
                    return _ip;

                var firstOrDefault = Dns.GetHostEntry(Dns.GetHostName());
                    //.AddressList.FirstOrDefault(ip => ip.AddressFamily == AddressFamily.InterNetwork);
                if (firstOrDefault.AddressList.Length > 1)
                {
                    _ip = RaiseResolveIp(firstOrDefault.AddressList);
                }
                else
                {
                    _ip = ResolveAddressByMySelf____Again____IfYouNeedSomethingToBeDoneRightDoItByYourSelf(firstOrDefault.AddressList);
                }
                return _ip;
            }
        }

        public static event Func<IPAddress[], IPAddress> ResolveIp;

        private static IPAddress RaiseResolveIp(IPAddress[] addresses)
        {
            var handler = ResolveIp;
            if (handler != null)
                return handler(addresses);
            return ResolveAddressByMySelf____Again____IfYouNeedSomethingToBeDoneRightDoItByYourSelf(addresses);
        }

        private static IPAddress ResolveAddressByMySelf____Again____IfYouNeedSomethingToBeDoneRightDoItByYourSelf(IEnumerable<IPAddress> addresses)
        {
            return addresses.FirstOrDefault(ip => ip.AddressFamily == AddressFamily.InterNetwork);
        }
    }
}