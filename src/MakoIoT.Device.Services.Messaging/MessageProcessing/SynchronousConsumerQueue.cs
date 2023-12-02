using System;
using MakoIoT.Device.Services.Interface;

namespace MakoIoT.Device.Services.Messaging.MessageProcessing
{
    public sealed class SynchronousConsumerQueue : ConsumerQueue
    {
        public SynchronousConsumerQueue(string name, ConsumerFactory consumerFactory, ILog logger) : base(name, consumerFactory, logger)
        {
        }

        public override void Consume(ConsumeContext context)
        {
            _logger.Trace($"[{Name}] Consuming message {context.MessageId} synchronously");
            try
            {
                _consumerFactory().Consume(context);
            }
            catch (Exception e)
            {
                _logger.Error($"[{Name}] Error processing message", e);
            }
        }
    }
}
