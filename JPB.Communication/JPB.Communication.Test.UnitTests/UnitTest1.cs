using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using JPB.Communication.ComBase;
using System.Threading;
using JPB.Communication.ComBase.Messages;
using System.Threading.Tasks;
using JPB.Communication.ComBase.Serializer;

namespace JPB.Communication.Test.UnitTests
{
    [TestClass]
    public class SenderReceiverTest
    {
        private const string TestInfoState = "testinfostate";
        private const string TestMessageObject = "testmessageobjectfor testing blöa";

        private ManualResetEvent waitOne = new ManualResetEvent(false);

        [TestMethod]
        public void TestSimpleSendAndReceive()
        {
            Networkbase.DefaultMessageSerializer = new NetContractSerializer();
            NetworkFactory.Create(new WinRT.WinRT.WinRTFactory());
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
            NetworkFactory.Create(new WinRT.WinRT.WinRTFactory());
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
