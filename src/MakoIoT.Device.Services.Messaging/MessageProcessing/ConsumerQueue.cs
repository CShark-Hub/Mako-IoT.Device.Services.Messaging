using MakoIoT.Device.Services.Interface;
using Microsoft.Extensions.Logging;

namespace MakoIoT.Device.Services.Messaging.MessageProcessing
{
    public delegate IConsumer ConsumerFactory();

    public abstract class ConsumerQueue : IConsumerQueue
    {
        protected readonly ConsumerFactory _consumerFactory;
        protected readonly ILogger _logger;
        public string Name { get; }

        public ConsumerQueue(string name, ConsumerFactory consumerFactory, ILogger logger)
        {
            Name = name;
            _consumerFactory = consumerFactory;
            _logger = logger;
        }

        public abstract void Consume(ConsumeContext context);
    }
}
