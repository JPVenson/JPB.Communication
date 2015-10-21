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
using System.Linq;
using JPB.Communication.ComBase;
using JPB.Communication.ComBase.Generic;
using JPB.Communication.ComBase.Messages;
using System.Runtime.Serialization;
using JPB.Communication.ComBase.Security;
using JPB.Communication.Contracts.Intigration;
using JPB.Communication.WinRT.Serilizer;
using JPB.Communication.WinRT.WinRT;
using System.Threading.Tasks;
using System.Threading;

namespace JPB.Communication.Example.Chat
{
    public class Program
    {
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
        private static ConsoleColor _defaultColor;

        private const string Lock = "";
        string server = null;
        GenericNetworkSender tcpNetworkSender;

        static int _line = 0;
        const int bottomLine = int.MaxValue;
        private void Clear()
        {
            _line = 0;
            Console.Clear();
        }

        private static void WriteLine(string format, int line = -1, params object[] keys)
        {
            lock (Lock)
            {
                var last = Console.CursorTop;
                if (line == -1)
                {
                    Console.SetCursorPosition(0, _line);
                    Console.WriteLine(string.Format(format, keys));
                }
                else
                {
                    if (line == bottomLine)
                    {
                        line = Console.WindowHeight - 1;
                    }

                    Console.SetCursorPosition(0, line);
                    Console.WriteLine(string.Format(format, keys));
                }
                Console.SetCursorPosition(0, last);
                _line++;
            }
        }

        private static void Write(string format, int line = -1, params object[] keys)
        {
            lock (Lock)
            {
                var last = Console.CursorTop;
                var posY = Console.CursorLeft;
                if (line == -1)
                {
                    Console.SetCursorPosition(posY, _line);
                    Console.Write(string.Format(format, keys));
                }
                else
                {
                    if (line == bottomLine)
                    {
                        line = Console.WindowHeight - 1;
                    }

                    Console.SetCursorPosition(posY, line);
                    Console.Write(string.Format(format, keys));
                }
            }
        }

        private static string ReadLine(int line = -1)
        {
            if (line == -1)
            {
                var input = Console.ReadLine();
                _line++;
                return input;
            }
            else
            {
                if (line == bottomLine)
                {
                    line = Console.WindowHeight - 1;
                }

                if (line <= _line)
                {
                    line = _line + 1;
                }

                var last = Console.CursorTop;
                Console.SetCursorPosition(0, line);
                var input = Console.ReadLine();
                Console.SetCursorPosition(0, last);
                _line++;
                return input;
            }
        }

        public Program()
        {
            Run();
        }

        public async void Run()
        {
            Networkbase.DefaultMessageSerializer = new FullXmlSerilizer(typeof(MessageBase), typeof(ChatMessage));
            NetworkFactory.Create(new WinRTFactory());

            //Maybe multible network Adapters ... on what do we want to Recieve?
            NetworkInfoBase.ResolveOwnIp += NetworkInfoBaseOnResolveOwnIp;
            NetworkInfoBase.ResolveDistantIp += NetworkInfoBaseOnResolveDistantIp;

            //Create an Instance that observe a Port
            ushort port = 1338; //LEED

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
                var domain = e.Username.Substring(0, e.Username.IndexOf("@", System.StringComparison.Ordinal));
                var good = domain == Environment.UserDomainName ? AuditState.AccessAllowed : AuditState.AccessDenyed;
                WriteLoginMessage(string.Format("Validate Username '{0}' -> {1}", e.Username, good));
                if (good == AuditState.AccessAllowed)
                    return AuditState.CheckPassword;
                return good;
            };

            NetworkAuthentificator.Instance.OnValidateUserPassword += (s, e) =>
            {
                WriteLoginMessage(string.Format("Validate Password '{0}' -> {1}", e.Password, AuditState.AccessAllowed));
                //Allow anonymus login
                return true;
            };

            //Register the callback that will be invoked when a new message is incomming
            //there are 2 ways that will would fit
            //If 'ChatMessage' inherts from MessageBase, we could simply use the RegisterMessageBaseInbound(Callback, Infostate)
            //else 
            //we can define an 'UnkownMessageHandler' and check by each message the Receiver gets that does not inhert from MessageBase
            //tcpNetworkReceiver.RegisterMessageBaseInbound(OnMessageBoostrap);
            //or
            //we define our own handler by adding a native typecallback
            //this will not take care of filter for any InfoState that is Maybe transfered
            tcpNetworkReceiver.AddNativeTypeCallback<ChatMessage>(s => OnMessageBoostrap(s));

            //-------------------------------------------------------------------------------------
            //we setup the incomming message handlers now we will send a message to the counterpart
            //-------------------------------------------------------------------------------------

            //create a Sender on the same port the same way we did on the Receiver
            tcpNetworkSender = NetworkFactory.Instance.GetSender(port);

            var login = new ComBase.Messages.LoginMessage()
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
            Clear();

            WriteSystemMessage("Server IP or Hostname:");
            bool serverOnline = false;


#if DEBUG
            server = NetworkInfoBase.IpAddress.ToString();
            serverOnline = true;
#else
            var host = Console.ReadLine();
            
            //Mehtod to Get the Ipaddress from an Host name
            do
            {
                try
                {
                    var hostAddresses = NetworkFactory.PlatformFactory.DnsFactory.GetHostAddresses(host);
                    server = NetworkInfoBaseOnResolveOwnIp(hostAddresses).ToString();
                    serverOnline = true;
                }
                catch (Exception)
                {
                    WriteLine("Server not known or other error try again");
                    serverOnline = false;
                }
            } while (!serverOnline);
#endif
            Console.Title = string.Format("{0} , That : {1}", Console.Title, null);
            WriteSystemMessage("Server Found");
            //Test it with send HelloWorld
            InitUser(Environment.UserName);

            //Setup our shared connection that will keep the connection open as long as we need it
            //this will also allow us to bypass our local NAT
            //The return value is not interesing for us at this moment

            if (NetworkFactory.PlatformFactory.SocketFactory.SupportsSharedState == Contracts.Factorys.SharedStateSupport.Full)
            {
                tcpNetworkSender.InitSharedConnection((string)null);
                tcpNetworkSender.SharedConnection = true;
            }

            PermColor = ConsoleColor.Gray;
            RunHumanInput();
        }

        private async void RunHumanInput()
        {
            InputWrapper input;
            //now, send as long as the user want to
            do
            {
                input = ParseInput();

                if (input == null)
                    continue;

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
                var sendMessage = await tcpNetworkSender.SendMessageAsync(chatMess, server);
                if (!sendMessage)
                {
                    WriteLine("Server may be offline ... message was not send ... stop program");
                    Console.ReadKey();
                    break;
                }
            } while (true);
        }

        private async void InitUser(string username)
        {
            var helloWorldMessage = new ChatMessage();
            helloWorldMessage.Message = string.Format("Hello world from {0}", username);
            helloWorldMessage.Color = ConsoleColor.Blue;
            await tcpNetworkSender.SendMessageAsync(helloWorldMessage, server);
        }

        //int chatBotNr = 0;
        //private Action RunChatBot(string username)
        //{
        //    chatBotNr++;
        //    var stop = false;
        //    var task = new Task(() =>
        //    {
        //        InitUser(username);
        //        var color = Colors.ElementAt(chatBotNr);

        //        do
        //        {
        //            Thread.Sleep(1000);
        //            var chatMess = new ChatMessage();
        //            chatMess.Message = "test";
        //            chatMess.Color = color;

        //            //create a new MessageBase object or one object that inherts from it
        //            var message = new MessageBase()
        //            {
        //                InfoState = ChatMessageContract,
        //                Message = chatMess
        //            };

        //            //Send the object over the network

        //            var login = new ComBase.Messages.LoginMessage()
        //            {
        //                Username = Environment.UserDomainName + "@" + Environment.UserName,
        //                Password = "Nothing"
        //            };
        //            //add our login cred buffer that will be send when we send our first message or everytime if we are not using a shared connection
        //            tcpNetworkSender.ChangeNetworkCredentials(true, login);
        //            var sendMessage = tcpNetworkSender.SendMessage(message, server);

        //        } while (!stop);
        //    });

        //    return () => stop = true;
        //}

        public class InputWrapper
        {
            public string Text { get; set; }
            public ConsoleColor Color { get; set; }
        }

        private readonly static ConsoleColor[] Colors = Enum.GetValues(typeof(ConsoleColor))
            .Cast<ConsoleColor>()
            .ToArray();

        public InputWrapper ParseInput()
        {
            var inputwrapper = new InputWrapper();

            if (PermColor != null)
                Console.ForegroundColor = PermColor.Value;

            var firstInput = ReadLine(bottomLine);

            if (PermColor != null)
                Console.ResetColor();

            var empty = "";
            for (int i = 0; i < firstInput.Length; i++)
            {
                empty += " ";
            }
            WriteLine(empty, bottomLine);
            _line--;

            if (firstInput == null)
                return null;

            var botCommand = firstInput.StartsWith("!bot ");
            if (botCommand)
            {
                var botName = firstInput.Remove(5);
                //RunChatBot(botName);
                return null;
            }

            var perm = firstInput.StartsWith("!!");

            if (perm)
            {
                firstInput = firstInput.Remove(0, 1);
            }

            bool isColor = firstInput.StartsWith("!");
            if (isColor)
                firstInput = firstInput.Remove(0, 1);

            var findConsoleColor = Colors.FirstOrDefault(s => Enum.GetName(typeof(ConsoleColor), s) == firstInput);
            if (findConsoleColor != default(ConsoleColor) && isColor)
            {
                _line--;
                inputwrapper.Color = findConsoleColor;
                Console.ForegroundColor = inputwrapper.Color;
                _defaultColor = inputwrapper.Color;
                inputwrapper.Text = ReadLine(bottomLine);

                empty = "";
                for (int i = 0; i < inputwrapper.Text.Length; i++)
                {
                    empty += " ";
                }
                WriteLine(empty, bottomLine);

                if (perm)
                    PermColor = inputwrapper.Color;

                Console.ResetColor();
            }
            else
            {
                inputwrapper.Color = _defaultColor;
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
            WriteLine("Multible Addresses detected choose one");
            WriteLine("ID | IP");
            WriteLine("-------");
            for (int index = 0; index < ipAddresses.Length; index++)
            {
                var ipAddress = ipAddresses[index];
                WriteLine("{0}  | {1}", -1, index, ipAddress.ToString());
            }

            int input = -1;
            do
            {
                WriteLine("Select the Id please");
                int.TryParse(ReadLine(bottomLine), out input);
            } while (input <= 0 && input >= ipAddresses.Length);

            return ipAddresses[input];
        }

        private static void WriteSystemMessage(string mess)
        {
            var old = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            WriteLine("System> {0}", -1, mess);
            Console.ForegroundColor = old;
        }

        private static void WriteLoginMessage(string mess)
        {
            var old = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            WriteLine("Login System> {0}", -1, mess);
            Console.ForegroundColor = old;
        }

        private bool OnMessageBoostrap(object obj)
        {
            if (obj is ChatMessage)
            {
                HandleUnkownMessage(obj);
                return true;
            }
            return false;
        }

        private void HandleUnkownMessage(object obj)
        {
            var line = Console.CursorTop;
            var column = Console.CursorLeft;
            var mess = (obj as ChatMessage);
            var old = Console.ForegroundColor;
            lock (this)
            {
                Write("> {0} :", -1, tcpNetworkReceiver.Session.Calle.Username);
                Console.ForegroundColor = mess.Color;
                Write("\"{0}\"", -1, mess.Message);
            }
            //WriteLine("> {0} : \"{1}\"", -1, tcpNetworkReceiver.Session.Calle.Username, mess.Message);
            Console.ForegroundColor = old;
            Console.SetCursorPosition(column, line);
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

        public static ConsoleColor? PermColor { get; private set; }
    }
}
