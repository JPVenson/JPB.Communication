using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JPB.Communication.ComBase;
using JPB.Communication.ComBase.Messages;
using JPB.Communication.ComBase.Messages.Wrapper;

namespace JPB.Communication.Test.LargeMessages
{
    public class Program
    {
        static void Main(string[] args)
        {
            var publicIp = NetworkInfoBase.GetPublicIp();
            Console.WriteLine(publicIp);

            Console.WriteLine("Sender?");

            NetworkFactory.Instance.InitCommonSenderAndReciver(1337, 1337);
            NetworkFactory.Instance.Reciever.LargeMessageSupport = true;
            NetworkFactory.Instance.Reciever.RegisterMessageBaseInbound(OnAction, "T");
            var fileStream = new FileStream(@"F:\SS_DL.dll", FileMode.Open, FileAccess.Read, FileShare.Read);

            NetworkFactory.Instance.Sender.SendStreamDataAsync(fileStream,
                new StreamMetaMessage("META DATA INBOUND", "T"),
                NetworkInfoBase.IpAddress.ToString());

            Console.ReadLine();
        }

        private static void OnAction(LargeMessage s)
        {
            Console.WriteLine(s.MetaData.Message);
            if (s.DataComplete)
            {
                s_OnLoadCompleted(s);
            }
            else
            {
                s.OnLoadCompleted += s_OnLoadCompleted;
            }
        }

        static void s_OnLoadCompleted(LargeMessage sender)
        {
            Console.WriteLine("File Loaded ... load content");
            var largeMessage = sender as LargeMessage;
            var infoLoaded = largeMessage.InfoLoaded() as FileStream;
            infoLoaded.Close();
        }
    }
}
