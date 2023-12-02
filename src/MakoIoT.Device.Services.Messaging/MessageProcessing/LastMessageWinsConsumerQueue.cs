using MakoIoT.Device.Services.Interface;

namespace MakoIoT.Device.Services.Messaging.MessageProcessing
{
    public sealed class LastMessageWinsConsumerQueue : FifoConsumerQueue
    {
        public LastMessageWinsConsumerQueue(string name, ConsumerFactory consumerFactory, ILog logger) : base(name, consumerFactory, logger)
        {
        }

        public override void Consume(ConsumeContext context)
        {
            _logger.Trace($"[{Name}] Removing {_queue.Count} message(s) from queue");
            lock (_queue)
            {
                _queue.Clear();
            }
            base.Consume(context);
        }
    }
}
