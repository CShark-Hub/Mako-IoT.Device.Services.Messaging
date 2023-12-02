using System;
using System.Collections;
using System.Threading;
using MakoIoT.Device.Services.Interface;

namespace MakoIoT.Device.Services.Messaging.MessageProcessing
{
    public class FifoConsumerQueue : ConsumerQueue
    {
        protected readonly Queue _queue;
        private bool _isProcessing;

        public event EventHandler ProcessingStarted;
        public event EventHandler ProcessingFinished;

        public FifoConsumerQueue(string name, ConsumerFactory consumerFactory, ILog logger) : base(name, consumerFactory, logger)
        {
            _queue = new Queue();
        }

        protected ConsumeContext SafeDequeue()
        {
            lock (_queue)
            {
                if (_queue.Count == 0)
                    return null;

                return (ConsumeContext)_queue.Dequeue();
            }
        }

        private void OnMessageEnqueue()
        {
            if (_isProcessing)
                return;

            _isProcessing = true;
            _logger.Trace($"[{Name}] Starting processing thread");
            new Thread(ProcessMessage).Start();
        }

        private void ProcessMessage()
        {
            _logger.Trace($"[{Name}] processing thread started");
            try
            {
                var context = SafeDequeue();
                if (context != null)
                {
                    _isProcessing = true;
                    ProcessingStarted?.Invoke(this, new ObjectEventArgs(Name));
                }
                while (context != null)
                {
                    _logger.Trace($"[{Name}] Consuming message {context.MessageId}");

                    try
                    {
                        _consumerFactory().Consume(context);
                    }
                    catch (Exception e)
                    {
                        _logger.Error($"[{Name}] Error processing message", e);
                    }

                    Thread.Sleep(1);

                    context = SafeDequeue();
                }
                _isProcessing = false;
                ProcessingFinished?.Invoke(this, new ObjectEventArgs(Name));
            }
            catch (Exception e)
            {
                _logger.Error($"[{Name}] Error processing message from queue", e);
            }
        }

        public override void Consume(ConsumeContext context)
        {
            lock (_queue)
            {
                _queue.Enqueue(context);
            }

            _logger.Trace($"[{Name}] Message {context.MessageId} queued");
            OnMessageEnqueue();
        }
    }
}
