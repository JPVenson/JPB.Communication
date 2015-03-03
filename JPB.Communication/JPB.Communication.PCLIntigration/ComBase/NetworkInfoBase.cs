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

using JPB.Communication.PCLIntigration.ComBase;
using JPB.Communication.PCLIntigration.Contracts;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace JPB.Communication.ComBase
{
    /// <summary>
    ///     This class contains informations and Mehtods for IP resolution
    /// </summary>
    public static class NetworkInfoBase
    {
        private static IPAddress _ip;
        private static IPAddress _exIp;

        public static readonly String IPADDRESS_PATTERN =
            @"\b\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}\b";

        static NetworkInfoBase()
        {
            IpCheckUrl = "http://checkip.dyndns.org/";
        }

        /// <summary>
        ///     Easy access to your preferred network Interface
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
                    _ip =
                        ResolveAddressByMySelf____Again____IfYouNeedSomethingToBeDoneRightDoItByYourSelf(
                            firstOrDefault.AddressList);
                }
                return _ip;
            }
        }

        /// <summary>
        ///     Returns the Cached last external IpAddress
        ///     May first a blocking call. To Prevent block call "GetPublicIp" first
        /// </summary>
        public static IPAddress IpAddressExternal
        {
            get
            {
                if (_exIp != null)
                {
                    return _exIp;
                }

                var blockingCall = GetPublicIp();
                blockingCall.Wait();

                _exIp = IPAddress.Parse(blockingCall.Result);
                return _exIp;
            }
        }

        public static string IpCheckUrl { get; set; }

        /// <summary>
        ///     Uses the NetworkInfoBase.ResolveDistantIp to resvoles an IP
        /// </summary>
        /// <param name="host"></param>
        /// <returns></returns>
        public static IPAddress ResolveIp(string host)
        {
            return RaiseResolveDistantIp(Dns.GetHostAddresses(host), host);
        }

        public class RequestState
        {
            // This class stores the State of the request. 
            const int BUFFER_SIZE = 1024;
            public StringBuilder requestData;
            public byte[] BufferRead;
            public HttpWebRequest request;
            public HttpWebResponse response;
            public Stream streamResponse;
            public RequestState()
            {
                BufferRead = new byte[BUFFER_SIZE];
                requestData = new StringBuilder("");
                request = null;
                streamResponse = null;
            }
        }

        /// <summary>
        ///     Uses IpCheckUrl for IP check
        /// </summary>
        /// <returns></returns>
        public async static Task<string> GetPublicIp()
        {
            String direction = "";
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(IpCheckUrl);
            var waitForResp = new ManualResetEventSlim();
            var requestWrapper = new RequestState();
            requestWrapper.request = request;
           
            request.BeginGetResponse(s => 
            {
                using (var stream = new StreamReader((s.AsyncState as RequestState).streamResponse))
                {
                    direction = stream.ReadToEnd();
                }
                waitForResp.Set();
            }, requestWrapper);


            waitForResp.Wait(90000);           

            Match match = Regex.Match(direction, IPADDRESS_PATTERN);

            if (!match.Success)
            {
                throw new KeyNotFoundException(
                    String.Format("Not able to find an ip address inside the Response from '{0}'", IpCheckUrl));
            }

            string ipAddress = match.Value;

            IPAddress address = IPAddress.Parse(ipAddress);

            if (!address.Equals(_exIp))
            {
                _exIp = address;
            }

            return ipAddress;
        }

        /// <summary>
        ///     If your pc is connected via multible network Interfaces you can here resolve your IP
        /// </summary>
        public static event Func<IPAddress[], IPAddress> ResolveOwnIp;

        internal static IPAddress RaiseResolveOwnIp(IPAddress[] addresses)
        {
            if (addresses.Length == 1)
                return addresses.First();

            Func<IPAddress[], IPAddress> handler = ResolveOwnIp;
            if (handler != null)
                return handler(addresses);
            return ResolveAddressByMySelf____Again____IfYouNeedSomethingToBeDoneRightDoItByYourSelf(addresses);
        }

        /// <summary>
        ///     If a Host is specified, use this event to resolve the IP
        /// </summary>
        public static event Func<IPAddress[], string, IPAddress> ResolveDistantIp;

        internal static IPAddress RaiseResolveDistantIp(IPAddress[] addresses, string hostName)
        {
            if (addresses.Length == 1)
                return addresses.First();

            Func<IPAddress[], string, IPAddress> handler = ResolveDistantIp;
            if (handler != null)
                return handler(addresses, hostName);
            return ResolveAddressByMySelf____Again____IfYouNeedSomethingToBeDoneRightDoItByYourSelf(addresses);
        }

        /// <summary>
        ///     /*No Comment!*/
        /// </summary>
        /// <param name="addresses"></param>
        /// <returns></returns>
        private static IPAddress ResolveAddressByMySelf____Again____IfYouNeedSomethingToBeDoneRightDoItByYourSelf(
            IEnumerable<IPAddress> addresses)
        {
            //The last address might be the local real address
            return addresses.LastOrDefault();
        }
    }
}