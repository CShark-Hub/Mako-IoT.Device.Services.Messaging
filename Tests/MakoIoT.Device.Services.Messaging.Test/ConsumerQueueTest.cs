using System;
using System.Diagnostics;
using System.Threading;
using MakoIoT.Device.Services.Interface;
using MakoIoT.Device.Services.Messaging.MessageProcessing;
using MakoIoT.Device.Services.Messaging.Test.Mocks;
using MakoIoT.Messages;
using nanoFramework.TestFramework;

namespace MakoIoT.Device.Services.Messaging.Test
{
    [TestClass]
    public class ConsumerQueueTest
    {
        [TestMethod]
        public void Consume_given_SynchronousStrategy_should_consume_message_synchronously()
        {
            var context = new ConsumeContext(new Envelope
            {
                Message = new MessageBusTest.TestMessage(),
                MessageId = Guid.NewGuid().ToString()
            });

            var consumer = new TestConsumer();
            var logger = new MockLogger();

            var sut = new SynchronousConsumerQueue("Test", () => consumer, logger);
            sut.Consume(context);

            Assert.AreSame(context, consumer.Context);
            Assert.IsFalse(logger.HasWarningsOrErrors);

        }

        [TestMethod]
        public void Consume_given_FIFOStrategy_should_consume_all_message_one_by_one()
        {
            var context1 = new ConsumeContext(new Envelope
            {
                Message = new MessageBusTest.TestMessage(),
                MessageId = "1"
            });

            var context2 = new ConsumeContext(new Envelope
            {
                Message = new MessageBusTest.TestMessage(),
                MessageId = "2"
            });

            var context3 = new ConsumeContext(new Envelope
            {
                Message = new MessageBusTest.TestMessage(),
                MessageId = "3"
            });

            bool context1Consumed = false, context2Consumed = false, context3Consumed = false;
            
            var consumer = new TestConsumer(true);
            consumer.MessageConsumed += (sender, args) =>
            {
                var consumerContext = ((ObjectEventArgs)args).Data as ConsumeContext;
                Debug.WriteLine($"message consumed: {consumerContext.MessageId}");
                switch (consumerContext.MessageId)
                {
                    case "1": context1Consumed = true; break;
                    case "2": context2Consumed = true; break;
                    case "3": context3Consumed = true; break;
                }
            };


            var logger = new MockLogger();

            var sut = new FifoConsumerQueue("Test", () => consumer, logger);
            sut.Consume(context1);
            Thread.Sleep(1);
            sut.Consume(context2);
            Thread.Sleep(1);
            sut.Consume(context3);

            Thread.Sleep(1);

            consumer.Semaphore.Set();
            Thread.Sleep(1);
            consumer.Semaphore.Set();
            Thread.Sleep(1);
            consumer.Semaphore.Set();
            Thread.Sleep(1);

            foreach (string message in logger.GetAllMessages())
            {
                Debug.WriteLine(message);
            }

            Assert.IsTrue(context1Consumed, "message 1 not consumed");
            Assert.IsTrue(context2Consumed, "message 2 not consumed");
            Assert.IsTrue(context3Consumed, "message 3 not consumed");
            Assert.IsFalse(logger.HasWarningsOrErrors);
        }

        [TestMethod]
        public void Consume_given_LastWinsStrategy_should_skip_middle_message()
        {
            var context1 = new ConsumeContext(new Envelope
            {
                Message = new MessageBusTest.TestMessage(),
                MessageId = "1"
            });

            var context2 = new ConsumeContext(new Envelope
            {
                Message = new MessageBusTest.TestMessage(),
                MessageId = "2"
            });

            var context3 = new ConsumeContext(new Envelope
            {
                Message = new MessageBusTest.TestMessage(),
                MessageId = "3"
            });

            bool context1Consumed = false, context2Consumed = false, context3Consumed = false;

            var consumer = new TestConsumer(true);
            consumer.MessageConsumed += (sender, args) =>
            {
                var consumerContext = ((ObjectEventArgs)args).Data as ConsumeContext;
                Debug.WriteLine($"message consumed: {consumerContext.MessageId}");
                switch (consumerContext.MessageId)
                {
                    case "1": context1Consumed = true; break;
                    case "2": context2Consumed = true; break;
                    case "3": context3Consumed = true; break;
                }
            };


            var logger = new MockLogger();

            var sut = new LastMessageWinsConsumerQueue("Test", () => consumer, logger);
            sut.Consume(context1);
            Thread.Sleep(1);
            sut.Consume(context2);
            Thread.Sleep(1);
            sut.Consume(context3);

            Thread.Sleep(1);

            consumer.Semaphore.Set();
            Thread.Sleep(1);
            consumer.Semaphore.Set();
            Thread.Sleep(1);
            consumer.Semaphore.Set();
            Thread.Sleep(1);

            foreach (string message in logger.GetAllMessages())
            {
                Debug.WriteLine(message);
            }

            Assert.IsTrue(context1Consumed, "message 1 not consumed");
            Assert.IsFalse(context2Consumed, "message 2 consumed");
            Assert.IsTrue(context3Consumed, "message 3 not consumed");
            Assert.IsFalse(logger.HasWarningsOrErrors);
        }

        public class TestConsumer : IConsumer
        {
            public bool Wait { get; set; }


            public AutoResetEvent Semaphore = new AutoResetEvent(false);
            public ConsumeContext Context { get; private set; }

            public event EventHandler MessageConsumed;

            public TestConsumer(bool wait = false)
            {
                Wait = wait;
            }

            public void Consume(ConsumeContext context)
            {
                Context = context;
                MessageConsumed?.Invoke(this, new ObjectEventArgs(context));
                if (Wait)
                    Semaphore.WaitOne(5000, true);
            }
        }
    }
}
