using System.Diagnostics;
using MakoIoT.Device.Services.Interface;
using MakoIoT.Device.Services.Messaging.Test.Mocks;
using MakoIoT.Messages;
using Microsoft.Extensions.Logging;
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
        
            var bus = new MessageBus(commSvc, logger, new MessageBusOptions());
        
            bus.RegisterDirectMessageConsumer(typeof(TestMessage), typeof(TestMessageConsumer), ConsumeStrategy.Synchronous);
        
            string messageText = "{\"Message\":{\"Text\":\"Hello!\",\"MessageType\":\"MakoIoT.Device.Services.Messaging.Test.MessageBusTest+TestMessage\"}}";
        
            commSvc.InvokeMessageReceived(messageText);
            
            Assert.True(ConsumerCalled);
            Assert.False(logger.HasWarningsOrErrors);
        }

        [TestMethod]
        public void MessageReceived_given_unknown_MessageType_should_log_error()
        {
            ConsumerCalled = false;

            var logger = new MockLogger();

            var commSvc = new MockCommunicationService();

            var bus = new MessageBus(commSvc, logger, new MessageBusOptions());

            bus.RegisterDirectMessageConsumer(typeof(TestMessage), typeof(TestMessageConsumer), ConsumeStrategy.Synchronous);

            string messageText = "{\"Message\":{\"Text\":\"Hello!\",\"MessageType\":\"MakoIoT.Device.Services.Messaging.Test.MessageBusTest+UnknownType\"}}";

            commSvc.InvokeMessageReceived(messageText);

            Assert.Equal(1, logger.GetMessages(LogLevel.Error).Count);
            
            Assert.False(ConsumerCalled);
        }

        [TestMethod]
        public void MessageReceived_given_no_consumer_for_MessageType_should_log_warning()
        {
            ConsumerCalled = false;

            var logger = new MockLogger();

            var commSvc = new MockCommunicationService();

            var bus = new MessageBus(commSvc, logger, new MessageBusOptions());

            bus.RegisterDirectMessageConsumer(typeof(TestMessage), typeof(TestMessageConsumer), ConsumeStrategy.Synchronous);

            string messageText = "{\"Message\":{\"Text\":\"Hello!\",\"MessageType\":\"MakoIoT.Device.Services.Messaging.Test.MessageBusTest+TestMessageWithNoConsumer\"}}";

            commSvc.InvokeMessageReceived(messageText);

            Assert.Equal(1, logger.GetMessages(LogLevel.Warning).Count);
            Assert.False(logger.HasErrors);
            Assert.False(ConsumerCalled);
        }

        [TestMethod]
        public void Start_should_pass_subscriptions_array()
        {
            var logger = new MockLogger();

            var commSvc = new MockCommunicationService();

            var bus = new MessageBus(commSvc, logger, new MessageBusOptions());

            bus.RegisterSubscriptionConsumer(typeof(TestMessage), typeof(TestMessageConsumer), ConsumeStrategy.Synchronous);

            bus.Start();

            Assert.NotNull(commSvc.Subscriptions);
            Assert.True(commSvc.Subscriptions.Contains(typeof(TestMessage).FullName));
        }


        // [TestMethod]
        // public void Publish_should_publish_wrapped_message()
        // {
        //     var expectedTopic = "CShark.Mako.Iot.Client.Test.MessageBusTest+TestMessage";
        //     var expectedMessageRegex =
        //         new Regex(
        //             "\\{.*\"Message\":\\{\"Text\":\"Hello!\",\"MessageType\":\"CShark\\.Mako\\.Iot\\.Client\\.Test\\.MessageBusTest\\+TestMessage\"\\}.*\\}");
        //
        //     var logger = new MockLogger();
        //
        //     var commSvc = new MockCommunicationService
        //     {
        //         CanSend = true
        //     };
        //
        //     var bus = new MessageBus(commSvc, logger);
        //
        //     var message = new TestMessage
        //     {
        //         Text = "Hello!"
        //     };
        //     
        //     bus.Publish(message);
        //
        //     Assert.True(expectedMessageRegex.IsMatch(commSvc.MessageSent));
        //     Assert.Equal(expectedTopic, commSvc.TopicSent);
        //     Assert.False(logger.HasWarningsOrErrors);
        // }
        //
        // [TestMethod]
        // public void Send_should_send_wrapped_message()
        // {
        //     var recipient = "message-recipient";
        //     var expectedMessageRegex =
        //         new Regex(
        //             "\\{.*\"Message\":\\{\"Text\":\"Hello!\",\"MessageType\":\"CShark\\.Mako\\.Iot\\.Client\\.Test\\.MessageBusTest\\+TestMessage\"\\}.*\\}");
        //
        //     var logger = new MockLogger();
        //
        //     var commSvc = new MockCommunicationService
        //     {
        //         CanSend = true
        //     };
        //
        //     var bus = new MessageBus(commSvc, logger);
        //
        //     var message = new TestMessage
        //     {
        //         Text = "Hello!"
        //     };
        //
        //     bus.Send(message, recipient);
        //
        //     Assert.True(expectedMessageRegex.IsMatch(commSvc.MessageSent));
        //     Assert.Equal(recipient, commSvc.TopicSent);
        //     Assert.False(logger.HasWarningsOrErrors);
        // }

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
                Assert.NotNull(m);
                Debug.WriteLine($"TestMessageConsumer : {m.Text}");

                ConsumerCalled = true;
            }
        }
    }
}
