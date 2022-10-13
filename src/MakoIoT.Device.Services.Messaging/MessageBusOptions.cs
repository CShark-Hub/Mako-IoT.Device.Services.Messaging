using System;
using System.Collections;
using MakoIoT.Device.Services.Interface;

namespace MakoIoT.Device.Services.Messaging
{
    public class MessageBusOptions
    {
        internal Hashtable DirectConsumers = new();
        internal Hashtable SubscriptionConsumers = new();

        public void AddDirectMessageConsumer(Type messageType, Type consumerType, ConsumeStrategy consumeStrategy)
        {
            DirectConsumers.Add(messageType, new ConsumersMapping(consumerType, consumeStrategy));
        }

        public void AddSubscriptionConsumer(Type messageType, Type consumerType, ConsumeStrategy consumeStrategy)
        {
            SubscriptionConsumers.Add(messageType, new ConsumersMapping(consumerType, consumeStrategy));
        }
    }

    internal struct ConsumersMapping
    {
        internal Type ConsumerType { get; }
        internal ConsumeStrategy Strategy { get; }

        internal ConsumersMapping(Type consumerType, ConsumeStrategy strategy)
        {
            ConsumerType = consumerType;
            Strategy = strategy;
        }
    }
}
