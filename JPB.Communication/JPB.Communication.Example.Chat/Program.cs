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
using JPB.Communication.PCLIntigration.ComBase;
using System.Diagnostics;
using JPB.Communication.WinRT.Serilizer;
using JPB.Communication.ComBase.TCP;

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
        [Serializable]
        public class ChatMessage
        {
            [DataMember]
            public string Message { get; set; }
            [DataMember]
            public ConsoleColor Color { get; set; }
        }

        GenericNetworkReceiver tcpNetworkReceiver;
        private const char ColorSeperator = ':';

        public Program()
        {
            Networkbase.DefaultMessageSerializer = new FullXmlSerilizer(typeof(MessageBase), typeof(ChatMessage));
            NetworkFactory.Create(new WinRTLocalFactory());

            //Maybe multible network Adapters ... on what do we want to Recieve?
            NetworkInfoBase.ResolveOwnIp += NetworkInfoBaseOnResolveOwnIp;
            NetworkInfoBase.ResolveDistantIp += NetworkInfoBaseOnResolveDistantIp;


            //Create an Instance that observe a Port
            ushort port = 1337; //LEED

            tcpNetworkReceiver
                //The complete access to Sender and Recieiver are done bei the Network Factory
                = NetworkFactory
                //Its a Singelton so we only got one instance
                .Instance
                //This mehtod will create or return an instance for the spezific port
                .GetReceiver(port);

            //Check for the platform relevant shared connection that allows us to bypass NAT restrictions
            if (NetworkFactory.PlatformFactory.SocketFactory.SupportsSharedState == Contracts.Factorys.SharedStateSupport.Full)
                tcpNetworkReceiver.SharedConnection = true;

            //Lets submit and Await the Credentials buffer at the very first of our messages
            tcpNetworkReceiver.CheckCredentials = true;

            //attach some more or less self explaining event handler to the Authentificator to handle logins
            NetworkAuthentificator.Instance.OnLoginInbound += (s, e) =>
            {
                WriteLoginMessage(string.Format("Login Inbound '{0}'", e.Username));
            };

            NetworkAuthentificator.Instance.OnValidateUnknownLogin += (s, e) =>
            {
                var domain = e.Username.Substring(0, e.Username.IndexOf("@"));
                var good = domain == Environment.UserDomainName ? AuditState.AccessAllowed : AuditState.AccessDenyed;
                WriteLoginMessage(string.Format("Validate Username '{0}' -> {1}", e.Username, good));
                return good;
            };

            NetworkAuthentificator.Instance.OnValidateUserPassword += (s, e) =>
            {
                WriteLoginMessage(string.Format("Validate Password '{0}' -> {1}", e.Password, AuditState.AccessAllowed));
                //Allow anonymus login
                return true;
            };

            //Register the callback that will be invoked when a new message is incomming that contains the InfoState ( Contract ) we defined
            tcpNetworkReceiver.RegisterMessageBaseInbound(OnIncommingMessage, ChatMessageContract);

            //-------------------------------------------------------------------------------------
            //we setup the incomming message handlers now we will send a message to the counterpart
            //-------------------------------------------------------------------------------------

            //create a Sender on the same port the same way we did on the Receiver
            var tcpNetworkSender = NetworkFactory.Instance.GetSender(port);

            var login = new PCLIntigration.ComBase.Messages.LoginMessage()
            {
                Username = Environment.UserDomainName + "@" + Environment.UserName,
                Password = "Nothing"
            };
            //add our login cred buffer that will be send when we send our first message or everytime if we are not using a shared connection
            tcpNetworkSender.ChangeNetworkCredentials(true, login);
            WriteSystemMessage("Set Cred:");
            WriteSystemMessage(login.ToString());

            //If you want to alter control the Process of Serilation set this interface but remember that both Sender and Receiver must use the same otherwise they will not able to work
            //In this case we are using one of the Predefined Serializers
            //tcpNetworkReceiver.Serlilizer = new JPB.Communication.ComBase.Serializer.NetContractSerializer();
            //tcpNetworkSender.Serlilizer = tcpNetworkReceiver.Serlilizer;     

            Console.Title = string.Format("This: {0}", NetworkInfoBase.IpAddress.ToString());
            Console.Clear();

            WriteSystemMessage("Server IP or Hostname:");
            bool serverOnline = false;
            string server = null;
            InputWrapper input;
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
            Console.Title = string.Format("{0} , That : {1}", Console.Title, server);
            WriteSystemMessage("Server Found");
            //Test it with send HelloWorld

            var helloWorldMessage = new ChatMessage();
            helloWorldMessage.Message = string.Format("Hello world from {0}", Environment.UserName);
            helloWorldMessage.Color = ConsoleColor.Blue;

            //Setup our shared connection that will keep the connection open as long as we need it
            //this will also allow us to bypass our local NAT
            //The return value is not interesing for us at this moment

            if (NetworkFactory.PlatformFactory.SocketFactory.SupportsSharedState == Contracts.Factorys.SharedStateSupport.Full)
            {
                tcpNetworkSender.InitSharedConnection(server);
                tcpNetworkSender.SharedConnection = true;
            }
            tcpNetworkSender.SendMessage(new MessageBase(helloWorldMessage, ChatMessageContract), server);

            //now, send as long as the user want to
            do
            {
                Console.WriteLine("Please enter Message");

                input = ParseInput();

                var chatMess = new ChatMessage();
                chatMess.Message = input.Text;
                chatMess.Color = input.Color;

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
            } while (true);
        }

        public class InputWrapper
        {
            public string Text { get; set; }
            public ConsoleColor Color { get; set; }
        }

        private InputWrapper ParseInput()
        {
            var inputwrapper = new InputWrapper();
            var firstInput = Console.ReadLine();

            var findConsoleColor = Enum.GetNames(typeof(ConsoleColor)).FirstOrDefault(s => s.ToLower() == firstInput);
            if (findConsoleColor != null)
            {
                inputwrapper.Color = (ConsoleColor)Enum.Parse(typeof(ConsoleColor), findConsoleColor);
                Console.ForegroundColor = inputwrapper.Color;
                inputwrapper.Text = Console.ReadLine();
                Console.ResetColor();
            }
            else
            {
                inputwrapper.Color = ConsoleColor.White;
                inputwrapper.Text = firstInput;
            }
            return inputwrapper;
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

        private static void WriteSystemMessage(string mess)
        {
            var old = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("System> {0}", mess);
            Console.ForegroundColor = old;
        }

        private static void WriteLoginMessage(string mess)
        {
            var old = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Login System> {0}", mess);
            Console.ForegroundColor = old;
        }

        private void OnIncommingMessage(MessageBase obj)
        {
            var mess = (obj.Message as ChatMessage);
            var old = Console.ForegroundColor;
            Console.ForegroundColor = mess.Color;
            Console.WriteLine("> {0} : \"{1}\"", tcpNetworkReceiver.Session.Calle.Username, mess.Message);
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
