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
    public static class NetworkInfoBase
    {
        private static IPAddress _ip;

        /// <summary>
        /// Easy access to your preferred network Interface 
        /// </summary>
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
                    _ip = RaiseResolveOwnIp(firstOrDefault.AddressList);
                }
                else
                {
                    _ip = ResolveAddressByMySelf____Again____IfYouNeedSomethingToBeDoneRightDoItByYourSelf(firstOrDefault.AddressList);
                }
                return _ip;
            }
        }

        /// <summary>
        /// If your pc is connected via multible network Interfaces you can here resolve your IP
        /// </summary>
        public static event Func<IPAddress[], IPAddress> ResolveOwnIp;

        internal static IPAddress RaiseResolveOwnIp(IPAddress[] addresses)
        {
            var handler = ResolveOwnIp;
            if (handler != null)
                return handler(addresses);
            return ResolveAddressByMySelf____Again____IfYouNeedSomethingToBeDoneRightDoItByYourSelf(addresses);
        }

        /// <summary>
        /// If a Host is specified, use this event to resolve the IP
        /// </summary>
        public static event Func<IPAddress[], string, IPAddress> ResolveDistantIp;

        internal static IPAddress RaiseResolveDistantIp(IPAddress[] addresses, string hostName)
        {
            if (addresses.Length == 1)
                return addresses.First();

            var handler = ResolveDistantIp;
            if (handler != null)
                return handler(addresses, hostName);
            return ResolveAddressByMySelf____Again____IfYouNeedSomethingToBeDoneRightDoItByYourSelf(addresses);
        }

        /// <summary>
        /// Not a Comment!
        /// </summary>
        /// <param name="addresses"></param>
        /// <returns></returns>
        private static IPAddress ResolveAddressByMySelf____Again____IfYouNeedSomethingToBeDoneRightDoItByYourSelf(IEnumerable<IPAddress> addresses)
        {
            //The last address might be the local real address
            return addresses.LastOrDefault(ip => ip.AddressFamily == AddressFamily.InterNetwork);
        }
    }
}