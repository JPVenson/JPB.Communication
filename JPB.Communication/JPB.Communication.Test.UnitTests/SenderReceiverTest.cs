using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using JPB.Communication.ComBase;
using System.Threading;
using JPB.Communication.ComBase.Messages;
using System.Threading.Tasks;
using JPB.Communication.ComBase.Serializer;
using JPB.Communication.NativeWin.Serilizer;
using JPB.Communication.NativeWin.WinRT;
using JPB.Communication.PCLIntigration.ComBase;

namespace JPB.Communication.Test.UnitTests
{
    [TestClass]
    public class SenderReceiverTest
    {
        private const string TestInfoState = "testinfostate";
        private string TestMessageObject = "testmessageobjectfor testing blöa";

        private ManualResetEvent waitOne = new ManualResetEvent(false);

        [TestMethod]
        public void TestAuthentification()
        {
            Networkbase.DefaultMessageSerializer = new NetContractSerializer();
            NetworkFactory.Create(new WinRTFactory());
            var sender = NetworkFactory.Instance.GetSender(1337);
            var receiver = NetworkFactory.Instance.GetReceiver(1337);
            var sharedPassword = Guid.NewGuid().ToString().Substring(0,10);

            sender.ChangeNetworkCredentials(true, new PCLIntigration.ComBase.Messages.LoginMessage()
            {
                Username = Environment.UserDomainName + "@" + Environment.UserName,
                Password = sharedPassword
            });

            receiver.CheckCredentials = true;
            NetworkAuthentificator.Instance.DefaultLoginBevavior = DefaultLoginBevavior.IpNameCheckOnly;

            receiver.RegisterMessageBaseInbound(MessageInbound, TestInfoState);

            var result = sender.SendMessage(new MessageBase()
            {
                Message = TestMessageObject,
                InfoState = TestInfoState
            }, NetworkInfoBase.IpAddress.ToString());

            Assert.IsTrue(result);
            var handel = waitOne.WaitOne(new TimeSpan(0, 0, 5));
            Assert.IsTrue(handel);
        }

        [TestMethod]
        public void TestSimpleSendAndReceive()
        {
            Networkbase.DefaultMessageSerializer = new NetContractSerializer();
            NetworkFactory.Create(new WinRTFactory());
            var sender = NetworkFactory.Instance.GetSender(1337);
            var receiver = NetworkFactory.Instance.GetReceiver(1337);

            receiver.RegisterMessageBaseInbound(MessageInbound, TestInfoState);

            var result = sender.SendMessage(new ComBase.Messages.MessageBase()
            {
                Message = TestMessageObject,
                InfoState = TestInfoState
            }, NetworkInfoBase.IpAddress.ToString());

            Assert.IsTrue(result);
            var handel = waitOne.WaitOne(new TimeSpan(0, 0, 15));
            Assert.IsTrue(handel);
        }

        [TestMethod]
        public async Task RequestTest()
        {
            Networkbase.DefaultMessageSerializer = new NetContractSerializer();
            NetworkFactory.Create(new WinRTFactory());
            var sender = NetworkFactory.Instance.GetSender(1337);
            var receiver = NetworkFactory.Instance.GetReceiver(1337);
            NetworkFactory.Instance.GetReceiver(1338);
            receiver.RegisterRequstHandler(MessageInboundRequest, TestInfoState);

            var result = await sender.SendRequstMessage<string>(new RequstMessage()
            {
                Message = TestMessageObject,
                InfoState = TestInfoState,
                ExpectedResult = 1338
            }, NetworkInfoBase.IpAddress.ToString());

            Assert.IsNotNull(result);
            Assert.AreEqual(result, TestMessageObject);
        }

        [TestMethod]
        public async Task SingleConnectionTest()
        {
            Networkbase.DefaultMessageSerializer = new NetContractSerializer();
            NetworkFactory.Create(new WinRTFactory());

            var currentIp = NetworkInfoBase.IpAddress.ToString();
            var sender = NetworkFactory.Instance.GetSender(1337);
            sender.SharedConnection = true;
            var receiver = NetworkFactory.Instance.GetReceiver(1337);
            await sender.InitSharedConnection(currentIp);

            receiver.RegisterMessageBaseInbound(MessageInbound, TestInfoState);

            var result = sender.SendMessage(new ComBase.Messages.MessageBase()
            {
                Message = TestMessageObject,
                InfoState = TestInfoState
            }, currentIp);

            Assert.IsTrue(result);
            var handel = waitOne.WaitOne(new TimeSpan(0, 0, 15));
            Assert.IsTrue(handel);
            TestMessageObject = "";

            var realMessageWithoutContent = new ComBase.Messages.MessageBase()
            {
                Message = TestMessageObject,
                InfoState = TestInfoState
            };

            var currentSize = calcSize(sender, realMessageWithoutContent);

            while (currentSize != 16384)
            {
                if (currentSize < 16384)
                    TestMessageObject += 0x01;
                if (currentSize < 16384)
                    TestMessageObject += TestMessageObject.Remove(TestMessageObject.Length - 1);
                realMessageWithoutContent.Message = TestMessageObject;
                currentSize = calcSize(sender, realMessageWithoutContent);
            }
            
            result = sender.SendMessage(realMessageWithoutContent, currentIp);

            Assert.IsTrue(result);
            handel = waitOne.WaitOne(new TimeSpan(0, 0, 15));
            Assert.IsTrue(handel);
        }

        private int calcSize(Networkbase baseS, MessageBase makeup)
        {
            var mess = new NetworkMessage();
            byte[] saveMessageBaseAsBinary = baseS.SaveMessageBaseAsContent(makeup);

            mess.MessageBase = saveMessageBaseAsBinary;
            mess.Reciver = makeup.Reciver;
            mess.Sender = makeup.Sender;

            var messSeri = baseS.Serialize(mess);
            return messSeri.Length;
        }

        private object MessageInboundRequest(ComBase.Messages.RequstMessage obj)
        {
            //Thread.Sleep(20000);
            Assert.AreEqual(obj.InfoState, TestInfoState);
            Assert.AreEqual(obj.Message, TestMessageObject);

            return TestMessageObject;
        }

        private void MessageInbound(ComBase.Messages.MessageBase obj)
        {
            Assert.AreEqual(obj.InfoState, TestInfoState);
            Assert.AreEqual(obj.Message, TestMessageObject);
            waitOne.Set();
        }
    }
}
