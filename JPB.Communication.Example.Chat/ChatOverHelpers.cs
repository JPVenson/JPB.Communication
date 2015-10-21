//using System;
//using System.Collections.Generic;
//using System.Collections.Specialized;
//using System.Linq;
//using System.Text;
//using System.Threading;
//using System.Threading.Tasks;
//using JPB.Communication.ComBase;
//using JPB.Communication.ComBase.Messages;
//using JPB.Communication.Shared;
//using JPB.Communication.WinRT.Serilizer;
//using JPB.Communication.WinRT.WinRT;

//namespace JPB.Communication.Example.Chat
//{
//    class ChatOverHelpers
//    {
//        public ChatOverHelpers()
//        {
//            Networkbase.DefaultMessageSerializer = new FullXmlSerilizer(typeof(MessageBase), typeof(Program.ChatMessage));
//            NetworkFactory.Create(new WinRTFactory());

//            Messages = NetworkValueBag<Program.ChatMessage>.CreateNetworkValueCollection(1338,
//                "20C67A45-6336-490E-B1CB-E8C9EE39D50E");
//        }

//        public void Run()
//        {
//            Console.WriteLine("Play server or Client? [C]");
//            var inp = Console.ReadKey(true).Key;
//            var server = inp == ConsoleKey.S && inp != ConsoleKey.Enter;

//            if (server)
//            {
//                Console.WriteLine("Host address:");
//                var address = Console.ReadLine();
//                var connect = Messages.Connect(address);
//                connect.Wait();
//            }

//            Messages.CollectionChanged += MessageReveived;

//            while (true)
//            {
//                var input = Program.ParseInput();

//                var chatMess = new Program.ChatMessage();
//                chatMess.Message = input.Text;
//                chatMess.Color = input.Color;

//                Messages.Add(new Program.ChatMessage());
//            }
//        }

//        private void MessageReveived(object sender, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
//        {
//            var mess = (Program.ChatMessage)notifyCollectionChangedEventArgs.NewItems[0];
//            lock (typeof(Console))
//            {
//                var oldColor = Console.BackgroundColor;
//                Console.BackgroundColor = mess.Color;
//                Console.WriteLine("> {0}", mess.Message);
//                Console.BackgroundColor = oldColor;
//            }
//        }

//        public NetworkValueBag<Program.ChatMessage> Messages { get; set; }

//    }
//}
