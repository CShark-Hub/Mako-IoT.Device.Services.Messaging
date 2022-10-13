using System;
using System.Collections;
using System.Threading;
using MakoIoT.Device.Services.Interface;
using Microsoft.Extensions.Logging;

namespace MakoIoT.Device.Services.Messaging.MessageProcessing
{
    public class FifoConsumerQueue : ConsumerQueue
    {
        protected readonly Queue _queue;
        private bool _isProcessing;

        public event EventHandler ProcessingStarted;
        public event EventHandler ProcessingFinished;

        public FifoConsumerQueue(string name, ConsumerFactory consumerFactory, ILogger logger) : base(name, consumerFactory, logger)
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
            _logger.LogTrace($"[{Name}] Starting processing thread");
            new Thread(ProcessMessage).Start();
        }

        private void ProcessMessage()
        {
            _logger.LogTrace($"[{Name}] processing thread started");
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
                    _logger.LogDebug($"[{Name}] Consuming message {context.MessageId}");

                    try
                    {
                        _consumerFactory().Consume(context);
                    }
                    catch (Exception e)
                    {
                        _logger.LogError(e, $"[{Name}] Error processing message");
                    }

                    Thread.Sleep(1);

                    context = SafeDequeue();
                }
                _isProcessing = false;
                ProcessingFinished?.Invoke(this, new ObjectEventArgs(Name));
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"[{Name}] Error processing message from queue");
            }
        }

        public override void Consume(ConsumeContext context)
        {
            lock (_queue)
            {
                _queue.Enqueue(context);
            }

            _logger.LogTrace($"[{Name}] Message {context.MessageId} queued");
            OnMessageEnqueue();
        }
    }
}
