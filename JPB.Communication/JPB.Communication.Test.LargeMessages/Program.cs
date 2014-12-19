using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JPB.Communication.ComBase;
using JPB.Communication.ComBase.Messages;

namespace JPB.Communication.Test.LargeMessages
{
    public class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Sender?");

            //var consoleKeyInfo = Console.ReadKey();
            //if (consoleKeyInfo.Key == ConsoleKey.N)
            //{
            //    Console.WriteLine("Wait for it");
            //    NetworkFactory.Instance.InitCommonSenderAndReciver(1337, 1338);
            //    NetworkFactory.Instance.Reciever.LargeMessageSupport = true;
            //    NetworkFactory.Instance.Reciever.RegisterMessageBaseInbound(OnAction, "T");
            //}
            //else
            //{
            //    NetworkFactory.Instance.InitCommonSenderAndReciver(1338, 1337);
            //    var fileStream = new FileStream(@"D:\LoadXmlTest.cs", FileMode.Open, FileAccess.Read, FileShare.Read);

            //    NetworkFactory.Instance.Sender.SendStreamDataAsync(fileStream,
            //        new MessageBase("META DATA INBOUND", "T"),
            //        NetworkInfoBase.IpAddress.ToString());
            //}


            NetworkFactory.Instance.InitCommonSenderAndReciver(1337, 1337);
            NetworkFactory.Instance.Reciever.LargeMessageSupport = true;
            NetworkFactory.Instance.Reciever.RegisterMessageBaseInbound(OnAction, "T");
            var fileStream = new FileStream(@"F:\SS_DL.dll", FileMode.Open, FileAccess.Read, FileShare.Read);

            NetworkFactory.Instance.Sender.SendStreamDataAsync(fileStream,
                new MessageBase("META DATA INBOUND", "T"),
                NetworkInfoBase.IpAddress.ToString());

            Console.ReadLine();
        }

        private static void OnAction(LargeMessage s)
        {
            Console.WriteLine(s.MetaData.Message);
            if (s.DataComplete)
            {
                s_OnLoadCompleted(s, null);
            }
            else
            {
                s.OnLoadCompleted += s_OnLoadCompleted;
            }
        }

        static void s_OnLoadCompleted(object sender, EventArgs e)
        {
            Console.WriteLine("File Loaded ... load content");

            var largeMessage = sender as LargeMessage;
            var infoLoaded = largeMessage.InfoLoaded() as FileStream;
            infoLoaded.Close();
        }
    }
}
