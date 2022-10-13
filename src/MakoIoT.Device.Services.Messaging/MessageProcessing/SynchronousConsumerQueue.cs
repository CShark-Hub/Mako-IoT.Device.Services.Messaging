using System;
using MakoIoT.Device.Services.Interface;
using Microsoft.Extensions.Logging;

namespace MakoIoT.Device.Services.Messaging.MessageProcessing
{
    public sealed class SynchronousConsumerQueue : ConsumerQueue
    {
        public SynchronousConsumerQueue(string name, ConsumerFactory consumerFactory, ILogger logger) : base(name, consumerFactory, logger)
        {
        }

        public override void Consume(ConsumeContext context)
        {
            _logger.LogDebug($"[{Name}] Consuming message {context.MessageId} synchronously");
            try
            {
                _consumerFactory().Consume(context);
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"[{Name}] Error processing message");
            }
        }
    }
}
