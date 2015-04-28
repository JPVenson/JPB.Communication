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
using JPB.Communication.ComBase.Serializer;
using System.Runtime.Serialization;
using JPB.Communication.NativeWin.Serilizer;
using JPB.Communication.NativeWin.WinRT;
using JPB.Communication.Contracts.Intigration;

namespace JPB.Communication.Example.Chat
{
    public class Program
    {
        /// <summary>
        /// this Program will explain the usage of the Communication API
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            //new SimpleChat();
            new Program();
        }

        [DataContract]
        public class ChatMessage
        {
            [DataMember]
            public string Message { get; set; }
            [DataMember]
            public ConsoleColor Color { get; set; }
        }

        public Program()
        {
            Networkbase.DefaultMessageSerializer = new NetContractSerializer();
            NetworkFactory.Create(new WinRTFactory());

            //Maybe multible network Adapters ... on what do we want to Recieve?
            NetworkInfoBase.ResolveOwnIp += NetworkInfoBaseOnResolveOwnIp;
            NetworkInfoBase.ResolveDistantIp += NetworkInfoBaseOnResolveDistantIp;

            Console.Title = string.Format("This: {0}", NetworkInfoBase.IpAddress.ToString());
            Console.Clear();
            //Create an Instance that observe a Port
            ushort port = 1337; //LEED


            var tcpNetworkReceiver

                //The complete access to Sender and Recieiver are done bei the Network Factory
                = NetworkFactory
                //Its a Singelton so we only got one instance
                .Instance
                //This mehtod will create or return an instance for the spezific port
                .GetReceiver(port);


            //Register the callback that will be invoked when a new message is incomming that contains the InfoState ( Contract ) we defined
            tcpNetworkReceiver.RegisterMessageBaseInbound(OnIncommingMessage, ChatMessageContract);

            //-------------------------------------------------------------------------------------
            //we setup the incomming message handlers now we will send a message to the counterpart
            //-------------------------------------------------------------------------------------

            //create a Sender on the same port the same way we did on the Receiver
            var tcpNetworkSender = NetworkFactory.Instance.GetSender(port);

            //If you want to alter control the Process of Serilation set this interface but remember that both Sender and Receiver must use the same otherwise they will not able to work
            //In this case we are using one of the Predefined Serializers
            //tcpNetworkReceiver.Serlilizer = new JPB.Communication.ComBase.Serializer.NetContractSerializer();
            //tcpNetworkSender.Serlilizer = tcpNetworkReceiver.Serlilizer;


            //then we can enable the build in Authentification!!!!!!!!!!!!!!!!!!!!!!

            Console.WriteLine("Server IP or Hostname:");
            bool serverOnline = false;
            string server = null;
            var input = string.Empty;
#if DEBUG
            server = NetworkInfoBase.IpAddress.ToString();
            serverOnline = true;
#else
            input = Console.ReadLine();
            
            //Mehtod to Get the Ipaddress from an Host name
            do
            {
                try
                {
                    var hostAddresses = NetworkFactory.PlatformFactory.DnsFactory.GetHostAddresses(input);
                    server = NetworkInfoBaseOnResolveOwnIp(hostAddresses).ToString();
                    serverOnline = true;
                }
                catch (Exception)
                {
                    Console.WriteLine("Server not known or other error try again");
                    serverOnline = false;
                }
            } while (!serverOnline);
#endif
            Console.Clear();
            Console.Title = string.Format("{0} , That : {1}", Console.Title, server);
            Console.WriteLine("Server Found");
            //now, send as long as the user want to
            do
            {
                Console.WriteLine("Please enter Message");
                Console.ForegroundColor = ConsoleColor.Green;
                input = Console.ReadLine();
                Console.ResetColor();
                var chatMess = new ChatMessage();
                chatMess.Message = input;
                chatMess.Color = ConsoleColor.Red;
                var indexOfColor = input.IndexOf(":");
                if (indexOfColor != -1)
                {
                    var maybeColor = input.Substring(0, indexOfColor).ToLower();
                    var findConsoleColor = Enum.GetNames(typeof(ConsoleColor)).FirstOrDefault(s => s.ToLower() == maybeColor);
                    if (findConsoleColor != null)
                    {
                        chatMess.Color = (ConsoleColor)Enum.Parse(typeof(ConsoleColor), findConsoleColor);
                        chatMess.Message = input.Remove(0, indexOfColor + 1);
                    }
                }

                //create a new MessageBase object or one object that inherts from it
                var message = new MessageBase()
                {
                    InfoState = ChatMessageContract,
                    Message = chatMess
                };

                //Send the object over the network
                var sendMessage = tcpNetworkSender.SendMessage(message, server);
                if (!sendMessage)
                {
                    Console.WriteLine("Server may be offline ... message was not send ... stop program");
                    Console.ReadKey();
                    break;
                }
            } while (!input.ToLower().Equals("exit"));
        }

        private IPAddress NetworkInfoBaseOnResolveDistantIp(IPAddress[] arg1, string arg2)
        {
            return NetworkInfoBaseOnResolveOwnIp(arg1);
        }

        private static IPAddress NetworkInfoBaseOnResolveOwnIp(IPAddress[] ipAddresses)
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

        private static void OnIncommingMessage(MessageBase obj)
        {
            var mess = (obj.Message as ChatMessage);
            var old = Console.ForegroundColor;
            Console.ForegroundColor = mess.Color;
            Console.WriteLine("> {0} : \"{1}\"", obj.Sender, mess.Message);
            Console.ForegroundColor = old;
        }

        /// <summary>
        /// This is the Contract that is defined to ensure a unambiguous Communication
        /// It could be any object that can be compared with the Equals mehtod
        /// it must be the Same value on all PC's
        /// </summary>
        public static string ChatMessageContract
        {
            get { return "379DFA0E-2E2F-425A-9325-6D59EC37A1AC"; }
        }
    }
}
