using System.Runtime.CompilerServices;
using System;
using System.Collections;
using MakoIoT.Device.Services.Interface;
using MakoIoT.Device.Services.Messaging.MessageProcessing;
using MakoIoT.Device.Utilities.String.Extensions;
using MakoIoT.Messages;
using Microsoft.Extensions.Logging;
using nanoFramework.Json;
using MakoIoT.Device.Services.Messaging.MessageConverters;
using Microsoft.Extensions.DependencyInjection;

[assembly: InternalsVisibleTo("NFUnitTest")]
namespace MakoIoT.Device.Services.Messaging
{
    public class MessageBus : IMessageBus
    {
        private readonly Hashtable _consumerQueues = new Hashtable();
        private readonly ArrayList _subscriptions = new ArrayList();

        private readonly ICommunicationService _communicationService;
        private readonly ILogger _logger;
        private readonly IServiceProvider serviceProvider;

        private static bool isInitialized;

        public MessageBus(ICommunicationService communicationService, ILogger logger, MessageBusOptions options, IServiceProvider serviceProvider)
        {
            if (!isInitialized)
            {
                nanoFramework.Json.Configuration.ConvertersMapping.Add(typeof(IMessage), new IMessageConvert());
                isInitialized = true;
            }

            _communicationService = communicationService;
            _logger = logger;
            this.serviceProvider = serviceProvider;
            foreach (Type key in options.DirectConsumers.Keys)
            {
                var value = (ConsumersMapping)options.DirectConsumers[key];
                RegisterDirectMessageConsumer(key, value.ConsumerType, value.Strategy);
            }

            foreach (Type key in options.SubscriptionConsumers.Keys)
            {
                var value = (ConsumersMapping)options.SubscriptionConsumers[key];
                RegisterSubscriptionConsumer(key, value.ConsumerType, value.Strategy);
            }

            _communicationService.MessageReceived += OnMessageReceived;
        }

        public void Start()
        {
            try
            {
                _communicationService.Connect((string[])_subscriptions.ToArray(typeof(string)));
            }
            catch (Exception e)
            {
                _logger.LogCritical(e, "Communication service can't connect");
            }
        }

        public void Stop()
        {
            try
            {
                _communicationService.Disconnect();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error disconnecting communication service");
            }
            
        }

        private IConsumerQueue CreateConsumerQueue(Type consumerType, ConsumeStrategy consumeStrategy)
        {
            return consumeStrategy switch
            {
                ConsumeStrategy.FIFO => new FifoConsumerQueue(consumerType.Name, () => (IConsumer)ActivatorUtilities.CreateInstance(serviceProvider, consumerType), _logger),
                ConsumeStrategy.LastMessageWins => new LastMessageWinsConsumerQueue(consumerType.Name, () => (IConsumer)ActivatorUtilities.CreateInstance(serviceProvider, consumerType), _logger),
                ConsumeStrategy.Synchronous => new SynchronousConsumerQueue(consumerType.Name, () => (IConsumer)ActivatorUtilities.CreateInstance(serviceProvider, consumerType), _logger),
                _ => throw new ArgumentException(string.Format("{0} is not supported", consumeStrategy))
            };
        }

        public void RegisterDirectMessageConsumer(Type messageType, Type consumerType, ConsumeStrategy consumeStrategy)
        {
            _consumerQueues.Add(messageType.FullName, CreateConsumerQueue(consumerType, consumeStrategy));
        }

        public void RegisterSubscriptionConsumer(Type messageType, Type consumerType, ConsumeStrategy consumeStrategy)
        {
            _consumerQueues.Add(messageType.FullName, CreateConsumerQueue(consumerType, consumeStrategy));
            _subscriptions.Add(messageType.FullName);
        }

        public void Publish(IMessage message, bool delay = false)
        {
            try
            {
                var envelopeString = WrapMessage(message);

                if (_communicationService.CanSend && !delay)
                {
                    _communicationService.Publish(envelopeString, message.MessageType);
                }
                else
                {
                    //TODO: store message to be sent later

                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Error publishing message {message.MessageType}");
            }
        }

        public void Send(IMessage message, string recipient)
        {
            try
            {
                var envelopeString = WrapMessage(message);

                if (_communicationService.CanSend)
                {
                    _communicationService.Send(envelopeString, recipient);
                }
                else
                {
                    //TODO: store message to be sent later

                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Error sending message {message.MessageType}");
            }
            
        }

        internal string WrapMessage(IMessage message)
        {
            var envelope = new Envelope
            {
                OriginTime = DateTime.UtcNow,
                MessageId = Guid.NewGuid().ToString(),
                Sender = _communicationService.ClientName,
                SenderAddress = _communicationService.ClientAddress,
                Message = message
            };

            var envelopeString = JsonConvert.SerializeObject(envelope);

            _logger.LogDebug(envelopeString.EscapeForInterpolation());
            return envelopeString;
        }

        internal void OnMessageReceived(object sender, EventArgs e)
        {
            try
            {
                var envelope = (Envelope)JsonConvert.DeserializeObject((string)((ObjectEventArgs)e).Data, typeof(Envelope));

                if (!_consumerQueues.Contains(envelope.Message.MessageType))
                {
                    _logger.LogWarning($"No consumer for message type {envelope.Message.MessageType}");
                    return;
                }

                _logger.LogDebug($"Received message of type {envelope.Message.MessageType}");

                var consumer = (IConsumerQueue)_consumerQueues[envelope.Message.MessageType];
                consumer.Consume(new ConsumeContext(envelope));
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Error processing message");
            }
        }
    }
}
