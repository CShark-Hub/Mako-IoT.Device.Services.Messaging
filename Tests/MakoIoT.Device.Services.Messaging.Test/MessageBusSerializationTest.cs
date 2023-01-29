using MakoIoT.Device.Services.Interface;
using MakoIoT.Device.Services.Messaging.Test.Mocks;
using MakoIoT.Messages;
using nanoFramework.Json;
using nanoFramework.TestFramework;
using System;
using System.Diagnostics;
using System.Text;
using static MakoIoT.Device.Services.Messaging.Test.MessageBusTest;

namespace MakoIoT.Device.Services.Messaging.Test
{
    [TestClass]
    public class MessageBusSendRecieveTests
    {
        public class TestMessageNested : IMessage
        {
            public TestMessageNested()
            {
                MessageType = this.GetType().FullName;
            }
            public string Text { get; set; }
            public string MessageType { get; set; }
            public TestMessage MessageInternal { get; set; }
        }

        public class TestMessageConsumer : IConsumer
        {
            public static object LastMessageReceived { get; private set; }

            public void Consume(ConsumeContext context)
            {
                LastMessageReceived = context.Message;
            }
        }

        [TestMethod]
        public void MessageBus_Should_BeAbleToSendAndRecieveMessage()
        {
            var textToCheck = "test";
            var internalTextToCheck = "test2";
            var msg = new TestMessageNested()
            {
                Text = textToCheck,
                MessageInternal = new TestMessage()
                {
                    Text = internalTextToCheck
                }
            };
            var mockComService = new MockCommunicationService();
            var msgBus = new MessageBus(mockComService, new MockLogger(), new MessageBusOptions(), null);
            msgBus.RegisterSubscriptionConsumer(typeof(TestMessageNested), typeof(TestMessageConsumer), ConsumeStrategy.Synchronous);

            var messageAsJson = msgBus.WrapMessage(msg);
            mockComService.InvokeMessageReceived(messageAsJson);

            var messageRecieved = TestMessageConsumer.LastMessageReceived;
            Assert.AreEqual(messageRecieved.GetType().FullName, typeof(TestMessageNested).FullName);
            var asMessage = (TestMessageNested)messageRecieved;
            Assert.AreEqual(asMessage.Text, textToCheck);
            Assert.AreEqual(asMessage.MessageInternal.Text, internalTextToCheck);
        }
    }
}
