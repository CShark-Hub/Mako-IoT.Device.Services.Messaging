using System.Diagnostics;
using MakoIoT.Device.Services.Interface;
using MakoIoT.Device.Services.Messaging.Test.Mocks;
using MakoIoT.Messages;
using nanoFramework.TestFramework;

namespace MakoIoT.Device.Services.Messaging.Test
{
    [TestClass]
    public class MessageBusTest
    {
        private static bool ConsumerCalled;


        [TestMethod]
        public void MessageReceived_should_invoke_Consumer()
        {
            ConsumerCalled = false;

            var logger = new MockLogger();
            var commSvc = new MockCommunicationService();
        
            var bus = new MessageBus(commSvc, logger, new MessageBusOptions(), null);
        
            bus.RegisterDirectMessageConsumer(typeof(TestMessage), typeof(TestMessageConsumer), ConsumeStrategy.Synchronous);
        
            string messageText = "{\"Message\":{\"Text\":\"Hello!\",\"MessageType\":\"MakoIoT.Device.Services.Messaging.Test.MessageBusTest+TestMessage\"}}";
        
            commSvc.InvokeMessageReceived(messageText);
            
            Assert.IsTrue(ConsumerCalled);
            Assert.IsFalse(logger.HasWarningsOrErrors);
        }

        [TestMethod]
        public void MessageReceived_given_unknown_MessageType_should_log_error()
        {
            ConsumerCalled = false;

            var logger = new MockLogger();

            var commSvc = new MockCommunicationService();

            var bus = new MessageBus(commSvc, logger, new MessageBusOptions(), null);

            bus.RegisterDirectMessageConsumer(typeof(TestMessage), typeof(TestMessageConsumer), ConsumeStrategy.Synchronous);

            string messageText = "{\"Message\":{\"Text\":\"Hello!\",\"MessageType\":\"MakoIoT.Device.Services.Messaging.Test.MessageBusTest+UnknownType\"}}";

            commSvc.InvokeMessageReceived(messageText);

            Assert.AreEqual(1, logger.GetMessages(LogEventLevel.Error).Count);
            
            Assert.IsFalse(ConsumerCalled);
        }

        [TestMethod]
        public void MessageReceived_given_no_consumer_for_MessageType_should_log_warning()
        {
            ConsumerCalled = false;

            var logger = new MockLogger();

            var commSvc = new MockCommunicationService();

            var bus = new MessageBus(commSvc, logger, new MessageBusOptions(), null);

            bus.RegisterDirectMessageConsumer(typeof(TestMessage), typeof(TestMessageConsumer), ConsumeStrategy.Synchronous);

            string messageText = "{\"Message\":{\"Text\":\"Hello!\",\"MessageType\":\"MakoIoT.Device.Services.Messaging.Test.MessageBusTest+TestMessageWithNoConsumer\"}}";

            commSvc.InvokeMessageReceived(messageText);

            Assert.AreEqual(1, logger.GetMessages(LogEventLevel.Warning).Count);
            Assert.IsFalse(logger.HasErrors);
            Assert.IsFalse(ConsumerCalled);
        }

        [TestMethod]
        public void Start_should_pass_subscriptions_array()
        {
            var logger = new MockLogger();

            var commSvc = new MockCommunicationService();

            var bus = new MessageBus(commSvc, logger, new MessageBusOptions(), null);

            bus.RegisterSubscriptionConsumer(typeof(TestMessage), typeof(TestMessageConsumer), ConsumeStrategy.Synchronous);

            bus.Start();

            Assert.IsNotNull(commSvc.Subscriptions);
            Assert.IsTrue(commSvc.Subscriptions.Contains(typeof(TestMessage).FullName));
        }

        public class TestMessage : IMessage
        {
            public TestMessage()
            {
                MessageType = this.GetType().FullName;
            }
            public string MessageType { get; set; }

            public string Text { get; set; }
        }

        public class TestMessageWithNoConsumer : IMessage
        {
            public TestMessageWithNoConsumer()
            {
                MessageType = this.GetType().FullName;
            }
            public string MessageType { get; set; }

            public string Text { get; set; }
        }

        public class TestMessageConsumer : IConsumer
        {
            public void Consume(ConsumeContext context)
            {
                var m = context.Message as TestMessage;
                Assert.IsNotNull(m);
                Debug.WriteLine($"TestMessageConsumer : {m.Text}");

                ConsumerCalled = true;
            }
        }
    }
}
