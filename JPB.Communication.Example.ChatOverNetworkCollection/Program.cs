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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JPB.Communication.ComBase;
using System.Runtime.Serialization;
using JPB.Communication.Contracts.Intigration;
using JPB.Communication.Shared;
using System.Threading;
using JPB.Communication.WinRT.Serilizer;
using JPB.Communication.WinRT.WinRT;

namespace JPB.Communication.Example.ChatOverNetworkCollection
{
    /// <summary>
    /// Creates a test program that is using the NetworkValueBag to sync messages over a local or the Internetwork.
    /// The Network Value bag will create a local instance an will attach to the given TCP Port
    /// When calling Connect it will be trying to connect to the given host and then it will get all other exisiting host and a copy of the values that are currently attached to this bag
    /// The order of the bag can but must not the same then on other Pcs
    /// The Bag is using its own contracts an Protocol to manage all items
    /// The Add, Clear and Remove mehtods are supported and are blocking to prevend a desync state
    /// </summary>
    public class Program
    {
        NetworkValueBag<string> networkValueCollection;

        /// <summary>
        /// This example will show the usage of the NetworkValueBag
        /// WIP
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            new Program();
            Thread.CurrentThread.Join();
        }

        public Program()
        {
            Networkbase.DefaultMessageSerializer = new NetContractSerializer();
            NetworkFactory.Create(new WinRTFactory());

            //create an Instance of the NetworkValueBag
            NetworkInfoBase.ResolveOwnIp += NetworkInfoBaseOnResolveOwnIp;
            NetworkInfoBase.ResolveDistantIp += NetworkInfoBaseOnResolveRemoteIp;

            networkValueCollection = NetworkValueBag<string>.CreateNetworkValueCollection(1337,

                //To have multiple NetworkBags on the Same system and Port we need a Valid Uniq identifier
                "AnyUniqValidStringLikeAGuid: 46801E06-AB14-4910-BA95-7E13F58F4186");

            SyncRoot = networkValueCollection.SyncRoot;


            Console.WriteLine("Connect to a server(y) or be the first(n)? y/n");

            var consoleKeyInfo = Console.ReadKey();
            if (consoleKeyInfo.Key == ConsoleKey.Y)
            {
                Console.WriteLine("Enter Ip or Host Name");
                var readLine = Console.ReadLine();
                var resolveIp = NetworkInfoBase.ResolveIp(readLine);
                Console.WriteLine("Remote ip is {0}", resolveIp);
                Console.WriteLine("Trys to connect to ip");
                var connect = networkValueCollection.Connect(resolveIp.ToString());
                connect.Wait();
                Console.WriteLine("Connect {0}", connect.Result ? "Successful" : "Failed");
            }

            networkValueCollection.CollectionChanged += networkValueCollection_CollectionChanged;

            QueueMessages();
        }

        private void QueueMessages()
        {
            Console.Clear();
            var input = "Hello world";

            while (!string.IsNullOrEmpty(input) && input != "quit")
            {
                networkValueCollection.Add(FormartChatMessage(input));
                input = Console.ReadLine();
            }
        }

        /// <summary>
        /// 0 = This IP
        /// 1 = This Username
        /// 2 = Message
        /// 3 = Date
        /// </summary>
        private string MessageFormat = "From: {0}\r\n{1}:{2}\r\n'{3}'";
        private string FormartChatMessage(string message)
        {
            return string.Format(MessageFormat, NetworkInfoBase.IpAddress.ToString(), Environment.UserName, message, DateTime.Now.ToShortTimeString());
        }

        public object SyncRoot;

        void networkValueCollection_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            lock (SyncRoot)
            {
                Console.Clear();
                var sb = new StringBuilder();
                var enumerable = networkValueCollection.Take(10);
                foreach (var item in enumerable)
                {
                    sb.AppendLine(item);
                }

                Console.WriteLine(sb.ToString());
            }
        }

        private IPAddress NetworkInfoBaseOnResolveRemoteIp(IPAddress[] arg1, string arg2)
        {
            Console.WriteLine("Remote host is {0}", arg2);
            return NetworkInfoBaseOnResolveOwnIp(arg1);
        }

        private IPAddress NetworkInfoBaseOnResolveOwnIp(IPAddress[] ipAddresses)
        {
            Console.WriteLine("Multible Addresses detected choose one");
            Console.WriteLine("ID | IP");
            Console.WriteLine("-------");
            for (int index = 0; index < ipAddresses.Length; index++)
            {
                var ipAddress = ipAddresses[index];
                Console.WriteLine("{0}  | {1}", index, ipAddress.ToString());
            }

            int input = -1;
            do
            {
                Console.WriteLine("Select the Id please");
                int.TryParse(Console.ReadLine(), out input);
            } while (input <= 0 && input >= ipAddresses.Length);

            return ipAddresses[input];
        }
    }
}
